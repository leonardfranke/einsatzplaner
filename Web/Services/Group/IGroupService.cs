namespace Web.Services
{
    public interface IGroupService
    {
        public Task<List<Models.Group>> GetAll(string departmentId);
        public Task<Models.Group> GetById(string departmentId, string groupId);
        public Task DeleteGroup(string departmentId, string groupId);
        public Task<string> UpdateOrCreateGroup(string departmentId, string? groupId, string name);
        public Task UpdateGroupMembers(string departmentId, string groupId, List<string> newMembers, List<string> formerMembers);
    }
}
