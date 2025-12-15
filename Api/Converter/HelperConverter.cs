using DTO;
using Api.Models;

namespace Api.Converter
{
    public static class HelperConverter
    {
        public static HelperDTO Convert(Requirement helper, string eventId)
        {
            return new HelperDTO
            {
                Id = helper.Id,
                RoleId = helper.RoleId,
                LockedMembers = helper.LockedMembers,
                PreselectedMembers = helper.PreselectedMembers,
                AvailableMembers = helper.AvailableMembers,
                RequiredAmount = helper.RequiredAmount,
                EventId = eventId,
                RequiredGroups = helper.RequiredGroups,
                LockingTime = helper.LockingTime,
                RequiredQualifications = helper.RequiredQualifications
            };
        }
        public static List<HelperDTO> Convert(List<Requirement> helpers, string eventId)
        {
            return helpers.Select(requirement => Convert(requirement, eventId)).ToList();            
        }
    }
}
