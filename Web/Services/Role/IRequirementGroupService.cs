namespace Web.Services
{
    public interface IRequirementGroupService
    {
        public Task<List<Models.RequirementGroup>> GetAll(string departmentId);

        public Task UpdateOrCreate(string departmentId, string? requirementsGroupId, Dictionary<string, uint> requirements);

        public Task Delete(string departmentId, string requirementsGroupId);
    }
}
