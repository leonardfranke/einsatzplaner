namespace DTO
{
    public class HelperDTO
    {
        public string Id { get; set; }

        public string HelperCategoryId { get; set; }

        public DateTime LockingTime { get; set; }

        public int RequiredAmount { get; set; }

        public string GameId { get; set; }

        public List<string> SetMembers { get; set; }

        public List<string> QueuedMembers { get; set; }
        
        public List<string> RequiredGroups { get; set; }
    }
}
