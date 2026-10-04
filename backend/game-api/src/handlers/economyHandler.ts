import { Router, Request, Response } from 'express';
import { verifyFirebaseToken } from '../middleware/authMiddleware';
import { getEconomyBalance, applyEconomyTransaction } from '../services/firestoreService';
import { ClaimEconomyRequest } from '../dto/types';

export const economyRouter = Router();

export async function getBalance(req: Request, res: Response): Promise<void> {
  try {
    const data = await getEconomyBalance(req.uid);
    if (!data) {
      res.status(404).json({ success: false, error: 'Economy not found. Call /player/init first.' });
      return;
    }
    res.json({ success: true, data });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'GET /economy/balance' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

export async function claimEconomy(req: Request, res: Response): Promise<void> {
  try {
    const uid = req.uid;
    const { idempotencyKey, coins, gems, reason, source } = req.body as ClaimEconomyRequest;

    if (!idempotencyKey) {
      res.status(400).json({ success: false, error: 'Missing idempotencyKey' });
      return;
    }

    if (typeof coins !== 'number' && typeof gems !== 'number') {
      res.status(400).json({ success: false, error: 'Missing coins or gems' });
      return;
    }

    const result = await applyEconomyTransaction(
      uid,
      coins ?? 0,
      gems ?? 0,
      reason ?? 'economy_claim',
      source ?? 'client',
      idempotencyKey,
    );

    res.json({ success: true, data: result });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /economy/claim' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

economyRouter.get('/balance', verifyFirebaseToken, getBalance);
economyRouter.post('/claim', verifyFirebaseToken, claimEconomy);
