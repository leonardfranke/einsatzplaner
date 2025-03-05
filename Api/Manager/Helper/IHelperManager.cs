using DTO;

namespace Api.Manager
{
    public interface IHelperManager
    {
        public Task<List<HelperDTO>> GetAll(string departmentId);
        public Task<List<HelperDTO>> GetAll(string departmentId, string gameId);
        public Task SetIsHelping(string departmentId, string gameId, string helperCategoryId, string memberId, bool isHelping);
    }
}
