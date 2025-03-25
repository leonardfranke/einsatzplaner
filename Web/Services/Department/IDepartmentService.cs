using Web.Models;

namespace Web.Services
{
    public interface IDepartmentService
    {
        public Task<List<Department>> GetAll();
        public Task<Department> GetById(string departmentId);
        public Task<Department> GetByUrl(string departmentUrl);
        public Task<bool> IsMemberInDepartment(string memberId, string departmentId);
        public Task<bool> RequestMembership(string departmentId, string userId);
        public Task<bool> MembershipRequested(string departmentId, string userId);
        public Task<List<MembershipRequest>> MembershipRequests(string departmentId);
        public Task AnswerRequest(string departmentId, string requestId, bool accept);
        public Task RemoveMember(string departmentId, string memberId);

    }
}
