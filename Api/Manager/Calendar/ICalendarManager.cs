namespace Api.Manager.Calendar
{
    public interface ICalendarManager
    {
        Task<string?> GetToken(string departmentId, string memberId);
        Task<(string, string)> GetIdsByToken(string token);
        Task<string> GenerateToken(string departmentId, string memberId);
        Task InvalidateToken(string departmentId, string memberId);
    }
}
