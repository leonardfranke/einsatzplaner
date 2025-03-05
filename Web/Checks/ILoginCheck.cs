namespace Web.Checks
{
    public interface ILoginCheck
    {
        public Task<bool> CheckLogin(bool needDepartment = false, string departmentId = null);
    }
}
