
using Api.Manager;
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
        private IEventManager _eventManager;
        private ILocationManager _locationManager;
        private IGroupManager _groupManager;
        private IRoleManager _roleManager;

        public CalendarController(IEventManager eventManager, ILocationManager locationManager, IGroupManager groupManager, IRoleManager roleManager)
        {
            _eventManager = eventManager;
            _locationManager = locationManager;
            _groupManager = groupManager;
            _roleManager = roleManager;
        }

        [HttpGet("{departmentId}/{memberId}")]
        public async Task<IActionResult> GetCalendar([FromRoute] string departmentId, [FromRoute] string memberId)
        {
            var memberRequirements = await _eventManager.GetEnteredMemberRequirements(departmentId, memberId);
            var calendar = new Calendar();
            foreach(var requirement in memberRequirements)
            {
                var @event = await _eventManager.GetEvent(departmentId, requirement.EventId);
                var role = await _roleManager.GetRole(departmentId, requirement.RoleId);
                var group = await _groupManager.GetById(departmentId, @event.GroupId);
                var calendarEvent = new CalendarEvent
                {
                    Start = new CalDateTime(@event.Date),
                    End = new CalDateTime(@event.Date.AddHours(1.5)),
                    Summary = role.Name + " " + group.Name
                };
                if(!string.IsNullOrEmpty(@event.LocationId))
                {
                    var location = await _locationManager.GetById(departmentId, @event.LocationId);
                    calendarEvent.GeographicLocation = new GeographicLocation(location.Latitude, location.Longitude);
                }
                else if(@event.Latitude.HasValue && @event.Longitude.HasValue)
                {
                    calendarEvent.GeographicLocation = new GeographicLocation(@event.Latitude.Value, @event.Longitude.Value);
                    calendarEvent.Location = @event.LocationText;
                }
                calendar.Events.Add(calendarEvent);
            }

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);

            return File(System.Text.Encoding.UTF8.GetBytes(serializedCalendar), "text/calendar", "calendar.ics");
        }
    }
}
