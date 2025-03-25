using Api.Models;
using DTO;

namespace Api.Manager
{
    public interface IDepartmentManager
    {
        public Task<List<DepartmentDTO>> GetAll();

        public Task<DepartmentDTO> GetById(string departmentId);

        public Task<DepartmentDTO> GetByUrl(string departmentUrl);

        public Task<bool> IsMemberInDepartment(string memberId, string departmentId);

        public Task RemoveMember(string departmentId, string memberId);

        public Task<bool> MembershipRequested(string departmentId, string userId);

        public Task<bool> AddRequest(string departmentId, string userId);

        public Task RemoveRequest(string departmentId, string requestId);

        public Task<List<MembershipRequestDTO>> MembershipRequests(string departmentId);

        public Task AnswerRequest(string departmentId, string requestId, bool accept);
    }
}
