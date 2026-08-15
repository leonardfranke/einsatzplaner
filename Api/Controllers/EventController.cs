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
            return _eventManager.GetAllEvents(departmentId, DateTime.MinValue, DateTime.MaxValue);
        }

        [HttpGet("{departmentId}/{eventId}")]
        public Task<EventDTO> GetEvent(string departmentId, string eventId)
        {
            return _eventManager.GetEvent(departmentId, eventId);
        }

        [HttpPost()]
        public async Task<string> CreateOrUpdateEvent([FromBody] UpdateEventDTO updateEvent)
        {
            if(string.IsNullOrEmpty(updateEvent.EventId))
                return await _eventManager.CreateEvent(updateEvent);
            else
            {
                await _eventManager.UpdateEvent(updateEvent);
                return updateEvent.EventId;
            }
        }

        [HttpDelete("{departmentId}/{eventId}")]
        public Task DeleteEvent([FromRoute] string departmentId, [FromRoute] string eventId)
        {
            return _eventManager.DeleteEvent(departmentId, eventId);
        }
    }
}
