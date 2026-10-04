import { Router, Request, Response } from 'express';
import * as admin from 'firebase-admin';
import { verifyFirebaseToken } from '../middleware/authMiddleware';
import {
  getPlayerProfile,
  initPlayerProfile,
  touchLastLogin,
  initEconomy,
  initDailyRewards,
} from '../services/firestoreService';

export const playerRouter = Router();

/**
 * POST /player/init
 * Creates player profile + economy + dailyRewards documents if they don't exist.
 * Idempotent — safe to call on every app launch.
 */
playerRouter.post('/init', verifyFirebaseToken, async (req: Request, res: Response) => {
  try {
    const uid = req.uid;
    const { gameVersion = '0.0.0' } = req.body as { gameVersion?: string };

    const existing = await getPlayerProfile(uid);
    if (!existing) {
      const authRecord = await admin.auth().getUser(uid);
      const authProvider = authRecord.providerData.some(p => p.providerId === 'google.com')
        ? 'google'
        : 'anonymous';
      await initPlayerProfile(uid, gameVersion, authProvider as 'anonymous' | 'google');
      await initEconomy(uid);
      await initDailyRewards(uid);
      return res.status(201).json({ success: true, data: { created: true, playerId: uid } });
    }

    await touchLastLogin(uid, gameVersion);
    return res.json({ success: true, data: { created: false, playerId: uid } });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /player/init' }));
    return res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

/**
 * GET /player/profile
 * Returns the player profile document.
 */
playerRouter.get('/profile', verifyFirebaseToken, async (req: Request, res: Response) => {
  try {
    const profile = await getPlayerProfile(req.uid);
    if (!profile) {
      return res.status(404).json({ success: false, error: 'Player not found. Call /player/init first.' });
    }
    return res.json({ success: true, data: profile });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'GET /player/profile' }));
    return res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

/**
 * POST /player/link-google
 * Updates authProvider in Firestore after successful Google link via Firebase Auth.
 */
playerRouter.post('/link-google', verifyFirebaseToken, async (req: Request, res: Response) => {
  try {
    const uid = req.uid;
    const authRecord = await admin.auth().getUser(uid);
    const isGoogle = authRecord.providerData.some(p => p.providerId === 'google.com');
    if (!isGoogle) {
      return res.status(400).json({ success: false, error: 'Google account not linked to this Firebase user' });
    }
    await admin.firestore().collection('players').doc(uid).update({ authProvider: 'google' });
    return res.json({ success: true, data: { authProvider: 'google' } });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /player/link-google' }));
    return res.status(500).json({ success: false, error: 'Internal server error' });
  }
});
