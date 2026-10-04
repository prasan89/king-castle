import * as admin from 'firebase-admin';
import { FieldValue } from 'firebase-admin/firestore';

const db = () => admin.firestore();

// ---- Collection path helpers ----

export const playerRef = (uid: string) =>
  db().collection('players').doc(uid);

export const progressionRef = (uid: string) =>
  db().collection('players').doc(uid).collection('progression').doc('main');

export const economyRef = (uid: string) =>
  db().collection('players').doc(uid).collection('economy').doc('main');

export const dailyRewardsRef = (uid: string) =>
  db().collection('players').doc(uid).collection('dailyRewards').doc('main');

export const missionRef = (uid: string, missionId: string) =>
  db().collection('players').doc(uid).collection('missions').doc(missionId);

export const achievementRef = (uid: string, achievementId: string) =>
  db().collection('players').doc(uid).collection('achievements').doc(achievementId);

export const transactionRef = (uid: string, txId: string) =>
  db().collection('players').doc(uid).collection('transactions').doc(txId);

// ---- Read helpers ----

export async function getPlayerProfile(uid: string) {
  const snap = await playerRef(uid).get();
  return snap.exists ? snap.data() : null;
}

export async function getProgression(uid: string) {
  const snap = await progressionRef(uid).get();
  return snap.exists ? snap.data() : null;
}

export async function saveProgression(
  uid: string,
  data: Record<string, unknown>,
): Promise<{ revision: number; updatedAt: FirebaseFirestore.Timestamp }> {
  const serverData = await getProgression(uid);
  const serverRevision = serverData ? (serverData.revision as number) : 0;
  const clientRevision = data.revision as number;

  if (clientRevision < serverRevision - 1) {
    const err = Object.assign(new Error('Revision rollback rejected'), { code: 409 });
    throw err;
  }

  const newRevision = Math.max(serverRevision, clientRevision) + 1;
  const toWrite = { ...data, revision: newRevision, updatedAt: FieldValue.serverTimestamp(), schemaVersion: 1 };
  await progressionRef(uid).set(toWrite, { merge: false });
  return { revision: newRevision, updatedAt: FieldValue.serverTimestamp() as unknown as FirebaseFirestore.Timestamp };
}

export async function getEconomy(uid: string) {
  const snap = await economyRef(uid).get();
  return snap.exists ? snap.data() : null;
}

export async function getEconomyBalance(uid: string) {
  return getEconomy(uid);
}

export async function applyEconomyTransaction(
  uid: string,
  coins: number,
  gems: number,
  reason: string,
  source: string,
  idempotencyKey: string,
): Promise<{ txId: string; newCoins: number; newGems: number }> {
  const existing = await db()
    .collection('players').doc(uid)
    .collection('transactions')
    .where('idempotencyKey', '==', idempotencyKey)
    .limit(1)
    .get();

  if (!existing.empty) {
    const econ = await getEconomy(uid);
    return { txId: existing.docs[0].id, newCoins: (econ?.coins ?? 0) as number, newGems: (econ?.gems ?? 0) as number };
  }

  const txId = require('crypto').randomUUID() as string;
  let newCoins = 0;
  let newGems = 0;

  await db().runTransaction(async (t) => {
    const econSnap = await t.get(economyRef(uid));
    const econ = econSnap.data() ?? { coins: 0, gems: 0, revision: 0 };
    newCoins = (econ.coins as number) + coins;
    newGems = (econ.gems as number) + gems;

    t.update(economyRef(uid), {
      coins: newCoins,
      gems: newGems,
      lastTransactionId: txId,
      revision: FieldValue.increment(1),
      updatedAt: FieldValue.serverTimestamp(),
    });

    t.set(transactionRef(uid, txId), {
      txId,
      playerId: uid,
      currency: coins !== 0 ? 'coins' : 'gems',
      amount: coins !== 0 ? coins : gems,
      type: (coins !== 0 ? coins : gems) > 0 ? 'earn' : 'spend',
      reason,
      source,
      referenceId: '',
      createdAt: FieldValue.serverTimestamp(),
      idempotencyKey,
    });
  });

  return { txId, newCoins, newGems };
}

