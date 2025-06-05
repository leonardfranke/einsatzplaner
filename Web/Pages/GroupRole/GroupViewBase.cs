using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Models;
using Web.Services;
using Web.Views.ChangeGroup;
using Web.Views.MemberSelection;

namespace Web.Pages
{
    public class GroupViewBase : ComponentBase
    {
        private string _departmentId;
        private List<Models.Member> _members;

        [Parameter]
        public Models.Department Department { private get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        public string HoveredId { get; set; }
        public List<Group> Groups { get; private set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        public bool IsViewLoading { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Department == null)
                return;

            IsViewLoading = true;
            _departmentId = Department.Id;
            await LoadGroups();
            IsViewLoading = false;
        }

        private async Task LoadGroups()
        {
            var membersTask = _memberService.GetAll(_departmentId);
            var groupsTask = _groupService.GetAll(_departmentId);
            _members = await membersTask;
            Groups = await groupsTask;
            StateHasChanged();
        }

        public string GetMemberNameById(string memberId)
        {
            return _members.Find(member => member.Id == memberId)?.Name ?? "Unbekannter Nutzer";
        }

        public async Task EditGroupMembers(Group group)
        {
            var oldSelectedMembers = _members
                .Where(member => group.MemberIds.Contains(member.Id))
                .Select(member => member.Id);
            var currentSelectedMembers = new List<string>(oldSelectedMembers);
            var confirmModalAction = async () =>
            {
                var newMembers = currentSelectedMembers.Except(oldSelectedMembers);
                var formerMembers = oldSelectedMembers.Except(currentSelectedMembers);
                await _groupService.UpdateGroupMembers(_departmentId, group.Id, newMembers, formerMembers);
                await LoadGroups();
            };
            var closeModalFunc = Modal.HideAsync;
            var parameters = new Dictionary<string, object>
            {
                { nameof(MemberSelectionModal.CloseModalFunc), closeModalFunc},
                { nameof(MemberSelectionModal.ConfirmModalAction), confirmModalAction},
                { nameof(MemberSelectionModal.Members), _members},
                { nameof(MemberSelectionModal.SelectedMembers), currentSelectedMembers }
            };
            await Modal.ShowAsync<MemberSelectionModal>(title: group.Name, parameters: parameters);
        }

        public async Task EditOrCreateGroup(Group? group)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteGroupFunc = DeleteGroup;
            var updateGroupFunc = UpdateGroup;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeGroup.DepartmentId), Department.Id },
                { nameof(ChangeGroup.Group), group },
                { nameof(ChangeGroup.CloseModalFunc), closeModalFunc },
                { nameof(ChangeGroup.DeleteGroupFunc), deleteGroupFunc },
                { nameof(ChangeGroup.UpdateGroupFunc), updateGroupFunc }
            };
            var title = group == null ? "Gruppe erstellen" : "Gruppe bearbeiten";
            await Modal.ShowAsync<ChangeGroup>(title: title, parameters: parameters);
        }

        private async Task DeleteGroup(string groupId)
        {
            await _groupService.DeleteGroup(_departmentId, groupId);
            await LoadGroups();
        }

        private async Task UpdateGroup(string groupId, string name)
        {
            var newGroupId = await _groupService.UpdateOrCreateGroup(_departmentId, groupId, name);
            await LoadGroups();
        }
    }
}
