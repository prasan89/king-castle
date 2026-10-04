import express from 'express';
import helmet from 'helmet';
import cors from 'cors';
import * as admin from 'firebase-admin';

import { requestLogger } from './middleware/authMiddleware';
import { generalLimiter, rewardLimiter } from './middleware/rateLimiter';
import { playerRouter } from './handlers/playerHandler';
import { progressionRouter } from './handlers/progressionHandler';
import { economyRouter } from './handlers/economyHandler';
import { rewardsRouter } from './handlers/rewardsHandler';

// Initialize Firebase Admin SDK using Application Default Credentials
admin.initializeApp();

const app = express();
const PORT = parseInt(process.env.PORT ?? '8080', 10);

// Security middleware
app.use(helmet());
app.use(cors({
  origin: (process.env.ALLOWED_ORIGINS ?? '').split(',').filter(Boolean),
  methods: ['GET', 'POST'],
  allowedHeaders: ['Authorization', 'Content-Type', 'X-Idempotency-Key'],
}));
app.use(express.json({ limit: '256kb' }));

// Structured request logging
app.use(requestLogger);

// Health check (no auth required)
app.get('/health', (_req, res) => {
  res.json({ success: true, data: { status: 'ok', timestamp: new Date().toISOString() } });
});

// Routes
app.use('/player', generalLimiter, playerRouter);
app.use('/progression', generalLimiter, progressionRouter);
app.use('/economy', generalLimiter, economyRouter);
app.use('/rewards', rewardsRouter);

// 404 fallthrough
app.use((_req, res) => {
  res.status(404).json({ success: false, error: 'Not found' });
});

// Global error handler
app.use((err: Error, _req: express.Request, res: express.Response, _next: express.NextFunction) => {
  console.error(JSON.stringify({ severity: 'ERROR', message: err.message, stack: err.stack }));
  res.status(500).json({ success: false, error: 'Internal server error' });
});

app.listen(PORT, () => {
  console.log(JSON.stringify({ severity: 'INFO', message: `game-api listening on port ${PORT}` }));
});

export default app;
export { generalLimiter, rewardLimiter };
