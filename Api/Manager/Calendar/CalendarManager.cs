using Api.Models;
using Supabase;

namespace Api.Manager.Calendar
{
    public class CalendarManager : ICalendarManager
    {
        private readonly Client _supabaseClient;

        public CalendarManager(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<string?> GetToken(string departmentId, string memberId)
        {
            var tokenResult = await _supabaseClient
                .From<CalendarToken>()
                .Where(token => token.DepartmentId == departmentId && token.MemberId == memberId)
                .Limit(1)
                .Get();

            return tokenResult.Models.FirstOrDefault()?.Token;
        }

        public async Task<(string, string)> GetIdsByToken(string token)
        {
            var tokenResult = await _supabaseClient
                .From<CalendarToken>()
                .Where(calendarToken => calendarToken.Token == token)
                .Limit(1)
                .Get();

            var tokenModel = tokenResult.Models.FirstOrDefault();
            if (tokenModel == null)
                return (string.Empty, string.Empty);

            return (tokenModel.DepartmentId, tokenModel.MemberId);
        }

        public async Task<string> GenerateToken(string departmentId, string memberId)
        {
            var newToken = Guid.NewGuid().ToString("N");

            var response = await _supabaseClient
                    .From<CalendarToken>()
                .Upsert(new CalendarToken
                    {
                        DepartmentId = departmentId,
                        MemberId = memberId,
                        Token = newToken
                    });

            return response.Model!.Token;
        }

        public Task InvalidateToken(string departmentId, string memberId)
        {
            return _supabaseClient
                .From<CalendarToken>()
                .Where(token => token.DepartmentId == departmentId && token.MemberId == memberId)
                .Limit(1)
                .Delete();
        }
    }
}
