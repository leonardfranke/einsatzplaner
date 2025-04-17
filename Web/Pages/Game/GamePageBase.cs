using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Checks;
using Web.Models;
using Web.Services;
using Web.Views.MemberSelection;

namespace Web.Pages
{
    public class GamePageBase : ComponentBase
    {
        private string _departmentId;

        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter]
        public string DepartmentUrl { get; set; }

        [Parameter]
        public string GameId { get; set; }

        public Models.Event Event { get; private set; }

        public Task<Event> GameTask { get; private set; }

        public List<Models.Helper> Helpers { get; private set; }


        [Inject]
        private ILoginCheck _loginCheck { get; set; }
        [Inject]
        private IEventService _gameService { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }
        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        private List<Models.Member> _members;
        private List<Group> _groups;
        private List<Role> _roles;
        private Models.EventCategory _eventCategory;
        private Models.Group _group;

        protected override async Task OnParametersSetAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;

            if (!await _loginCheck.CheckLogin(department))
                return;

            _departmentId = department.Id;
            GameTask = _gameService.GetEvent(department.Id, GameId);
            Event = await GameTask;
            if (Event == null)
                return;

            var groupsTask = _groupService.GetAll(department.Id);
            var rolesTask = _roleService.GetAll(department.Id);
            var membersTask = _memberService.GetAll(department.Id);
            _groups = await groupsTask;
            _group = _groups?.Find(group => group.Id == Event.GroupId);
            if (!string.IsNullOrEmpty(Event.EventCategoryId))
            {
                var eventCategoriesTask = _eventCategoryService.GetById(department.Id, Event.EventCategoryId);
                _eventCategory = await eventCategoriesTask;
            }            

            _roles = await rolesTask;
            _members = await membersTask;
            await ReloadHelpers();
        }

        private async Task ReloadHelpers()
        {
            Helpers = await _helperService.GetAll(_departmentId, Event.Id);
            StateHasChanged();
        }

        public Role GetRoleById(string roleId)
        {
            return _roles.Find(role => role.Id == roleId);
        }

        protected string GetDisplayMemberNames(List<string> memberIds)
        {
            if (memberIds == null || memberIds.Count == 0)
                return "-";
            return string.Join(", ", memberIds.Select(id => _members.FirstOrDefault(member => member.Id == id)?.Name ?? "Ohne Name"));
        }

        protected string GetDisplayGroupNames(List<string> groupIds)
        {
            if (groupIds == null || groupIds.Count == 0)
                return "-";
            return string.Join(", ", groupIds.Select(id => _groups.FirstOrDefault(group => group.Id == id)?.Name ?? "Ohne Name"));
        }

        protected async Task OpenLockedMembersSelected(Models.Helper helper)
        {
            var lockedMembers = new List<string>(helper.LockedMembers);
            var confirmModalAction = async () =>
            {
                var lockedMembersToRemove = helper.LockedMembers.Except(lockedMembers).ToList();
                var lockedMembersToAdd = lockedMembers.Except(helper.LockedMembers).ToList();
                await _helperService.UpdateLockedMembers(_departmentId, helper.EventId, helper.Id, lockedMembersToRemove, lockedMembersToAdd);
                await ReloadHelpers();
            };
            var closeModalFunc = Modal.HideAsync;
            var parameters = new Dictionary<string, object>
            {
                { nameof(MemberSelectionModal.CloseModalFunc), closeModalFunc},
                { nameof(MemberSelectionModal.ConfirmModalAction), confirmModalAction},
                { nameof(MemberSelectionModal.Members), _members},
                { nameof(MemberSelectionModal.SelectedMembers), lockedMembers }
            };
            await Modal.ShowAsync<MemberSelectionModal>(title: "Feste Helfer auswählen", parameters: parameters);
        }

        protected string GetPageTitle()
        {
            var pageTitles = new List<string>();
            var groupId = Event?.GroupId;
            if(!string.IsNullOrEmpty(groupId))
            {
                pageTitles.Add((_group == null) ? "Unbekannte Gruppe" : _group.Name);
            }

            if (!string.IsNullOrEmpty(Event.EventCategoryId))
            {
                pageTitles.Add((_eventCategory == null) ? "Unbekannte Eventkategorie" : _eventCategory.Name);
            }

            var eventInfos = string.Join(" - ", pageTitles);
            var dateInfo = Event.EventDate.ToString("dd.MM.yyyy HH:mm") + " Uhr";
            return string.Join(", ", eventInfos, dateInfo);
        }
    }
}
