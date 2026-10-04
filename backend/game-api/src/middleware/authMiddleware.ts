import { Request, Response, NextFunction } from 'express';
import * as admin from 'firebase-admin';
import * as crypto from 'crypto';

// Extend Express Request to carry uid after token verification
declare global {
  namespace Express {
    interface Request {
      uid: string;
      requestId: string;
      startTime: number;
    }
  }
}

/**
 * Verifies the Firebase ID token from the Authorization header.
 * Attaches req.uid on success. Returns 401 on failure.
 */
export async function verifyFirebaseToken(
  req: Request,
  res: Response,
  next: NextFunction,
): Promise<void> {
  const authHeader = req.headers.authorization;
  if (!authHeader?.startsWith('Bearer ')) {
    res.status(401).json({ success: false, error: 'Missing or malformed Authorization header' });
    return;
  }

  const idToken = authHeader.slice(7);
  try {
    const decoded = await admin.auth().verifyIdToken(idToken);
    req.uid = decoded.uid;
    next();
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Token verification failed';
    res.status(401).json({ success: false, error: `Unauthorized: ${message}` });
  }
}

/**
 * Structured JSON request logger.
 * Attaches requestId and startTime to req; logs on response finish.
 */
export function requestLogger(
  req: Request,
  res: Response,
  next: NextFunction,
): void {
  req.requestId = crypto.randomUUID();
  req.startTime = Date.now();

  res.on('finish', () => {
    const latency = Date.now() - req.startTime;
    const hashedPlayerId = req.uid
      ? crypto.createHash('sha256').update(req.uid).digest('hex').slice(0, 12)
      : 'anonymous';

    const severity = res.statusCode >= 500 ? 'ERROR'
      : res.statusCode >= 400 ? 'WARNING'
      : 'INFO';

    console.log(JSON.stringify({
      severity,
      message: `${req.method} ${req.path} ${res.statusCode}`,
      requestId: req.requestId,
      playerId: hashedPlayerId,
      endpoint: `${req.method} ${req.path}`,
      latencyMs: latency,
      statusCode: res.statusCode,
    }));
  });

  next();
}
