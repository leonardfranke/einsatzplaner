namespace Web.Services.Stats
{
    public interface IStatsService
    {
        public Task<Dictionary<string, Tuple<int, int>>> GetStats(string departmentId, string roleId, DateTime fromDate, DateTime toDate);
    }
}
