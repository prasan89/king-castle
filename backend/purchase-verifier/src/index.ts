import express from 'express';
import cors from 'cors';
import helmet from 'helmet';
import * as admin from 'firebase-admin';
import { verifyPurchaseHandler } from './verifyPurchaseHandler';

admin.initializeApp();

const app = express();
const PORT = process.env.PORT || 8080;

app.use(helmet());
app.use(cors());
app.use(express.json());

app.post('/api/v1/purchases/google/verify', verifyPurchaseHandler);

app.get('/health', (_req, res) => {
  res.json({ status: 'ok' });
});

app.listen(PORT, () => {
  console.log(JSON.stringify({ severity: 'INFO', message: `purchase-verifier listening on port ${PORT}` }));
});
