using DTO;

namespace Web.Services
{
    public interface IMemberService
    {
        public Task<List<Models.Member>> GetAll(string departmentId);
        public Task<Models.Member> GetMember(string departmentId, string memberId);        
    }
}
