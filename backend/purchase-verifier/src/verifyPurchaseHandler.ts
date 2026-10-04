import { Request, Response } from 'express';
import * as admin from 'firebase-admin';
import { checkIdempotency, writePurchaseRecord, grantReward } from './firestoreService';
import { verifyConsumable, verifyNonConsumable, acknowledgeConsumable, acknowledgeNonConsumable } from './googlePlayService';
import { getRewardForProduct, isNonConsumable } from './rewardConfig';

const ALLOWED_PRODUCT_IDS = (process.env.ALLOWED_PRODUCT_IDS ?? '').split(',').map(s => s.trim()).filter(Boolean);
const PACKAGE_NAME = process.env.PACKAGE_NAME ?? '';

export async function verifyPurchaseHandler(req: Request, res: Response): Promise<void> {
  const authHeader = req.headers.authorization;
  if (!authHeader?.startsWith('Bearer ')) {
    res.status(401).json({ isValid: false, error: 'MISSING_AUTH' });
    return;
  }

  const idToken = authHeader.slice(7);
  let uid: string;
  try {
    const decoded = await admin.auth().verifyIdToken(idToken);
    uid = decoded.uid;
  } catch {
    res.status(401).json({ isValid: false, error: 'INVALID_AUTH' });
    return;
  }

  const { productId, purchaseToken, packageName, playerId } = req.body;

  if (!productId || !purchaseToken || !packageName || !playerId) {
    res.status(400).json({ isValid: false, error: 'MISSING_FIELDS' });
    return;
  }

  if (playerId !== uid) {
    res.status(400).json({ isValid: false, error: 'PLAYER_ID_MISMATCH' });
    return;
  }

  if (packageName !== PACKAGE_NAME) {
    res.status(400).json({ isValid: false, error: 'INVALID_PACKAGE' });
    return;
  }

  if (!ALLOWED_PRODUCT_IDS.includes(productId)) {
    res.status(400).json({ isValid: false, error: 'UNKNOWN_PRODUCT' });
    return;
  }

  const existing = await checkIdempotency(playerId, purchaseToken);
  if (existing?.rewarded === true) {
    res.json({ isValid: true, isAlreadyProcessed: true, transactionId: existing.orderId ?? null });
    return;
  }

  const nonConsumable = isNonConsumable(productId);
  let verificationResult: Awaited<ReturnType<typeof verifyConsumable>>;
  try {
    verificationResult = nonConsumable
      ? await verifyNonConsumable(packageName, productId, purchaseToken)
      : await verifyConsumable(packageName, productId, purchaseToken);
  } catch (err) {
    console.error(JSON.stringify({ severity: 'ERROR', message: 'Google Play verification failed', productId, error: String(err) }));
    res.status(200).json({ isValid: false, error: 'VERIFICATION_FAILED' });
    return;
  }

  if (verificationResult.purchaseState !== 0) {
    res.json({ isValid: false, error: 'PURCHASE_NOT_COMPLETED' });
    return;
  }

  const orderId = verificationResult.orderId ?? `${productId}_${Date.now()}`;
  const purchaseTimeMillis = verificationResult.purchaseTimeMillis ?? Date.now();

  try {
    await writePurchaseRecord(playerId, purchaseToken, { productId, orderId, purchaseTimeMillis });
  } catch (err) {
    console.error(JSON.stringify({ severity: 'ERROR', message: 'Failed to write purchase record', productId, error: String(err) }));
    res.status(500).json({ isValid: false, error: 'INTERNAL_ERROR' });
    return;
  }

  const reward = getRewardForProduct(productId);
  const coinsGranted = reward?.coins ?? 0;
  const gemsGranted  = reward?.gems  ?? 0;
  const powerUps     = reward?.powerUps ?? [];

  try {
    await grantReward(playerId, purchaseToken, orderId, coinsGranted, gemsGranted);
  } catch (err) {
    console.error(JSON.stringify({ severity: 'ERROR', message: 'Failed to grant reward', productId, orderId, error: String(err) }));
    res.status(500).json({ isValid: false, error: 'REWARD_FAILED' });
    return;
  }

  try {
    if (nonConsumable) {
      await acknowledgeNonConsumable(packageName, productId, purchaseToken);
    } else {
      await acknowledgeConsumable(packageName, productId, purchaseToken);
    }
  } catch (err) {
    console.error(JSON.stringify({ severity: 'WARNING', message: 'Acknowledge failed — reward already granted', productId, orderId, error: String(err) }));
  }

  res.json({ isValid: true, transactionId: orderId, coinsGranted, gemsGranted, powerUps, isAlreadyProcessed: false });
}
