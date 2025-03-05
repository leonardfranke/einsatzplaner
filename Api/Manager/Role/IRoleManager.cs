using DTO;

namespace Api.Manager
{
    public interface IRoleManager
    {
        public Task<List<RoleDTO>> GetAll(string departmentId);

        public Task Delete(string departmentId, string roleId);

        public Task<string> UpdateOrCreate(string departmentId, string? roleId, string name, int lockingPeriod);
    }
}
