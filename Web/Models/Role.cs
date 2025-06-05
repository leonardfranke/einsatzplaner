namespace Web.Models
{
    public class Role
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int LockingPeriod { get; set; }
        public bool IsFree { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
