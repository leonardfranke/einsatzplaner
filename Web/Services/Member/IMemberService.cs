namespace Web.Services
{
    public interface IMemberService
    {
        public Task<List<Models.Member>> GetAll(string departmentId);
        public Task<Models.Member> GetMember(string departmentId, string memberId);
        public Task<string> CreateDummyMember(string departmentId);
        public Task UpdateMember(string departmentId, string memberId, string? name, bool? isAdmin);
    }
}
