using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Services;
using Web.Services.Locations;
using Web.Views.ChangeEvent;
using Web.Views.EnteringsSelection;
using Web.Views.InfoModal;

namespace Web.Pages
{
    public class HomeBase : ComponentBase
    {
        private const string _showAllRolesKey = "ShowAllRoles";
        private const string _groupByGroupKey = $"GroupByGroup{nameof(Home)}";
        private const string _groupByEventCategoryKey = $"GroupByEventCategory{nameof(Home)}";
        private const string _hidePastEventsKey = "HidePastEvents"; 
        private const string _hideEventsWithoutEnteringKey = "HideEventsWithoutEntering"; 
        private string _currentUserId;
        public List<Role> _roles;

        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter]
        public string DepartmentUrl { get; set; }

        public string? HoveredGameId { get; set; }

        [Inject]
        protected ToastService _toastService { get; set; }

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private ILocalStorageService _localStorage { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        [Inject]
        private NavigationManager _navigationManager { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private ILocationsService _locationService { get; set; }

        [Inject]
        private IEventService _eventService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IQualificationService _qualificationService { get; set; }

        [Inject]
        private IRequirementService _helperService { get; set; }

        [Inject]
        private IDialogService _dialogService { get; set; }

        public List<Group> Groups { get; private set; }

        public List<Requirement> Helpers { get; private set; }

        public Models.Member Member { get; private set; }

        public List<Models.Member> Members { get; private set; }

        public bool IsPageLoading { get; set; }

        public bool ShowAllRoles
        {
            get => _showAllRoles;
            set
            {
                _showAllRoles = value;
                _localStorage.SetItemAsync(_showAllRolesKey, value);
                StateHasChanged();
            }
        }

        public bool HidePastEvents
        {
            get => _hidePastEvents;
            set
            {
                _hidePastEvents = value;
                _localStorage.SetItemAsync(_hidePastEventsKey, value);
                StateHasChanged();
            }
        }

        public bool HideEventsWithoutEntering
        {
            get => _hideEventsWithoutEntering;
            set
            {
                _hideEventsWithoutEntering = value;
                _localStorage.SetItemAsync(_hideEventsWithoutEnteringKey, value);
            }
        }

        public MudDataGrid<Models.Event> EventsGrid { get; set; }

        public bool GroupGroupingActive { get; set; }
        public bool EventCategoryGroupingActive { get; set; }
        public bool IsLoadingEventData { get; set; }

        public IEnumerable<Models.Event> Events { get; private set; }
        private List<Models.EventCategory> _eventCategories;
        private List<Qualification> _qualifications;
        private string _departmentId;
        private bool _hidePastEvents;
        private bool _hideEventsWithoutEntering;
        private bool _showAllRoles;
        private List<Location> _locations;

        protected override async Task OnInitializedAsync()
        {
            IsPageLoading = true;
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department))
                return;

            _currentUserId = (await _authManager.GetLocalUser()).Id;

            var rolesTask = _roleService.GetAll(_departmentId);
            var groupsTask = _groupService.GetAll(_departmentId);
            var membersTask = _memberService.GetAll(_departmentId);
            var qualificationsTask = _qualificationService.GetAll(_departmentId);
            var locationsTask = _locationService.GetAll(_departmentId);

            _locations = await locationsTask;
            var eventCategoriesTask = _eventCategoryService.GetAll(_departmentId);
            Members = await membersTask;
            Member = Members.FirstOrDefault(member => member.Id == _currentUserId);
            _roles = await rolesTask;
            _eventCategories = await eventCategoriesTask;
            Groups = await groupsTask;
            _qualifications = await qualificationsTask;
            GroupGroupingActive = await _localStorage.GetItemAsync<bool>(_groupByGroupKey);
            EventCategoryGroupingActive = await _localStorage.GetItemAsync<bool>(_groupByEventCategoryKey);
            if (await _localStorage.ContainKeyAsync(_hidePastEventsKey))
                HidePastEvents = await _localStorage.GetItemAsync<bool>(_hidePastEventsKey);
            else
                HidePastEvents = true;
            HideEventsWithoutEntering = await _localStorage.GetItemAsync<bool>(_hideEventsWithoutEnteringKey);
            ShowAllRoles = await _localStorage.GetItemAsync<bool>(_showAllRolesKey);
            await LoadEventData();
            IsPageLoading = false;
        }

        protected async Task LoadEventData()
        {
            IsLoadingEventData = true;
            StateHasChanged();
            var helpersTask = _helperService.GetAll(_departmentId);
            var eventTask = _eventService.GetAll(_departmentId);

            Events = (await eventTask).OrderBy(@event => @event.EventDate);
            Helpers = await helpersTask;
            IsLoadingEventData = false;
            StateHasChanged();
        }

