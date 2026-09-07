using Api.Models;
using DTO;

namespace Api.Manager
{
    public interface IEventManager
    {
        public Task<List<Event>> GetAllEvents(string departmentId, DateTime fromDate, DateTime toDate);
        public Task<Event> GetEvent(string departmentId, string eventId);
        public Task<string> CreateEvent(UpdateEventDTO updateEventDTO);
        public Task UpdateEvent(UpdateEventDTO updateEventDTO);
        public Task DeleteEvent(string departmentId, string eventId);
        public Task CreateRequirement(UpdateRequirementDTO updateRequirementDTO);
        public Task UpdateRequirement(UpdateRequirementDTO updateRequirementDTO);
        public Task DeleteRequirement(string departmentId, string eventId, string roleId);
        public Task UpdateOrCreateQualificationRequirement(UpdateQualificationRequirementDTO updateQualificationRequirementDTO);
        public Task DeleteQualificationRequirement(string departmentId, string eventId, string roleId, string qualificationId);
        public IAsyncEnumerable<RequirementDTO> GetRequirements(string departmentId, string? eventId, string? roleId);
        public IAsyncEnumerable<RequirementDTO> GetEnteredMemberRequirements(string departmentId, string memberId);
        public Task SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isAvailable);
        public Task SetMembersEntering(string departmentId, string eventId, string roleId, List<string> memberIds, EnteringType? type);
        public Task SendHelperNotifications();
        public Task<IEnumerable<StatDTO>> GetStats(string departmentId, string roleId, DateTime fromDate, DateTime toDate);
    }
}
