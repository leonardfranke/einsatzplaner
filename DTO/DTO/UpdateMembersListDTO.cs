namespace DTO
{
    public class UpdateMembersListDTO
    {
        public IEnumerable<string> FormerMembers { get; set; }
        public IEnumerable<string> NewMembers { get; set; }
    }
}
