using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Services.Firebase;
using UnityEngine;
using UnityEngine.Networking;

namespace KingSmash.Shop
{
    public sealed class CloudRunReceiptVerificationService : IReceiptVerificationService
    {
        private const string Tag = "CloudRunReceiptVerification";
        private const int TimeoutSeconds = 5;
        private const string Endpoint = "/api/v1/purchases/google/verify";

        [Serializable]
        private class RequestDto
        {
            public string productId;
            public string purchaseToken;
            public string packageName;
            public string playerId;
        }

        [Serializable]
        private class PowerUpRewardDto
        {
            public string powerUpTypeId;
            public int count;
        }

        [Serializable]
        private class ResponseDto
        {
            public bool isValid;
            public string transactionId;
            public long coinsGranted;
            public int gemsGranted;
            public List<PowerUpRewardDto> powerUps;
            public bool isAlreadyProcessed;
            public string error;
        }

        public async Task<VerificationResult> VerifyAsync(VerificationRequest request)
        {
            var dto = new RequestDto
            {
                productId     = request.ProductId,
                purchaseToken = request.PurchaseToken,
                packageName   = request.PackageName,
                playerId      = request.PlayerId,
            };

            string jsonBody  = JsonUtility.ToJson(dto);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            string url = FirebaseEnvironmentConfig.CloudRunBaseUrl + Endpoint;
            using var www = new UnityWebRequest(url, "POST");
            www.uploadHandler   = new UploadHandlerRaw(bodyBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {request.IdToken}");
            www.timeout = TimeoutSeconds;

            var tcs = new TaskCompletionSource<VerificationResult>();
            var operation = www.SendWebRequest();
            operation.completed += _ =>
            {
                try
                {
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        GameLogger.Error(Tag, $"HTTP error: {www.responseCode}");
                        tcs.TrySetResult(new VerificationResult
                            { IsValid = false, Error = $"HTTP {www.responseCode}" });
                        return;
                    }

                    var response = JsonUtility.FromJson<ResponseDto>(www.downloadHandler.text);
                    var rewards  = new List<PowerUpReward>();
                    if (response.powerUps != null)
                        foreach (var pu in response.powerUps)
                            rewards.Add(new PowerUpReward { powerUpTypeId = pu.powerUpTypeId, count = pu.count });

                    tcs.TrySetResult(new VerificationResult
                    {
                        IsValid            = response.isValid,
                        TransactionId      = response.transactionId,
                        CoinsGranted       = response.coinsGranted,
                        GemsGranted        = response.gemsGranted,
                        PowerUpsGranted    = rewards,
                        IsAlreadyProcessed = response.isAlreadyProcessed,
                        Error              = response.error,
                    });
                }
                catch (Exception ex)
                {
                    GameLogger.Error(Tag, $"Response parse error: {ex.Message}");
                    tcs.TrySetResult(new VerificationResult { IsValid = false, Error = ex.Message });
                }
            };

            return await tcs.Task;
        }
    }
}
