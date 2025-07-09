using Microsoft.AspNetCore.Components;
using Web.Checks;

namespace Web.Pages.GroupRole
{
    public class GroupRolePageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { private get; set; }

        [Inject]
        public IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        public ILoginCheck _loginCheck { get; set; }

        public Models.Department Department { get; private set; }

        public bool IsPageLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsPageLoading = true;
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department, true))
                return;
            Department = department;
            IsPageLoading = false;
        }
    }
}
