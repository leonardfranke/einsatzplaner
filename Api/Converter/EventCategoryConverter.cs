using Api.Models;
using DTO;

namespace Api.Converter
{
    public class EventCategoryConverter
    {
        public static List<EventCategoryDTO> Convert(List<EventCategory> eventCategories)
        {
            return eventCategories.Select(Convert).ToList();
        }

        public static EventCategoryDTO Convert(EventCategory eventCategory)
        {
            if (eventCategory == null)
                return null;

            return new EventCategoryDTO
            {
                Id = eventCategory.Id,
                Name = eventCategory.Name
            };
        }
    }
}
