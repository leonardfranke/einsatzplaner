using DTO;

namespace Api.Manager
{
    public interface IQualificationManager
    {
        public IAsyncEnumerable<QualificationDTO> GetAll(string departmentId);

        public Task Delete(string departmentId, string qualificationId);

        public Task UpdateOrCreate(string departmentId, string? roleId, string? qualificationId, string? newName);

        public Task UpdateRoleMembers(string departmentId, string roleId, string qualificationId, UpdateMembersListDTO updateMembersList);

        public Task RemoveMembersFromQualifications(string departmentId, string roleId, IEnumerable<string>? members);
    }
}
