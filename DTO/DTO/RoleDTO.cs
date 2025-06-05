namespace DTO
{
    public class RoleDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int LockingPeriod { get; set; }
        public bool IsFree { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
