using DTO;
using System.Net.Http.Json;

namespace Web.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly HttpClient _httpClient;

        public CalendarService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public async Task<CalendarTokenDTO?> GetCalendarToken(string departmentId, string memberId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Calendar/token/{departmentId}/{memberId}", UriKind.Relative));
            return await response.Content.ReadFromJsonAsync<CalendarTokenDTO>();
        }

        public async Task<CalendarTokenDTO?> GenerateCalendarToken(string departmentId, string memberId)
        {
            var response = await _httpClient.PostAsync(new Uri($"/api/Calendar/token/{departmentId}/{memberId}", UriKind.Relative), null);
            return await response.Content.ReadFromJsonAsync<CalendarTokenDTO>();
        }

        public Task InvalidateCalendarToken(string departmentId, string memberId)
        {
            return _httpClient.DeleteAsync(new Uri($"/api/Calendar/token/{departmentId}/{memberId}", UriKind.Relative));
        }
    }
}
