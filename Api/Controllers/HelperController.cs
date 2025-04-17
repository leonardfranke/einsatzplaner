using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelperController : ControllerBase
    {
        private IHelperManager _helperManager;

        public HelperController(IHelperManager helperManager)
        {
            _helperManager = helperManager;
        }

        [HttpGet]
        public Task<List<HelperDTO>> GetAll(string departmentId, string? eventId)
        {
            if(!string.IsNullOrEmpty(eventId))
                return _helperManager.GetAll(departmentId, eventId);

            return _helperManager.GetAll(departmentId);
        }

        [HttpPost("SetIsHelping/{departmentId}/{eventId}/{helperId}/{memberId}")]
        public async Task SetIsHelping([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string helperId, [FromRoute] string memberId, [FromQuery] string isAvailableString)
        {
            var parsed = bool.TryParse(isAvailableString, out bool isAvailable);
            if (!parsed)
                return;
            await _helperManager.SetIsAvailable(departmentId, eventId, helperId, memberId, isAvailable);
        }

        [HttpPost("UpdateLockedMembers/{departmentId}/{eventId}/{helperId}")]
        public Task UpdateLockedMembers([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string helperId, [FromBody] UpdateMembersListDTO updateMembersList)
        {
            return _helperManager.UpdateLockedMembers(departmentId, eventId, helperId, updateMembersList);
        }


    }
}
