import * as admin from 'firebase-admin';
import { FieldValue } from 'firebase-admin/firestore';
import * as crypto from 'crypto';
import { economyRef, transactionRef } from './firestoreService';

export interface RewardGrant {
  coins: number;
  gems: number;
  powerUpTypeId?: string;
  powerUpCount?: number;
}

export interface GrantResult {
  newCoins: number;
  newGems: number;
  txId: string;
  alreadyClaimed: boolean;
}

/**
 * Grants coins/gems to the player using a Firestore transaction.
 * Idempotent: if idempotencyKey already exists in transactions, returns alreadyClaimed=true.
 */
export async function grantReward(
  uid: string,
  reward: RewardGrant,
  reason: string,
  source: string,
  referenceId: string,
  idempotencyKey: string,
): Promise<GrantResult> {
  const db = admin.firestore();
  const txId = crypto.randomUUID();

  const existingSnap = await db
    .collection('players').doc(uid)
    .collection('transactions')
    .where('idempotencyKey', '==', idempotencyKey)
    .limit(1)
    .get();

  if (!existingSnap.empty) {
    const econ = await economyRef(uid).get();
    const data = econ.data() ?? { coins: 0, gems: 0 };
    return {
      newCoins: data.coins as number,
      newGems: data.gems as number,
      txId: existingSnap.docs[0].id,
      alreadyClaimed: true,
    };
  }

  let newCoins = 0;
  let newGems = 0;

  await db.runTransaction(async (t) => {
    const econSnap = await t.get(economyRef(uid));
    const econ = econSnap.data() ?? { coins: 0, gems: 0, revision: 0 };
    newCoins = (econ.coins as number) + (reward.coins ?? 0);
    newGems = (econ.gems as number) + (reward.gems ?? 0);

    t.update(economyRef(uid), {
      coins: newCoins,
      gems: newGems,
      lastTransactionId: txId,
      revision: FieldValue.increment(1),
      updatedAt: FieldValue.serverTimestamp(),
    });

    if (reward.coins && reward.coins !== 0) {
      t.set(transactionRef(uid, txId), {
        txId,
        playerId: uid,
        currency: 'coins',
        amount: reward.coins,
        type: reward.coins > 0 ? 'earn' : 'spend',
        reason,
        source,
        referenceId,
        createdAt: FieldValue.serverTimestamp(),
        idempotencyKey,
      });
    }

    if (reward.gems && reward.gems !== 0) {
      const gemsTxId = crypto.randomUUID();
      t.set(transactionRef(uid, gemsTxId), {
        txId: gemsTxId,
        playerId: uid,
        currency: 'gems',
        amount: reward.gems,
        type: reward.gems > 0 ? 'earn' : 'spend',
        reason,
        source,
        referenceId,
        createdAt: FieldValue.serverTimestamp(),
        idempotencyKey: `${idempotencyKey}_gems`,
      });
    }
  });

  return { newCoins, newGems, txId, alreadyClaimed: false };
}
