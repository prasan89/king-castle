using System;
using System.Threading.Tasks;

namespace KingSmash.Services
{
    public class AuthUser
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public interface IAuthService
    {
        AuthUser CurrentUser { get; }
        bool IsSignedIn { get; }
        Task<AuthUser> SignInAnonymouslyAsync();
        Task<AuthUser> SignInWithEmailAsync(string email, string password);
        Task SignOutAsync();
        Task DeleteAccountAsync();
    }
}
