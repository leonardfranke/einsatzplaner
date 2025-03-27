
using Microsoft.AspNetCore.Components;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Checks
{
    public class DepartmentUrlCheck : IDepartmentUrlCheck
    {
        private readonly IDepartmentService _departmentService;
        private readonly NavigationManager _navigationManager;
        private readonly IMemberService _memberService;
        private readonly IAuthManager _authManager;

        public DepartmentUrlCheck(IDepartmentService departmentService, NavigationManager navigationManager, IMemberService memberService, IAuthManager authManager)
        {
            _departmentService = departmentService;
            _navigationManager = navigationManager;
            _memberService = memberService;
            _authManager = authManager;
        }

        public async Task<Department> LogIntoDepartment(string departmentUrl)
        {
            var department = await _departmentService.GetByUrl(departmentUrl);
            if (department == null)
                _navigationManager.NavigateTo("./404");
            else
                await _authManager.SetCurrentDepartment(department.Id);

            return department;
        }
    }
}
