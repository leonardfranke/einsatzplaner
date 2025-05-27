using Api.Manager;
using Microsoft.AspNetCore.Mvc;
using DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequirementGroupController : ControllerBase
    {
        private IRequirementGroupManager _helperCategoryGroupManager;

        public RequirementGroupController(IRequirementGroupManager helperCategoryGroupManager)
        {
            _helperCategoryGroupManager = helperCategoryGroupManager;
        }

        [HttpGet("{departmentId}")]
        public Task<List<RequirementGroupDTO>> GetAll([FromRoute] string departmentId)
        {
            return _helperCategoryGroupManager.GetAllGroups(departmentId);
        }

        [HttpDelete]
        public Task Delete(string departmentId, string requirementGroupId)
        {
            return _helperCategoryGroupManager.DeleteGroup(departmentId, requirementGroupId);
        }

        [HttpPost("{departmentId}")]
        public Task UpdateOrCreate([FromRoute] string departmentId, [FromBody] RequirementGroupDTO updateGroupDTO)
        {
            return _helperCategoryGroupManager.UpdateOrCreateGroup(departmentId, updateGroupDTO.Id, updateGroupDTO.Requirements);
        }
    }
}
