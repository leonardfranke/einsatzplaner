using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private IRoleManager _roleManager;
        private IMemberManager _memberManager;

        public RoleController(IRoleManager roleManager, IMemberManager memberManager)
        {
            _roleManager = roleManager;
            _memberManager = memberManager;
        }

        [HttpGet("{departmentId}")]
        public Task<List<RoleDTO>> GetAll([FromRoute] string departmentId)
        {
            return _roleManager.GetAll(departmentId);
        }

        [HttpDelete]
        public Task DeleteRole(string departmentId, string roleId)
        {
            var roleTask = _roleManager.Delete(departmentId, roleId);
            var memberTask = _memberManager.RemoveAllRoleMembers(departmentId, roleId);
            return Task.WhenAll(roleTask, memberTask);
        }

        [HttpPost]
        public Task<string> UpdateOrCreate([FromBody] UpdateRoleDTO updateDTO)
        {
            return _roleManager.UpdateOrCreate(updateDTO.DepartmentId, updateDTO.RoleId, updateDTO.Name, updateDTO.LockingPeriod);
        }

        [HttpPatch("{departmentId}/{roleId}")]
        public Task UpdateRoleMembers([FromRoute] string departmentId, [FromRoute] string roleId, [FromBody] UpdateMembersListDTO updateMemberList)
        {
            var addTask = _memberManager.AddRoleMembers(departmentId, roleId, updateMemberList.NewMembers);
            var removeTask = _memberManager.RemoveRoleMembers(departmentId, roleId, updateMemberList.FormerMembers);
            return Task.WhenAll(addTask, removeTask);
        }
    }
}
