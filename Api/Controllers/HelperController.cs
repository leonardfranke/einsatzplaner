using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelperController : ControllerBase
    {
        private IEventManager _eventManager;

        public HelperController(IEventManager eventManager)
        {
            _eventManager = eventManager;
        }

        [HttpGet]
        public Task<List<HelperDTO>> GetAll(string departmentId, string? eventId)
        {
            if(!string.IsNullOrEmpty(eventId))
                return _eventManager.GetRequirementsOfEvent(departmentId, eventId);

            return _eventManager.GetAllRequirements(departmentId);
        }

        [HttpPost("SetIsHelping/{departmentId}/{eventId}/{helperId}/{memberId}")]
        public async Task SetIsHelping([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string helperId, [FromRoute] string memberId, [FromQuery] string isAvailableString)
        {
            var parsed = bool.TryParse(isAvailableString, out bool isAvailable);
            if (!parsed)
                return;
            await _eventManager.SetIsAvailable(departmentId, eventId, helperId, memberId, isAvailable);
        }

        [HttpPost("UpdateLockedMembers/{departmentId}/{eventId}/{helperId}")]
        public Task UpdateLockedMembers([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string helperId, [FromBody] UpdateMembersListDTO updateMembersList)
        {
            return _eventManager.UpdateLockedMembers(departmentId, eventId, helperId, updateMembersList);
        }


    }
}
