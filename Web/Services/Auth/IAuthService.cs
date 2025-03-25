using DTO;

namespace Web.Services
{
    public interface IAuthService
    {
        public Task<TokenDTO> SignUpUser(string email, string password, string displayName);
        public Task<TokenDTO> SignInUser(string email, string password);
        public Task<UserDTO> GetUserData(string idToken);
        public Task ResetPassword(string email);
        public Task SendVerificationMail(string idToken);
    }
}
