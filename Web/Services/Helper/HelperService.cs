using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;

namespace Web.Services
{
    public class HelperService : IHelperService
    {

        private HttpClient _httpClient;

        public HelperService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public async Task<List<Models.Helper>> GetAll(string departmentId, string? eventId = null)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("eventId", eventId));
            var response = await _httpClient.GetAsync(new Uri($"/api/Helper{query}", UriKind.Relative));
            var helperDTOs = await response.Content.ReadFromJsonAsync<List<HelperDTO>>();
            return HelperConverter.Convert(helperDTOs);
        }

        public async Task<bool> SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isAvailable)
        {
            var query = QueryBuilder.Build(("isAvailableString", isAvailable.ToString()));
            var response = await _httpClient.PostAsync(
                new Uri($"/api/Helper/SetIsHelping/{departmentId}/{eventId}/{helperId}/{memberId}{query}", UriKind.Relative), null);
            var ret = await response.Content.ReadAsStringAsync();
            var parsed = bool.TryParse(ret, out bool result);
            if (parsed)
                return result;
            else
                return false;
        }

        public async Task UpdateLockedMembers(string departmentId, string eventId, string helperId, List<string> formerMembers, List<string> newMembers)
        {
            var updateMembersDTO = new UpdateMembersListDTO
            {
                FormerMembers = formerMembers,
                NewMembers = newMembers
            };
            var content = JsonContent.Create(updateMembersDTO);
            var response = await _httpClient.PostAsync(new Uri($"/api/Helper/UpdateLockedMembers/{departmentId}/{eventId}/{helperId}", UriKind.Relative), content);
        }
    }
}
