namespace DTO
{
    public class UpdateMembersListDTO
    {
        public IEnumerable<string> NewMembers { get; set; }
        public IEnumerable<string> FormerMembers { get; set; }
    }
}
