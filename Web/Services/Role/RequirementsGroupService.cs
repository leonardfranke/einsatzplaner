using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Manager;

namespace Web.Services
{
    public class RequirementsGroupService : IRequirementGroupService
    {

        private IBackendManager _backendManager;

        public RequirementsGroupService(IBackendManager backendManager)
        {
            _backendManager = backendManager;
        }

        public Task Delete(string departmentId, string helperCategoryGroupId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("helperCategoryGroupId", helperCategoryGroupId));
            return _backendManager.HttpClient.DeleteAsync(new Uri($"/api/HelperCategoryGroup{query}", UriKind.Relative));
        }

        public async Task<List<Models.RequirementGroup>> GetAll(string departmentId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/HelperCategoryGroup{query}", UriKind.Relative));
            var categoryDTOs = await response.Content.ReadFromJsonAsync<List<RequirementGroupDTO>>();
            return HelperCategoryGroupConverter.Convert(categoryDTOs);
        }

        public async Task UpdateOrCreate(string departmentId, string? helperCategoryGroupId, Dictionary<string, uint> requirements)
        {
            var updateCategoryDTO = new UpdateHelperCategoryGroupDTO
            {
                Requirements = requirements
            };
            var content = JsonContent.Create(updateCategoryDTO);
            var query = QueryBuilder.Build(("departmentId", departmentId), ("helperCategoryGroupId", helperCategoryGroupId));
            var res = await _backendManager.HttpClient.PostAsync(new Uri($"/api/HelperCategoryGroup{query}", UriKind.Relative), content);
        }
    }
}
