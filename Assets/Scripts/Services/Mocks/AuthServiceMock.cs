using System;
using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class AuthServiceMock : IAuthService
    {
        private AuthUser _user;

        public AuthUser CurrentUser => _user;
        public bool IsSignedIn => _user != null;
        public bool IsLinked   => _user != null && _user.Provider == AuthProvider.Google;

        public Task<AuthUser> SignInAnonymouslyAsync()
        {
            _user = new AuthUser
            {
                UserId      = Guid.NewGuid().ToString(),
                IsAnonymous = true,
                Provider    = AuthProvider.Anonymous
            };
            GameLogger.Info("AuthServiceMock", $"Signed in anonymously: {_user.UserId}");
            return Task.FromResult(_user);
        }

        public Task<AuthUser> SignInWithEmailAsync(string email, string password)
        {
            _user = new AuthUser
            {
                UserId      = Guid.NewGuid().ToString(),
                Email       = email,
                DisplayName = email,
                IsAnonymous = false,
                Provider    = AuthProvider.Google
            };
            GameLogger.Info("AuthServiceMock", $"Signed in with email: {email}");
            return Task.FromResult(_user);
        }

        public Task<AuthUser> SignInWithGoogleAsync()
        {
            _user = new AuthUser
            {
                UserId      = Guid.NewGuid().ToString(),
                DisplayName = "Mock Google User",
                Email       = "mockuser@gmail.com",
                IsAnonymous = false,
                Provider    = AuthProvider.Google
            };
            GameLogger.Info("AuthServiceMock", $"Signed in with Google (stub): {_user.UserId}");
            return Task.FromResult(_user);
        }

        public Task<AuthUser> LinkGoogleAccountAsync(string googleIdToken)
        {
            if (_user == null)
                throw new InvalidOperationException("Not signed in.");

            _user.Provider    = AuthProvider.Google;
            _user.IsAnonymous = false;
            _user.DisplayName = "Linked Google User (stub)";
            GameLogger.Info("AuthServiceMock", "Google account linked (stub).");
            return Task.FromResult(_user);
        }

        public Task SignOutAsync()
        {
            GameLogger.Info("AuthServiceMock", "Signed out.");
            _user = null;
            return Task.CompletedTask;
        }

        public Task DeleteAccountAsync()
        {
            GameLogger.Info("AuthServiceMock", "Account deleted.");
            _user = null;
            return Task.CompletedTask;
        }
    }
}
