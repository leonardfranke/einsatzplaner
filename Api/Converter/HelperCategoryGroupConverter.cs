using Api.Models;
using DTO;

namespace Api.Converter
{
    public static class HelperCategoryGroupConverter
    {
        public static List<RequirementGroupDTO> Convert(List<HelperCategoryGroup> categories)
        {
            return categories.Select(category =>
            {
                return new RequirementGroupDTO
                {
                    Id = category.Id,
                    Requirements = category.Requirements
                };
            }).ToList();            
        }
    }
}
