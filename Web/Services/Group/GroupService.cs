using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Models;

namespace Web.Services
{
    public class GroupService : IGroupService
    {
        private HttpClient _httpClient;

        public GroupService(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        public async Task<string> UpdateOrCreateGroup(string departmentId, string? groupId, string name)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("groupId", groupId), ("name", name));
            var response = await _httpClient.PostAsync(new Uri($"/api/Group{query}", UriKind.Relative), null);
            return await response.Content.ReadAsStringAsync();
        }

        public Task DeleteGroup(string departmentId, string groupId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("groupId", groupId));
            return _httpClient.DeleteAsync(new Uri($"/api/Group/{query}", UriKind.Relative));
        }

        public async Task<List<Group>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Group/{departmentId}", UriKind.Relative));
            var groupDTOs = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();
            return GroupConverter.Convert(groupDTOs);
        }

        public async Task<Group?> GetById(string departmentId, string groupId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Group/{departmentId}/{groupId}", UriKind.Relative));
            var groupDTO = await response.Content.ReadFromJsonAsync<GroupDTO>();
            return GroupConverter.Convert(groupDTO);
        }

        public Task UpdateGroupMembers(string departmentId, string groupId, List<string> newMembers, List<string> formerMembers)
        {
            var updateMembersListDTO = new UpdateMembersListDTO
            {
                NewMembers = newMembers,
                FormerMembers = formerMembers
            };
            var content = JsonContent.Create(updateMembersListDTO);
            return _httpClient.PatchAsync(new Uri($"/api/Group/{departmentId}/{groupId}", UriKind.Relative), content);
        }
    }
}
