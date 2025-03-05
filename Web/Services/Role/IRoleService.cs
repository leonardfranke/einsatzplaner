namespace Web.Services
{
    public interface IRoleService
    {
        public Task<List<Models.Role>> GetAll(string departmentId);
        public Task<Models.Role> GetById(string departmentId, string roleId);
        public Task Delete(string departmentId, string roleId);
        public Task<string> UpdateOrCreate(string departmentId, string? roleId, string name, int lockingPeriod);
        public Task UpdateRoleMembers(string departmentId, string roleId, List<string> newMembers, List<string> formerMembers);
    }
}
