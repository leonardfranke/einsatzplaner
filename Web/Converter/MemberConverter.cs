using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class MemberConverter
    {
        public static List<Member> Convert(List<MemberDTO> members)
        {
            return members.Select(Convert).ToList();
        }

        public static Member Convert(MemberDTO? member)
        {
            if (member == null)
                return null;

            return new Member
            {
                Id = member.Id,
                Name = member.Name,
                IsAdmin = member.IsAdmin
            };
        }
    }
}
