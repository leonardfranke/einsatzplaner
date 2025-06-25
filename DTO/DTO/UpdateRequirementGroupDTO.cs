namespace DTO
{
    public class UpdateRequirementGroupDTO
    {
        public string? Id { get; set; }
        public Dictionary<string, int> NewRequirementsRole { get; set; }
        public IEnumerable<string> FormerRequirementsRole { get; set; }
        public Dictionary<string, int> NewRequirementsQualifications { get; set; }
        public IEnumerable<string> FormerRequirementsQualifications { get; set; }
    }
}
