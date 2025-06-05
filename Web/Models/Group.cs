namespace Web.Models
{
    public class Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
