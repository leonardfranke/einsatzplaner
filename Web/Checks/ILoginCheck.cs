using Web.Models;

namespace Web.Checks
{
    public interface ILoginCheck
    {
        public Task<bool> CheckLogin(Department department = null);
    }
}
