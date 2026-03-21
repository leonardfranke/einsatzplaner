using Api.Models;
using DTO;

namespace Api.Converter
{
    public class RoleConverter
    {
        public static RoleDTO Convert(Role role, List<string> members)
        {
            if (role == null)
                return null;
            return new RoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                LockingPeriod = role.LockingPeriod,
                IsFree = role.IsFree,
                MemberIds = members
            };
        }
    }
}
