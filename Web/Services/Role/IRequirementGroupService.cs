namespace Web.Services
{
    public interface IRequirementGroupService
    {
        public Task<List<Models.RequirementGroup>> GetAll(string departmentId);

        public Task UpdateOrCreate(string departmentId, string requirementGroupId, Dictionary<string, int> newRoleRequirements, IEnumerable<string> formerRoleRequirements, Dictionary<string, int> newQualificationsRequirements, IEnumerable<string> formerQualificationsRequirements);

        public Task Delete(string departmentId, string requirementsGroupId);
    }
}
