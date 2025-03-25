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
        Task SignOut();
        Task<User?> GetLocalUser();

        public Task<string?> GetLocalDepartmentId();
        public Task<Department> GetLocalDepartment();
        public Task SetLocalDepartmentId(string departmentId);
    }
}
