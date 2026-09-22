using DTO;

namespace Web.Services
{
    public interface ICalendarService
    {
        Task<CalendarTokenDTO?> GetCalendarToken(string departmentId, string memberId);
        Task<CalendarTokenDTO?> GenerateCalendarToken(string departmentId, string memberId);
        Task InvalidateCalendarToken(string departmentId, string memberId);
    }
}
