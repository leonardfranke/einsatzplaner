namespace Web.Services
{
    public interface IHelperService
    {
        public Task<List<Models.Helper>> GetAll(string departmentId, string? eventId = null);

        public Task<bool> SetIsHelping(string departmentId, string eventId, string helperCategoryId, string memberId, bool isHelping);
    }
}
