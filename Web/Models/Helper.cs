namespace Web.Models
{
    public class Helper
    {
        public string Id { get; set; }

        public string EventId { get; set; }

        public string RoleId { get; set; }

        public DateTime LockingTime { get; set; }

        public int RequiredAmount { get; set; }

        public List<string> SetMemberIds { get; set; }

        public List<string> QueuedMemberIds { get; set; }

        public List<string> RequiredGroups { get; set; }
    }
}
