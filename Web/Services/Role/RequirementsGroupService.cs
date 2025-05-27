using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;

namespace Web.Services
{
    public class RequirementsGroupService : IRequirementGroupService
    {

        private HttpClient _httpClient;

        public RequirementsGroupService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public Task Delete(string departmentId, string requirementGroupId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("requirementGroupId", requirementGroupId));
            return _httpClient.DeleteAsync(new Uri($"/api/RequirementGroup{query}", UriKind.Relative));
        }

        public async Task<List<Models.RequirementGroup>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/RequirementGroup/{departmentId}", UriKind.Relative));
            var categoryDTOs = await response.Content.ReadFromJsonAsync<List<RequirementGroupDTO>>();
            return RequirementGroupConverter.Convert(categoryDTOs);
        }

        public async Task UpdateOrCreate(string departmentId, string? requirementGroupId, Dictionary<string, uint> requirements)
        {
            var updateCategoryDTO = new RequirementGroupDTO
            {
                Id = requirementGroupId,
                Requirements = requirements
            };
            var content = JsonContent.Create(updateCategoryDTO);
            var res = await _httpClient.PostAsync(new Uri($"/api/RequirementGroup/{departmentId}", UriKind.Relative), content);
        }
    }
}
