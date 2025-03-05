using Api.Models;
using DTO;
namespace Api.Converter
{
    public class DepartmentConverter
    {
        public static List<DepartmentDTO?> Convert(List<Department?> departments)
        {
            return departments.Select(Convert).ToList();
        }

        public static DepartmentDTO? Convert(Department? department)
        {
            if (department == null)
                return null;
            return new DepartmentDTO
            {
                Id = department.Id,
                Name = department.Name
            };
        }
    }
}
