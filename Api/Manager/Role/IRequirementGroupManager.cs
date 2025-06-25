using DTO;

namespace Api.Manager
{
    public interface IRequirementGroupManager
    {
        public Task<List<RequirementGroupDTO>> GetAllGroups(string departmentId);

        public Task UpdateOrCreateGroup(string departmentId, UpdateRequirementGroupDTO requirementGroupDTO);

        public Task DeleteGroup(string departmentId, string helperCategoryGroupId);
    }
}
