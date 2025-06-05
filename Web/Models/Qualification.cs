namespace Web.Models
{
    public class Qualification
    {
        public string Id { get; set; }
        public string RoleId { get; set; }
        public string Name { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
