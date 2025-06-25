using Api.Models;
using DTO;

namespace Api.Converter
{
    public static class RequirementGroupConverter
    {
        public static List<RequirementGroupDTO> Convert(List<RequirementGroup> categories)
        {
            return categories.Select(category =>
            {
                return new RequirementGroupDTO
                {
                    Id = category.Id,
                    RequirementsQualifications = category.RequirementsQualifications,
                    RequirementsRoles = category.RequirementsRoles,
                };
            }).ToList();            
        }
    }
}
