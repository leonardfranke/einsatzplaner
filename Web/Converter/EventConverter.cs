using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class EventConverter
    {
        public static List<Event> Convert(List<EventDTO> events)
        {
            return events.Select(Convert).ToList();
        }

        public static Event Convert(EventDTO? @event)
        {
            if (@event == null)
                return null;

            return new Event
            {
                EventDate = @event.Date.ToLocalTime(),
                DepartmentId = @event.DepartmentId,
                GroupId = @event.GroupId,
                EventCategoryId = @event.EventCategoryId,
                Id = @event.Id,
                Place = @event.Place
            };
        }
    }
}
