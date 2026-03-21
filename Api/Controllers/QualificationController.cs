using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QualificationController : ControllerBase
    {
        private IQualificationManager _qualificationManager;

        public QualificationController(IQualificationManager qualificationManager)
        {
            _qualificationManager = qualificationManager;
        }

        [HttpGet("{departmentId}")]
        public IAsyncEnumerable<QualificationDTO> GetAll([FromRoute] string departmentId)
        {
            return _qualificationManager.GetAll(departmentId);
        }

        [HttpDelete]
        public Task DeleteQualification(string departmentId, string qualificationId)
        {
            return _qualificationManager.Delete(departmentId, qualificationId);
        }

        [HttpPost]
        public Task UpdateOrCreate([FromBody] UpdateQualificationDTO updateDTO)
        {
            return _qualificationManager.UpdateOrCreate(updateDTO.DepartmentId, updateDTO.RoleId, updateDTO.QualificationId, updateDTO.NewName);
        }

        [HttpPatch("{departmentId}/{roleId}/{qualificationId}")]
        public Task UpdateQualificationMembers([FromRoute] string departmentId, [FromRoute] string roleId, [FromRoute] string qualificationId, [FromBody] UpdateMembersListDTO updateMembersList)
        {
            return _qualificationManager.UpdateRoleMembers(departmentId, roleId, qualificationId, updateMembersList);
        }
    }
}
