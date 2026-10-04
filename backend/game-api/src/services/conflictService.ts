// Server-side progression conflict resolution.
// Strategy: server revision wins when there is a genuine conflict.
// Client revision wins when the server has never been written or client is ahead.

export interface ProgressionData {
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
  [key: string]: unknown;
}

export type ConflictOutcome = 'client_wins' | 'server_wins' | 'merged';

export interface MergeResult {
  outcome: ConflictOutcome;
  data: ProgressionData;
}

/**
 * Merges client progression with server progression.
 *
 * Rules:
 * - If no server data exists, client wins unconditionally.
 * - If client.revision > server.revision, client wins (client is ahead).
 * - Otherwise: server wins but additive fields (completed levels, stars, bosses, worlds) union.
 */
export function mergeProgression(
  client: ProgressionData,
  server: ProgressionData | null,
): MergeResult {
  if (!server) {
    return { outcome: 'client_wins', data: { ...client, revision: 1 } };
  }

  if (client.revision > server.revision) {
    return {
      outcome: 'client_wins',
      data: {
        ...client,
        revision: client.revision,
      },
    };
  }

  // Server wins — merge additive fields to avoid regressing client progress
  const mergedCompletedLevels = Array.from(
    new Set([...server.completedLevels, ...client.completedLevels]),
  ).sort((a, b) => a - b);

  const mergedCompletedBosses = Array.from(
    new Set([...server.completedBosses, ...client.completedBosses]),
  ).sort((a, b) => a - b);

  const mergedCompletedWorlds = Array.from(
    new Set([...server.completedWorlds, ...client.completedWorlds]),
  ).sort((a, b) => a - b);

  // highestUnlockedLevel: max of both
  const highestUnlockedLevel = Math.max(server.highestUnlockedLevel, client.highestUnlockedLevel);

  // Merge starsPerLevel: take max stars per level
  const starsMap = new Map<number, number>();
  for (const entry of server.starsPerLevel) {
    starsMap.set(entry.levelIndex, entry.stars);
  }
  for (const entry of client.starsPerLevel) {
    const existing = starsMap.get(entry.levelIndex) ?? 0;
    starsMap.set(entry.levelIndex, Math.max(existing, entry.stars));
  }
  const mergedStars = Array.from(starsMap.entries())
    .map(([levelIndex, stars]) => ({ levelIndex, stars }))
    .sort((a, b) => a.levelIndex - b.levelIndex);

  return {
    outcome: 'merged',
    data: {
      ...server,
      highestUnlockedLevel,
      completedLevels: mergedCompletedLevels,
      completedBosses: mergedCompletedBosses,
      completedWorlds: mergedCompletedWorlds,
      starsPerLevel: mergedStars,
      revision: server.revision + 1,
    },
  };
}
