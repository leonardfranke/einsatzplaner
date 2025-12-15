using DTO;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Checks;
using Web.Services;
using Web.Services.Stats;

namespace Web.Pages
{
    public class StatsPageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { get; set; }

        [Inject]
        public IStatsService _statsService { get; set; }

        [Inject]
        public IMemberService _memberService { get; set; }

        [Inject]
        public IRoleService _roleService { get; set; }

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        public List<Models.Member> Members { get; set; }

        public List<Models.Role> Roles { get; set; } = new();

        public string SelectedRoleId { get; set; } = "";

        public DateRange DateRange { get; set; }

        public Dictionary<string, List<StatDTO>> Stats { get; set; } = new();

        public bool IsLoading { get; set; }

        private string _departmentId;

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department, true))
                return;

            var taskMembers = _memberService.GetAll(_departmentId);
            var taskRoles = _roleService.GetAll(_departmentId);

            DateRange = new DateRange(DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1));

            Roles = await taskRoles;
            Members = await taskMembers;
        }

        public Models.Member GetMemberById(string id)
        {
            return Members.FirstOrDefault(member => member.Id == id);
        }

        public async Task LoadStats()
        {
            if (string.IsNullOrEmpty(SelectedRoleId))
                return;

            IsLoading = true;
            var newStats = await _statsService.GetStats(_departmentId, SelectedRoleId, DateRange.Start ?? DateTime.MinValue, DateRange.End?.AddDays(1) ?? DateTime.MaxValue);
            Stats[SelectedRoleId] = newStats;
            IsLoading = false;
        }
    }
}
