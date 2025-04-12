using DTO;
using Api.Models;

namespace Api.Converter
{
    public static class HelperConverter
    {
        public static List<HelperDTO> Convert(List<Helper> helpers, string gameId)
        {
            return helpers.Select(helper =>
            {
                return new HelperDTO
                {
                    Id = helper.Id,
                    RoleId = helper.RoleId,
                    LockedMembers = helper.LockedMembers,
                    PreselectedMembers = helper.PreselectedMembers,
                    AvailableMembers = helper.AvailableMembers,
                    RequiredAmount = helper.RequiredAmount,
                    EventId = gameId,
                    RequiredGroups = helper.RequiredGroups,
                    LockingTime = helper.LockingTime
                };
            }).ToList();            
        }
    }
}
