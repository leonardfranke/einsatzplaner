using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class ChangeMemberBase : ComponentBase
    {
        [Parameter]
        public string DepartmentId { get; set; }

        [Parameter]
        public Member Member { get; set; }

        public List<Group> Groups { get; set; } = new();

        public List<Role> Roles { get; set; } = new();

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteMemberFunc { get; set; }

        [Parameter]
        public Func<string, List<string>, List<string>, bool, Task> SaveMemberFunc { get; set; }

        [Inject]
        public IRoleService _roleService { private get; set; }
        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        public IGroupService _groupService { private get; set; }

        [Inject]
        public IAuthManager _authManager { private get; set; }

        private List<string> _oldGroupIds;
        private List<string> _oldRoleIds;
        private bool _oldIsAdmin;

        public ElementReference GroupSelect;

        [SupplyParameterFromForm]
        public FormModel MemberData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsMemberSaving { get; set; }
        public bool IsMemberDeleting { get; set; }
        public bool IsMemberLoading { get; set; }

        private ValidationMessageStore _messageStore;

        protected override void OnInitialized()
        {
            MemberData = new();
            EditContext = new(MemberData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
            _oldGroupIds = new();
            _oldRoleIds = new();
        }

        protected override async Task OnInitializedAsync()
        {
            IsMemberLoading = true;
            Roles = await _roleService.GetAll(DepartmentId);
            Groups = await _groupService.GetAll(DepartmentId);
            IsMemberLoading = false;
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {

        }

        protected override void OnParametersSet()
        {
            MemberData = new FormModel();
            MemberData.GroupIds = Member.GroupIds ?? new();
            _oldGroupIds.Clear();
            _oldGroupIds.AddRange(MemberData.GroupIds);
            MemberData.RoleIds = Member.RoleIds ?? new();
            _oldRoleIds.Clear();
            _oldRoleIds.AddRange(MemberData.RoleIds);
            MemberData.IsAdmin = Member.IsAdmin;
            _oldIsAdmin = MemberData.IsAdmin;
        }

        public void AddGroup(Group group)
        {
            MemberData.GroupIds.Add(group.Id);
            StateHasChanged();
        }
        public void RemoveGroup(Group group) => MemberData.GroupIds.Remove(group.Id);

        public void AddRole(Role role)
        {
            MemberData.RoleIds.Add(role.Id);
            StateHasChanged();
        }
        public void RemoveRole(string roleId) => MemberData.RoleIds.Remove(roleId);

        public async Task SaveMember()
        {
            IsMemberSaving = true;
            if (_oldGroupIds.Count != MemberData.GroupIds.Count || _oldGroupIds.Any(oldGroup => !MemberData.GroupIds.Contains(oldGroup))
                || _oldRoleIds.Count != MemberData.RoleIds.Count || _oldRoleIds.Any(oldRole => !MemberData.RoleIds.Contains(oldRole))
                || _oldIsAdmin != MemberData.IsAdmin)
                await SaveMemberFunc(Member.Id, MemberData.GroupIds, MemberData.RoleIds, MemberData.IsAdmin);
            await CloseModal();
            IsMemberSaving = false;
        }

        public async Task DeleteMember()
        {
            IsMemberDeleting = true;
            await DeleteMemberFunc(Member.Id);
            await CloseModal();
            IsMemberDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public List<string> GroupIds { get; set; } = new();

            public List<string> RoleIds { get; set; } = new();

            public bool IsAdmin { get; set; }
        }
    }
}
