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
        public FormModel RequirementGroupData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsHelperCategoryGroupSaving { get; set; }
        public bool IsHelperCategoryGroupDeleting { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private Dictionary<string, uint> _oldRequirements = new();

        protected override void OnInitialized()
        {
            RequirementGroupData = new();
            EditContext = new(RequirementGroupData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();
        }

        protected override void OnParametersSet()
        {
            IsUpdate = HelperCategoryGroup != null;
            if (IsUpdate)
                RequirementGroupData.Requirements = HelperCategoryGroup.Requirements ?? new();
            else
                RequirementGroupData.Requirements.Clear();

            _oldRequirements = new(RequirementGroupData.Requirements);
        }

        public Role GetCategoryById(string helperCategoryId)
        {
            return Roles.Find(category => category.Id == helperCategoryId);
        }

        public void AddRequirement(string categoryId)
        {
            RequirementGroupData.Requirements.TryAdd(categoryId, 1);
        }

        public void RemoveRequirement(string categoryId)
        {
            RequirementGroupData.Requirements.Remove(categoryId);
        }

        public void SetRequirement(string categoryId, string stringValue)
        {
            var parsed = uint.TryParse(stringValue, out uint value);
            if(!parsed)
                value = 1;
            RequirementGroupData.Requirements[categoryId] = value;
        }

        public async Task SaveHelperCategoryGroup()
        {
            IsHelperCategoryGroupSaving = true;
            if(RequirementGroupData.Requirements.Count != _oldRequirements.Count 
                || RequirementGroupData.Requirements.Any(requirement =>
            {
                var exists = _oldRequirements.TryGetValue(requirement.Key, out var value);
                if (!exists)
                    return true;
                return requirement.Value != value;
            }))
            {
                await SaveHelperCategoryGroupFunc(HelperCategoryGroup?.Id, 
                    RequirementGroupData.Requirements);
            }
            await CloseModal();
            IsHelperCategoryGroupSaving = false;
        }

        public async Task DeleteHelperCategoryGroup()
        {
            IsHelperCategoryGroupDeleting = true;
            await DeleteHelperCategoryGroupFunc(HelperCategoryGroup.Id);
            await CloseModal();
            IsHelperCategoryGroupDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public Dictionary<string, uint> Requirements { get; set; } = new();
        }

    }
}
