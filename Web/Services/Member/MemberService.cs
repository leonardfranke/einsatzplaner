using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;

namespace Web.Services.Member
{
    public class MemberService : IMemberService
    {
        private HttpClient _httpClient;

        public MemberService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Models.Member>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Member/{departmentId}", UriKind.Relative));
            var gameDTOs = await response.Content.ReadFromJsonAsync<List<MemberDTO>>();
            return MemberConverter.Convert(gameDTOs);
        }

        public async Task<Models.Member> GetMember(string departmentId, string memberId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Member/{departmentId}/{memberId}", UriKind.Relative));
            var gameDTOs = await response.Content.ReadFromJsonAsync<MemberDTO>();
            return MemberConverter.Convert(gameDTOs);
        }

        public async Task UpdateMember(string departmentId, string userId, List<string> groupIds, List<string> roleIds, bool isAdmin)
        {
            var updateMemberDTO = new UpdateMemberDTO
            {
                GroupIds = groupIds,
                RoleIds = roleIds,
                IsAdmin = isAdmin
            };
            var content = JsonContent.Create(updateMemberDTO);
            var query = QueryBuilder.Build(("departmentId", departmentId), ("memberId", userId));
            var response = await _httpClient.PatchAsync(new Uri($"/api/Member{query}", UriKind.Relative), content);
        }
    }
}
