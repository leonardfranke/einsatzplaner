using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class RequirementGroupConverter
    {
        public static List<RequirementGroup> Convert(List<RequirementGroupDTO> categories)
        {
            return categories.Select(category =>
            {
                return new RequirementGroup
                {
                    Id = category.Id,
                    Requirements = category.Requirements
                };
            }).ToList();            
        }
    }
}
