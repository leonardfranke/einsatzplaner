using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Services;
using Web.Views.ChangeGroup;

namespace Web.Pages
{
    public class GroupViewBase : ComponentBase
    {
        private string _departmentId;
        private List<Models.Member> _members;
        private List<Group> _groups;

        public Dictionary<Group, List<Models.Member>> GroupMembersDict { get; set; } = new();

        [Parameter]
        public Models.Department Department { private get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        public string HoveredId { get; set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!await _loginCheck.CheckLogin(Department))
                return;            
            _departmentId = Department.Id;

            await LoadGroups();
        }

        private async Task LoadGroups()
        {
            _members = await _memberService.GetAll(_departmentId); 
            _groups = await _groupService.GetAll(_departmentId);
            GroupMembersDict.Clear();
            foreach (var group in _groups)
                GroupMembersDict[group] = _members.Where(member => member.GroupIds.Contains(group.Id)).ToList();
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

        private async Task UpdateGroup(string groupId, string name, IEnumerable<string> newMembers, IEnumerable<string> formerMembers)
        {
            var newGroupId = await _groupService.UpdateOrCreateGroup(_departmentId, groupId, name);
            if(newMembers.Any() || formerMembers.Any())
                await _groupService.UpdateGroupMembers(_departmentId, newGroupId, newMembers.ToList(), formerMembers.ToList());
            await LoadGroups();
        }
    }
}
