import * as admin from 'firebase-admin';

const db = () => admin.firestore();

export interface PurchaseRecord {
  productId: string;
  orderId: string;
  purchaseTimeMillis: number;
  verified: boolean;
  rewarded: boolean;
  timestamp: admin.firestore.FieldValue;
}

export async function checkIdempotency(playerId: string, purchaseToken: string): Promise<(Partial<PurchaseRecord> & { orderId?: string }) | null> {
  const ref = db().collection('players').doc(playerId).collection('purchases').doc(purchaseToken);
  const snap = await ref.get();
  if (!snap.exists) return null;
  return snap.data() as Partial<PurchaseRecord>;
}

export async function writePurchaseRecord(
  playerId: string,
  purchaseToken: string,
  data: { productId: string; orderId: string; purchaseTimeMillis: number }
): Promise<void> {
  const ref = db().collection('players').doc(playerId).collection('purchases').doc(purchaseToken);
  await ref.set({
    ...data,
    verified: true,
    rewarded: false,
    timestamp: admin.firestore.FieldValue.serverTimestamp(),
  });
}

export async function grantReward(
  playerId: string,
  purchaseToken: string,
  orderId: string,
  coins: number,
  gems: number
): Promise<void> {
  const playerRef  = db().collection('players').doc(playerId);
  const purchaseRef = playerRef.collection('purchases').doc(purchaseToken);
  const txRef      = playerRef.collection('transactions').doc(orderId);
  const economyRef = playerRef.collection('economy').doc('state');

  const batch = db().batch();
  batch.update(purchaseRef, { rewarded: true });
  batch.set(txRef, { orderId, coins, gems, timestamp: admin.firestore.FieldValue.serverTimestamp() });
  batch.set(economyRef, {
    coins: admin.firestore.FieldValue.increment(coins),
    gems:  admin.firestore.FieldValue.increment(gems),
  }, { merge: true });

  await batch.commit();
}
