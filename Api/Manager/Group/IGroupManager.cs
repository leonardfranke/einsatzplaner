using Api.Models;
using DTO;

namespace Api.Manager
{
    public interface IGroupManager
    {
        public IAsyncEnumerable<GroupDTO> GetAll(string departmentId);

        public Task<Group> GetById(string departmentId, string groupId);

        public Task Delete(string departmentId, string groupId);

        public Task UpdateOrCreate(string departmentId, string? groupId, string name);

        public Task UpdateGroupMembers(string departmentId, string groupId, UpdateMembersListDTO updateMembersList);
    }
}
