namespace DTO
{
    public class UpdateHelperDTO
    {
        public string HelperCategoryId { get; set; }

        public int RequiredAmount { get; set; }

        public DateTime LockingTime { get; set; }

        public List<string> RequiredGroups { get; set; }
    }
}
