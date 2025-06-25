using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Primitives;
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
        public IEnumerable<Qualification> Qualifications { get; set; }

        [Parameter]
        public Func<string, Task> DeleteHelperCategoryGroupFunc { get; set; }

        [Parameter]
        public Func<string, Dictionary<string, int>, IEnumerable<string>, Dictionary<string, int>, IEnumerable<string>, Task> SaveHelperCategoryGroupFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel RequirementGroupData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsHelperCategoryGroupSaving { get; set; }
        public bool IsHelperCategoryGroupDeleting { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private Dictionary<string, int> _oldRequirementsRoles = new();
        private Dictionary<string, int> _oldRequirementsQualifications = new();

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
            {
                RequirementGroupData.RequirementsRoles = HelperCategoryGroup.RequirementsRoles.ToDictionary(pair => pair.Key, pair => (int)pair.Value);
                RequirementGroupData.RequirementsQualifications = HelperCategoryGroup.RequirementsQualifications;
            }
            else
            {
                RequirementGroupData.RequirementsRoles.Clear();
                RequirementGroupData.RequirementsQualifications.Clear();

            }

            _oldRequirementsRoles = new(RequirementGroupData.RequirementsRoles);
            _oldRequirementsQualifications = new(RequirementGroupData.RequirementsQualifications);
        }

        public Role GetRoleById(string helperCategoryId)
        {
            return Roles.Find(category => category.Id == helperCategoryId);
        }

        public void AddRoleRequirement(string roleId)
        {
            RequirementGroupData.RequirementsRoles.TryAdd(roleId, 1);
        }

        public void RemoveRoleRequirement(string roleId)
        {
            RequirementGroupData.RequirementsRoles.Remove(roleId);
        }

        public void SetRoleRequirement(string roleId, string stringValue)
        {
            var parsed = int.TryParse(stringValue, out int value);
            if (!parsed)
                value = 1;
            RequirementGroupData.RequirementsRoles[roleId] = value;
        }

        public void AddQualificationRequirement(string qualificationId)
        {
            RequirementGroupData.RequirementsQualifications.TryAdd(qualificationId, 1);
        }

        public void RemoveQualificationRequirement(string qualificationId)
        {
            RequirementGroupData.RequirementsQualifications.Remove(qualificationId);
        }

        public void SetQualificationRequirement(string qualificationId, string stringValue)
        {
            var parsed = int.TryParse(stringValue, out int value);
            if (!parsed)
                value = 1;
            RequirementGroupData.RequirementsQualifications[qualificationId] = value;
        }

        public async Task SaveHelperCategoryGroup()
        {
            IsHelperCategoryGroupSaving = true;
            var newRoleRequirements = RequirementGroupData.RequirementsRoles
                .Where(pair => !_oldRequirementsRoles.ContainsKey(pair.Key) || _oldRequirementsRoles[pair.Key] != pair.Value).ToDictionary();
            var formerRoleRequirements = _oldRequirementsRoles.Where(pair => !RequirementGroupData.RequirementsRoles.ContainsKey(pair.Key)).Select(pair => pair.Key);
            var newQualificationsRequirements = RequirementGroupData.RequirementsQualifications
                .Where(pair => !_oldRequirementsQualifications.ContainsKey(pair.Key) || _oldRequirementsQualifications[pair.Key] != pair.Value).ToDictionary();
            var formerQualificationsRequirements = _oldRequirementsQualifications.Where(pair => !RequirementGroupData.RequirementsQualifications.ContainsKey(pair.Key)).Select(pair => pair.Key);

            if (newRoleRequirements.Any() || formerRoleRequirements.Any() || newQualificationsRequirements.Any() || formerQualificationsRequirements.Any())
            {
                await SaveHelperCategoryGroupFunc(HelperCategoryGroup?.Id, newRoleRequirements, formerRoleRequirements, newQualificationsRequirements, formerQualificationsRequirements);
            }
            await CloseModal();
            IsHelperCategoryGroupSaving = false;
        }

        public IEnumerable<Qualification> GetQualificationsOfRole(string roleId)
        {
            return Qualifications.Where(qualification => qualification.RoleId == roleId);
        }

        public Qualification GetQualificationById(string qualificationId)
        {
            return Qualifications.First(qualification => qualification.Id == qualificationId);
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
            public Dictionary<string, int> RequirementsRoles { get; set; } = new ();
            public Dictionary<string, int> RequirementsQualifications { get; set; } = new();
        }

    }
}
