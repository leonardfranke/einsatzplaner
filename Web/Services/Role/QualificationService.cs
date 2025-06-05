using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Models;

namespace Web.Services
{
    public class QualificationService : IQualificationService
    {
        private HttpClient _httpClient;

        public QualificationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public async Task<string> UpdateOrCreate(string departmentId, string? roleId, string? qualificationId, string? newName)
        {
            var updatequalificationDTO = new UpdateQualificationDTO
            {
                RoleId = roleId,
                DepartmentId = departmentId,
                QualificationId = qualificationId,
                NewName = newName
            };
            var content = JsonContent.Create(updatequalificationDTO);
            var response = await _httpClient.PostAsync(new Uri($"/api/Qualification", UriKind.Relative), content);
            return await response.Content.ReadAsStringAsync();
        }

        public Task Delete(string departmentId, string qualificationId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("qualificationId", qualificationId));
            return _httpClient.DeleteAsync(new Uri($"/api/Qualification/{query}", UriKind.Relative));
        }

        public async Task<List<Qualification>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Qualification/{departmentId}", UriKind.Relative));
            var roleDTOs = await response.Content.ReadFromJsonAsync<List<QualificationDTO>>();
            return QualificationConverter.Convert(roleDTOs);
        }

        public async Task<Qualification?> GetById(string departmentId, string qualificationId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Qualification/{departmentId}/{qualificationId}", UriKind.Relative));
            var roleDTO = await response.Content.ReadFromJsonAsync<QualificationDTO>();
            return QualificationConverter.Convert(roleDTO);
        }

        public Task UpdateQualificationMembers(string departmentId, string qualificationId, IEnumerable<string> newMembers, IEnumerable<string> formerMembers)
        {
            var updateMembersListDTO = new UpdateMembersListDTO
            {
                NewMembers = newMembers,
                FormerMembers = formerMembers
            };
            var content = JsonContent.Create(updateMembersListDTO);
            return _httpClient.PatchAsync(new Uri($"/api/Qualification/{departmentId}/{qualificationId}", UriKind.Relative), content);
        }
    }
}
