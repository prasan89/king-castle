import { FieldValue } from 'firebase-admin/firestore';

// ---- Request DTOs ----

export interface SaveProgressionRequest {
  revision: number;
  currentWorld: number;
  highestUnlockedLevel: number;
  completedLevels: number[];
  starsPerLevel: { levelIndex: number; stars: number }[];
  completedBosses: number[];
  completedWorlds: number[];
  kingLevel: number;
  kingXp: number;
  powerLevel: number;
  speedLevel: number;
  smashRadiusLevel: number;
  armorLevel: number;
  powerUpInventory: { powerUpTypeId: string; count: number }[];
  idempotencyKey?: string;
}

export interface ClaimDailyRequest {
  idempotencyKey: string;
}

export interface ClaimMissionRequest {
  missionId: string;
  idempotencyKey: string;
}

export interface ClaimAchievementRequest {
  achievementId: string;
  tierIndex: number;
  idempotencyKey: string;
}

export interface LinkGoogleRequest {
  googleIdToken: string;
}

// ---- Response DTOs ----

export interface PlayerProfileResponse {
  playerId: string;
  authProvider: string;
  gameVersion: string;
  profileVersion: number;
  createdAt: string;
  lastLoginAt: string;
}

export interface ProgressionResponse {
  revision: number;
  currentWorld: number;
  highestUnlockedLevel: number;
  completedLevels: number[];
  starsPerLevel: { levelIndex: number; stars: number }[];
  completedBosses: number[];
  completedWorlds: number[];
  kingLevel: number;
  kingXp: number;
  powerLevel: number;
  speedLevel: number;
  smashRadiusLevel: number;
  armorLevel: number;
  powerUpInventory: { powerUpTypeId: string; count: number }[];
  updatedAt: string;
}

export interface EconomyBalanceResponse {
  coins: number;
  gems: number;
  revision: number;
  updatedAt: string;
}

export interface DailyRewardStatusResponse {
  currentStreakDay: number;
  claimedToday: boolean;
  lastClaimTimestamp: string | null;
  totalClaims: number;
  nextReward: {
    day: number;
    coinsReward: number;
    gemsReward: number;
    powerUpTypeId: string;
    powerUpCount: number;
    isTreasureChest: boolean;
    displayName: string;
  };
}

export interface ClaimRewardResponse {
  coins: number;
  gems: number;
  powerUpTypeId: string;
  powerUpCount: number;
  isTreasureChest: boolean;
  newStreakDay?: number;
  newBalance: EconomyBalanceResponse;
}
