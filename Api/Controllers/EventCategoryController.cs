using Api.Manager;
using Api.Models;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventCategoryController : ControllerBase
    {
        private IEventCategoryManager _eventCategoryManager;

        public EventCategoryController(IEventCategoryManager eventCategoryManager)
        {
            _eventCategoryManager = eventCategoryManager;
        }

        [HttpGet("{departmentId}")]
        public Task<List<EventCategoryDTO>> GetAll([FromRoute] string departmentId)
        {
            return _eventCategoryManager.GetAll(departmentId);
        }

        [HttpGet("{departmentId}/{eventCategoryId}")]
        public Task<EventCategoryDTO> GetAll([FromRoute] string departmentId, [FromRoute] string eventCategoryId)
        {
            return _eventCategoryManager.GetById(departmentId, eventCategoryId);
        }

        [HttpDelete]
        public Task DeleteGroup(string departmentId, string eventCategoryId) 
        {
            return _eventCategoryManager.Delete(departmentId, eventCategoryId);
        }

        [HttpPost]
        public Task UpdateOrCreate([FromQuery] string departmentId, [FromQuery] string? eventCategoryId, [FromQuery] string name)
        {
            return _eventCategoryManager.UpdateOrCreate(departmentId, eventCategoryId, name);
        }
    }
}
