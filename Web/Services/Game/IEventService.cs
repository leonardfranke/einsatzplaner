using DTO;

namespace Web.Services
{
    public interface IEventService
    {
        public Task<Models.Event?> GetEvent(string departmentId, string eventId);
        public Task<List<Models.Event>> GetAll(string departmentId);
        public Task<bool> UpdateOrCreate(string departmentId, string? eventId, string? groupId, string? eventCategoryId, DateTime gameDate, Geolocation? place, Dictionary<string, Tuple<int, DateTime, List<string>, Dictionary<string, int>>> helperCategories, bool removeMembers);
        public Task DeleteGame(string departmentId, string gameId);
    }
}
