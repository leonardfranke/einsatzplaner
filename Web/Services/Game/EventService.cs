using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;

namespace Web.Services
{
    public class EventService : IEventService
    {

        private HttpClient _httpClient;

        public EventService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public async Task<bool> UpdateOrCreate(UpdateEventDTO updateEventDTO)
        {            
            try
            {
                var content = JsonContent.Create(updateEventDTO);
                var response = await _httpClient.PostAsync(new Uri($"/api/Event", UriKind.Relative), content);
                var ret = await response.Content.ReadAsStringAsync();
                var parsed = bool.TryParse(ret, out bool result);
                if (parsed)
                    return result;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public Task DeleteGame(string departmentId, string gameId)
        {
            return _httpClient.DeleteAsync(new Uri($"/api/Event/{departmentId}/{gameId}", UriKind.Relative));
        }

        public async Task<List<Models.Event>> GetAll(string departmentId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId));
            var response = await _httpClient.GetAsync(new Uri($"/api/Event{query}", UriKind.Relative));
            var gameDTOs = await response.Content.ReadFromJsonAsync<List<EventDTO>>();
            return EventConverter.Convert(gameDTOs);
        }

        public async Task<Models.Event?> GetEvent(string departmentId, string gameId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Event/{departmentId}/{gameId}", UriKind.Relative));
            try
            {
                var gameDTO = await response.Content.ReadFromJsonAsync<EventDTO>();
                return EventConverter.Convert(gameDTO);
            }
            catch
            {
                return null;
            }            
        }
    }
}
