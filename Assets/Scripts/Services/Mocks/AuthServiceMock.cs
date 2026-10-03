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

        public Task<AuthUser> SignInAnonymouslyAsync()
        {
            _user = new AuthUser { UserId = Guid.NewGuid().ToString(), IsAnonymous = true };
            GameLogger.Info("AuthServiceMock", $"Signed in anonymously: {_user.UserId}");
            return Task.FromResult(_user);
        }

        public Task<AuthUser> SignInWithEmailAsync(string email, string password)
        {
            _user = new AuthUser { UserId = Guid.NewGuid().ToString(), Email = email, IsAnonymous = false };
            GameLogger.Info("AuthServiceMock", $"Signed in with email: {email}");
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
