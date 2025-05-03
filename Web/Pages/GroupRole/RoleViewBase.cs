using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Models;
using Web.Services;
using Web.Views.ChangeHelperCategoryGroup;
using Web.Views.ChangeRole;

namespace Web.Pages
{
    public class RoleViewBase : ComponentBase
    {
        private List<Models.Member> _members;
        private List<Role> _roles;

        public Dictionary<Role, List<Models.Member>> RoleMembersDict { get; set; } = new();

        [Parameter]
        public Models.Department Department { private get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private IRequirementGroupService _requirementGroupService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        public string HoveredId { get; set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        public List<RequirementGroup> RequirementGroups { get; set; }
        public bool IsViewLoading { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Department == null)
                return;

            IsViewLoading = true;
            var membersTask = _memberService.GetAll(Department.Id);
            await LoadRoles();
            var requirementGroupsTask = LoadRequirementGroups();
            _members = await membersTask;
            await requirementGroupsTask;
            IsViewLoading = false;
        }

        protected IEnumerable<Models.Member> GetMembersWithRoleId(string roleId)
        {
            return _members?.Where(member => member.RoleIds.Contains(roleId)) ?? new List<Models.Member>();
        }

        private async Task LoadRequirementGroups()
        {
            RequirementGroups = await _requirementGroupService.GetAll(Department.Id);
            StateHasChanged();
        }

        private async Task LoadRoles()
        {
            _members = await _memberService.GetAll(Department.Id);
            _roles = await _roleService.GetAll(Department.Id);
            RoleMembersDict.Clear();
            foreach (var role in _roles)
                RoleMembersDict[role] = _members.Where(member => member.RoleIds.Contains(role.Id)).ToList();
            StateHasChanged();
        }

        public string GetGroupDisplayText(RequirementGroup group)
        {
            var names = group.Requirements.Select(requirement =>
            {
                var category = GetCategoryById(requirement.Key);
                return $"{requirement.Value}x {category?.Name ?? "Keine Bezeichnung"}";
            });
            return string.Join(Environment.NewLine, names);
        }

        public Role GetCategoryById(string categoryId)
        {
            return _roles.Find(category => category.Id == categoryId);
        }

        public async Task EditOrCreateRole(Role? role)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteRoleFunc = DeleteRole;
            var updateRoleFunc = UpdateRole;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeRole.DepartmentId), Department.Id },
                { nameof(ChangeRole.Role), role },
                { nameof(ChangeRole.CloseModalFunc), closeModalFunc },
                { nameof(ChangeRole.DeleteRoleFunc), deleteRoleFunc },
                { nameof(ChangeRole.UpdateRoleFunc), updateRoleFunc }
            };
            var title = role == null ? "Rolle erstellen" : "Rolle bearbeiten";
            await Modal.ShowAsync<ChangeRole>(title: title, parameters: parameters);
        }

        private async Task DeleteRole(string roleId)
        {
            await _roleService.Delete(Department.Id, roleId);
            await LoadRoles();
        }

        private async Task UpdateRole(string roleId, string name, int lockingPeriod, IEnumerable<string> newMembers, IEnumerable<string> formerMembers)
        {
            var newRoleId = await _roleService.UpdateOrCreate(Department.Id, roleId, name, lockingPeriod);
            if (newMembers.Any() || formerMembers.Any())
                await _roleService.UpdateRoleMembers(Department.Id, newRoleId, newMembers.ToList(), formerMembers.ToList());            
            await LoadRoles();
        }

        public async Task EditOrCreateRequirementGroup(RequirementGroup? categoryGroup)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteHelperCategoryFunc = DeleteRequirementGroup;
            var safeHelperCategoryFunc = SaveRequirementGroup;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeHelperCategoryGroup.HelperCategoryGroup), categoryGroup},
                { nameof(ChangeHelperCategoryGroup.Roles), _roles},
                { nameof(ChangeHelperCategoryGroup.CloseModalFunc), closeModalFunc },
                { nameof(ChangeHelperCategoryGroup.DeleteHelperCategoryGroupFunc), deleteHelperCategoryFunc },
                { nameof(ChangeHelperCategoryGroup.SaveHelperCategoryGroupFunc), safeHelperCategoryFunc }
            };
            var title = categoryGroup == null ? "Gruppe erstellen" : "Gruppe bearbeiten";
            await Modal.ShowAsync<ChangeHelperCategoryGroup>(title: title, parameters: parameters);
        }

        private async Task DeleteRequirementGroup(string helperCategoryGroupId)
        {
            await _requirementGroupService.Delete(Department.Id, helperCategoryGroupId);
            await LoadRequirementGroups();
        }

        private async Task SaveRequirementGroup(string helperCategoryId, Dictionary<string, uint> requirements)
        {
            await _requirementGroupService.UpdateOrCreate(Department.Id, helperCategoryId, requirements);
            await LoadRequirementGroups();
        }
    }
}
