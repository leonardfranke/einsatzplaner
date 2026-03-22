using Api.Converter;
using Api.Models;
using DTO;
using Supabase;

namespace Api.Manager
{
    public class EventCategoryManager : IEventCategoryManager
    {
        private Client _supabaseClient;

        public EventCategoryManager(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public Task Delete(string departmentId, string eventCategoryId)
        {
            return _supabaseClient
                .From<EventCategory>()
                .Where(category => category.DepartmentId == departmentId && category.Id == eventCategoryId)
                .Limit(1)
                .Delete();
        }

        public async Task<List<EventCategoryDTO>> GetAll(string departmentId)
        {
            var res = await _supabaseClient.From<EventCategory>().Where(category => category.DepartmentId == departmentId).Get();
            return EventCategoryConverter.Convert(res.Models);
        }

        public Task UpdateOrCreate(string departmentId, string? eventCategoryId, string name)
        {
            if (string.IsNullOrEmpty(eventCategoryId))
            {
                var newCategory = new EventCategory
                {
                    Name = name,
                    DepartmentId = departmentId
                };

                return _supabaseClient.From<EventCategory>().Insert(newCategory);
            }
            else
            {
                return _supabaseClient
                    .From<EventCategory>()
                    .Where(category => category.Id == eventCategoryId && category.DepartmentId == departmentId)
                    .Limit(1)
                    .Set(category => category.Name, name)
                    .Update();
            }
        }

        public async Task<EventCategoryDTO> GetById(string departmentId, string eventCategoryId)
        {
            var category = await _supabaseClient.From<EventCategory>().Where(category => category.Id == eventCategoryId && category.DepartmentId == departmentId).Single();
            return EventCategoryConverter.Convert(category);
        }
    }
}
