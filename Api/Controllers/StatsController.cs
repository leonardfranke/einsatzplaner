using Api.Manager;
using DTO;
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
        public Task<IEnumerable<StatDTO>> GetStats([FromRoute] string departmentId, [FromRoute] string roleId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            return _eventManager.GetStats(departmentId, roleId, fromDate, toDate);
        }
    }
}
