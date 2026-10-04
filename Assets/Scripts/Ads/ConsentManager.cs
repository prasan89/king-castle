using System.Threading.Tasks;
using UnityEngine;

#if GOOGLE_MOBILE_ADS
using Google.MobileAds;
#endif

namespace KingSmash.Ads
{
    public static class ConsentManager
    {
        public enum ConsentStatus { Unknown, Required, NotRequired, Obtained }

        private const string PrefsKey = "ump_consent_status";

        public static ConsentStatus LastKnownStatus
        {
            get => (ConsentStatus)PlayerPrefs.GetInt(PrefsKey, (int)ConsentStatus.Unknown);
            private set => PlayerPrefs.SetInt(PrefsKey, (int)value);
        }

        public static Task<ConsentStatus> RequestConsentInfoUpdateAsync()
        {
#if GOOGLE_MOBILE_ADS
            var tcs = new System.Threading.Tasks.TaskCompletionSource<ConsentStatus>();

            var debugSettings = new ConsentDebugSettings();
            var request = new ConsentRequestParameters { ConsentDebugSettings = debugSettings };

            ConsentInformation.Update(request, error =>
            {
                if (error != null)
                {
                    tcs.TrySetResult(ConsentStatus.Unknown);
                    return;
                }

                ConsentStatus status;
                switch (ConsentInformation.ConsentStatus)
                {
                    case Google.MobileAds.Ump.ConsentStatus.Required:
                        status = ConsentStatus.Required;
                        break;
                    case Google.MobileAds.Ump.ConsentStatus.NotRequired:
                        status = ConsentStatus.NotRequired;
                        break;
                    case Google.MobileAds.Ump.ConsentStatus.Obtained:
                        status = ConsentStatus.Obtained;
                        break;
                    default:
                        status = ConsentStatus.Unknown;
                        break;
                }

                LastKnownStatus = status;
                tcs.TrySetResult(status);
            });

            return tcs.Task;
#else
            return Task.FromResult(ConsentStatus.NotRequired);
#endif
        }

        public static bool IsConsentRequired() => LastKnownStatus == ConsentStatus.Required;

        public static void MarkConsentObtained()
        {
            LastKnownStatus = ConsentStatus.Obtained;
            PlayerPrefs.Save();
        }
    }
}
