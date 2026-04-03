using DTO;
using Api.Models;

namespace Api.Converter
{
    public static class RequirementConverter
    {
        public static RequirementDTO Convert(Requirement requirement, List<string> lockedMembers, List<string> preselectedMembers, List<string> availableMembers, List<string> fillMembers, Dictionary<string, int> qualificationRequirements)
        {
            return new RequirementDTO
            {
                DepartmentId = requirement.DepartmentId,
                EventId = requirement.EventId,
                RoleId = requirement.RoleId,
                LockingTime = requirement.LockingTime,
                RequiredAmount = requirement.RequiredAmount,
                LockedMembers = lockedMembers,
                PreselectedMembers = preselectedMembers,
                AvailableMembers = availableMembers,
                FillMembers = fillMembers,
                RequiredGroups = requirement.RecommendedGroups,
                RequiredQualifications = qualificationRequirements
            };
        }
    }
}
