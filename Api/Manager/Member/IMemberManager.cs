using DTO;

namespace Api.Manager
{
    public interface IMemberManager
    {
        public Task<List<MemberDTO>> GetAll(string departmentId);
        public Task<MemberDTO> GetMember(string departmentId, string memberId);
        public Task UpdateMember(string departmentId, string userId, List<string> groupIds, List<string> roleIds, bool isAdmin);
        public Task CreateMember(string departmentId, string userId, bool isAdmin = false);
        public Task AddGroupMembers(string departmentId, string groupId, List<string> members);
        public Task RemoveGroupMembers(string departmentId, string groupId, List<string> members);
        public Task RemoveAllGroupMembers(string departmentId, string groupId);
        public Task AddRoleMembers(string departmentId, string roleId, List<string> members);
        public Task RemoveRoleMembers(string departmentId, string roleId, List<string> members);
        public Task RemoveAllRoleMembers(string departmentId, string roleId);
        public Task<long?> MemberCount(string departmentId);
    }
}
