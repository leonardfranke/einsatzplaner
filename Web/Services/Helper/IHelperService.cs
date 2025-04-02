namespace Web.Services
{
    public interface IHelperService
    {
        public Task<List<Models.Helper>> GetAll(string departmentId, string? eventId = null);

        public Task<bool> SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isHelping);
    }
}
