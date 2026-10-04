using System.Collections.Generic;
using System.Threading.Tasks;

namespace KingSmash.Shop
{
    public sealed class ReceiptVerificationServiceMock : IReceiptVerificationService
    {
        public const string InvalidReceiptToken = "INVALID_TOKEN";

        private bool _nextVerificationResult = true;

        private static readonly Dictionary<string, long> _coinRewards = new()
        {
            { "coins_1000",  1000  },
            { "coins_5000",  5000  },
            { "coins_15000", 15000 },
            { "coins_50000", 50000 },
        };

        private static readonly Dictionary<string, int> _gemRewards = new()
        {
            { "gems_small",  10  },
            { "gems_medium", 50  },
            { "gems_large",  100 },
            { "gems_mega",   500 },
        };

        private static readonly Dictionary<string, List<PowerUpReward>> _powerUpRewards = new()
        {
            { "starter_bundle", new List<PowerUpReward>
                {
                    new PowerUpReward { powerUpTypeId = "powerup_bomb", count = 3 },
                    new PowerUpReward { powerUpTypeId = "powerup_fire", count = 3 },
                }
            },
            { "king_bundle", new List<PowerUpReward>
                {
                    new PowerUpReward { powerUpTypeId = "powerup_megaking",  count = 2 },
                    new PowerUpReward { powerUpTypeId = "powerup_lightning", count = 2 },
                }
            },
        };

        public void SimulateNextVerificationOutcome(bool isValid)
            => _nextVerificationResult = isValid;

        public Task<VerificationResult> VerifyAsync(VerificationRequest request)
            => VerifyAsync(request.ProductId, request.PurchaseToken);

        public Task<VerificationResult> VerifyAsync(string productId, string purchaseToken)
        {
            if (purchaseToken == InvalidReceiptToken)
            {
                return Task.FromResult(new VerificationResult
                {
                    IsValid = false, Error = "Invalid receipt"
                });
            }

            bool isValid = _nextVerificationResult;
            _nextVerificationResult = true;

            if (!isValid)
            {
                return Task.FromResult(new VerificationResult
                {
                    IsValid = false, Error = "Verification failed"
                });
            }

            _coinRewards.TryGetValue(productId, out long coins);
            _gemRewards.TryGetValue(productId, out int gems);
            _powerUpRewards.TryGetValue(productId, out var powerUps);

            return Task.FromResult(new VerificationResult
            {
                IsValid = true,
                TransactionId = $"mock-verify-{productId}",
                CoinsGranted  = coins,
                GemsGranted   = gems,
                PowerUpsGranted = powerUps ?? new List<PowerUpReward>(),
            });
        }

        public long GetExpectedCoins(string productId)
        {
            _coinRewards.TryGetValue(productId, out long coins);
            return coins;
        }
    }
}
