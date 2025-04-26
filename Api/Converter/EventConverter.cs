using DTO;
using Api.Models;
using Microsoft.CodeAnalysis.Elfie.Model;

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
            var eventDTO = new EventDTO
            {
                Date = @event.Date,
                DepartmentId = departmentId,
                GroupId = @event.GroupId,
                EventCategoryId = @event.EventCategoryId,
                Id = @event.Id
            };

            if (@event.Place.HasValue)
            {
                eventDTO.Place = new Geolocation()
                {
                    Latitude = @event.Place.Value.Latitude,
                    Longitude = @event.Place.Value.Longitude
                };
            }
            return eventDTO;
        }
    }
}
