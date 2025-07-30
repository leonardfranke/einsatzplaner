using BlazorBootstrap;
using Blazored.LocalStorage;
using DTO;
using Microsoft.AspNetCore.Components;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Services;
using Web.Views.BasicModals;
using Web.Views.ChangeEvent;

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

        public List<IGrouping<object, Models.Event>> BothEventGrouping { get => _bothEventGrouping.Value; }
        public List<IGrouping<object, Models.Event>> GroupEventGrouping { get => _groupEventGrouping.Value; }
        public List<IGrouping<object, Models.Event>> CategoryEventGrouping { get => _categoryEventGrouping.Value; }

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
        private IEventService _eventService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IQualificationService _qualificationService { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        public List<Group> Groups { get; private set; }


        public List<Models.Helper> Helpers { get; private set; }

        public Models.Member Member { get; private set; }

        public bool IsPageLoading { get; set; }

        public bool GroupByGroup {
            get => _groupByGroup;
            set
            {
                _groupByGroup = value;
                _localStorage.SetItemAsync(_groupByGroupKey, value);
            }
        }
        public bool GroupByEventCategory
        {
            get => _groupByEventCategory;
            set
            {
                _groupByEventCategory = value;
                _localStorage.SetItemAsync(_groupByEventCategoryKey, value);
            }
        }
        public bool ShowAllRoles
        {
            get => _showAllRoles;
            set
            {
                _showAllRoles = value;
                _localStorage.SetItemAsync(_showAllRolesKey, value);
            }
        }
        public bool HidePastEvents { 
            get => _hidePastEvents;
            set 
            {
                _hidePastEvents = value;
                _localStorage.SetItemAsync(_hidePastEventsKey, value);
                RecalculateGrouping();
            }
        }
        public bool HideEventsWithoutEntering
        {
            get => _hideEventsWithoutEntering;
            set
            {
                _hideEventsWithoutEntering = value;
                _localStorage.SetItemAsync(_hideEventsWithoutEnteringKey, value);
                RecalculateGrouping();
            }
        }

        public string HoveredEventId { get; set; }

        public IEnumerable<Models.Event> FilteredEvents => events?
            .Where(@event => !HidePastEvents || @event.EventDate.AddDays(2) >= DateTime.Now)
            .Where(@event => !HideEventsWithoutEntering || GetHelpers(@event).Any(helper => helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Contains(_currentUserId)));

        private List<Models.Event> events;
        private List<Models.EventCategory> _eventCategories;
        private List<Models.Member> members;
        private List<Qualification> _qualifications;
        private string _departmentId;
        private Lazy<List<IGrouping<object, Models.Event>>> _bothEventGrouping;
        private Lazy<List<IGrouping<object, Models.Event>>> _groupEventGrouping;
        private Lazy<List<IGrouping<object, Models.Event>>> _categoryEventGrouping;
        private bool _hidePastEvents;
        private bool _hideEventsWithoutEntering;
        private bool _groupByGroup;
        private bool _groupByEventCategory;
        private bool _showAllRoles;

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

            var eventCategoriesTask = _eventCategoryService.GetAll(_departmentId);
            members = await membersTask;
            Member = members.FirstOrDefault(member => member.Id == _currentUserId);
            _roles = await rolesTask;
            _eventCategories = await eventCategoriesTask;
            Groups = await groupsTask;
            _qualifications = await qualificationsTask;
            GroupByGroup = await _localStorage.GetItemAsync<bool>(_groupByGroupKey);
            GroupByEventCategory = await _localStorage.GetItemAsync<bool>(_groupByEventCategoryKey);
            HidePastEvents = await _localStorage.GetItemAsync<bool>(_hidePastEventsKey);
            HideEventsWithoutEntering = await _localStorage.GetItemAsync<bool>(_hideEventsWithoutEnteringKey);
            ShowAllRoles = await _localStorage.GetItemAsync<bool>(_showAllRolesKey);
            await LoadEventData();
            IsPageLoading = false;
        }

        protected async Task LoadEventData()
        {
            var helpersTask = _helperService.GetAll(_departmentId);
            var eventTask = _eventService.GetAll(_departmentId);

            events = await eventTask;
            Helpers = await helpersTask;
            RecalculateGrouping();
            StateHasChanged();
        }

        public string GetGroupHeading(IGrouping<object, Models.Event> group)
        {
            var unknown = "Sonstiges";
            var groupName = string.Empty;
            var eventCategoryName = string.Empty;
            switch (GroupByGroup, GroupByEventCategory)
            {
                case (true, true):
                    var key1 = ((Group, Models.EventCategory))group.Key;
                    groupName = key1.Item1?.Name;
                    eventCategoryName = key1.Item2?.Name;
                    if(string.IsNullOrEmpty(groupName) && string.IsNullOrEmpty(eventCategoryName))
                        return unknown;
                    else 
                        return $"{groupName ?? unknown} - {eventCategoryName ?? unknown}";
                case (false, true):
                    var key2 = (Models.EventCategory)group.Key;
                    return key2?.Name ?? unknown;
                case (true, false):
                    var key3 = (Group)group.Key;
                    return key3?.Name ?? unknown;
                default:
                    return string.Empty;
            }            
        }

        protected IEnumerable<Role> GetRelevantRoles()
        {
            var allHelpers = FilteredEvents.SelectMany(GetHelpers);
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

        private void RecalculateGrouping()
        {
            _bothEventGrouping = new(() =>
                FilteredEvents.GroupBy<Models.Event, object>(game =>
                {
                    var group = Groups.FirstOrDefault(group => group.Id.Equals(game.GroupId));
                    var eventCategory = _eventCategories.FirstOrDefault(category => category.Id.Equals(game.EventCategoryId));
                    return (group, eventCategory);
                })
                .OrderBy(eventGroup =>
                {
                    var key = ((Models.Group, Models.EventCategory))eventGroup.Key;

                    int groupIndex;
                    if (key.Item1 is Models.Group group)
                        groupIndex = Groups.IndexOf(group);
                    else
                        groupIndex = Groups.Count;

                    int eventCategoryIndex;
                    if (key.Item2 is Models.EventCategory eventCategory)
                        eventCategoryIndex = _eventCategories.IndexOf(eventCategory);
                    else
                        eventCategoryIndex = _eventCategories.Count;

                    return groupIndex * Groups.Count + eventCategoryIndex;

                }).ToList()
            );
            _groupEventGrouping = new(() =>
                FilteredEvents.GroupBy<Models.Event, object>(game =>
                    Groups.FirstOrDefault(group => group.Id.Equals(game.GroupId)))
                .OrderBy(eventGroup =>
                {
                    if (eventGroup.Key is Models.Group group)
                        return Groups.IndexOf(group);
                    else
                        return Groups.Count;
                }).ToList());
            _categoryEventGrouping = new(() =>
                FilteredEvents.GroupBy<Models.Event, object>(game =>
                    _eventCategories.FirstOrDefault(category => category.Id.Equals(game.EventCategoryId)))
                .OrderBy(eventGroup =>
                {
                    if (eventGroup.Key is Models.EventCategory eventCategory)
                        return _eventCategories.IndexOf(eventCategory);
                    else
                        return _eventCategories.Count;
                }).ToList());
        }

        protected Role GetRoleById(string roleId) => _roles.Find(role => role.Id == roleId);

        protected Group GetGroupById(string groupId) => Groups.Find(group => group.Id == groupId);

        protected Models.EventCategory GetEventCategoryById(string categoryId) => _eventCategories.FirstOrDefault(category => category.Id == categoryId);

        protected List<Models.Helper> GetHelpers(Models.Event @event)
        {
            if (@event == null)
                return null;
            return Helpers.Where(helper => helper.EventId == @event.Id).ToList();
        }

        private async Task SaveGame(string? eventId, string? groupId, string? eventCategoryId, DateTime gameDate, Geolocation? place, Dictionary<string, Tuple<int, DateTime, List<string>, Dictionary<string, int>>> helpers, bool dateHasChanged)
        {
            var sendChangesFunc = async (bool removeMembers) =>
            {
                await _eventService.UpdateOrCreate(_departmentId, eventId, groupId, eventCategoryId, gameDate, place, helpers, removeMembers);
                await LoadEventData();
                if(string.IsNullOrEmpty(eventId))
                    _toastService.Notify(new ToastMessage(ToastType.Primary, $"Das Event wurde erstellt."));
                else if(removeMembers)
                    _toastService.Notify(new ToastMessage(ToastType.Primary, $"Das Event wurde aktualisiert. Die eingetragenen Helfer wurden entfernt."));
                else
                    _toastService.Notify(new ToastMessage(ToastType.Primary, $"Das Event wurde aktualisiert."));
            };

            if(dateHasChanged)
            {
                var closeModalFunc = Modal.HideAsync;
                var parameters = new Dictionary<string, object>
                {
                    { nameof(YesNoModal.Text), "Das Datum des Events wird verändert. Sollen die Eintragungen zu verfügbaren und fest eingetragenen Helfern entfernt werden?" },
                    { nameof(YesNoModal.TrueButtonText), "Entfernen" },
                    { nameof(YesNoModal.FalseButtonText), "Einträge belassen" },
                    { nameof(YesNoModal.ResultFunc), sendChangesFunc },
                    { nameof(YesNoModal.CloseModalFunc), closeModalFunc }
                };
                await Modal.ShowAsync<YesNoModal>(title: "Eintragungen entfernen", parameters: parameters);
            }
            else
            {
                await sendChangesFunc(false);
            }            
        }

        protected void OpenGame(Models.Event @event)
        {
            _navigationManager.NavigateTo($"./{DepartmentUrl}/event/{@event.Id}");
        }

        public async Task EditGame(Models.Event? @event)
        {
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeEvent.DepartmentId), _departmentId },
                { nameof(ChangeEvent.Event), @event },
                { nameof(ChangeEvent.CloseModalFunc), Modal.HideAsync },
                { nameof(ChangeEvent.DeleteGameFunc), DeleteGame },
                { nameof(ChangeEvent.SaveEventFunc), SaveGame },
                { nameof(ChangeEvent.Roles), _roles },
                { nameof(ChangeEvent.EventCategories), _eventCategories },
                { nameof(ChangeEvent.Groups), Groups },
                { nameof(ChangeEvent.Qualifications), _qualifications }
            };
            await Modal.ShowAsync<ChangeEvent>(title: @event == null ? "Event erstellen" : "Event bearbeiten", parameters: parameters);
        }

        private async Task DeleteGame(string gameId)
        {
            await _eventService.DeleteGame(_departmentId, gameId);
            await LoadEventData();
        }
    }
}
