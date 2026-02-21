using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Extensions;
using MudBlazor;
using Web.Models;
using Web.Services;
using Web.Views.ChangeHelperCategoryGroup;
using Web.Views.ChangeQualifications;
using Web.Views.ChangeRole;
using Web.Views.MemberSelection;

namespace Web.Pages
{
    public class RoleViewBase : ComponentBase
    {
        private List<Models.Member> _members;


        [Parameter]
        public Models.Department Department { private get; set; }

        [Inject]
        private IQualificationService _qualificationService { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private IDialogService _dialogService { get; set; }

        [Inject]
        private IRequirementGroupService _requirementGroupService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        public string HoveredId { get; set; }

        public List<Role> Roles { get; set; }
        public IEnumerable<Qualification> Qualifications { get; set; }

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
            var loadQualificationsTask = LoadQualifications();
            _members = await membersTask;
            await requirementGroupsTask;
            await loadQualificationsTask;
            IsViewLoading = false;
        }

        public MarkupString GetMemberNameById(string memberId)
        {
            return _members.Find(member => member.Id == memberId)?.GetMemberName() ?? new MarkupString("Unbekannter Nutzer");
        }

        private async Task LoadRequirementGroups()
        {
            RequirementGroups = await _requirementGroupService.GetAll(Department.Id);
            StateHasChanged();
        }

        private async Task LoadRoles()
        {
            var membersTask = _memberService.GetAll(Department.Id);
            var rolesTask = _roleService.GetAll(Department.Id);
            _members = await membersTask;
            Roles = await rolesTask;
            StateHasChanged();
        }

        private async Task LoadQualifications()
        {
            Qualifications = await _qualificationService.GetAll(Department.Id);
            StateHasChanged();
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

        private async Task UpdateRole(string roleId, string? newName, int? newLockingPeriod, bool? newIsFree)
        {
            if(newName != null || newLockingPeriod != null || newIsFree != null)
            {
                await _roleService.UpdateOrCreate(Department.Id, roleId, newName, newLockingPeriod, newIsFree);           
                await LoadRoles();
                if (newIsFree == false)
                    await LoadQualifications();
            }
        }

        public async Task EditRoleMembers(Role role)
        {
            var oldSelectedMembers = role.MemberIds;
            var currentSelectedMembers = new List<string>(oldSelectedMembers);
            var confirmModalAction = async () =>
            {
                var newMembers = currentSelectedMembers.Except(oldSelectedMembers);
                var formerMembers = oldSelectedMembers.Except(currentSelectedMembers);
                await _roleService.UpdateRoleMembers(Department.Id, role.Id, newMembers, formerMembers);
                await LoadRoles();
                if (formerMembers.Any())
                    await LoadQualifications();
            };

            var parameter = new DialogParameters<MemberSelection>()
            {
                { x => x.Members, _members },
                { x => x.SelectedMembers, currentSelectedMembers }
            };
            var dialog = await _dialogService.ShowAsync<MemberSelection>(role.Name, parameter);
        }
        public async Task EditQualificationMembers(Qualification qualification)
        {
         
            var oldSelectedMembers = qualification.MemberIds;
            var roleOfQualification = Roles.First(role => role.Id == qualification.RoleId);
            var permittedMembers = _members.Where(member => roleOfQualification.MemberIds.Union(oldSelectedMembers).Contains(member.Id));
            var currentSelectedMembers = new List<string>(oldSelectedMembers);
            var confirmModalAction = async () =>
            {
                var newMembers = currentSelectedMembers.Except(oldSelectedMembers);
                var formerMembers = oldSelectedMembers.Except(currentSelectedMembers);
                await _qualificationService.UpdateQualificationMembers(Department.Id, qualification.Id, newMembers, formerMembers);
                await LoadQualifications();
            };

            var parameter = new DialogParameters<MemberSelection>()
            {
                { x => x.Members, permittedMembers },
                { x => x.SelectedMembers, currentSelectedMembers }
            };
            var dialog = await _dialogService.ShowAsync<MemberSelection>($"{qualification.Name} auswählen", parameter);
        }

        public string GetRoleHeader(Role role)
        {
            return role.Name + (role.IsFree ? " - Offene Rolle" : string.Empty);
        }

        public IEnumerable<Qualification> GetQualificationsOfRole(string roleId)
        {
            return Qualifications.Where(qualification => qualification.RoleId == roleId);
        }

        public async Task EditOrCreateQualification(Role? role, Qualification? qualification)
        {
            var closeModalFunc = Modal.HideAsync;
            var safeHelperCategoryFunc = SaveRequirementGroup;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeQualification.Role), role},
                { nameof(ChangeQualification.Qualification), qualification},
                { nameof(ChangeQualification.CloseModalFunc), closeModalFunc },
                { nameof(ChangeQualification.DeleteQualificationFunc), DeleteQualification },
                { nameof(ChangeQualification.UpdateQualificationFunc), UpdateQualification }
            };
            var title = qualification == null ? "Qualifikation erstellen" : "Qualifikation bearbeiten";
            await Modal.ShowAsync<ChangeQualification>(title: title, parameters: parameters);
        }

        private async Task DeleteQualification(string qualificationId)
        {
            await _qualificationService.Delete(Department.Id, qualificationId);
            await LoadQualifications();
        }

        private async Task UpdateQualification(string roleId, string qualificationId, string newName)
        {
            if(qualificationId == null || newName != null)
            {
                await _qualificationService.UpdateOrCreate(Department.Id, roleId, qualificationId, newName);
                await LoadQualifications();
            }
        }

        public async Task EditOrCreateRequirementGroup(RequirementGroup? categoryGroup)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteHelperCategoryFunc = DeleteRequirementGroup;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeHelperCategoryGroup.HelperCategoryGroup), categoryGroup},
                { nameof(ChangeHelperCategoryGroup.Roles), Roles},
                { nameof(ChangeHelperCategoryGroup.Qualifications), Qualifications },
                { nameof(ChangeHelperCategoryGroup.CloseModalFunc), closeModalFunc },
                { nameof(ChangeHelperCategoryGroup.DeleteHelperCategoryGroupFunc), deleteHelperCategoryFunc },
                { nameof(ChangeHelperCategoryGroup.SaveHelperCategoryGroupFunc), SaveRequirementGroup }
            };
            var title = categoryGroup == null ? "Gruppe erstellen" : "Gruppe bearbeiten";
            await Modal.ShowAsync<ChangeHelperCategoryGroup>(title: title, parameters: parameters);
        }

        private async Task DeleteRequirementGroup(string helperCategoryGroupId)
        {
            await _requirementGroupService.Delete(Department.Id, helperCategoryGroupId);
            await LoadRequirementGroups();
        }

        private async Task SaveRequirementGroup(string requirementGroupId, Dictionary<string, int> newRoleRequirements, IEnumerable<string> formerRoleRequirements, Dictionary<string, int> newQualificationsRequirements, IEnumerable<string> formerQualificationsRequirements)
        {
            await _requirementGroupService.UpdateOrCreate(Department.Id, requirementGroupId, newRoleRequirements, formerRoleRequirements, newQualificationsRequirements, formerQualificationsRequirements);
            await LoadRequirementGroups();
        }
    }
}
