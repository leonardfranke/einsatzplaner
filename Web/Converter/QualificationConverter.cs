using DTO;
using Web.Models;

namespace Web.Converter
{
    public class QualificationConverter
    {
        public static List<Qualification> Convert(List<QualificationDTO> qualifications)
        {
            return qualifications.Select(Convert).ToList();
        }

        public static Qualification Convert(QualificationDTO qualification)
        {
            if (qualification == null)
                return null;
            return new Qualification
            {
                Id = qualification.Id,
                RoleId = qualification.RoleId,
                Name = qualification.Name,
                MemberIds = qualification.MemberIds
            };
        }
    }
}
