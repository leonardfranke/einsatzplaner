using Api.Manager;
using Api.Manager.Calendar;
using DTO;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IEventManager _eventManager;
        private readonly ILocationManager _locationManager;
        private readonly IGroupManager _groupManager;
        private readonly IRoleManager _roleManager;
        private readonly IDepartmentManager _departmentManager;
        private readonly ICalendarManager _calendarManager;

        public CalendarController(
            IDepartmentManager departmentManager,
            IEventManager eventManager,
            ILocationManager locationManager,
            IGroupManager groupManager,
            IRoleManager roleManager,
            ICalendarManager calendarManager)
        {
            _eventManager = eventManager;
            _locationManager = locationManager;
            _groupManager = groupManager;
            _roleManager = roleManager;
            _departmentManager = departmentManager;
            _calendarManager = calendarManager;
        }

        [HttpGet("token/{departmentId}/{memberId}")]
        public async Task<CalendarTokenDTO> GetToken([FromRoute] string departmentId, [FromRoute] string memberId)
        {
            var token = await _calendarManager.GetToken(departmentId, memberId);
            return BuildTokenDTO(departmentId, token);
        }

        [HttpPost("token/{departmentId}/{memberId}")]
        public async Task<CalendarTokenDTO> GenerateToken([FromRoute] string departmentId, [FromRoute] string memberId)
        {
            var token = await _calendarManager.GenerateToken(departmentId, memberId);
            return BuildTokenDTO(departmentId, token);
        }

        private CalendarTokenDTO BuildTokenDTO(string departmentId, string? token)
        {
            if (string.IsNullOrEmpty(token))
                return new CalendarTokenDTO();

            return new CalendarTokenDTO
            {
                Url = $"{Request.Scheme}://{Request.Host}/api/Calendar/{token}"
            };
        }

        [HttpDelete("token/{departmentId}/{memberId}")]
        public Task InvalidateToken([FromRoute] string departmentId, [FromRoute] string memberId)
        {
            return _calendarManager.InvalidateToken(departmentId, memberId);
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetCalendar([FromRoute] string token)
        {
            var (departmentId, memberId) = await _calendarManager.GetIdsByToken(token);
            if (string.IsNullOrEmpty(departmentId) || string.IsNullOrEmpty(memberId))
                return NotFound();

            return await BuildCalendarFile(departmentId, memberId);
        }

        private async Task<IActionResult> BuildCalendarFile(string departmentId, string memberId)
        {
            var department = await _departmentManager.GetById(departmentId);
            var memberRequirements = _eventManager.GetEnteredMemberRequirements(departmentId, memberId);
            var calendar = new Calendar();
            await foreach (var requirement in memberRequirements)
            {
                var @event = await _eventManager.GetEvent(departmentId, requirement.EventId);
                var role = await _roleManager.GetRole(departmentId, requirement.RoleId);
                var group = await _groupManager.GetById(departmentId, @event.GroupId);
                var calendarEvent = new CalendarEvent
                {
                    Start = new CalDateTime(@event.Date.UtcDateTime),
                    End = new CalDateTime(@event.Date.UtcDateTime.AddHours(1.5)),
                    Summary = role.Name + " " + group?.Name,
                    Uid = @event.Id
                };
                if (!string.IsNullOrEmpty(@event.LocationId))
                {
                    var location = await _locationManager.GetById(departmentId, @event.LocationId);
                    calendarEvent.GeographicLocation = new GeographicLocation(location.Latitude, location.Longitude);
                    calendarEvent.Location = location.Name;
                }
                else if (@event.LocationLatitude.HasValue && @event.LocationLongitude.HasValue)
                {
                    calendarEvent.GeographicLocation = new GeographicLocation(@event.LocationLatitude.Value, @event.LocationLongitude.Value);
                    calendarEvent.Location = @event.LocationText;
                }
                calendarEvent.Categories = [role.Name];
                var eventUri = new Uri($"https://einsatzplaner.net/{department.URL}/event/{@event.Id}");
                calendarEvent.Url = eventUri;
                calendarEvent.Description = eventUri.ToString();
                calendar.Events.Add(calendarEvent);
            }

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);

            return File(System.Text.Encoding.UTF8.GetBytes(serializedCalendar), "text/calendar", "calendar.ics");
        }
    }
}
