using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequirementController : ControllerBase
    {
        private IEventManager _eventManager;

        public RequirementController(IEventManager eventManager)
        {
            _eventManager = eventManager;
        }

        [HttpGet]
        public IAsyncEnumerable<RequirementDTO> GetAll(string departmentId, string? eventId)
        {
            return _eventManager.GetRequirements(departmentId, eventId, null);
        }

        [HttpPost]
        public Task CreateRequirement([FromBody] UpdateRequirementDTO updateRequirement)
        {
            return _eventManager.CreateRequirement(updateRequirement);
        }

        [HttpPatch]
        public Task UpdateRequirement([FromBody] UpdateRequirementDTO updateRequirement)
        {
            return _eventManager.UpdateRequirement(updateRequirement);
        }


        [HttpDelete("{departmentId}/{eventId}/{roleId}")]
        public Task DeleteRequirement([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string roleId)
        {
            return _eventManager.DeleteRequirement(departmentId, eventId, roleId);
        }

        [HttpPost("SetIsHelping/{departmentId}/{eventId}/{roleId}/{memberId}")]
        public async Task SetIsHelping([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string roleId, [FromRoute] string memberId, [FromQuery] string isAvailableString)
        {
            var parsed = bool.TryParse(isAvailableString, out bool isAvailable);
            if (!parsed)
                return;
            await _eventManager.SetIsAvailable(departmentId, eventId, roleId, memberId, isAvailable);
        }

        [HttpPost("UpdateEnteringType/{departmentId}/{eventId}/{roleId}")]
        public Task UpdateEnteringType([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string roleId, [FromBody] UpdateEnteringsDTO updateEnteringsDTO)
        {
            return _eventManager.SetMembersEntering(departmentId, eventId, roleId, updateEnteringsDTO.Members.ToList(), updateEnteringsDTO.EnteringType);
        }


    }
}
