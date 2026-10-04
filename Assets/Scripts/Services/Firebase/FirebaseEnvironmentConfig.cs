// Environment configuration for Firebase/GCP.
// Values are injected at build time — never hard-code secrets here.
// Use Unity's Scripting Define Symbols:
//   KING_SMASH_DEV      -> Development environment
//   KING_SMASH_STAGING  -> Staging environment
//   (no symbol)         -> Production
//
// google-services.json must match the target environment.
// Swap the file using a CI/CD step before each build.

using KingSmash.Core;

namespace KingSmash.Services.Firebase
{
    public static class FirebaseEnvironmentConfig
    {
#if KING_SMASH_DEV
        public const string Environment        = "development";
        public const string ProjectId          = "king-smash-dev";
        public const string CloudRunBaseUrl    = "https://api-dev-xxx.run.app";
        public const string CloudRunGameApiUrl = "https://game-api-dev-xxx.run.app";
#elif KING_SMASH_STAGING
        public const string Environment        = "staging";
        public const string ProjectId          = "king-smash-staging";
        public const string CloudRunBaseUrl    = "https://api-staging-xxx.run.app";
        public const string CloudRunGameApiUrl = "https://game-api-staging-xxx.run.app";
#else
        public const string Environment        = "production";
        public const string ProjectId          = "king-smash-prod";
        public const string CloudRunBaseUrl    = "https://api-xxx.run.app";
        public const string CloudRunGameApiUrl = "https://game-api-xxx.run.app";
#endif

        public static void LogEnvironment()
            => GameLogger.Info("FirebaseEnv", $"Environment: {Environment}, Project: {ProjectId}, GameApiUrl: {CloudRunGameApiUrl}");
    }
}
