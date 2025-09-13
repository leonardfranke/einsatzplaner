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
            _httpClient = httpClientFactory.CreateClient("FIREBASE_AUTH");
            _apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY") ?? "NO-KEY";
        }

        public async Task<UserDTO> GetUserData(string uid)
        {
            try
            {
                var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                return new UserDTO
                {
                    Name = userRecord.DisplayName,
                    Uid = userRecord.Uid,
                    IsDisabled = userRecord.Disabled,
                    Email = userRecord.Email,
                    IsEmailVerified = userRecord.EmailVerified
                };
            }
            catch
            {
                return null;
            }
            
        }

        public async Task ResetPassword(string email)
        {
            var requestBody = new ResetPasswordRequest
            {
                email = email
            };
            try
            {
                var uri = Path.Combine(_httpClient.BaseAddress.ToString(), ResetPasswordRequest.URL + _apiKey);
                var httpResponse = await _httpClient.PostAsJsonAsync(uri, requestBody);                
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
                var uri = Path.Combine(_httpClient.BaseAddress.ToString(), SendVerificationMailRequest.URL + _apiKey);
                var httpResponse = await _httpClient.PostAsJsonAsync(uri, requestBody);                
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
            var uri = Path.Combine(_httpClient.BaseAddress.ToString(), SignInRequest.URL + _apiKey);
            var httpResponse = await _httpClient.PostAsJsonAsync(uri, requestBody);
            if(httpResponse.IsSuccessStatusCode)
            {
                var result = await httpResponse.Content.ReadFromJsonAsync<SignInResponse>();
                return new TokenDTO
                {
                    Uid = result.localId,
                    IdToken = result.idToken
                };
            }
            else
            {
                var result = await httpResponse.Content.ReadFromJsonAsync<AuthRestApiErrorResponse>();
                if (result.error.message == "EMAIL_NOT_FOUND")
                    return new TokenDTO { Error = TokenDTO.AuthError.EmailNotFound };
                else if (result.error.message == "INVALID_PASSWORD")
                    return new TokenDTO { Error = TokenDTO.AuthError.InvalidPassword };
                else if (result.error.message == "USER_DISABLED")
                    return new TokenDTO { Error = TokenDTO.AuthError.UserDisabled };
                else
                    return new TokenDTO { Error = TokenDTO.AuthError.Unknown };
            }               
        }

        public async Task<TokenDTO> SignUpUser(string email, string password, string displayName)
        {
            try
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
            catch(FirebaseAuthException authExp)
            {
                if (authExp.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
                    return new TokenDTO { Error = TokenDTO.AuthError.EmailAlreadyExists };
                else
                    return new TokenDTO { Error = TokenDTO.AuthError.Unknown };
            }            
        }
    }
}
