namespace Web.Models
{
    public class Member
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> GroupIds { get; set; }
        public List<string> RoleIds { get; set; }
        public bool IsAdmin { get; set; }
    }
}
