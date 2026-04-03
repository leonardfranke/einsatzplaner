namespace DTO
{
    public class UpdateRequirementDTO
    {
        public string DepartmentId { get; set; }

        public string EventId { get; set; }

        public string RoleId { get; set; }

        public int? RequiredAmount { get; set; }

        public DateTimeOffset? LockingTime { get; set; }

        public List<string>? RecommendedGroups { get; set; }
    }
}
