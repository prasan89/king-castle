import { Router, Request, Response } from 'express';
import { FieldValue } from 'firebase-admin/firestore';
import { verifyFirebaseToken } from '../middleware/authMiddleware';
import { getProgression, saveProgression, progressionRef } from '../services/firestoreService';
import { mergeProgression, ProgressionData } from '../services/conflictService';
import { SaveProgressionRequest } from '../dto';

export const progressionRouter = Router();

export async function loadProgression(req: Request, res: Response): Promise<void> {
  try {
    const data = await getProgression(req.uid);
    if (!data) {
      res.status(404).json({ success: false, error: 'Progression not found' });
      return;
    }
    res.json({ success: true, data });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'GET /progression/load' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

export async function saveProgressionHandler(req: Request, res: Response): Promise<void> {
  try {
    const uid = req.uid;
    const client = req.body as SaveProgressionRequest;

    if (typeof client.revision !== 'number') {
      res.status(400).json({ success: false, error: 'Missing required field: revision' });
      return;
    }

    const result = await saveProgression(uid, client as unknown as Record<string, unknown>);
    res.json({
      success: true,
      data: {
        revision: result.revision,
        updatedAt: result.updatedAt,
      },
    });
  } catch (err) {
    const code = (err as { code?: number }).code;
    if (code === 409) {
      res.status(409).json({ success: false, error: 'Revision rollback rejected' });
      return;
    }
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /progression/save' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

export async function syncProgression(req: Request, res: Response): Promise<void> {
  try {
    const uid = req.uid;
    const { clientProgression, clientRevision } = req.body as {
      clientProgression?: ProgressionData;
      clientRevision?: number;
    };

    if (!clientProgression) {
      res.status(400).json({ success: false, error: 'Missing clientProgression' });
      return;
    }

    const serverData = await getProgression(uid);
    const mergeResult = mergeProgression(
      { ...clientProgression, revision: clientRevision ?? clientProgression.revision },
      serverData as ProgressionData | null,
    );

    const saved = await saveProgression(uid, mergeResult.data as unknown as Record<string, unknown>);

    res.json({
      success: true,
      data: {
        outcome: mergeResult.outcome,
        mergedProgression: { ...mergeResult.data, revision: saved.revision },
        revision: saved.revision,
      },
    });
  } catch (err) {
    const code = (err as { code?: number }).code;
    if (code === 409) {
      res.status(409).json({ success: false, error: 'Revision conflict' });
      return;
    }
    const message = err instanceof Error ? err.message : 'Unknown error';
    console.error(JSON.stringify({ severity: 'ERROR', message, endpoint: 'POST /progression/sync' }));
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
}

// Register routes
progressionRouter.get('/load', verifyFirebaseToken, loadProgression);
progressionRouter.post('/save', verifyFirebaseToken, saveProgressionHandler);
progressionRouter.post('/sync', verifyFirebaseToken, syncProgression);
