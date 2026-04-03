using DTO;

namespace Web.Converter
{
    public static class HelperConverter
    {
        public static List<Models.Requirement> Convert(List<RequirementDTO> requirements)
        {
            return requirements.Select(requirement =>
            {
                return new Models.Requirement
                {
                    DepartmentId = requirement.DepartmentId,
                    RoleId = requirement.RoleId,
                    LockedMembers = requirement.LockedMembers,
                    PreselectedMembers = requirement.PreselectedMembers,
                    AvailableMembers = requirement.AvailableMembers,
                    FillMembers = requirement.FillMembers,
                    EventId = requirement.EventId,
                    RequiredGroups = requirement.RequiredGroups,
                    LockingTime = requirement.LockingTime,
                    RequiredAmount = requirement.RequiredAmount,
                    RequiredQualifications = requirement.RequiredQualifications
                };
            }).ToList();
        }
    }
}
