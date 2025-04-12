using Api.Manager;
using Microsoft.AspNetCore.Mvc;
using DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelperCategoryGroupController : ControllerBase
    {
        private IRequirementGroupManager _helperCategoryGroupManager;
        private IUpdatedTimeManager _updatedTimeManager;

        public HelperCategoryGroupController(IRequirementGroupManager helperCategoryGroupManager, IUpdatedTimeManager updatedTimeManager)
        {
            _helperCategoryGroupManager = helperCategoryGroupManager;
            _updatedTimeManager = updatedTimeManager;
        }

        [HttpGet]
        public Task<List<RequirementGroupDTO>> GetAll(string departmentId)
        {
            return _helperCategoryGroupManager.GetAllGroups(departmentId);
        }

        [HttpGet("Updated")]
        public Task<DateTime> GetUpdatedTime(string departmentId)
        {
            return _updatedTimeManager.GetHelperCategory(departmentId);
        }

        [HttpDelete]
        public Task Delete(string departmentId, string helperCategoryGroupId)
        {
            return _helperCategoryGroupManager.DeleteGroup(departmentId, helperCategoryGroupId);
        }

        [HttpPost]
        public Task UpdateOrCreate(string departmentId, string? helperCategoryGroupId, RequirementGroupDTO updateGroupDTO)
        {
            return _helperCategoryGroupManager.UpdateOrCreateGroup(departmentId, helperCategoryGroupId, updateGroupDTO.Requirements);
        }
    }
}
