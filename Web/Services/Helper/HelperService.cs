using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Manager;

namespace Web.Services
{
    public class HelperService : IHelperService
    {

        private IBackendManager _backendManager;

        public HelperService(IBackendManager backendManager)
        {
            _backendManager = backendManager;
        }

        public async Task<List<Models.Helper>> GetAll(string departmentId, string? eventId = null)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("eventId", eventId));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Helper{query}", UriKind.Relative));
            var helperDTOs = await response.Content.ReadFromJsonAsync<List<HelperDTO>>();
            return HelperConverter.Convert(helperDTOs);
        }

        public async Task<bool> SetIsHelping(string departmentId, string eventId, string helperCategoryId, string memberId, bool isHelping)
        {
            var query = QueryBuilder.Build(("isHelpingString", isHelping.ToString()));
            var response = await _backendManager.HttpClient.PostAsync(
                new Uri($"/api/Helper/SetIsHelping/{departmentId}/{eventId}/{helperCategoryId}/{memberId}{query}", UriKind.Relative), null);
            var ret = await response.Content.ReadAsStringAsync();
            var parsed = bool.TryParse(ret, out bool result);
            if (parsed)
                return result;
            else
                return false;
        }
    }
}
