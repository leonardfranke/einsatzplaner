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
                    HelperCategoryId = helper.HelperCategoryId,
                    SetMembers = helper.SetMembers,
                    QueuedMembers = helper.QueuedMembers,
                    RequiredAmount = helper.RequiredAmount,
                    GameId = gameId,
                    RequiredGroups = helper.RequiredGroups,
                    LockingTime = helper.LockingTime
                };
            }).ToList();            
        }
    }
}
