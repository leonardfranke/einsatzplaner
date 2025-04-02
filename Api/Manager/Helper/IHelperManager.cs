using DTO;

namespace Api.Manager
{
    public interface IHelperManager
    {
        public Task<List<HelperDTO>> GetAll(string departmentId);
        public Task<List<HelperDTO>> GetAll(string departmentId, string gameId);
        public Task SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isAvailable);
    }
}
