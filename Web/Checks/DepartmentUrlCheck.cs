
using Microsoft.AspNetCore.Components;
using Web.Models;
using Web.Services;

namespace Web.Checks
{
    public class DepartmentUrlCheck : IDepartmentUrlCheck
    {
        private readonly IDepartmentService _departmentService;
        private readonly NavigationManager _navigationManager;

        public DepartmentUrlCheck(IDepartmentService departmentService, NavigationManager navigationManager)
        {
            _departmentService = departmentService;
            _navigationManager = navigationManager;
        }

        public async Task<Department> CheckDepartmentUrl(string departmentUrl)
        {
            var department = await _departmentService.GetByUrl(departmentUrl);
            if(department == null)
            {
                _navigationManager.NavigateTo("/404");
            }
            return department;
        }
    }
}
