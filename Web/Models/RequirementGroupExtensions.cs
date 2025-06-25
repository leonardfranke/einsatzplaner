namespace Web.Models
{
    public static class RequirementGroupExtensions
    {
        public static string GetRequirementGroupDisplayText(this RequirementGroup requirementGroup, IEnumerable<Role> roles, IEnumerable<Qualification> qualifications)
        {
            var groupedQualificationRequírements = requirementGroup.RequirementsQualifications
                .ToDictionary(pair => qualifications.First(qualification => qualification.Id == pair.Key), pair => pair.Value)
                .GroupBy(pair => pair.Key.RoleId);
            var names = requirementGroup.RequirementsRoles.Select(requirementPair =>
            {
                var category = roles.First(category => category.Id == requirementPair.Key);
                var roleString = $"{requirementPair.Value}x {category.Name}";

                var qualificationRequirementsOfRole = groupedQualificationRequírements.FirstOrDefault(group => group.Key == requirementPair.Key);
                if (qualificationRequirementsOfRole != null)
                {
                    var qualificationStrings = qualificationRequirementsOfRole.Select(qualificationPair => $"{qualificationPair.Value}x {qualificationPair.Key.Name}");
                    roleString += $" ({string.Join(", ", qualificationStrings)})";
                }
                return roleString;
            });
            return string.Join(Environment.NewLine, names);
        }
    }
}
