using Api.Manager;
using Api.Models;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private IGroupManager _groupManager;
        private IMemberManager _memberManager;

        public GroupController(IGroupManager groupManager, IMemberManager memberManager)
        {
            _groupManager = groupManager;
            _memberManager = memberManager;
        }

        [HttpGet("{departmentId}")]
        public Task<List<GroupDTO>> GetAll([FromRoute] string departmentId)
        {
            return _groupManager.GetAll(departmentId);
        }

        [HttpDelete]
        public Task DeleteGroup(string departmentId, string groupId) 
        {
            return _groupManager.Delete(departmentId, groupId);
        }

        [HttpPost]
        public Task<string> UpdateOrCreate([FromQuery] string departmentId, [FromQuery] string? groupId, [FromQuery] string name)
        {
            return _groupManager.UpdateOrCreate(departmentId, groupId, name);
        }

        [HttpPatch("{departmentId}/{groupId}")]
        public Task UpdateGroupMembers([FromRoute] string departmentId, [FromRoute] string groupId, [FromBody] UpdateMembersListDTO updateMembersList)
        {
            return _groupManager.UpdateGroupMembers(departmentId, groupId, updateMembersList);
        }
    }
}
