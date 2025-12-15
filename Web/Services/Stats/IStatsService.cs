using DTO;

namespace Web.Services.Stats
{
    public interface IStatsService
    {
        public Task<List<StatDTO>> GetStats(string departmentId, string roleId, DateTime fromDate, DateTime toDate);
    }
}
