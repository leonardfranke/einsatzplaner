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

        public RoleController(IRoleManager roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet("{departmentId}")]
        public Task<List<RoleDTO>> GetAll([FromRoute] string departmentId)
        {
            return _roleManager.GetAll(departmentId);
        }

        [HttpDelete]
        public Task DeleteRole(string departmentId, string roleId)
        {
            return _roleManager.Delete(departmentId, roleId);
        }

        [HttpPost]
        public Task<string> UpdateOrCreate([FromBody] UpdateRoleDTO updateDTO)
        {
            return _roleManager.UpdateOrCreate(updateDTO.DepartmentId, updateDTO.RoleId, updateDTO.NewName, updateDTO.NewLockingPeriod, updateDTO.NewIsFree);
        }

        [HttpPatch("{departmentId}/{roleId}")]
        public async Task UpdateRoleMembers([FromRoute] string departmentId, [FromRoute] string roleId, [FromBody] UpdateMembersListDTO updateMembersList)
        {
            await _roleManager.UpdateRoleMembers(departmentId, roleId, updateMembersList);
        }
    }
}
