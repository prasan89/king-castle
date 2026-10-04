using System;

namespace KingSmash.Ads
{
    [Serializable]
    public class AdSessionStats
    {
        public int interstitialsShownThisSession = 0;
        public int totalInterstitialsShown = 0;
        public long sessionStartTimestamp = 0;
        public int lastInterstitialLevelIndex = -1;

        public void ResetSession(long nowTimestamp)
        {
            interstitialsShownThisSession = 0;
            sessionStartTimestamp = nowTimestamp;
        }

        public void RecordInterstitialShown(int levelIndex)
        {
            interstitialsShownThisSession++;
            totalInterstitialsShown++;
            lastInterstitialLevelIndex = levelIndex;
        }
    }
}
