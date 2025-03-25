using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Models;

namespace Web.Services
{
    public class EventCategoryService : IEventCategoryService
    {
        private HttpClient _httpClient;

        public EventCategoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public Task Delete(string departmentId, string eventCategoryId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("eventCategoryId", eventCategoryId));
            return _httpClient.DeleteAsync(new Uri($"/api/EventCategory{query}", UriKind.Relative));
        }

        public async Task<List<EventCategory>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/EventCategory/{departmentId}", UriKind.Relative));
            var categoryDTOs = await response.Content.ReadFromJsonAsync<List<EventCategoryDTO>>();
            return EventCategoryConverter.Convert(categoryDTOs);
        }

        public async Task<EventCategory> GetById(string departmentId, string eventCategoryId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/EventCategory/{departmentId}/{eventCategoryId}", UriKind.Relative));
            var categoryDTOs = await response.Content.ReadFromJsonAsync<EventCategoryDTO>();
            return EventCategoryConverter.Convert(categoryDTOs);
        }

        public Task UpdateOrCreate(string departmentId, string? eventCategoryId, string name)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("eventCategoryId", eventCategoryId), ("name", name));
            return _httpClient.PostAsync(new Uri($"/api/EventCategory{query}", UriKind.Relative), null);
        }
    }
}
