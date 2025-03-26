using Web.Models;

namespace Web.Checks
{
    public interface IDepartmentUrlCheck
    {
        public Task<Department> LogIntoDepartment(string departmentId);
    }
}
