using Web.Models;

namespace Web.Checks
{
    public interface ILoginCheck
    {
        public Task<bool> CheckLogin(string departmentUrl, Department department = null, bool requiresAdminRole = false);
    }
}
