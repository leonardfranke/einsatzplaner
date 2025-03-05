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
        public async Task SetIsHelping([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string helperId, [FromRoute] string memberId, [FromQuery] string isHelpingString)
        {
            var parsed = bool.TryParse(isHelpingString, out bool isHelping);
            if (!parsed)
                return;
            await _helperManager.SetIsHelping(departmentId, eventId, helperId, memberId, isHelping);
        }


    }
}
