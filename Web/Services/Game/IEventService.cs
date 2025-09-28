using DTO;

namespace Web.Services
{
    public interface IEventService
    {
        public Task<Models.Event?> GetEvent(string departmentId, string eventId);
        public Task<List<Models.Event>> GetAll(string departmentId);
        public Task<bool> UpdateOrCreate(UpdateEventDTO updateEventDTO);
        public Task DeleteGame(string departmentId, string gameId);
    }
}
