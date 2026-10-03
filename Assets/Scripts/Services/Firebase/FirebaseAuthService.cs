// Firebase implementation — requires com.google.firebase.auth Unity package.
// Wire this in GameBootstrap.RegisterServices() replacing AuthServiceMock
// when Firebase credentials are configured.
//
// SETUP REQUIRED:
//   1. Add google-services.json to Assets/ (Android)
//   2. Import Firebase Unity SDK (Auth, Firestore, Analytics, RemoteConfig, Crashlytics)
//   3. Replace mock registrations in GameBootstrap with real implementations
//
// DO NOT commit google-services.json to source control.
// Add it to .gitignore:
//   Assets/google-services.json
//   Assets/google-services.json.meta
//   Assets/GoogleService-Info.plist
//   Assets/GoogleService-Info.plist.meta

using System;
using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Services.Firebase
{
    /// Firebase Auth implementation. Uncomment and complete after SDK import.
    public class FirebaseAuthService : IAuthService
    {
        private AuthUser _currentUser;

        public AuthUser CurrentUser => _currentUser;
        public bool IsSignedIn => _currentUser != null;

        public async Task<AuthUser> SignInAnonymouslyAsync()
        {
            // var result = await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
            // _currentUser = new AuthUser { UserId = result.User.UserId, IsAnonymous = true };
            GameLogger.Warning("FirebaseAuthService", "Firebase SDK not yet imported — using stub.");
            _currentUser = new AuthUser { UserId = Guid.NewGuid().ToString(), IsAnonymous = true };
            return await Task.FromResult(_currentUser);
        }

        public Task<AuthUser> SignInWithEmailAsync(string email, string password)
            => throw new NotImplementedException("Implement after Firebase SDK import.");

        public Task SignOutAsync()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }

        public Task DeleteAccountAsync()
            => throw new NotImplementedException("Implement after Firebase SDK import.");
    }
}
