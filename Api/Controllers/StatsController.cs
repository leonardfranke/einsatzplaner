using Api.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private IEventManager _eventManager;

        public StatsController(IEventManager eventManager) 
        {
            _eventManager = eventManager;
        }

        [HttpGet("{departmentId}/{roleId}")]
        public Task<Dictionary<string, int>> GetStats([FromRoute] string departmentId, [FromRoute] string roleId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            return _eventManager.GetStats(departmentId, roleId, fromDate, toDate);
        }
    }
}
