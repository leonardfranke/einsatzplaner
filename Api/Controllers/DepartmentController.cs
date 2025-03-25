using Api.Manager;
using Api.Models;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private IDepartmentManager _departmentManager;

        public DepartmentController(IDepartmentManager departmentManager)
        {
            _departmentManager = departmentManager;
        }

        [HttpGet]
        public Task<List<DepartmentDTO>> GetAll()
        {
            return _departmentManager.GetAll();
        }

        [HttpGet("Id/{departmentId}")]
        public Task<DepartmentDTO> GetById(string departmentId) 
        {
            return _departmentManager.GetById(departmentId);
        }

        [HttpGet("Url/{departmentUrl}")]
        public Task<DepartmentDTO> GetByUrl(string departmentUrl)
        {
            return _departmentManager.GetByUrl(departmentUrl);
        }

        [HttpGet("IsMemberInDepartment")]
        public Task<bool> IsMemberInDepartment(string memberId, string departmentId)
        {
            return _departmentManager.IsMemberInDepartment(memberId, departmentId);
        }

        [HttpDelete]
        public Task RemoveMember(string departmentId, string memberId)
        {
            return _departmentManager.RemoveMember(departmentId, memberId);
        }

        [HttpGet("RequestMembership")]
        public Task<bool> RequestMembership(string departmentId, string userId)
        {
            return _departmentManager.AddRequest(departmentId, userId);
        }

        [HttpGet("MembershipRequested")]
        public Task<bool> MembershipRequested(string departmentId, string userId)
        {
            return _departmentManager.MembershipRequested(departmentId, userId);
        }

        [HttpGet("MembershipRequests")]
        public Task<List<MembershipRequestDTO>> MembershipRequests(string departmentId)
        {
            return _departmentManager.MembershipRequests(departmentId);
        }

        [HttpGet("AnswerRequest")]
        public Task AnswerRequest(string departmentId, string requestId, string acceptString)
        {
            var parsed = bool.TryParse(acceptString, out bool accept);
            if (!parsed)
                return Task.CompletedTask;

            return _departmentManager.AnswerRequest(departmentId, requestId, accept);
        }
    }
}
