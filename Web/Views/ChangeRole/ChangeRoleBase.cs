using DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class ChangeRoleBase : ComponentBase
    {
        [Parameter]
        public string DepartmentId { get; set; }

        [Parameter]
        public Role Role { private get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteRoleFunc { get; set; }

        [Parameter]
        public Func<string, string, int, IEnumerable<string>, IEnumerable<string>, Task> UpdateRoleFunc { get; set; }

        [Inject]
        public IMemberService _memberService { private get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [SupplyParameterFromForm]
        public FormModel RoleData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsRoleSaving { get; set; }
        public bool IsRoleDeleting { get; set; }
        public bool IsRoleLoading { get; set; }

        protected List<string> SelectedMembers { get; private set; }

        protected List<Member> Members { get; private set; } = new();

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;
        private int _oldLockingPeriod;
        private List<string> _oldSelectedMembers;

        protected override void OnInitialized()
        {
            RoleData = new();
            EditContext = new(RoleData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();

            if (string.IsNullOrWhiteSpace(RoleData.Name))
                _messageStore.Add(() => RoleData.Name, "Die Rolle benötigt einen Namen.");
        }

        protected override async Task OnParametersSetAsync()
        {
            IsRoleLoading = true;
            IsUpdate = Role != null;
            if (!IsUpdate)
            {
                _oldName = string.Empty;
                RoleData.Name = _oldName;
                _oldLockingPeriod = 0;
                RoleData.LockingPeriod = _oldLockingPeriod;
                _oldSelectedMembers = new();
                SelectedMembers = new();
                return;
            }

            Members = await _memberService.GetAll(DepartmentId);
            RoleData.Name = Role.Name;
            _oldName = RoleData.Name;
            RoleData.LockingPeriod = Role.LockingPeriod;
            _oldLockingPeriod = RoleData.LockingPeriod;
            _oldSelectedMembers = Members.Where(member => member.RoleIds.Contains(Role.Id)).Select(member => member.Id).ToList();
            SelectedMembers = new(_oldSelectedMembers);
            IsRoleLoading = false;
        }

        public async Task SaveRole()
        {
            IsRoleSaving = true;
            var newRoleMembers = SelectedMembers.Except(_oldSelectedMembers);
            var formerRoleMembers = _oldSelectedMembers.Except(SelectedMembers);
            if (RoleData.Name != _oldName || RoleData.LockingPeriod != _oldLockingPeriod || newRoleMembers.Any() || formerRoleMembers.Any())
                await UpdateRoleFunc(Role?.Id, RoleData.Name, RoleData.LockingPeriod, newRoleMembers, formerRoleMembers);
            await CloseModal();
            IsRoleSaving = false;
        }

        public async Task DeleteRole()
        {
            IsRoleDeleting = true;
            await DeleteRoleFunc(Role.Id);
            await CloseModal();
            IsRoleDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
            public int LockingPeriod { get; set; }
        }

    }
}
