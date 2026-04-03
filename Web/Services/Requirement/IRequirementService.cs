using DTO;

namespace Web.Services
{
    public interface IRequirementService
    {
        public Task<List<Models.Requirement>> GetAll(string departmentId, string? eventId = null);

        public Task<bool> SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isHelping);

        public Task UpdateEnteringType(string departmentId, string eventId, string roleId, List<string> members, EnteringType? enteringType);

        public Task CreateRequirement(UpdateRequirementDTO updateRequirementDTO);

        public Task UpdateRequirement(UpdateRequirementDTO updateRequirementDTO);

        public Task DeleteRequirement(string departmentId, string eventId, string roleId);

        public Task CreateOrUpdateQualificationRequirement(UpdateQualificationRequirementDTO updateQualificationRequirementDTO);

        public Task DeleteQualificationRequirement(string departmentId, string eventId, string roleId, string qualificationId);
    }
}
