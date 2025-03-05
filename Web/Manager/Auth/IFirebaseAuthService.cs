namespace Web.Manager.Auth
{
    public interface IFirebaseAuthService
    {
        public Task<SignUpResponse> SignUpUser(string email, string password, string displayName);
        public Task<SignInResponse> SignInUser(string email, string password);
        public Task<GetUserDataResponse> GetUserData(string idToken);
        public Task<ResetPasswordResponse> ResetPassword(string email);
        public Task<SendVerificationMailResponse> SendVerificationMail(string idToken);

        private static readonly string _apiKey = Environment.GetEnvironmentVariable("FIREBASE_APIKEY");        

        internal class UpdateProfileRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={_apiKey}";

            public string idToken { get; set; }
            public string displayName { get; set; }
        }

        internal class SignInRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";

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

        internal class SignUpRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";

            public string email { get; set; }
            public string password { get; set; }
            public bool returnSecureToken { get; set; }
        }

        public class SignUpResponse
        {
            public string idToken { get; set; }
            public string localId { get; set; }

            public SignUpErrorCode? ErrorCode { get; set; }

            public enum SignUpErrorCode
            {
                UNKNOWN,
                EMAIL_EXISTS
            }
        }
        internal class GetUserDataRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";

            public string idToken { get; set; }
        }

        public class GetUserDataResponse
        {
            public List<GetUserDataUser> users { get; set; }

            public GetUserDatErrorCode? ErrorCode { get; set; }

            public enum GetUserDatErrorCode
            {
                UNKNOWN
            }
        }

        public class GetUserDataUser
        {
            public string localId { get; set; }
            public bool disabled { get; set; }
            public bool emailVerified { get; set; }
        }

        internal class ResetPasswordRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";

            public string requestType => "PASSWORD_RESET";
            public string email { get; set; }
        }

        public class ResetPasswordResponse
        {
            public ResetPasswordErrorCode? ErrorCode { get; set; }

            public enum ResetPasswordErrorCode
            {
                UNKNOWN,
                EMAIL_NOT_FOUND
            }
        }

        internal class SendVerificationMailRequest
        {
            internal static string URL = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";

            public string requestType => "VERIFY_EMAIL";
            public string idToken { get; set; }
        }

        public class SendVerificationMailResponse
        {
            public SendVerificationMailErrorCode? ErrorCode { get; set; }

            public enum SendVerificationMailErrorCode
            {
                UNKNOWN,
                USER_NOT_FOUND
            }
        }
    }
}
