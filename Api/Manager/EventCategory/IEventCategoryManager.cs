using DTO;

namespace Api.Manager
{
    public interface IEventCategoryManager
    {
        public Task<List<EventCategoryDTO>> GetAll(string departmentId);

        public Task<EventCategoryDTO> GetById(string departmentId, string eventCategoryId);

        public Task Delete(string departmentId, string eventCategoryId);

        public Task UpdateOrCreate(string departmentId, string? eventCategoryId, string name);
    }
}
