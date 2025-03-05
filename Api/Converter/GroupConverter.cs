using Api.Models;
using DTO;

namespace Api.Converter
{
    public class GroupConverter
    {
        public static List<GroupDTO> Convert(List<Group> groups)
        {
            return groups.Select(Convert).ToList();
        }

        public static GroupDTO Convert(Group group)
        {
            if (group == null)
                return null;
            return new GroupDTO
            {
                Id = group.Id,
                Name = group.Name
            };
        }
    }
}
