// ============================================================
// King Smash — game-api  DTO / shared TypeScript interfaces
// ============================================================

export interface ApiResponse<T = undefined> {
  success: boolean;
  error?: string;
  data?: T;
}

// ---- Player ------------------------------------------------
export interface PlayerProfile {
  playerId: string;
  authProvider: 'anonymous' | 'google';
  createdAt: FirebaseFirestore.Timestamp | null;
  lastLoginAt: FirebaseFirestore.Timestamp | null;
  gameVersion: string;
  profileVersion: number;
  schemaVersion: number;
}

// ---- Progression -------------------------------------------
export interface StarsEntry {
  levelIndex: number;
  stars: number;
}

export interface PowerUpEntry {
  powerUpTypeId: string;
  count: number;
}

export interface ProgressionData {
  schemaVersion: number;
  updatedAt?: FirebaseFirestore.Timestamp | null;
  revision: number;
  currentWorld: number;
  highestUnlockedLevel: number;
  completedLevels: number[];
  starsPerLevel: StarsEntry[];
  completedBosses: number[];
  completedWorlds: number[];
  kingLevel: number;
  kingXp: number;
  powerLevel: number;
  speedLevel: number;
  smashRadiusLevel: number;
  armorLevel: number;
  powerUpInventory: PowerUpEntry[];
}

// ---- Economy -----------------------------------------------
export interface EconomyBalance {
  coins: number;
  gems: number;
  revision: number;
  lastTransactionId: string;
}

export interface TransactionRecord {
  txId: string;
  playerId: string;
  currency: 'coins' | 'gems';
  amount: number;
  type: 'earn' | 'spend';
  reason: string;
  source: string;
  referenceId: string;
  createdAt: FirebaseFirestore.FieldValue;
  idempotencyKey: string;
}

// ---- Daily Rewards -----------------------------------------
export interface DailyRewardState {
  schemaVersion: number;
  updatedAt: FirebaseFirestore.Timestamp | null;
  currentStreakDay: number;
  lastClaimTimestamp: FirebaseFirestore.Timestamp | null;
  claimedToday: boolean;
  totalClaims: number;
}

export interface DailyRewardConfig {
  day: number;
  coins: number;
  gems: number;
  powerUpTypeId: string | null;
  powerUpCount: number;
  isTreasureChest: boolean;
}

// ---- Missions ----------------------------------------------
export interface MissionState {
  missionId: string;
  progress: number;
  claimed: boolean;
  lastResetTimestamp: FirebaseFirestore.Timestamp | null;
  updatedAt: FirebaseFirestore.Timestamp | null;
}

// ---- Achievements ------------------------------------------
export interface AchievementState {
  achievementId: string;
  progress: number;
  claimedTierCount: number;
  updatedAt: FirebaseFirestore.Timestamp | null;
}

export interface AchievementTierConfig {
  threshold: number;
  coins: number;
  gems: number;
}

// ---- Request bodies ----------------------------------------
export interface InitPlayerRequest {
  gameVersion: string;
}

export interface LinkGoogleRequest {
  googleIdToken: string;
}

export interface ClaimEconomyRequest {
  idempotencyKey: string;
  source: string;
  levelIndex?: number;
  coins: number;
  gems: number;
  reason: string;
}

export interface SaveProgressionRequest {
  schemaVersion: number;
  revision: number;
  currentWorld: number;
  highestUnlockedLevel: number;
  completedLevels: number[];
  starsPerLevel: StarsEntry[];
  completedBosses: number[];
  completedWorlds: number[];
  kingLevel: number;
  kingXp: number;
  powerLevel: number;
  speedLevel: number;
  smashRadiusLevel: number;
  armorLevel: number;
  powerUpInventory: PowerUpEntry[];
}

export interface SyncProgressionRequest {
  clientProgression: ProgressionData;
  clientRevision: number;
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
