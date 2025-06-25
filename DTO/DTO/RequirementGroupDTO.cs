namespace DTO
{
    public class RequirementGroupDTO
    {
        public string Id { get; set; }

        public Dictionary<string, int> RequirementsRoles { get; set; }

        public Dictionary<string, int> RequirementsQualifications { get; set; }
    }
}
