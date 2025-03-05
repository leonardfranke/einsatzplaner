using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class EventCategoryConverter
    {
        public static List<EventCategory> Convert(List<EventCategoryDTO> eventCategories)
        {
            return eventCategories.Select(Convert).ToList();
        }

        public static EventCategory Convert(EventCategoryDTO eventCategory)
        {
            return new EventCategory
            {
                Id = eventCategory.Id,
                Name = eventCategory.Name
            };
        }
    }
}
