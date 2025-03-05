using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private IEventManager _eventManager;

        public EventController(IEventManager gameManager)
        {
            _eventManager = gameManager;
        }

        [HttpGet]
        public Task<List<EventDTO>> GetAll(string departmentId)
        {
            return _eventManager.GetAll(departmentId);
        }

        [HttpGet("{eventId}")]
        public Task<EventDTO> GetEvent(string eventId)
        {
            return _eventManager.GetEvent(eventId);
        }

        [HttpPost()]
        public Task CreateEvent([FromBody] UpdateEventDTO updateEvent)
        {
            return _eventManager.UpdateOrCreate(updateEvent);
        }

        [HttpDelete("{departmentId}/{eventId}")]
        public Task DeleteEvent([FromRoute] string departmentId, [FromRoute] string eventId)
        {
            return _eventManager.DeleteEvent(departmentId, eventId);
        }
    }
}
