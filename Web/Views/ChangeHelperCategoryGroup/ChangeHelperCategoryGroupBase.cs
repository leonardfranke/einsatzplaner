using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Models;

namespace Web.Views
{
    public class ChangeHelperCategoryGroupBase : ComponentBase
    {
        [Parameter]
        public RequirementGroup HelperCategoryGroup { get; set; }

        [Parameter]
        public List<Role> Roles { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteHelperCategoryGroupFunc { get; set; }

        [Parameter]
        public Func<string, Dictionary<string, uint>, Task> SaveHelperCategoryGroupFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel HelperCategoryGroupData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private Dictionary<string, uint> _oldRequirements = new();

        protected override void OnInitialized()
        {
            HelperCategoryGroupData = new();
            EditContext = new(HelperCategoryGroupData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();
        }

        protected override async Task OnParametersSetAsync()
        {
            IsUpdate = HelperCategoryGroup != null;
            if (IsUpdate)
                HelperCategoryGroupData.Requirements = HelperCategoryGroup.Requirements ?? new();
            else
                HelperCategoryGroupData.Requirements.Clear();

            _oldRequirements = new(HelperCategoryGroupData.Requirements);
        }

        public Role GetCategoryById(string helperCategoryId)
        {
            return Roles.Find(category => category.Id == helperCategoryId);
        }

        public void AddRequirement(string categoryId)
        {
            HelperCategoryGroupData.Requirements.TryAdd(categoryId, 1);
        }

        public void RemoveRequirement(string categoryId)
        {
            HelperCategoryGroupData.Requirements.Remove(categoryId);
        }

        public void SetRequirement(string categoryId, string stringValue)
        {
            var parsed = uint.TryParse(stringValue, out uint value);
            if(!parsed)
                value = 1;
            HelperCategoryGroupData.Requirements[categoryId] = value;
        }

        public async Task SaveHelperCategoryGroup()
        {
            if(HelperCategoryGroupData.Requirements.Count != _oldRequirements.Count 
                || HelperCategoryGroupData.Requirements.Any(requirement =>
            {
                var exists = _oldRequirements.TryGetValue(requirement.Key, out var value);
                if (!exists)
                    return true;
                return requirement.Value != value;
            }))
            {
                await SaveHelperCategoryGroupFunc(HelperCategoryGroup?.Id, 
                    HelperCategoryGroupData.Requirements);
            }
            await CloseModal();
        }

        public async Task DeleteHelperCategoryGroup()
        {
            await DeleteHelperCategoryGroupFunc(HelperCategoryGroup.Id);
            await CloseModal();
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public Dictionary<string, uint> Requirements { get; set; } = new();
        }

    }
}
