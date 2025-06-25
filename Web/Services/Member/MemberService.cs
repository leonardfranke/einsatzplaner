using DTO;
using System.Net.Http.Json;
using Web.Converter;

namespace Web.Services.Member
{
    public class MemberService : IMemberService
    {
        private HttpClient _httpClient;

        public MemberService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
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
            try
            {
                var gameDTOs = await response.Content.ReadFromJsonAsync<MemberDTO>();
                return MemberConverter.Convert(gameDTOs);
            }
            catch
            {
                return null;
            }
        }

        public Task UpdateMember(string departmentId, string memberId, bool isAdmin)
        {
            var updateMembersDTO = new UpdateMemberDTO
            {
                IsAdmin = isAdmin
            };
            var content = JsonContent.Create(updateMembersDTO);
            return _httpClient.PatchAsync(new Uri($"/api/Member/{departmentId}/{memberId}", UriKind.Relative), content);
        }
    }
}