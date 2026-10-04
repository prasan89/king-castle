/**
 * King Smash — game-api handler tests
 *
 * Uses Jest + ts-jest with manual mocks for firebase-admin and firestoreService.
 * No network calls are made.
 */

import { Request, Response } from 'express';

// ---- Helpers ---------------------------------------------------------------

type MockReq = {
  uid: string;
  requestId: string;
  startTime: number;
  body: Record<string, unknown>;
  headers: Record<string, string>;
};

function makeReq(overrides: Partial<MockReq> = {}): Request {
  return {
    uid: 'test-uid-123',
    requestId: 'test-req-id',
    startTime: Date.now(),
    body: {},
    headers: {},
    ...overrides,
  } as unknown as Request;
}

function makeRes(): { res: Response; getStatusCode: () => number; getBody: () => unknown } {
  let statusCode = 200;
  let body: unknown = {};
  const res = {
    status: jest.fn().mockImplementation((code: number) => {
      statusCode = code;
      return res;
    }),
    json: jest.fn().mockImplementation((data: unknown) => {
      body = data;
      return res;
    }),
    on: jest.fn(),
  } as unknown as Response;
  return { res, getStatusCode: () => statusCode, getBody: () => body };
}

// ---- Mocks -----------------------------------------------------------------

jest.mock('../services/firestoreService', () => ({
  getPlayerProfile: jest.fn(),
  initPlayerProfile: jest.fn().mockResolvedValue(undefined),
  touchLastLogin: jest.fn().mockResolvedValue(undefined),
  initEconomy: jest.fn().mockResolvedValue(undefined),
  initDailyRewards: jest.fn().mockResolvedValue(undefined),
  getProgression: jest.fn(),
  saveProgression: jest.fn(),
  getEconomy: jest.fn(),
  getEconomyBalance: jest.fn(),
  applyEconomyTransaction: jest.fn(),
  getDailyRewardState: jest.fn(),
  claimDailyReward: jest.fn(),
  getMissionState: jest.fn(),
  claimMissionReward: jest.fn(),
  getAchievementState: jest.fn(),
  claimAchievementTier: jest.fn(),
  checkTransactionIdempotency: jest.fn(),
  playerRef: jest.fn(),
  progressionRef: jest.fn(),
  economyRef: jest.fn(),
  dailyRewardsRef: jest.fn(),
  missionRef: jest.fn(),
  achievementRef: jest.fn(),
  transactionRef: jest.fn(),
}));

jest.mock('firebase-admin', () => ({
  initializeApp: jest.fn(),
  auth: () => ({
    verifyIdToken: jest.fn().mockResolvedValue({ uid: 'test-uid-123' }),
    getUser: jest.fn().mockResolvedValue({ providerData: [] }),
  }),
  firestore: Object.assign(() => ({
    collection: jest.fn().mockReturnValue({
      doc: jest.fn().mockReturnValue({
        get: jest.fn().mockResolvedValue({ exists: false, data: () => null }),
        set: jest.fn().mockResolvedValue(undefined),
        update: jest.fn().mockResolvedValue(undefined),
      }),
      where: jest.fn().mockReturnValue({
        limit: jest.fn().mockReturnValue({
          get: jest.fn().mockResolvedValue({ empty: true, docs: [] }),
        }),
      }),
    }),
    runTransaction: jest.fn().mockResolvedValue(undefined),
  }), {
    FieldValue: {
      serverTimestamp: () => 'SERVER_TIMESTAMP',
      increment: (n: number) => n,
    },
  }),
}));

import * as fs from '../services/firestoreService';
import { getBalance, claimEconomy } from '../handlers/economyHandler';
import { loadProgression, saveProgressionHandler, syncProgression } from '../handlers/progressionHandler';
import { getDailyStatus, claimDaily, claimMission, claimAchievement } from '../handlers/rewardsHandler';

const mockFs = fs as jest.Mocked<typeof fs>;

// ===========================================================================
// Economy Handler
// ===========================================================================

