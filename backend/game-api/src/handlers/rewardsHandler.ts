import { Router, Request, Response } from 'express';
import { verifyFirebaseToken } from '../middleware/authMiddleware';
import { rewardLimiter, generalLimiter } from '../middleware/rateLimiter';
import {
  getDailyRewardState,
  claimDailyReward,
  getMissionState,
  claimMissionReward,
  getAchievementState,
  claimAchievementTier,
} from '../services/firestoreService';
import {
  getDailyRewardEntry,
  getAchievementDefinition,
  getAchievementTier,
} from '../config/rewardConfig';
import {
  ClaimDailyRequest,
  ClaimMissionRequest,
  ClaimAchievementRequest,
} from '../dto';

export const rewardsRouter = Router();

export async function getDailyStatus(req: Request, res: Response): Promise<void> {
  try {
    const data = await getDailyRewardState(req.uid);
    if (!data) {
      res.status(404).json({ success: false, error: 'Daily rewards not initialized. Call /player/init first.' });
      return;
    }

    // Determine if already claimed today
    let claimedToday = data.claimedToday === true;
    if (data.lastClaimTimestamp) {
      const lastClaim = (data.lastClaimTimestamp as { toDate: () => Date }).toDate();
      const nowUtc = new Date();
      const lastDayUtc = new Date(Date.UTC(lastClaim.getUTCFullYear(), lastClaim.getUTCMonth(), lastClaim.getUTCDate()));
      const todayUtc = new Date(Date.UTC(nowUtc.getUTCFullYear(), nowUtc.getUTCMonth(), nowUtc.getUTCDate()));
      if (lastDayUtc.getTime() < todayUtc.getTime()) {
        claimedToday = false;
      }
    }

    const nextDay = (data.currentStreakDay as number) + 1;
    const nextReward = getDailyRewardEntry(nextDay);

    res.json({
      success: true,
      data: {
        currentStreakDay: data.currentStreakDay,
        claimedToday,
        canClaim: !claimedToday,
        lastClaimTimestamp: data.lastClaimTimestamp ?? null,
        totalClaims: data.totalClaims,
        nextReward,
      },
    });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'GET /rewards/daily/status' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

export async function claimDaily(req: Request, res: Response): Promise<void> {
  try {
    const uid = req.uid;
    const { idempotencyKey } = req.body as ClaimDailyRequest;

    if (!idempotencyKey) {
      res.status(400).json({ success: false, error: 'Missing idempotencyKey' });
      return;
    }

    const data = await getDailyRewardState(uid);
    if (!data) {
      res.status(404).json({ success: false, error: 'Daily rewards not initialized' });
      return;
    }

    // Check already claimed
    if (data.claimedToday === true) {
      res.status(409).json({ success: false, error: 'Already claimed today' });
      return;
    }

    if (data.lastClaimTimestamp) {
      const lastClaim = (data.lastClaimTimestamp as { toDate: () => Date }).toDate();
      const nowUtc = new Date();
      const lastDayUtc = new Date(Date.UTC(lastClaim.getUTCFullYear(), lastClaim.getUTCMonth(), lastClaim.getUTCDate()));
      const todayUtc = new Date(Date.UTC(nowUtc.getUTCFullYear(), nowUtc.getUTCMonth(), nowUtc.getUTCDate()));
      if (lastDayUtc.getTime() === todayUtc.getTime()) {
        res.status(409).json({ success: false, error: 'Already claimed today' });
        return;
      }
    }

    const newStreakDay = (data.currentStreakDay as number) + 1;
    const entry = getDailyRewardEntry(newStreakDay);

    const result = await claimDailyReward(
      uid,
      idempotencyKey,
      entry.coinsReward,
      entry.gemsReward,
      newStreakDay,
      entry.powerUpTypeId,
      entry.powerUpCount,
      entry.isTreasureChest,
    );

    res.json({ success: true, data: result });
  } catch (err) {
    const code = (err as { code?: number }).code;
    if (code === 409) {
      res.status(409).json({ success: false, error: 'Already claimed today' });
      return;
    }
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /rewards/daily/claim' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

export async function claimMission(req: Request, res: Response): Promise<void> {
  try {
    const uid = req.uid;
    const { missionId, idempotencyKey } = req.body as ClaimMissionRequest;

    if (!missionId || !idempotencyKey) {
      res.status(400).json({ success: false, error: 'Missing missionId or idempotencyKey' });
      return;
    }

    const missionData = await getMissionState(uid, missionId);
    if (!missionData) {
      res.status(400).json({ success: false, error: 'Mission not found' });
      return;
    }
    if (missionData.claimed === true) {
      res.status(409).json({ success: false, error: 'Mission already claimed' });
      return;
    }

    const MISSION_COIN_REWARD = 100;
    const result = await claimMissionReward(uid, missionId, MISSION_COIN_REWARD, idempotencyKey);

    res.json({
      success: true,
      data: {
        missionId,
        coins: MISSION_COIN_REWARD,
        txId: result.txId,
        newBalance: { coins: result.newCoins, gems: result.newGems },
      },
    });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /rewards/mission/claim' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

export async function claimAchievement(req: Request, res: Response): Promise<void> {
  try {
    const uid = req.uid;
    const { achievementId, idempotencyKey } = req.body as ClaimAchievementRequest;

    if (!achievementId || !idempotencyKey) {
      res.status(400).json({ success: false, error: 'Missing achievementId or idempotencyKey' });
      return;
    }

    const achievementData = await getAchievementState(uid, achievementId);
    if (!achievementData) {
      res.status(400).json({ success: false, error: 'Achievement not found' });
      return;
    }

    const claimedTierCount = achievementData.claimedTierCount as number;
    const def = getAchievementDefinition(achievementId);
    if (!def) {
      res.status(404).json({ success: false, error: 'Achievement definition not found' });
      return;
    }

    if (claimedTierCount >= def.tiers.length) {
      res.status(409).json({ success: false, error: 'All tiers already claimed' });
      return;
    }

    const tierIndex = claimedTierCount;
    const tier = getAchievementTier(achievementId, tierIndex);
    if (!tier) {
      res.status(404).json({ success: false, error: 'Achievement tier not found' });
      return;
    }

    if ((achievementData.progress as number) < tier.targetCount) {
      res.status(400).json({ success: false, error: 'Achievement tier target not yet reached' });
      return;
    }

    const result = await claimAchievementTier(uid, achievementId, tierIndex, tier.coinReward, idempotencyKey);

    res.json({
      success: true,
      data: {
        achievementId,
        tierIndex,
        reward: { coins: tier.coinReward },
        txId: result.txId,
        newBalance: { coins: result.newCoins, gems: result.newGems },
      },
    });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /rewards/achievement/claim' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

rewardsRouter.get('/daily/status', generalLimiter, verifyFirebaseToken, getDailyStatus);
rewardsRouter.post('/daily/claim', rewardLimiter, verifyFirebaseToken, claimDaily);
rewardsRouter.post('/mission/claim', rewardLimiter, verifyFirebaseToken, claimMission);
rewardsRouter.post('/achievement/claim', rewardLimiter, verifyFirebaseToken, claimAchievement);
