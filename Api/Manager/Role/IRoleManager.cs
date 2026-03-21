using Api.Models;
using DTO;

namespace Api.Manager
{
    public interface IRoleManager
    {
        public IAsyncEnumerable<RoleDTO> GetAll(string departmentId);

        public Task<Role> GetRole(string departmentId, string roleId);

        public Task Delete(string departmentId, string roleId);

        public Task UpdateOrCreate(string departmentId, string? roleId, string? newName, int? newLockingPeriod, bool? newIsFree);

        public Task UpdateRoleMembers(string departmentId, string roleId, UpdateMembersListDTO updateMembersList);
    }
}
