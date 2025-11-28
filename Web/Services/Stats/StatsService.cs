using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace Web.Services.Stats
{
    public class StatsService : IStatsService
    {
        private IFlurlClient _httpClient;

        public StatsService(IFlurlClientCache clientCache)
        {
            _httpClient = clientCache.Get("BACKEND");
        }

        public Task<Dictionary<string, Tuple<int, int>>> GetStats(string departmentId, string roleId, DateTime fromDate, DateTime toDate)
        {            
            return _httpClient
                .Request("/api/Stats/", departmentId, roleId)
                .AppendQueryParam("fromDate", fromDate)
                .AppendQueryParam("toDate", toDate)
                .GetJsonAsync<Dictionary<string, Tuple<int, int>>>();
        }
    }
}
