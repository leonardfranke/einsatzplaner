namespace Web.Models
{
    public class Helper
    {
        public string Id { get; set; }

        public string EventId { get; set; }

        public string RoleId { get; set; }

        public DateTime LockingTime { get; set; }

        public int RequiredAmount { get; set; }

        public List<string> LockedMembers { get; set; }

        public List<string> PreselectedMembers { get; set; }

        public List<string> AvailableMembers { get; set; }

        public List<string> FillMembers { get; set; }

        public List<string> RequiredGroups { get; set; }

        public Dictionary<string, int> RequiredQualifications { get; set; }
    }
}
