using DTO;
using FirebaseAdmin.Auth;
using static Api.Manager.IUserManager;

namespace Api.Manager
{
    public class UserManager : IUserManager
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public UserManager(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY"); 
            if(string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("FIREBASE_API_KEY is not set.");
            }
        }

        public async Task<UserDTO> GetUserData(string uid)
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
            return new UserDTO
            {
                Uid = userRecord.Uid,
                IsDisabled = userRecord.Disabled,
                IsEmailVerified = userRecord.EmailVerified
            };
        }

        public async Task ResetPassword(string email)
        {           
            var requestBody = new ResetPasswordRequest
            {
                email = email
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(ResetPasswordRequest.URL + _apiKey, requestBody);                
            }
            catch
            {
                throw new Exception("Failed to reset password.");
            }
        }

        public async Task SendVerificationMail(string idToken)
        {
            var requestBody = new SendVerificationMailRequest
            {
                idToken = idToken
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(SendVerificationMailRequest.URL + _apiKey, requestBody);                
            }
            catch
            {
                throw new Exception("Failed to send verification mail.");
            }
        }

        public async Task<TokenDTO> SignInUser(string email, string password)
        {
            var requestBody = new SignInRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(SignInRequest.URL + _apiKey, requestBody);
                var result = await httpResponse.Content.ReadFromJsonAsync<SignInResponse>();
                return new TokenDTO
                {
                    Uid = result.localId,
                    IdToken = result.idToken
                };               
            }
            catch
            {
                throw new Exception("Failed to sign in user.");
            }
        }

        public async Task<TokenDTO> SignUpUser(string email, string password, string displayName)
        {
            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = displayName
            });
            var signIn = await SignInUser(email, password);
            return new TokenDTO
            {
                Uid = userRecord.Uid,
                IdToken = signIn.IdToken
            };
        }
    }
}