        public Task OpenInfoDialog()
        {
            return _dialogService.ShowAsync<InfoModal>(null);
        }

        protected IEnumerable<Role> GetRelevantRoles()
        {
            var allHelpers = Events.SelectMany(GetHelpers);
            var rolesWithUserEntered = allHelpers
                .Where(helper => helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Contains(_currentUserId))
                .Select(role => role.RoleId);
            var rolesWithEvent = allHelpers.Select(helper => helper.RoleId);
            var relevantRoles = ShowAllRoles ? rolesWithEvent : rolesWithEvent.Where(roleId =>
            {
                var role = GetRoleById(roleId);
                return role.IsFree || role.MemberIds.Contains(_currentUserId);                
            });
            return relevantRoles.Union(rolesWithUserEntered).Select(GetRoleById);
        }

        protected Role GetRoleById(string roleId) => _roles.Find(role => role.Id == roleId);

        protected Group GetGroupById(string groupId) => Groups.Find(group => group.Id == groupId);

        protected Models.EventCategory GetEventCategoryById(string categoryId) => _eventCategories.FirstOrDefault(category => category.Id == categoryId);

        protected List<Requirement> GetHelpers(Models.Event @event)
        {
            if (@event == null)
                return null;
            return Helpers.Where(helper => helper.EventId == @event.Id).ToList();
        }
        
        public MarkupString GetLocationText(Models.Event @event)
        {
            var res = "";
            if (string.IsNullOrEmpty(@event.LocationId))
            {
                if (!string.IsNullOrEmpty(@event.LocationText))
                    res = @event.LocationText;
            }
            else
            {
                var location = _locations.FirstOrDefault(loc => loc.Id == @event.LocationId);
                if (location == null)
                    res = "<i>Unbekannte Adresse<i>";
                else
                    res = location.Name;
            }
            return new MarkupString(res);
        }

        public MarkupString GetLocationGroupText(string val)
        {
            var res = "";
            var location = _locations.FirstOrDefault(loc => loc.Id == val);
            if (location != null)
                res = location.Name;
            return new MarkupString(res);
        }

        public bool FilterEvent(Models.Event @event)
        {
            return (!HidePastEvents || @event.EventDate.AddDays(2) >= DateTime.Now)
                && (!HideEventsWithoutEntering || GetHelpers(@event).Any(helper => helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Contains(_currentUserId)));
        }

        public void GroupGroupingChanged(bool groupingActive)
        {
            GroupGroupingActive = groupingActive;
            _localStorage.SetItemAsync(_groupByGroupKey, GroupGroupingActive);
        }

        public void EventCategoryGroupingChanged(bool groupingActive)
        {
            EventCategoryGroupingActive = groupingActive;
            _localStorage.SetItemAsync(_groupByEventCategoryKey, EventCategoryGroupingActive);
        }

        protected void OpenGame(Models.Event @event)
        {
            _navigationManager.NavigateTo($"./{DepartmentUrl}/event/{@event.Id}");
        }

        public async Task EditGame(Models.Event? @event)
        {
            var parameter = new DialogParameters<EnteringsSelection>()
            {
                { nameof(ChangeEvent.DepartmentId), _departmentId },
                { nameof(ChangeEvent.Event), @event },
                { nameof(ChangeEvent.CloseModalFunc), Modal.HideAsync },
                { nameof(ChangeEvent.DeleteGameFunc), DeleteGame },
                { nameof(ChangeEvent.Roles), _roles },
                { nameof(ChangeEvent.EventCategories), _eventCategories },
                { nameof(ChangeEvent.Groups), Groups },
                { nameof(ChangeEvent.Qualifications), _qualifications },
                { nameof(ChangeEvent.Locations), _locations }
            };
            var dialog = await _dialogService.ShowAsync<ChangeEvent>(title: @event == null ? "Event erstellen" : "Event bearbeiten", parameter);
            var res = await dialog.Result;
            var removeMembers = (bool)res.Data;

            if (string.IsNullOrEmpty(@event?.Id))
                _toastService.Notify(new ToastMessage(ToastType.Primary, $"Das Event wurde erstellt."));
            else if (removeMembers)
                _toastService.Notify(new ToastMessage(ToastType.Primary, $"Das Event wurde aktualisiert. Alle eingetragenen Helfer wurden entfernt."));
            else
                _toastService.Notify(new ToastMessage(ToastType.Primary, $"Das Event wurde aktualisiert."));

            await LoadEventData();
        }

        private async Task DeleteGame(string gameId)
        {
            await _eventService.DeleteGame(_departmentId, gameId);
            await LoadEventData();
        }
    }
}