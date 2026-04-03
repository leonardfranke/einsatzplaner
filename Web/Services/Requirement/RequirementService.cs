using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;

namespace Web.Services
{
    public class RequirementService : IRequirementService
    {

        private HttpClient _httpClient;

        public RequirementService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public Task CreateRequirement(UpdateRequirementDTO updateRequirementDTO)
        {
            var content = JsonContent.Create(updateRequirementDTO);
            return _httpClient.PostAsync(new Uri($"/api/Requirement/", UriKind.Relative), content);
        }

        public Task UpdateRequirement(UpdateRequirementDTO updateRequirementDTO)
        {
            var content = JsonContent.Create(updateRequirementDTO);
            return _httpClient.PatchAsync(new Uri($"/api/Requirement/", UriKind.Relative), content);
        }

        public Task DeleteRequirement(string departmentId, string eventId, string roleId)
        {
            return _httpClient.DeleteAsync(new Uri($"/api/Requirement/{departmentId}/{eventId}/{roleId}", UriKind.Relative));
        }

        public Task CreateOrUpdateQualificationRequirement(UpdateQualificationRequirementDTO updateQualificationRequirementDTO)
        {
            var content = JsonContent.Create(updateQualificationRequirementDTO);
            return _httpClient.PostAsync(new Uri($"/api/QualificationRequirement/", UriKind.Relative), content);
        }

        public Task DeleteQualificationRequirement(string departmentId, string eventId, string roleId, string qualificationId)
        {
            return _httpClient.DeleteAsync(new Uri($"/api/QualificationRequirement/{departmentId}/{eventId}/{roleId}/{qualificationId}", UriKind.Relative));
        }

        public async Task<List<Models.Requirement>> GetAll(string departmentId, string? eventId = null)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("eventId", eventId));
            var response = await _httpClient.GetAsync(new Uri($"/api/Requirement{query}", UriKind.Relative));
            var helperDTOs = await response.Content.ReadFromJsonAsync<List<RequirementDTO>>();
            return HelperConverter.Convert(helperDTOs);
        }

        public async Task<bool> SetIsAvailable(string departmentId, string eventId, string roleId, string memberId, bool isAvailable)
        {
            var query = QueryBuilder.Build(("isAvailableString", isAvailable.ToString()));
            var response = await _httpClient.PostAsync(
                new Uri($"/api/Requirement/SetIsHelping/{departmentId}/{eventId}/{roleId}/{memberId}{query}", UriKind.Relative), null);
            var ret = await response.Content.ReadAsStringAsync();
            var parsed = bool.TryParse(ret, out bool result);
            if (parsed)
                return result;
            else
                return false;
        }

        public async Task UpdateEnteringType(string departmentId, string eventId, string roleId, List<string> members, EnteringType? enteringType)
        {
            var updateEnteringsDTO = new UpdateEnteringsDTO
            {
                Members = members,
                EnteringType = enteringType
            };
            var content = JsonContent.Create(updateEnteringsDTO);
            var response = await _httpClient.PostAsync(new Uri($"/api/Requirement/UpdateEnteringType/{departmentId}/{eventId}/{roleId}", UriKind.Relative), content);
        }
    }
}
