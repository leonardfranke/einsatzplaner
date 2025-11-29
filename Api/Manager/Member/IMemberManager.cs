using DTO;

namespace Api.Manager
{
    public interface IMemberManager
    {
        public Task<List<MemberDTO>> GetAll(string departmentId);
        public Task<MemberDTO> GetMember(string departmentId, string memberId);
        public Task UpdateMember(string departmentId, UpdateMemberDTO updateMemberDTO);
        public Task CreateMember(string departmentId, string? id, string name, bool isAdmin);
        public Task<string> CreateDummyMember(string departmentId);
        public Task<long?> MemberCount(string departmentId);
    }
}
