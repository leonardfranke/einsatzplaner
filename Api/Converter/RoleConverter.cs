using Api.Models;
using DTO;

namespace Api.Converter
{
    public class RoleConverter
    {
        public static List<RoleDTO> Convert(List<Role> roles)
        {
            return roles.Select(Convert).ToList();
        }

        public static RoleDTO Convert(Role role)
        {
            if (role == null)
                return null;
            return new RoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                LockingPeriod = role.LockingPeriod
            };
        }
    }
}
