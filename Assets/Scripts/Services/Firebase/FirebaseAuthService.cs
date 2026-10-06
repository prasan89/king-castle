// FirebaseAuthService — implements IAuthService.
// Real Firebase calls are inside #if FIREBASE_ENABLED.
// Editor / CI builds without the SDK compile via the stub path.
//
// SETUP REQUIRED:
//   1. Add google-services.json to Assets/ (Android)
//   2. Import Firebase Unity SDK (Auth, Firestore, Analytics, RemoteConfig, Crashlytics)
//   3. Replace AuthServiceMock registration in GameBootstrap with this service
//
// DO NOT commit google-services.json to source control.

using System;
using System.Text;
using System.Threading.Tasks;
using KingSmash.Core;
using UnityEngine.Networking;

namespace KingSmash.Services.Firebase
{
    public class FirebaseAuthService : IAuthService
    {
        private AuthUser _currentUser;

        public AuthUser CurrentUser => _currentUser;
        public bool IsSignedIn => _currentUser != null;
        public bool IsLinked   => _currentUser != null && _currentUser.Provider == AuthProvider.Google;

        public async Task<AuthUser> SignInAnonymouslyAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                var result = await global::Firebase.Auth.FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
                _currentUser = new AuthUser
                {
                    UserId      = result.User.UserId,
                    IsAnonymous = true,
                    Provider    = AuthProvider.Anonymous
                };
                GameLogger.Info("FirebaseAuth", $"Signed in anonymously: {_currentUser.UserId}");
                return _currentUser;
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseAuth", $"SignInAnonymouslyAsync failed: {ex.Message}");
                throw;
            }
#else
            GameLogger.Warning("FirebaseAuth", "Firebase not enabled — stub anonymous sign-in.");
            _currentUser = new AuthUser { UserId = Guid.NewGuid().ToString(), IsAnonymous = true, Provider = AuthProvider.Anonymous };
            return await Task.FromResult(_currentUser);
#endif
        }

        public async Task<AuthUser> SignInWithEmailAsync(string email, string password)
        {
#if FIREBASE_ENABLED
            try
            {
                var result = await global::Firebase.Auth.FirebaseAuth.DefaultInstance
                    .SignInWithEmailAndPasswordAsync(email, password);
                _currentUser = new AuthUser
                {
                    UserId      = result.User.UserId,
                    Email       = result.User.Email,
                    DisplayName = result.User.DisplayName,
                    IsAnonymous = false,
                    Provider    = AuthProvider.Google
                };
                return _currentUser;
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseAuth", $"SignInWithEmailAsync failed: {ex.Message}");
                throw;
            }
#else
            GameLogger.Warning("FirebaseAuth", "Firebase not enabled — stub email sign-in.");
            _currentUser = new AuthUser { UserId = Guid.NewGuid().ToString(), Email = email, IsAnonymous = false, Provider = AuthProvider.Google };
            return await Task.FromResult(_currentUser);
#endif
        }

        public async Task<AuthUser> SignInWithGoogleAsync()
        {
#if FIREBASE_ENABLED
            // Google Sign-In Unity SDK required. Uncomment after importing the plugin.
            //
            // GoogleSignIn.Configuration = new GoogleSignInConfiguration
            // {
            //     WebClientId = "<web-client-id>.apps.googleusercontent.com",
            //     RequestIdToken = true
            // };
            // var googleTask = GoogleSignIn.DefaultInstance.SignIn();
            // await googleTask;
            // if (googleTask.IsFaulted || googleTask.IsCanceled)
            //     throw new Exception("Google Sign-In cancelled or faulted.");
            // var credential = global::Firebase.Auth.GoogleAuthProvider.GetCredential(googleTask.Result.IdToken, null);
            // var result = await global::Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
            // _currentUser = MapFirebaseUser(result.User);
            // return _currentUser;

            GameLogger.Warning("FirebaseAuth", "Google Sign-In Unity SDK not yet configured — stub.");
            _currentUser = new AuthUser { UserId = Guid.NewGuid().ToString(), DisplayName = "Google User", IsAnonymous = false, Provider = AuthProvider.Google };
            return await Task.FromResult(_currentUser);
#else
            GameLogger.Warning("FirebaseAuth", "Firebase not enabled — stub Google sign-in.");
            _currentUser = new AuthUser { UserId = Guid.NewGuid().ToString(), DisplayName = "Google User (stub)", IsAnonymous = false, Provider = AuthProvider.Google };
            return await Task.FromResult(_currentUser);
#endif
        }

        public async Task<AuthUser> LinkGoogleAccountAsync(string googleIdToken)
        {
#if FIREBASE_ENABLED
            if (_currentUser == null)
                throw new InvalidOperationException("Must be signed in before linking.");

            try
            {
                var url     = $"{FirebaseEnvironmentConfig.CloudRunGameApiUrl}/api/v1/player/link-google";
                var idToken = await GetFirebaseIdTokenAsync();
                var body    = Encoding.UTF8.GetBytes($"{{\"googleIdToken\":\"{googleIdToken}\"}}");

                using var req = new UnityWebRequest(url, "POST");
                req.uploadHandler   = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", $"Bearer {idToken}");
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                    throw new Exception($"link-google API error: {req.error}");

                _currentUser.Provider    = AuthProvider.Google;
                _currentUser.IsAnonymous = false;
                GameLogger.Info("FirebaseAuth", "Google account linked successfully.");
                return _currentUser;
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseAuth", $"LinkGoogleAccountAsync failed: {ex.Message}");
                throw;
            }
#else
            await System.Threading.Tasks.Task.CompletedTask;
            throw new NotImplementedException("LinkGoogleAccountAsync requires FIREBASE_ENABLED.");
#endif
        }

        public Task SignOutAsync()
        {
#if FIREBASE_ENABLED
            global::Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
#endif
            _currentUser = null;
            return Task.CompletedTask;
        }

        public async Task DeleteAccountAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                var user = global::Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
                if (user != null) await user.DeleteAsync();
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseAuth", $"DeleteAccountAsync failed: {ex.Message}");
                throw;
            }
#else
            await Task.CompletedTask;
#endif
            _currentUser = null;
        }

        private async Task<string> GetFirebaseIdTokenAsync()
        {
#if FIREBASE_ENABLED
            var user = global::Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null) return string.Empty;
            return await user.TokenAsync(false);
#else
            return await Task.FromResult("stub-id-token");
#endif
        }
    }
}