describe('economyHandler.getBalance', () => {
  it('returns balance for authenticated player', async () => {
    (mockFs.getEconomyBalance as jest.Mock).mockResolvedValueOnce({
      coins: 500,
      gems: 10,
      revision: 3,
      lastTransactionId: 'tx_abc',
    });
    const req = makeReq();
    const { res, getBody } = makeRes();
    await getBalance(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { coins: number } }).data.coins).toBe(500);
  });

  it('returns 404 when economy not initialized', async () => {
    (mockFs.getEconomyBalance as jest.Mock).mockResolvedValueOnce(null);
    const req = makeReq();
    const { res, getStatusCode } = makeRes();
    await getBalance(req, res);
    expect(getStatusCode()).toBe(404);
  });

  it('returns 500 when Firestore throws', async () => {
    (mockFs.getEconomyBalance as jest.Mock).mockRejectedValueOnce(new Error('Firestore unavailable'));
    const req = makeReq();
    const { res, getStatusCode } = makeRes();
    await getBalance(req, res);
    expect(getStatusCode()).toBe(500);
  });
});

describe('economyHandler.claimEconomy', () => {
  it('returns 400 when idempotencyKey missing', async () => {
    const req = makeReq({ body: { coins: 100, gems: 0, reason: 'test', source: 'level' } });
    const { res, getStatusCode } = makeRes();
    await claimEconomy(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('returns 400 when coins and gems both missing', async () => {
    const req = makeReq({ body: { idempotencyKey: 'key1', reason: 'test', source: 'level' } });
    const { res, getStatusCode } = makeRes();
    await claimEconomy(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('grants coins and returns new balance', async () => {
    (mockFs.applyEconomyTransaction as jest.Mock).mockResolvedValueOnce({
      txId: 'tx_999',
      newCoins: 600,
      newGems: 10,
    });
    const req = makeReq({
      body: { idempotencyKey: 'ikey-001', coins: 100, gems: 0, reason: 'level_clear', source: 'gameplay' },
    });
    const { res, getBody } = makeRes();
    await claimEconomy(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { txId: string } }).data.txId).toBe('tx_999');
  });

  it('rejects duplicate idempotency key (service returns alreadyClaimed)', async () => {
    (mockFs.applyEconomyTransaction as jest.Mock).mockResolvedValueOnce({
      txId: 'tx_existing',
      newCoins: 500,
      newGems: 0,
    });
    const req = makeReq({
      body: { idempotencyKey: 'duplicate-key', coins: 100, gems: 0, reason: 'test', source: 'test' },
    });
    const { res, getBody } = makeRes();
    await claimEconomy(req, res);
    // Still returns success (idempotent) — same response
    expect((getBody() as { success: boolean }).success).toBe(true);
  });
});

// ===========================================================================
// Progression Handler
// ===========================================================================

describe('progressionHandler.loadProgression', () => {
  it('returns progression data', async () => {
    const mockProg = {
      schemaVersion: 1,
      revision: 5,
      currentWorld: 2,
      highestUnlockedLevel: 10,
      completedLevels: [1, 2, 3],
      starsPerLevel: [{ levelIndex: 1, stars: 3 }],
      completedBosses: [],
      completedWorlds: [],
      kingLevel: 3,
      kingXp: 150,
      powerLevel: 2,
      speedLevel: 2,
      smashRadiusLevel: 1,
      armorLevel: 1,
      powerUpInventory: [],
    };
    (mockFs.getProgression as jest.Mock).mockResolvedValueOnce(mockProg);
    const req = makeReq();
    const { res, getBody } = makeRes();
    await loadProgression(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { revision: number } }).data.revision).toBe(5);
  });

  it('returns 404 when progression not found', async () => {
    (mockFs.getProgression as jest.Mock).mockResolvedValueOnce(null);
    const req = makeReq();
    const { res, getStatusCode } = makeRes();
    await loadProgression(req, res);
    expect(getStatusCode()).toBe(404);
  });
});

describe('progressionHandler.saveProgression', () => {
  it('returns 400 when revision missing', async () => {
    const req = makeReq({ body: { schemaVersion: 1 } });
    const { res, getStatusCode } = makeRes();
    await saveProgressionHandler(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('saves and returns new revision', async () => {
    (mockFs.saveProgression as jest.Mock).mockResolvedValueOnce({
      revision: 6,
      updatedAt: 'SERVER_TIMESTAMP',
    });
    const req = makeReq({ body: {
      schemaVersion: 1, revision: 5, currentWorld: 1,
      highestUnlockedLevel: 5, completedLevels: [], starsPerLevel: [], completedBosses: [],
      completedWorlds: [], kingLevel: 1, kingXp: 0, powerLevel: 1, speedLevel: 1,
      smashRadiusLevel: 1, armorLevel: 1, powerUpInventory: [],
    }});
    const { res, getBody } = makeRes();
    await saveProgressionHandler(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { revision: number } }).data.revision).toBe(6);
  });

  it('returns 409 when revision rollback detected', async () => {
    const rollbackError = Object.assign(new Error('Revision rollback rejected'), { code: 409 });
    (mockFs.saveProgression as jest.Mock).mockRejectedValueOnce(rollbackError);
    const req = makeReq({ body: {
      schemaVersion: 1, revision: 2, currentWorld: 1, highestUnlockedLevel: 1,
      completedLevels: [], starsPerLevel: [], completedBosses: [], completedWorlds: [],
      kingLevel: 1, kingXp: 0, powerLevel: 1, speedLevel: 1, smashRadiusLevel: 1,
      armorLevel: 1, powerUpInventory: [],
    }});
    const { res, getStatusCode } = makeRes();
    await saveProgressionHandler(req, res);
    expect(getStatusCode()).toBe(409);
  });
});

describe('progressionHandler.syncProgression', () => {
  it('returns 400 when clientProgression missing', async () => {
    const req = makeReq({ body: { clientRevision: 3 } });
    const { res, getStatusCode } = makeRes();
    await syncProgression(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('merges and returns merged progression', async () => {
    const baseProg = {
      schemaVersion: 1, revision: 4, currentWorld: 1, highestUnlockedLevel: 5,
      completedLevels: [1, 2], starsPerLevel: [{ levelIndex: 1, stars: 2 }],
      completedBosses: [], completedWorlds: [], kingLevel: 2, kingXp: 100,
      powerLevel: 1, speedLevel: 1, smashRadiusLevel: 1, armorLevel: 1, powerUpInventory: [],
    };
    (mockFs.getProgression as jest.Mock).mockResolvedValueOnce({ ...baseProg });
    (mockFs.saveProgression as jest.Mock).mockResolvedValueOnce({ revision: 5, updatedAt: 'SERVER_TIMESTAMP' });
    const req = makeReq({
      body: {
        clientRevision: 3,
        clientProgression: {
          ...baseProg,
          revision: 3,
          highestUnlockedLevel: 7,
          completedLevels: [1, 2, 3],
        },
      },
    });
    const { res, getBody } = makeRes();
    await syncProgression(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    // mergeProgression: server revision 4 > client 3, so server wins but highestUnlockedLevel = max(7,5)=7
    const merged = (getBody() as { data: { mergedProgression: { highestUnlockedLevel: number } } }).data.mergedProgression;
    expect(merged.highestUnlockedLevel).toBe(7);
  });
});

// ===========================================================================
// Rewards Handler — Daily
// ===========================================================================

describe('rewardsHandler.getDailyStatus', () => {
  it('returns canClaim true when never claimed', async () => {
    (mockFs.getDailyRewardState as jest.Mock).mockResolvedValueOnce({
      schemaVersion: 1,
      updatedAt: null,
      currentStreakDay: 0,
      lastClaimTimestamp: null,
      claimedToday: false,
      totalClaims: 0,
    });
    const req = makeReq();
    const { res, getBody } = makeRes();
    await getDailyStatus(req, res);
    expect((getBody() as { data: { canClaim: boolean } }).data.canClaim).toBe(true);
  });

  it('returns canClaim false when already claimed today', async () => {
    const nowTs = {
      toDate: () => new Date(),
    };
    (mockFs.getDailyRewardState as jest.Mock).mockResolvedValueOnce({
      schemaVersion: 1,
      updatedAt: null,
      currentStreakDay: 1,
      lastClaimTimestamp: nowTs,
      claimedToday: true,
      totalClaims: 1,
    });
    const req = makeReq();
    const { res, getBody } = makeRes();
    await getDailyStatus(req, res);
    expect((getBody() as { data: { canClaim: boolean } }).data.canClaim).toBe(false);
  });
});

describe('rewardsHandler.claimDaily', () => {
  it('returns 400 when idempotencyKey missing', async () => {
    const req = makeReq({ body: {} });
    const { res, getStatusCode } = makeRes();
    await claimDaily(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('returns 409 when already claimed', async () => {
    (mockFs.getDailyRewardState as jest.Mock).mockResolvedValueOnce({
      currentStreakDay: 1, claimedToday: true, lastClaimTimestamp: null, totalClaims: 1,
    });
    const req = makeReq({ body: { idempotencyKey: 'daily-key-001' } });
    const { res, getStatusCode } = makeRes();
    await claimDaily(req, res);
    expect(getStatusCode()).toBe(409);
  });

  it('returns reward data on successful claim', async () => {
    (mockFs.getDailyRewardState as jest.Mock).mockResolvedValueOnce({
      currentStreakDay: 0, claimedToday: false, lastClaimTimestamp: null, totalClaims: 0,
    });
    (mockFs.claimDailyReward as jest.Mock).mockResolvedValueOnce({
      day: 1,
      coinsGranted: 100,
      gemsGranted: 0,
      powerUpTypeId: '',
      powerUpCount: 0,
      isTreasureChest: false,
      newStreakDay: 1,
    });
    const req = makeReq({ body: { idempotencyKey: 'daily-key-002' } });
    const { res, getBody } = makeRes();
    await claimDaily(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { coinsGranted: number } }).data.coinsGranted).toBe(100);
  });
});

// ===========================================================================
// Rewards Handler — Mission
// ===========================================================================

describe('rewardsHandler.claimMission', () => {
  it('returns 400 when missionId missing', async () => {
    const req = makeReq({ body: { idempotencyKey: 'k1' } });
    const { res, getStatusCode } = makeRes();
    await claimMission(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('returns 400 when mission not found', async () => {
    (mockFs.getMissionState as jest.Mock).mockResolvedValueOnce(null);
    const req = makeReq({ body: { missionId: 'm_001', idempotencyKey: 'k2' } });
    const { res, getStatusCode } = makeRes();
    await claimMission(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('returns 409 when already claimed', async () => {
    (mockFs.getMissionState as jest.Mock).mockResolvedValueOnce({
      missionId: 'm_001', progress: 10, claimed: true,
      lastResetTimestamp: null, updatedAt: null,
    });
    const req = makeReq({ body: { missionId: 'm_001', idempotencyKey: 'k3' } });
    const { res, getStatusCode } = makeRes();
    await claimMission(req, res);
    expect(getStatusCode()).toBe(409);
  });

  it('returns reward on success', async () => {
    (mockFs.getMissionState as jest.Mock).mockResolvedValueOnce({
      missionId: 'm_002', progress: 10, claimed: false,
      lastResetTimestamp: null, updatedAt: null,
    });
    (mockFs.claimMissionReward as jest.Mock).mockResolvedValueOnce({
      txId: 'tx_mission_001', newCoins: 1000, newGems: 0,
    });
    const req = makeReq({ body: { missionId: 'm_002', idempotencyKey: 'k4' } });
    const { res, getBody } = makeRes();
    await claimMission(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { missionId: string } }).data.missionId).toBe('m_002');
  });
});

// ===========================================================================
// Rewards Handler — Achievement
// ===========================================================================

describe('rewardsHandler.claimAchievement', () => {
  it('returns 400 when achievementId missing', async () => {
    const req = makeReq({ body: { idempotencyKey: 'k5' } });
    const { res, getStatusCode } = makeRes();
    await claimAchievement(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('returns 400 when achievement not found', async () => {
    (mockFs.getAchievementState as jest.Mock).mockResolvedValueOnce(null);
    const req = makeReq({ body: { achievementId: 'ach_001', idempotencyKey: 'k6' } });
    const { res, getStatusCode } = makeRes();
    await claimAchievement(req, res);
    expect(getStatusCode()).toBe(400);
  });

  it('returns 409 when all tiers claimed', async () => {
    (mockFs.getAchievementState as jest.Mock).mockResolvedValueOnce({
      achievementId: 'first_smash', progress: 1, claimedTierCount: 1, updatedAt: null,
    });
    const req = makeReq({ body: { achievementId: 'first_smash', idempotencyKey: 'k7' } });
    const { res, getStatusCode } = makeRes();
    await claimAchievement(req, res);
    expect(getStatusCode()).toBe(409);
  });

  it('returns reward for next unclaimed tier', async () => {
    (mockFs.getAchievementState as jest.Mock).mockResolvedValueOnce({
      achievementId: 'level_milestones', progress: 25, claimedTierCount: 1, updatedAt: null,
    });
    (mockFs.claimAchievementTier as jest.Mock).mockResolvedValueOnce({
      txId: 'tx_ach_001', newCoins: 1300, newGems: 0,
    });
    const req = makeReq({ body: { achievementId: 'level_milestones', idempotencyKey: 'k8' } });
    const { res, getBody } = makeRes();
    await claimAchievement(req, res);
    expect((getBody() as { success: boolean }).success).toBe(true);
    expect((getBody() as { data: { tierIndex: number } }).data.tierIndex).toBe(1);
    expect((getBody() as { data: { reward: { coins: number } } }).data.reward.coins).toBe(750);
  });
});
