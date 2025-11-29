using Api.Models;
using DTO;

namespace Api.Converter
{
    public class MemberConverter
    {
        public static List<MemberDTO?> Convert(List<Member?> members)
        {
            return members.Select(Convert).ToList();
        }

        public static MemberDTO? Convert(Member? member)
        {
            if (member == null)
                return null;
            return new MemberDTO
            {
                Id = member.Id,
                Name = member.Name,
                IsAdmin = member.IsAdmin,
                EmailNotificationActive = member.EmailNotificationActive,
                IsDummy = member.IsDummy
            };
        }
    }
}
