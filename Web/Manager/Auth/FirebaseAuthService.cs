using System.Net.Http.Json;
using System.Text.Json.Nodes;
using static Web.Manager.Auth.IFirebaseAuthService;
using ResetPasswordRequest = Web.Manager.Auth.IFirebaseAuthService.ResetPasswordRequest;
using ResetPasswordResponse = Web.Manager.Auth.IFirebaseAuthService.ResetPasswordResponse;

namespace Web.Manager.Auth
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly HttpClient _httpClient;

        public FirebaseAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GetUserDataResponse> GetUserData(string idToken)
        {
            var requestBody = new GetUserDataRequest
            {
                idToken = idToken
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(GetUserDataRequest.URL, requestBody);
                if (httpResponse is { StatusCode: System.Net.HttpStatusCode.OK })
                {
                    var result = await httpResponse.Content.ReadFromJsonAsync<GetUserDataResponse>();
                    return result;
                }
                var errorResult = await JsonNode.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
                GetUserDataResponse.GetUserDatErrorCode getUserErrorCode;
                var parsed = Enum.TryParse(errorResult?["error"]?["message"]?.GetValue<string>(), out getUserErrorCode);
                if (parsed)
                    return new GetUserDataResponse { ErrorCode = getUserErrorCode };
                else
                    return new GetUserDataResponse { ErrorCode = GetUserDataResponse.GetUserDatErrorCode.UNKNOWN };
            }
            catch
            {
                return new GetUserDataResponse { ErrorCode = GetUserDataResponse.GetUserDatErrorCode.UNKNOWN };
            }
        }

        public async Task<ResetPasswordResponse> ResetPassword(string email)
        {
            var requestBody = new ResetPasswordRequest
            {
                email = email
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(ResetPasswordRequest.URL, requestBody);
                if (httpResponse is { StatusCode: System.Net.HttpStatusCode.OK })
                {
                    var result = await httpResponse.Content.ReadFromJsonAsync<ResetPasswordResponse>();
                    return result;
                }
                var errorResult = await JsonNode.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
                ResetPasswordResponse.ResetPasswordErrorCode resetPasswordErrorCode;
                var parsed = Enum.TryParse(errorResult?["error"]?["message"]?.GetValue<string>(), out resetPasswordErrorCode);
                if (parsed)
                    return new ResetPasswordResponse { ErrorCode = resetPasswordErrorCode };
                else
                    return new ResetPasswordResponse { ErrorCode = ResetPasswordResponse.ResetPasswordErrorCode.UNKNOWN };
            }
            catch
            {
                return new ResetPasswordResponse { ErrorCode = ResetPasswordResponse.ResetPasswordErrorCode.UNKNOWN };
            }
        }

        public async Task<SendVerificationMailResponse> SendVerificationMail(string idToken)
        {
            var requestBody = new SendVerificationMailRequest
            {
                idToken = idToken
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(SendVerificationMailRequest.URL, requestBody);
                if (httpResponse is { StatusCode: System.Net.HttpStatusCode.OK })
                {
                    var result = await httpResponse.Content.ReadFromJsonAsync<SendVerificationMailResponse>();
                    return result;
                }
                var errorResult = await JsonNode.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
                SendVerificationMailResponse.SendVerificationMailErrorCode sendVerificationMailErrorCode;
                var parsed = Enum.TryParse(errorResult?["error"]?["message"]?.GetValue<string>(), out sendVerificationMailErrorCode);
                if (parsed)
                    return new SendVerificationMailResponse { ErrorCode = sendVerificationMailErrorCode };
                else
                    return new SendVerificationMailResponse { ErrorCode = SendVerificationMailResponse.SendVerificationMailErrorCode.UNKNOWN };
            }
            catch
            {
                return new SendVerificationMailResponse { ErrorCode = SendVerificationMailResponse.SendVerificationMailErrorCode.UNKNOWN };
            }
        }

        public async Task<SignInResponse> SignInUser(string email, string password)
        {
            var requestBody = new SignInRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(SignInRequest.URL, requestBody);
                if (httpResponse is { StatusCode: System.Net.HttpStatusCode.OK })
                {
                    var result = await httpResponse.Content.ReadFromJsonAsync<SignInResponse>();
                    return result;
                }
                var errorResult = await JsonNode.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
                SignInResponse.SignInErrorCode signInErrorCode;
                var parsed = Enum.TryParse(errorResult?["error"]?["message"]?.GetValue<string>(), out signInErrorCode);
                if (parsed)
                    return new SignInResponse { ErrorCode = signInErrorCode };
                else
                    return new SignInResponse { ErrorCode = SignInResponse.SignInErrorCode.UNKNOWN };
            }
            catch
            {
                return new SignInResponse { ErrorCode = SignInResponse.SignInErrorCode.UNKNOWN };
            }
        }

        public async Task<SignUpResponse> SignUpUser(string email, string password, string displayName)
        {
            var requestBody = new SignUpRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync(SignUpRequest.URL, requestBody);

                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var errorResult = await JsonNode.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
                    SignUpResponse.SignUpErrorCode signUpErrorCode;
                    var parsed = Enum.TryParse(errorResult?["error"]?["message"]?.GetValue<string>(), out signUpErrorCode);
                    if (parsed)
                        return new SignUpResponse { ErrorCode = signUpErrorCode };
                    else
                        return new SignUpResponse { ErrorCode = SignUpResponse.SignUpErrorCode.UNKNOWN };
                }                

                var signUpResult = await httpResponse.Content.ReadFromJsonAsync<SignUpResponse>();

                var updateBody = new UpdateProfileRequest
                {
                    idToken = signUpResult.idToken,
                    displayName = displayName
                };
                var updateResponse = await _httpClient.PostAsJsonAsync(UpdateProfileRequest.URL, updateBody);

                return signUpResult;
            }
            catch
            {
                return new SignUpResponse { ErrorCode = SignUpResponse.SignUpErrorCode.UNKNOWN };
            }
            
        }
    }
}
