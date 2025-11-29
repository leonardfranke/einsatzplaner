using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Checks;
using Web.Models;
using Web.Services;

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

        public List<MembershipRequest> MembershipRequests { get; set; } = new();

        public List<Models.Member> Members { get; set; } = new();

        public List<Group> Groups { get; set; } = new();

        public List<Role> Roles { get; set; } = new();

        public MudTable<Models.Member> Table { get; set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        public bool IsPageLoading { get; set; }
        private Models.Member EditingMember { get; set; }

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

        public async void UpdateMember(object obj)
        {
            var member = (Models.Member)obj;
            await _memberService.UpdateMember(_departmentId, member.Id, member.Name, member.IsAdmin);
        }

        public void CreateEditPreview(object obj)
        {
            var member = (Models.Member)obj;
            EditingMember = new Models.Member 
            { 
                Name = member.Name,
                IsAdmin = member.IsAdmin
            };
        }

        public void EditCancel(object obj)
        {
            var member = (Models.Member)obj;
            member.Name = EditingMember.Name;
            member.IsAdmin = EditingMember.IsAdmin;
        }

        public async Task AddDummyMember()
        {
            var dummyMemberId = await _memberService.CreateDummyMember(_departmentId);
            var dummyMember = new Models.Member
            {
                Id = dummyMemberId,
                Name = "",
                IsDummy = true,
                IsAdmin = false,
            };
            Members.Insert(0, dummyMember);
            StateHasChanged();
        }

        public async Task DeleteMember(Models.Member member)
        {
            Table.SetEditingItem(null);
            Members.Remove(member);
            StateHasChanged();
            await _departmentService.RemoveMember(_departmentId, member.Id);
        }
    }
}
