using Web.Models;

namespace Web.Manager
{
    public interface IAuthManager
    {
        public static readonly string IsActiveClaim = "IsActive";
        public static readonly string EmailVerifiedClaim = "EmailVerified";

        Task<User> Authenticate(string email, string password, string displayName, bool isSignUp);
        Task ResetPassword(string email);
        Task SendVerificationMail(string idToken);
        Task RemoveLocalUser();
        Task<User?> GetLocalUser();
        Task SetCurrentDepartment(string departmentId);

    }
}
