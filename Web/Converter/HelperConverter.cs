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
                    RoleId = helper.HelperCategoryId,
                    SetMemberIds = helper.SetMembers,
                    QueuedMemberIds = helper.QueuedMembers ?? new(),
                    EventId = helper.GameId,
                    RequiredGroups = helper.RequiredGroups,
                    LockingTime = helper.LockingTime,
                    RequiredAmount = helper.RequiredAmount
                };
            }).ToList();
        }
    }
}
