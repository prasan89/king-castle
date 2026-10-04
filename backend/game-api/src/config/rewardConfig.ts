// Daily reward table mirrors DailyRewardConfig.InitializeDefaults() in Unity
// Achievement tier config mirrors AchievementConfig.InitializeDefaults() in Unity

export interface DailyRewardEntry {
  day: number;
  coinsReward: number;
  gemsReward: number;
  powerUpTypeId: string;
  powerUpCount: number;
  isTreasureChest: boolean;
  displayName: string;
}

export interface AchievementTier {
  targetCount: number;
  coinReward: number;
  displayName: string;
}

export interface AchievementDefinition {
  achievementId: string;
  displayName: string;
  description: string;
  trackingType: string;
  tiers: AchievementTier[];
}

// 7-day reward cycle — matches Unity DailyRewardConfig.InitializeDefaults()
const DEFAULT_DAILY_REWARDS: DailyRewardEntry[] = [
  { day: 1, coinsReward: 100,  gemsReward: 0,  powerUpTypeId: '',             powerUpCount: 0, isTreasureChest: false, displayName: 'Day 1' },
  { day: 2, coinsReward: 200,  gemsReward: 0,  powerUpTypeId: '',             powerUpCount: 0, isTreasureChest: false, displayName: 'Day 2' },
  { day: 3, coinsReward: 0,    gemsReward: 10, powerUpTypeId: '',             powerUpCount: 0, isTreasureChest: false, displayName: 'Day 3' },
  { day: 4, coinsReward: 300,  gemsReward: 0,  powerUpTypeId: '',             powerUpCount: 0, isTreasureChest: false, displayName: 'Day 4' },
  { day: 5, coinsReward: 0,    gemsReward: 0,  powerUpTypeId: 'powerup_bomb', powerUpCount: 1, isTreasureChest: false, displayName: 'Day 5' },
  { day: 6, coinsReward: 500,  gemsReward: 0,  powerUpTypeId: '',             powerUpCount: 0, isTreasureChest: false, displayName: 'Day 6' },
  { day: 7, coinsReward: 1000, gemsReward: 5,  powerUpTypeId: '',             powerUpCount: 0, isTreasureChest: true,  displayName: 'Day 7' },
];

// Achievement definitions — matches AchievementConfig.InitializeDefaults() in Unity
const DEFAULT_ACHIEVEMENT_CONFIG: AchievementDefinition[] = [
  {
    achievementId: 'first_smash',
    displayName: 'First Smash',
    description: 'Play your first level',
    trackingType: 'LEVELS_PLAYED',
    tiers: [
      { targetCount: 1, coinReward: 100, displayName: 'First Smash' },
    ],
  },
  {
    achievementId: 'level_milestones',
    displayName: 'Level Master',
    description: 'Complete levels to become a master',
    trackingType: 'LEVELS_COMPLETED',
    tiers: [
      { targetCount: 10,  coinReward: 500,  displayName: 'Novice' },
      { targetCount: 25,  coinReward: 750,  displayName: 'Apprentice' },
      { targetCount: 50,  coinReward: 1000, displayName: 'Expert' },
      { targetCount: 100, coinReward: 2000, displayName: 'Master' },
    ],
  },
  {
    achievementId: 'queen_rescuer',
    displayName: 'Queen Rescuer',
    description: 'Rescue the queen across multiple levels',
    trackingType: 'QUEENS_RESCUED',
    tiers: [
      { targetCount: 1,   coinReward: 200,  displayName: 'First Rescue' },
      { targetCount: 10,  coinReward: 500,  displayName: 'Royal Guard' },
      { targetCount: 25,  coinReward: 750,  displayName: 'Champion' },
      { targetCount: 100, coinReward: 1500, displayName: 'Legendary Rescuer' },
    ],
  },
  {
    achievementId: 'castle_destroyer',
    displayName: 'Castle Destroyer',
    description: 'Demolish castles across the kingdom',
    trackingType: 'CASTLES_DESTROYED',
    tiers: [
      { targetCount: 10,  coinReward: 300,  displayName: 'Demolisher' },
      { targetCount: 25,  coinReward: 500,  displayName: 'Wrecker' },
      { targetCount: 50,  coinReward: 750,  displayName: 'Destroyer' },
      { targetCount: 100, coinReward: 1000, displayName: 'Annihilator' },
    ],
  },
  {
    achievementId: 'enemy_slayer',
    displayName: 'Enemy Slayer',
    description: 'Defeat enemies to earn glory',
    trackingType: 'ENEMIES_DEFEATED',
    tiers: [
      { targetCount: 50,   coinReward: 300,  displayName: 'Brawler' },
      { targetCount: 200,  coinReward: 500,  displayName: 'Warrior' },
      { targetCount: 500,  coinReward: 750,  displayName: 'Slayer' },
      { targetCount: 1000, coinReward: 1500, displayName: 'Legend' },
    ],
  },
  {
    achievementId: 'powerup_master',
    displayName: 'Power-Up Master',
    description: 'Use power-ups to dominate the battlefield',
    trackingType: 'POWERUPS_USED',
    tiers: [
      { targetCount: 3,  coinReward: 200, displayName: 'Initiate' },
      { targetCount: 10, coinReward: 400, displayName: 'Adept' },
      { targetCount: 25, coinReward: 600, displayName: 'Master' },
    ],
  },
  {
    achievementId: 'three_star',
    displayName: 'Star Collector',
    description: 'Earn stars by completing levels with excellence',
    trackingType: 'STARS_EARNED',
    tiers: [
      { targetCount: 3,  coinReward: 200, displayName: 'Rising Star' },
      { targetCount: 15, coinReward: 500, displayName: 'Star Collector' },
      { targetCount: 30, coinReward: 750, displayName: 'Star Champion' },
    ],
  },
];

const CYCLE_LENGTH = DEFAULT_DAILY_REWARDS.length;

/**
 * Returns the daily reward entry for a given 1-based streak day.
 * Cycles through the 7-day table (day 8 wraps back to day 1 config).
 */
export function getDailyRewardEntry(streakDay: number): DailyRewardEntry {
  const idx = ((streakDay - 1) % CYCLE_LENGTH + CYCLE_LENGTH) % CYCLE_LENGTH;
  return DEFAULT_DAILY_REWARDS[idx];
}

export function getAllDailyRewards(): DailyRewardEntry[] {
  return DEFAULT_DAILY_REWARDS;
}

export function getAchievementDefinition(achievementId: string): AchievementDefinition | null {
  return DEFAULT_ACHIEVEMENT_CONFIG.find(a => a.achievementId === achievementId) ?? null;
}

export function getAllAchievements(): AchievementDefinition[] {
  return DEFAULT_ACHIEVEMENT_CONFIG;
}

export function getAchievementTier(achievementId: string, tierIndex: number): AchievementTier | null {
  const def = getAchievementDefinition(achievementId);
  if (!def) return null;
  return def.tiers[tierIndex] ?? null;
}
