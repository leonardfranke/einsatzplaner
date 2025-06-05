namespace Web.Services
{
    public interface IRoleService
    {
        public Task<List<Models.Role>> GetAll(string departmentId);
        public Task<Models.Role> GetById(string departmentId, string roleId);
        public Task Delete(string departmentId, string roleId);
        public Task<string> UpdateOrCreate(string departmentId, string? roleId, string? newName, int? newLockingPeriod, bool? newIsFree);
        public Task UpdateRoleMembers(string departmentId, string roleId, IEnumerable<string> newMembers, IEnumerable<string> formerMembers);
    }
}
