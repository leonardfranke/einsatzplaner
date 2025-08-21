using Web.Models;

namespace Web.Manager
{
    public interface IAuthManager
    {
        public static readonly string IsActiveClaim = "IsActive";
        public static readonly string EmailVerifiedClaim = "EmailVerified";
        public static readonly string AdminClaim = "IsAdmin";

        Task<User> Authenticate(string email, string password, string displayName, bool isSignUp);
        Task ResetPassword(string email);
        Task SendVerificationMail(string idToken);
        Task RemoveLocalUser();
        Task<User?> GetLocalUser();
        Task SetCurrentDepartment(string departmentId);

        public class AuthException : Exception
        {
            public AuthError Error { get; set; }

            public enum AuthError
            {
                InvalidPassword,
                EmailAlreadyExists,
                EmailNotFound,
                UserDisabled,
                Unknown
            }
        }

    }
}
