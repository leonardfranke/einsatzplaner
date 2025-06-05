using Api.FirestoreModels;
using DTO;

namespace Api.Converter
{
    public class QualificationConverter
    {
        public static List<QualificationDTO> Convert(List<Qualification> qualifications)
        {
            return qualifications.Select(Convert).ToList();
        }

        public static QualificationDTO Convert(Qualification qualification)
        {
            if (qualification == null)
                return null;
            return new QualificationDTO
            {
                Id = qualification.Id,
                Name = qualification.Name,
                RoleId = qualification.RoleId,
                MemberIds = qualification.MemberIds
            };
        }
    }
}
