export interface PowerUpReward {
  typeId: string;
  count: number;
}

export interface RewardConfig {
  coins: number;
  gems: number;
  powerUps: PowerUpReward[];
  isEntitlement?: boolean;
}

const DEFAULT_REWARD_CONFIG: Record<string, RewardConfig> = {
  coins_1000:  { coins: 1000,  gems: 0,   powerUps: [] },
  coins_5000:  { coins: 5000,  gems: 0,   powerUps: [] },
  coins_15000: { coins: 15000, gems: 0,   powerUps: [] },
  coins_50000: { coins: 50000, gems: 0,   powerUps: [] },
  gems_small:  { coins: 0,     gems: 10,  powerUps: [] },
  gems_medium: { coins: 0,     gems: 50,  powerUps: [] },
  gems_large:  { coins: 0,     gems: 100, powerUps: [] },
  gems_mega:   { coins: 0,     gems: 500, powerUps: [] },
  remove_ads:  { coins: 0,     gems: 0,   powerUps: [], isEntitlement: true },
  starter_bundle: {
    coins: 5000, gems: 50,
    powerUps: [
      { typeId: 'powerup_bomb', count: 3 },
      { typeId: 'powerup_fire', count: 3 },
    ],
  },
  king_bundle: {
    coins: 15000, gems: 100,
    powerUps: [
      { typeId: 'powerup_megaking',  count: 2 },
      { typeId: 'powerup_lightning', count: 2 },
    ],
  },
};

const NON_CONSUMABLE_IDS = new Set(['remove_ads']);

function loadRewardConfig(): Record<string, RewardConfig> {
  const raw = process.env.REWARD_CONFIG_JSON;
  if (!raw) return DEFAULT_REWARD_CONFIG;
  try {
    const parsed = JSON.parse(raw) as Record<string, Partial<RewardConfig>>;
    const merged: Record<string, RewardConfig> = { ...DEFAULT_REWARD_CONFIG };
    for (const [id, override] of Object.entries(parsed)) {
      merged[id] = {
        coins: override.coins ?? 0,
        gems:  override.gems  ?? 0,
        powerUps: override.powerUps ?? [],
        isEntitlement: override.isEntitlement,
      };
    }
    return merged;
  } catch {
    console.error(JSON.stringify({ severity: 'ERROR', message: 'Failed to parse REWARD_CONFIG_JSON — using defaults' }));
    return DEFAULT_REWARD_CONFIG;
  }
}

const REWARD_CONFIG = loadRewardConfig();

export function getRewardForProduct(productId: string): RewardConfig | null {
  return REWARD_CONFIG[productId] ?? null;
}

export function isNonConsumable(productId: string): boolean {
  return NON_CONSUMABLE_IDS.has(productId);
}
