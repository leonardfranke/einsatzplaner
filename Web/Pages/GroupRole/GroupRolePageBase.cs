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

        public Models.Department Department { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            Department = department;
        }
    }
}
