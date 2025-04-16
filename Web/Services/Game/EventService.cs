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

        public async Task<bool> UpdateOrCreate(string departmentId, string? eventId, string groupId, string? eventCategoryId, DateTime gameDate, Dictionary<string, Tuple<int, DateTime, List<string>>> helpers, bool removeMembers)
        {
            try
            {                
                var updateEventDTO = new UpdateEventDTO
                {
                    DepartmentId = departmentId,
                    EventId = eventId,
                    GroupId = groupId,
                    EventCategoryId = eventCategoryId,
                    Date = gameDate,
                    RemoveMembers = removeMembers
                };

                if (helpers != null)
                {
                    var updateHelpers = helpers.Select(helper =>
                    {
                        var roleId = helper.Key;
                        var value = helper.Value;
                        var requiredAmount = value.Item1;
                        var lockingTime = value.Item2;
                        var requiredGroups = value.Item3;
                        return new UpdateHelperDTO
                        {
                            RoleId = roleId,
                            RequiredAmount = requiredAmount,
                            LockingTime = lockingTime,
                            RequiredGroups = requiredGroups
                        };                        
                    });
                    updateEventDTO.Helpers = updateHelpers.ToList();
                }

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
