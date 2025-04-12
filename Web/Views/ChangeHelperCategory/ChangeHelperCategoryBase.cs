using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Models;

namespace Web.Views
{
    public class ChangeHelperCategoryBase : ComponentBase
    {
        [Parameter]
        public Role Role { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteHelperCategoryFunc { get; set; }

        [Parameter]
        public Func<string, string, int, Task> SaveHelperCategoryFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel RoleData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;
        private int _oldLockingPeriod;

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
                _messageStore.Add(() => RoleData.Name, "Bezeichnung muss gesetzt sein.");

            if (RoleData.LockingPeriod < 0)
                _messageStore.Add(() => RoleData.LockingPeriod, "Sperrzeitraum muss positiv sein.");
        }

        protected override async Task OnParametersSetAsync()
        {
            IsUpdate = Role != null;
            if (!IsUpdate)
                return;

            RoleData.Name = Role.Name;
            RoleData.LockingPeriod = Role.LockingPeriod;
            _oldName = RoleData.Name;
            _oldLockingPeriod = RoleData.LockingPeriod;
        }

        public async Task SaveHelperCategory()
        {
            if(RoleData.Name != _oldName || RoleData.LockingPeriod != _oldLockingPeriod)
                await SaveHelperCategoryFunc(Role?.Id, RoleData.Name, RoleData.LockingPeriod);
            await CloseModal();
        }

        public async Task DeleteHelperCategory()
        {
            await DeleteHelperCategoryFunc(Role.Id);
            await CloseModal();
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
            public int LockingPeriod { get; set; }
        }

    }
}
