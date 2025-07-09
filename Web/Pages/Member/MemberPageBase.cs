using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Services;
using Web.Views.ChangeMember;

namespace Web.Pages
{
    public class MemberPageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { get; set; }

        private string _departmentId;

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        private IDepartmentService _departmentService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        public List<MembershipRequest> MembershipRequests { get; set; } = new();

        public List<Models.Member> Members { get; set; } = new();

        public List<Group> Groups { get; set; } = new();

        public List<Role> Roles { get; set; } = new();

        public int? HoveredIndex { get; set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        public bool IsPageLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsPageLoading = true;
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department, true))
                return;

            var groupTask = _groupService.GetAll(_departmentId);
            var rolesTask = _roleService.GetAll(_departmentId);
            var membersTask = LoadMembers();
            var requestsTask = LoadRequests();

            Groups = await groupTask;
            Roles = await rolesTask;
            await membersTask;
            await requestsTask;
            IsPageLoading = false;
        }

        private async Task LoadRequests()
        {
            MembershipRequests = await _departmentService.MembershipRequests(_departmentId);
            StateHasChanged();
        }

        private async Task LoadMembers()
        {
            Members = await _memberService.GetAll(_departmentId);
            StateHasChanged();
        }

        public string GetGroupNamesByIds(string memberId)
        {
            var memberGroups = Groups.Where(group => group.MemberIds.Contains(memberId));
            return string.Join(", ", memberGroups);
        }

        public string GetRoleNamesByIds(string memberId)
        {
            var memberRoles = Roles.Where(role => role.MemberIds.Contains(memberId));
            return string.Join(", ", memberRoles);
        }

        public async Task AnswerRequest(string requestId, bool accept)
        {
            await _departmentService.AnswerRequest(_departmentId, requestId, accept);
            await LoadRequests();
            if (accept)
                await LoadMembers();
        }

        public async Task EditMember(Models.Member member)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteMemberFunc = DeleteMember;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeMember.DepartmentId), _departmentId },
                { nameof(ChangeMember.Member), member },
                { nameof(ChangeMember.CloseModalFunc), closeModalFunc },
                { nameof(ChangeMember.DeleteMemberFunc), deleteMemberFunc },
                { nameof(ChangeMember.SaveMemberFunc), SaveMember }
            };
            await Modal.ShowAsync<ChangeMember>(title: "Mitglied bearbeiten", parameters: parameters);
        }

        private async Task DeleteMember(string memberId)
        {
            await _departmentService.RemoveMember(_departmentId, memberId);
            await LoadMembers();
        }

        private async Task SaveMember(string memberId, bool? newIsAdmin)
        {
            if(newIsAdmin != null)
            {
                await _memberService.UpdateMember(_departmentId, memberId, (bool)newIsAdmin);
                await LoadMembers();
            }
        }
    }
}
