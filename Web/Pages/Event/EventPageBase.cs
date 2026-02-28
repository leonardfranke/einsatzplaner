using BlazorBootstrap;
using LeafletForBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Web.Checks;
using Web.Extensions;
using Web.Manager;
using Web.Models;
using Web.Services;
using Web.Services.Locations;
using Web.Views.MemberSelection;

namespace Web.Pages
{
    public class EventPageBase : ComponentBase
    {
        private string _departmentId;

        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter]
        public string DepartmentUrl { get; set; }

        [Parameter]
        public string GameId { get; set; }

        public Models.Event Event { get; private set; }

        public Task<Models.Event> EventTask { get; private set; }

        public List<Models.Helper> Helpers { get; private set; }

        public Location Location { get; private set; }

        public RealTimeMap.LoadParameters LoadParameters { get; private set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private ILocationsService _locationService { get; set; }

        [Inject]
        private IEventService _gameService { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        [Inject]
        private IDialogService _dialogService { get; set; }

        [Inject]
        private AuthenticationStateProvider _authStateProvider { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }
        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }
        public Models.Member Member { get; private set; }
        public List<Group> Groups { get; private set; }
        public AuthenticationState AuthState { get; set; }

        public bool IsPageLoading { get; set; }

        private List<Models.Member> _members;
        private List<Role> _roles;
        private Models.EventCategory _eventCategory;
        private Models.Group _group;

        protected override async Task OnParametersSetAsync()
        {
            IsPageLoading = true;
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;

            if (!await _loginCheck.CheckLogin(DepartmentUrl, department))
                return;

            _departmentId = department.Id;
            EventTask = _gameService.GetEvent(department.Id, GameId);
            Event = await EventTask;
            if (Event == null)
                return;

            var groupsTask = _groupService.GetAll(department.Id);
            var rolesTask = _roleService.GetAll(department.Id);
            var membersTask = _memberService.GetAll(department.Id);
            if(!string.IsNullOrEmpty(Event.LocationId))
            {
                Location = await _locationService.GetById(_departmentId, Event.LocationId);
            }

            AuthState = await _authStateProvider.GetAuthenticationStateAsync();

            Groups = await groupsTask;
            _group = Groups?.Find(group => group.Id == Event.GroupId);
            if (!string.IsNullOrEmpty(Event.EventCategoryId))
            {
                var eventCategoriesTask = _eventCategoryService.GetById(department.Id, Event.EventCategoryId);
                _eventCategory = await eventCategoriesTask;
            }

            _roles = await rolesTask;
            _members = await membersTask;
            var reloadHelpersTask = ReloadHelpers();
            var currentUserId = (await _authManager.GetLocalUser()).Id;
            Member = _members.FirstOrDefault(member => member.Id == currentUserId);
            await reloadHelpersTask;
            IsPageLoading = false;
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

        protected MarkupString GetDisplayMemberNames(List<string> memberIds)
        {
            if (memberIds == null || memberIds.Count == 0)
                return new MarkupString("-");
            return new MarkupString(string.Join(", ", memberIds.Select(id => _members.FirstOrDefault(member => member.Id == id)?.GetMemberName() ?? new MarkupString("Ohne Name"))));
        }

        protected (string, bool) GetDisplayGroupNames(List<string> groupIds)
        {
            if (groupIds == null || groupIds.Count == 0)
                return ("Frei", true);
            return (string.Join(", ", groupIds.Select(id => Groups.FirstOrDefault(group => group.Id == id)?.Name ?? "Ohne Name")), false);
        }

        protected async Task OpenLockedMembersSelected(Models.Helper helper)
        {
            var role = GetRoleById(helper.RoleId);
            var lockedMembers = new List<string>(helper.LockedMembers);
            var permittedMembers = 
                _members.Where(member => {
                    if (helper.LockedMembers.Contains(member.Id))
                        return true;
                    var requiredRole = _roles.Find(role => helper.RoleId == role.Id);
                    if (!requiredRole.IsFree && !requiredRole.MemberIds.Contains(member.Id))
                        return false;
                    var requiredGroups = helper.RequiredGroups.Select(requiredGroup => Groups.Find(group => group.Id == requiredGroup));
                    if (requiredGroups.Count() == 0)
                        return true;
                    return requiredGroups.SelectMany(group => group.MemberIds).Contains(member.Id);
                });

            var groupingFunctions = new List<(string, Func<Models.Member, bool>)>()
            {
                { ("Bereits zugewiesen", member => helper.LockedMembers.Contains(member.Id)) },
                { ("Vorausgewählt", member => helper.PreselectedMembers.Contains(member.Id)) },
                { ("Verfügbar", member => helper.AvailableMembers.Contains(member.Id)) },
                { ("Empfehlungen", member => helper.FillMembers.Contains(member.Id)) },
            };
            var parameter = new DialogParameters<MemberSelection>()
            {
                { x => x.Members, permittedMembers },
                { x => x.SelectedMembers, lockedMembers },
                { x => x.GroupingFunctions, groupingFunctions }
            };
            var dialog = await _dialogService.ShowAsync<MemberSelection>($"{role.Name} ({helper.RequiredAmount})", parameter);
            var result = await dialog.Result;
            if(!result.Canceled)
            {
                var newLockedMembers = result.Data as List<string>;
                var lockedMembersToRemove = helper.LockedMembers.Except(newLockedMembers).ToList();
                var lockedMembersToAdd = newLockedMembers.Except(helper.LockedMembers).ToList();
                await _helperService.UpdateLockedMembers(_departmentId, helper.EventId, helper.Id, lockedMembersToRemove, lockedMembersToAdd);
                await ReloadHelpers();
            }
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
            string[] titleparts = [eventInfos, dateInfo];
            return string.Join(", ", titleparts.Where(part => !string.IsNullOrEmpty(part)));
        }

        public RealTimeMap.LoadParameters GetMapParameter(Location location)
        {
            return new RealTimeMap.LoadParameters
            {
                location = new RealTimeMap.Location
                {
                    latitude = location.Latitude,
                    longitude = location.Longitude
                },
                zoomLevel = 17,
                basemap = new RealTimeMap.Basemap
                {
                    basemapLayers = new List<RealTimeMap.BasemapLayer>
                    {
                        new RealTimeMap.BasemapLayer
                        {
                            url = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                            attribution = "© OpenStreetMap",
                            title = "OSM",
                            detectRetina = true
                        }
                    }
                }
            };
        }
    }
}
