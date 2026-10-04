using System;
using System.Threading.Tasks;

namespace KingSmash.Services
{
    public enum AuthProvider
    {
        Anonymous,
        Google
    }

    public class AuthUser
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsAnonymous { get; set; }
        public AuthProvider Provider { get; set; } = AuthProvider.Anonymous;
    }

    public interface IAuthService
    {
        AuthUser CurrentUser { get; }
        bool IsSignedIn { get; }
        bool IsLinked { get; }
        Task<AuthUser> SignInAnonymouslyAsync();
        Task<AuthUser> SignInWithEmailAsync(string email, string password);
        Task<AuthUser> SignInWithGoogleAsync();
        Task<AuthUser> LinkGoogleAccountAsync(string googleIdToken);
        Task SignOutAsync();
        Task DeleteAccountAsync();
    }
}
