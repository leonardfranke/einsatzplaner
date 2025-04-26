namespace DTO
{
    public class HelperDTO
    {
        public string Id { get; set; }

        public string RoleId { get; set; }

        public DateTime LockingTime { get; set; }

        public int RequiredAmount { get; set; }

        public string EventId { get; set; }

        public List<string> LockedMembers { get; set; }

        public List<string> PreselectedMembers { get; set; }

        public List<string> AvailableMembers { get; set; }
        
        public List<string> RequiredGroups { get; set; }
    }
}
