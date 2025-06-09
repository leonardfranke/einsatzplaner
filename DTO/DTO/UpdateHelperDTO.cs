namespace DTO
{
    public class UpdateHelperDTO
    {
        public string RoleId { get; set; }

        public int RequiredAmount { get; set; }

        public DateTime LockingTime { get; set; }

        public List<string> RequiredGroups { get; set; }

        public Dictionary<string, int> RequiredQualifications { get; set; }
    }
}