export async function getDailyRewardState(uid: string) {
  const snap = await dailyRewardsRef(uid).get();
  return snap.exists ? snap.data() : null;
}

export async function claimDailyReward(
  uid: string,
  idempotencyKey: string,
  coinsReward: number,
  gemsReward: number,
  newStreakDay: number,
  powerUpTypeId: string,
  powerUpCount: number,
  isTreasureChest: boolean,
): Promise<{ day: number; coinsGranted: number; gemsGranted: number; powerUpTypeId: string; powerUpCount: number; isTreasureChest: boolean; newStreakDay: number }> {
  await applyEconomyTransaction(uid, coinsReward, gemsReward, 'daily_reward', 'daily_reward', idempotencyKey);
  await dailyRewardsRef(uid).update({
    currentStreakDay: newStreakDay,
    lastClaimTimestamp: FieldValue.serverTimestamp(),
    claimedToday: true,
    totalClaims: FieldValue.increment(1),
    updatedAt: FieldValue.serverTimestamp(),
  });
  return { day: newStreakDay, coinsGranted: coinsReward, gemsGranted: gemsReward, powerUpTypeId, powerUpCount, isTreasureChest, newStreakDay };
}

export async function getMissionState(uid: string, missionId: string) {
  const snap = await missionRef(uid, missionId).get();
  return snap.exists ? snap.data() : null;
}

export async function claimMissionReward(
  uid: string,
  missionId: string,
  coins: number,
  idempotencyKey: string,
): Promise<{ txId: string; newCoins: number; newGems: number }> {
  await missionRef(uid, missionId).update({ claimed: true, updatedAt: FieldValue.serverTimestamp() });
  return applyEconomyTransaction(uid, coins, 0, 'mission_complete', 'mission', idempotencyKey);
}

export async function getAchievementState(uid: string, achievementId: string) {
  const snap = await achievementRef(uid, achievementId).get();
  return snap.exists ? snap.data() : null;
}

export async function claimAchievementTier(
  uid: string,
  achievementId: string,
  tierIndex: number,
  coins: number,
  idempotencyKey: string,
): Promise<{ txId: string; newCoins: number; newGems: number }> {
  await achievementRef(uid, achievementId).update({
    claimedTierCount: tierIndex + 1,
    updatedAt: FieldValue.serverTimestamp(),
  });
  return applyEconomyTransaction(uid, coins, 0, 'achievement_tier_claimed', 'achievement', idempotencyKey);
}

export async function checkTransactionIdempotency(uid: string, idempotencyKey: string): Promise<boolean> {
  const snap = await db()
    .collection('players').doc(uid)
    .collection('transactions')
    .where('idempotencyKey', '==', idempotencyKey)
    .limit(1)
    .get();
  return !snap.empty;
}

export async function initPlayerProfile(
  uid: string,
  gameVersion: string,
  authProvider: 'anonymous' | 'google',
): Promise<void> {
  const now = FieldValue.serverTimestamp();
  await playerRef(uid).set({
    playerId: uid,
    createdAt: now,
    lastLoginAt: now,
    gameVersion,
    profileVersion: 1,
    authProvider,
    schemaVersion: 1,
  }, { merge: false });
}

export async function touchLastLogin(uid: string, gameVersion: string): Promise<void> {
  await playerRef(uid).update({
    lastLoginAt: FieldValue.serverTimestamp(),
    gameVersion,
  });
}

export async function initEconomy(uid: string): Promise<void> {
  await economyRef(uid).set({
    schemaVersion: 1,
    updatedAt: FieldValue.serverTimestamp(),
    revision: 1,
    coins: 0,
    gems: 0,
    lastTransactionId: '',
  }, { merge: false });
}

export async function initDailyRewards(uid: string): Promise<void> {
  await dailyRewardsRef(uid).set({
    schemaVersion: 1,
    updatedAt: FieldValue.serverTimestamp(),
    currentStreakDay: 0,
    lastClaimTimestamp: null,
    claimedToday: false,
    totalClaims: 0,
  }, { merge: false });
}
