using Api.Manager;
using Microsoft.AspNetCore.Mvc;
using DTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private IMemberManager _memberManager;

        public MemberController(IMemberManager memberManager)
        {
            _memberManager = memberManager;
        }

        [HttpGet("{departmentId}")]
        public Task<List<MemberDTO>> GetAll([FromRoute] string departmentId)
        {
            return _memberManager.GetAll(departmentId);
        }

        [HttpGet("{departmentId}/{memberId}")]
        public Task<MemberDTO> GetMember([FromRoute] string departmentId, [FromRoute] string memberId)
        {
            return _memberManager.GetMember(departmentId, memberId);
        }

        [HttpPatch]
        public Task UpdateMember([FromQuery] string departmentId, [FromQuery] string memberId, [FromBody] UpdateMemberDTO updateMemberDTO) 
        {
            return _memberManager.UpdateMember(departmentId, memberId, updateMemberDTO.GroupIds, updateMemberDTO.RoleIds, updateMemberDTO.IsAdmin);
        }
    }
}
