using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
        public Func<string, string?, int?, bool?, Task> UpdateRoleFunc { get; set; }

        [Inject]
        public IMemberService _memberService { private get; set; }

        [SupplyParameterFromForm]
        public FormModel RoleData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsRoleSaving { get; set; }
        public bool IsRoleDeleting { get; set; }
        public bool IsRoleLoading { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;
        private int _oldLockingPeriod;
        private bool _oldIsFree;

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

        protected override void OnParametersSet()
        {
            IsRoleLoading = true;
            IsUpdate = Role != null;
            if (!IsUpdate)
            {
                _oldName = string.Empty;
                RoleData.Name = _oldName;
                _oldLockingPeriod = 0;
                RoleData.LockingPeriod = _oldLockingPeriod;
                _oldIsFree = true;
                RoleData.IsFree = _oldIsFree;
            }
            else
            {
                RoleData.Name = Role.Name;
                _oldName = RoleData.Name;
                RoleData.LockingPeriod = (int)Role.LockingPeriod;
                _oldLockingPeriod = RoleData.LockingPeriod;
                _oldIsFree = Role.IsFree;
                RoleData.IsFree = _oldIsFree;
            }
            IsRoleLoading = false;
        }

        public async Task SaveRole()
        {
            IsRoleSaving = true;
            var newRoleName = RoleData.Name != _oldName || !IsUpdate ? RoleData.Name : null;
            int? newLockingPeriodName = RoleData.LockingPeriod != _oldLockingPeriod || !IsUpdate ? RoleData.LockingPeriod : null;
            bool? newIsFree = RoleData.IsFree != _oldIsFree || !IsUpdate ? RoleData.IsFree : null;
            await UpdateRoleFunc(Role?.Id, newRoleName, (int?)newLockingPeriodName, newIsFree);
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
            public bool IsFree { get; set; }
        }

    }
}
