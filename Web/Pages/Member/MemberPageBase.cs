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
        private ILoginCheck _loginCheck { get; set; }

        public List<MembershipRequest> MembershipRequests { get; set; } = new();

        public List<Models.Member> Members { get; set; } = new();

        public MudTable<Models.Member> Table { get; set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        public bool IsMembersLoading { get; set; }

        public bool IsRequestsLoading { get; set; }

        private Models.Member EditingMember { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department, true))
                return;

            var membersTask = LoadMembers();
            var requestsTask = LoadRequests();

            await membersTask;
            await requestsTask;
        }

        private async Task LoadRequests()
        {
            IsRequestsLoading = true;
            MembershipRequests = await _departmentService.MembershipRequests(_departmentId);
            StateHasChanged();
            IsRequestsLoading = false;
        }

        private async Task LoadMembers()
        {
            IsMembersLoading = true;
            Members = await _memberService.GetAll(_departmentId);
            StateHasChanged();
            IsMembersLoading = false;
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
