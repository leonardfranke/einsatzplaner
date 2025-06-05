using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Models;

namespace Web.Services
{
    public class RoleService : IRoleService
    {
        private HttpClient _httpClient;

        public RoleService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public async Task<string> UpdateOrCreate(string departmentId, string? roleId, string? newName, int? newLockingPeriod, bool? newIsFree)
        {
            var updateCategoryDTO = new UpdateRoleDTO
            {
                DepartmentId = departmentId,
                RoleId = roleId,
                NewName = newName,
                NewLockingPeriod = newLockingPeriod,
                NewIsFree = newIsFree
            };
            var content = JsonContent.Create(updateCategoryDTO);
            var response = await _httpClient.PostAsync(new Uri($"/api/Role", UriKind.Relative), content);
            return await response.Content.ReadAsStringAsync();
        }

        public Task Delete(string departmentId, string roleId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("roleId", roleId));
            return _httpClient.DeleteAsync(new Uri($"/api/Role/{query}", UriKind.Relative));
        }

        public async Task<List<Role>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Role/{departmentId}", UriKind.Relative));
            var roleDTOs = await response.Content.ReadFromJsonAsync<List<RoleDTO>>();
            return RoleConverter.Convert(roleDTOs);
        }

        public async Task<Role?> GetById(string departmentId, string roleId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Role/{departmentId}/{roleId}", UriKind.Relative));
            var roleDTO = await response.Content.ReadFromJsonAsync<RoleDTO>();
            return RoleConverter.Convert(roleDTO);
        }

        public Task UpdateRoleMembers(string departmentId, string roleId, IEnumerable<string> newMembers, IEnumerable<string> formerMembers)
        {
            var updateMembersListDTO = new UpdateMembersListDTO
            {
                NewMembers = newMembers,
                FormerMembers = formerMembers
            };
            var content = JsonContent.Create(updateMembersListDTO);
            return _httpClient.PatchAsync(new Uri($"/api/Role/{departmentId}/{roleId}", UriKind.Relative), content);
        }
    }
}
