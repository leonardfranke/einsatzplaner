using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class GroupConverter
    {
        public static List<Group> Convert(List<GroupDTO>? groups)
        {
            if(groups == null)
                return new List<Group>();

            return groups.Select(Convert).ToList();
        }

        public static Group Convert(GroupDTO group)
        {
            if (group == null)
                return null;

            return new Group
            {
                Id = group.Id,
                Name = group.Name,
                MemberIds = group.MemberIds,
            };
        }
    }
}
