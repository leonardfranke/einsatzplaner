using DTO;
using Web.Models;

namespace Web.Converter
{
    public static class DepartmentConverter
    {
        public static List<Department> Convert(List<DepartmentDTO> departments)
        {
            return departments.Select(Convert).ToList();
        }

        public static Department Convert(DepartmentDTO? department)
        {
            if (department == null)
                return null;

            return new Department
            {
                Id = department.Id,
                Name = department.Name
            };
        }
    }
}
