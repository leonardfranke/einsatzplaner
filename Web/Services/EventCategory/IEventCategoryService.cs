using Web.Models;

namespace Web.Services
{
    public interface IEventCategoryService
    {
        public Task<List<EventCategory>> GetAll(string departmentId);
        public Task<EventCategory> GetById(string departmentId, string eventCategoryId);

        public Task UpdateOrCreate(string departmentId, string? eventCategoryId, string name);

        public Task Delete(string departmentId, string eventCategoryId);
    }
}
