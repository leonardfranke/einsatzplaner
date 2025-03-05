using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class RoleConverter
    {
        public static List<Role> Convert(List<RoleDTO> roles)
        {
            return roles.Select(Convert).ToList();          
        }

        public static Role Convert(RoleDTO role)
        {            
            return new Role
            {
                Id = role.Id,
                Name = role.Name,
                LockingPeriod = role.LockingPeriod
            };
        }
    }
}
