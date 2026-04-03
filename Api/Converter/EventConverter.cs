using DTO;
using Api.Models;

namespace Api.Converter
{
    public static class EventConverter
    {
        public static List<EventDTO> Convert(List<Event> events, string departmentId)
        {
            return events.Select(@event => Convert(@event, departmentId)).ToList();            
        }

        public static EventDTO Convert(Event @event, string departmentId)
        {
            if(@event == null)
                return null;

            var eventDTO = new EventDTO
            {
                Date = @event.Date,
                DepartmentId = departmentId,
                GroupId = @event.GroupId,
                EventCategoryId = @event.EventCategoryId,
                Id = @event.Id,
                Latitude = @event.LocationLatitude,
                Longitude = @event.LocationLongitude,
                LocationId = @event.LocationId,
                LocationText = @event.LocationText
            };
            return eventDTO;
        }
    }
}
