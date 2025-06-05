using DTO;

namespace Api.Manager
{
    public interface IMemberManager
    {
        public Task<List<MemberDTO>> GetAll(string departmentId);
        public Task<MemberDTO> GetMember(string departmentId, string memberId);
        public Task UpdateMember(string departmentId, string userId, bool isAdmin);
        public Task CreateMember(string departmentId, string userId, bool isAdmin = false);
        public Task<long?> MemberCount(string departmentId);
    }
}
