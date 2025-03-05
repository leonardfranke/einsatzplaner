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
        public FormModel HelperCategoryData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;
        private int _oldLockingPeriod;

        protected override void OnInitialized()
        {
            HelperCategoryData = new();
            EditContext = new(HelperCategoryData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();

            if (string.IsNullOrWhiteSpace(HelperCategoryData.Name))
                _messageStore.Add(() => HelperCategoryData.Name, "Bezeichnung muss gesetzt sein.");

            if (HelperCategoryData.LockingPeriod < 0)
                _messageStore.Add(() => HelperCategoryData.LockingPeriod, "Sperrzeitraum muss positiv sein.");
        }

        protected override async Task OnParametersSetAsync()
        {
            IsUpdate = Role != null;
            if (!IsUpdate)
                return;

            HelperCategoryData.Name = Role.Name;
            HelperCategoryData.LockingPeriod = Role.LockingPeriod;
            _oldName = HelperCategoryData.Name;
            _oldLockingPeriod = HelperCategoryData.LockingPeriod;
        }

        public async Task SaveHelperCategory()
        {
            if(HelperCategoryData.Name != _oldName || HelperCategoryData.LockingPeriod != _oldLockingPeriod)
                await SaveHelperCategoryFunc(Role?.Id, HelperCategoryData.Name, HelperCategoryData.LockingPeriod);
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
