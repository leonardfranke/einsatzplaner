using Api.Models;
using DTO;

namespace Api.Converter
{
    public class GroupConverter
    {
        public static GroupDTO Convert(Group group, List<string> members)
        {
            if (group == null)
                return null;
            return new GroupDTO
            {
                Id = group.Id,
                Name = group.Name,
                MemberIds = members
            };
        }
    }
}
