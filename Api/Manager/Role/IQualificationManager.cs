using DTO;

namespace Api.Manager
{
    public interface IQualificationManager
    {
        public Task<List<QualificationDTO>> GetAll(string departmentId);

        public Task Delete(string departmentId, string qualificationId);

        public Task<string> UpdateOrCreate(string departmentId, string? roleId, string? qualificationId, string? newName);

        public Task UpdateRoleMembers(string departmentId, string qualificationId, UpdateMembersListDTO updateMembersList);

        public Task RemoveMembersFromQualifications(string departmentId, string roleId, IEnumerable<string>? members);
    }
}
