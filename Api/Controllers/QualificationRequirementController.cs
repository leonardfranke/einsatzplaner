using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QualificationRequirementController : ControllerBase
    {
        private IEventManager _eventManager;

        public QualificationRequirementController(IEventManager eventManager)
        {
            _eventManager = eventManager;
        }

        [HttpPost]
        public Task CreateOrUpdateQualificationRequirement([FromBody] UpdateQualificationRequirementDTO updateQualificationRequirement)
        {
            return _eventManager.UpdateOrCreateQualificationRequirement(updateQualificationRequirement);
        }


        [HttpDelete("{departmentId}/{eventId}/{roleId}/{qualificationId}")]
        public Task DeleteQualificationRequirement([FromRoute] string departmentId, [FromRoute] string eventId, [FromRoute] string roleId, [FromRoute] string qualificationId)
        {
            return _eventManager.DeleteQualificationRequirement(departmentId, eventId, roleId, qualificationId);
        }
    }
}
