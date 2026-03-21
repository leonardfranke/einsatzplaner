using Api.Models;
using DTO;

namespace Api.Converter
{
    public class QualificationConverter
    {
        public static QualificationDTO Convert(Qualification qualification, List<string> members)
        {
            if (qualification == null)
                return null;
            return new QualificationDTO
            {
                Id = qualification.Id,
                Name = qualification.Name,
                RoleId = qualification.RoleId,
                MemberIds = members
            };
        }
    }
}
