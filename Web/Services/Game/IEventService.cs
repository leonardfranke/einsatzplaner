namespace Web.Services
{
    public interface IEventService
    {
        public Task<Models.Event?> GetEvent(string eventId);
        public Task<List<Models.Event>> GetAll(string departmentId);
        public Task<bool> UpdateOrCreate(string departmentId, string? eventId, string? groupId, string? eventCategoryId, DateTime gameDate, Dictionary<string, Tuple<int, DateTime, List<string>>> helperCategories);
        public Task DeleteGame(string departmentId, string gameId);
    }
}
