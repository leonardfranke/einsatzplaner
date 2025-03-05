namespace DTO
{
    public class UpdateMemberDTO
    {
        public List<string> GroupIds { get; set; }
        public List<string> RoleIds { get; set; }
        public bool IsAdmin { get; set; }
    }
}
