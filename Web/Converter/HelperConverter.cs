using DTO;

namespace Web.Converter
{
    public static class HelperConverter
    {
        public static List<Models.Helper> Convert(List<HelperDTO> helpers)
        {
            return helpers.Select(helper =>
            {
                return new Models.Helper
                {
                    Id = helper.Id,
                    RoleId = helper.RoleId,
                    LockedMembers = helper.LockedMembers,
                    PreselectedMembers = helper.PreselectedMembers,
                    AvailableMembers = helper.AvailableMembers,
                    EventId = helper.EventId,
                    RequiredGroups = helper.RequiredGroups,
                    LockingTime = helper.LockingTime,
                    RequiredAmount = helper.RequiredAmount
                };
            }).ToList();
        }
    }
}
