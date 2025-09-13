using Api.FirestoreModels;
using DTO;

namespace Api.Manager
{
    public interface IEventManager
    {
        public Task<List<EventDTO>> GetAllEvents(string departmentId);
        public Task<EventDTO> GetEvent(string departmentId, string eventId);
        public Task UpdateOrCreateEvent(UpdateEventDTO updateEventDTO);
        public Task DeleteEvent(string departmentId, string eventId);
        public Task<List<HelperDTO>> GetAllRequirements(string departmentId);
        public Task<List<HelperDTO>> GetRequirementsOfEvent(string departmentId, string eventId);
        public Task SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isAvailable);
        public Task UpdateLockedMembers(string departmentId, string eventId, string helperId, UpdateMembersListDTO updateMembersList);
        public Task UpdateChangedStatus(string departmentId, string eventId, string roleId, IEnumerable<string> memberIds, HelperStatus previousStatus, HelperStatus newStatus);
        public Task SendHelperNotifications();
    }
}
