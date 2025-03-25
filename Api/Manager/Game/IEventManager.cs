using DTO;

namespace Api.Manager
{
    public interface IEventManager
    {
        public Task<List<EventDTO>> GetAll(string departmentId);
        public Task<EventDTO> GetEvent(string departmentId, string eventId);
        public Task UpdateOrCreate(UpdateEventDTO updateEventDTO);
        public Task DeleteEvent(string departmentId, string eventId);
    }
}
