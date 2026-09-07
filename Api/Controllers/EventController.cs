using Api.Converter;
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
        public async Task<List<EventDTO>> GetAll(string departmentId)
        {
            var events = await _eventManager.GetAllEvents(departmentId, DateTime.MinValue, DateTime.MaxValue);
            return EventConverter.Convert(events, departmentId);
        }

        [HttpGet("{departmentId}/{eventId}")]
        public async Task<EventDTO> GetEvent(string departmentId, string eventId)
        {
            var @event = await _eventManager.GetEvent(departmentId, eventId);
            return EventConverter.Convert(@event, departmentId);
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
