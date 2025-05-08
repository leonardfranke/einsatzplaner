using DTO;
using System.Net.Http.Json;
using Web.Helper;

namespace Web.Services
{
    public class AuthService : IAuthService
    {
        private HttpClient _httpClient;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public async Task<UserDTO?> GetUserData(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync(new Uri($"/api/User/GetUserData/{userId}", UriKind.Relative));
                return await response.Content.ReadFromJsonAsync<UserDTO>();
            }
            catch
            {
                return null;
            }
        }

        public Task ResetPassword(string email)
        {
            var query = QueryBuilder.Build(("email", email));
            return _httpClient.GetAsync(new Uri($"/api/User/ResetPassword{query}", UriKind.Relative));
        }

        public Task SendVerificationMail(string idToken)
        {
            var query = QueryBuilder.Build(("idtoken", idToken));
            return _httpClient.GetAsync(new Uri($"/api/User/SendVerificationMail{query}", UriKind.Relative));
        }

        public async Task<TokenDTO> SignInUser(string email, string password)
        {
            var authDTO = new AuthenticationDTO
            {
                Email = email,
                Password = password
            };
            var content = JsonContent.Create(authDTO);
            var response = await _httpClient.PostAsync(new Uri("/api/User/SignIn", UriKind.Relative), content);
            return await response.Content.ReadFromJsonAsync<TokenDTO>();
        }

        public async Task<TokenDTO> SignUpUser(string email, string password, string displayName)
        {
            var authDTO = new AuthenticationDTO
            {
                Email = email,
                Password = password,
                DisplayName = displayName
            };
            var content = JsonContent.Create(authDTO);
            var response = await _httpClient.PostAsync(new Uri($"/api/User/SignUp", UriKind.Relative), content);
            return await response.Content.ReadFromJsonAsync<TokenDTO>();
        }
    }
}
