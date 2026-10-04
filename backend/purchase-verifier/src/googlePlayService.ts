import { google } from 'googleapis';

const ANDROID_PUBLISHER_SCOPE = 'https://www.googleapis.com/auth/androidpublisher';

export interface PlayVerificationResult {
  purchaseState: number;
  consumptionState: number;
  orderId: string;
  purchaseTimeMillis: number;
}

function getClient() {
  const auth = new google.auth.GoogleAuth({ scopes: [ANDROID_PUBLISHER_SCOPE] });
  return google.androidpublisher({ version: 'v3', auth });
}

export async function verifyConsumable(packageName: string, productId: string, purchaseToken: string): Promise<PlayVerificationResult> {
  const client = getClient();
  const response = await client.purchases.products.get({ packageName, productId, token: purchaseToken });
  const data = response.data;
  return {
    purchaseState:    data.purchaseState    ?? -1,
    consumptionState: data.consumptionState ?? -1,
    orderId:          data.orderId          ?? '',
    purchaseTimeMillis: Number(data.purchaseTimeMillis ?? 0),
  };
}

export async function verifyNonConsumable(packageName: string, productId: string, purchaseToken: string): Promise<PlayVerificationResult> {
  return verifyConsumable(packageName, productId, purchaseToken);
}

export async function acknowledgeConsumable(packageName: string, productId: string, purchaseToken: string): Promise<void> {
  const client = getClient();
  await client.purchases.products.consume({ packageName, productId, token: purchaseToken });
}

export async function acknowledgeNonConsumable(packageName: string, productId: string, purchaseToken: string): Promise<void> {
  const client = getClient();
  await client.purchases.products.acknowledge({ packageName, productId, token: purchaseToken, requestBody: {} });
}
