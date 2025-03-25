using DTO;

namespace Api.Manager
{
    public interface IUserManager
    {
        public Task<TokenDTO> SignUpUser(string email, string password, string displayName);
        public Task<TokenDTO> SignInUser(string email, string password);
        public Task<UserDTO> GetUserData(string uid);
        public Task ResetPassword(string email);
        public Task SendVerificationMail(string idToken);

        internal class SignInRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";

            public string email { get; set; }
            public string password { get; set; }
            public bool returnSecureToken { get; set; }
        }

        public class SignInResponse
        {
            public string localId { get; set; }
            public string idToken { get; set; }

            public SignInErrorCode? ErrorCode { get; set; }

            public enum SignInErrorCode
            {
                UNKNOWN,
                INVALID_PASSWORD
            }
        }

        internal class ResetPasswordRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=";

            public string requestType => "PASSWORD_RESET";
            public string email { get; set; }
        }

        internal class SendVerificationMailRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=";

            public string requestType => "VERIFY_EMAIL";
            public string idToken { get; set; }
        }
    }
}
