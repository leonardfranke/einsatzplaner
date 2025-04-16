using BlazorBootstrap;
using Blazored.LocalStorage;
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
        private const string _groupByGroupKey = $"GroupByGroup{nameof(Home)}";
        private const string _groupByEventCategoryKey = $"GroupByEventCategory{nameof(Home)}";
        private const string _hidePastEventsKey = "HidePastEvents";

        [CascadingParameter]
        public Modal Modal { get; set; }

        public List<IGrouping<object, Event>> BothEventGrouping { get => _bothEventGrouping.Value; }
        public List<IGrouping<object, Event>> GroupEventGrouping { get => _groupEventGrouping.Value; }
        public List<IGrouping<object, Event>> CategoryEventGrouping { get => _categoryEventGrouping.Value; }

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
        private IRequirementGroupService _requirementsGroupService { get; set; }

        [Inject]
        private IEventService _eventService { get; set; }

        [Inject]
        private IDepartmentService _departmentService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        public List<string> MemberGroupIds { get; set; }


        public List<Group> Groups { get; private set; }

        public List<Role> Roles { get; private set; }

        public List<Models.Helper> Helpers { get; private set; }

        public Models.Member Member { get; private set; }

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
        public bool HidePastEvents { 
            get => _hidePastEvents;
            set 
            {
                _hidePastEvents = value;
                _localStorage.SetItemAsync(_hidePastEventsKey, value);
                RecalculateGrouping();
            } 
        }

        public string HoveredEventId { get; set; }

        public IEnumerable<Event> FilteredEvents => events?.Where(@event => !HidePastEvents || @event.EventDate.AddDays(2) >= DateTime.Now);

        private List<Event> events;
        private List<Models.EventCategory> eventCategories;
        private List<Models.Member> members;
        private List<string> _memberRoleIds;
        private List<RequirementGroup> helperCategoryGroups;
        private string _departmentId;
        private Lazy<List<IGrouping<object, Event>>> _bothEventGrouping;
        private Lazy<List<IGrouping<object, Event>>> _groupEventGrouping;
        private Lazy<List<IGrouping<object, Event>>> _categoryEventGrouping;
        private bool _hidePastEvents;
        private bool _groupByGroup;
        private bool _groupByEventCategory;

        protected override async Task OnInitializedAsync()
        {
            if(await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(department))
                return;
            var user = await _authManager.GetLocalUser();

            var rolesTask = _roleService.GetAll(_departmentId);
            var groupsTask = _groupService.GetAll(_departmentId);
            var membersTask = _memberService.GetAll(_departmentId);

            var eventCategoriesTask = _eventCategoryService.GetAll(_departmentId);
            members = await membersTask;
            Member = members.FirstOrDefault(member => member.Id == user.Id);
            MemberGroupIds = Member.GroupIds ?? new();
            _memberRoleIds = Member.RoleIds ?? new();
            Roles = await rolesTask;
            eventCategories = await eventCategoriesTask;
            Groups = await groupsTask;
            GroupByGroup = await _localStorage.GetItemAsync<bool>(_groupByGroupKey);
            GroupByEventCategory = await _localStorage.GetItemAsync<bool>(_groupByEventCategoryKey);
            HidePastEvents = await _localStorage.GetItemAsync<bool>(_hidePastEventsKey);
            await LoadEventData();
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

        public string GetGroupHeading(IGrouping<object, Event> group)
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
            var relevantRoles = allHelpers.Select(helper => helper.RoleId).Distinct();
            var relevantPermittedRoles = relevantRoles.Intersect(_memberRoleIds);
            return relevantPermittedRoles.Select(GetRoleById);
        }

        private void RecalculateGrouping()
        {
            _bothEventGrouping = new(() =>
                FilteredEvents.GroupBy<Event, object>(game =>
                {
                    var group = Groups.FirstOrDefault(group => group.Id.Equals(game.GroupId));
                    var eventCategory = eventCategories.FirstOrDefault(category => category.Id.Equals(game.EventCategoryId));
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
                        eventCategoryIndex = eventCategories.IndexOf(eventCategory);
                    else
                        eventCategoryIndex = eventCategories.Count;

                    return groupIndex * Groups.Count + eventCategoryIndex;

                }).ToList()
            );
            _groupEventGrouping = new(() =>
                FilteredEvents.GroupBy<Event, object>(game =>
                    Groups.FirstOrDefault(group => group.Id.Equals(game.GroupId)))
                .OrderBy(eventGroup =>
                {
                    if (eventGroup.Key is Models.Group group)
                        return Groups.IndexOf(group);
                    else
                        return Groups.Count;
                }).ToList());
            _categoryEventGrouping = new(() =>
                FilteredEvents.GroupBy<Event, object>(game =>
                    eventCategories.FirstOrDefault(category => category.Id.Equals(game.EventCategoryId)))
                .OrderBy(eventGroup =>
                {
                    if (eventGroup.Key is Models.EventCategory eventCategory)
                        return eventCategories.IndexOf(eventCategory);
                    else
                        return eventCategories.Count;
                }).ToList());
        }

        protected Role GetRoleById(string roleId) => Roles.Find(role => role.Id == roleId);

        protected Group GetGroupById(string groupId) => Groups.Find(group => group.Id == groupId);

        protected Models.EventCategory GetEventCategoryById(string categoryId) => eventCategories.FirstOrDefault(category => category.Id == categoryId);

        protected List<Models.Helper> GetHelpers(Event @event)
        {
            if (@event == null)
                return null;
            return Helpers.Where(helper => helper.EventId == @event.Id).ToList();
        }

        public async Task CreateGame()
        {
            if(helperCategoryGroups == null)
                helperCategoryGroups = await _requirementsGroupService.GetAll(_departmentId);

            var closeModalFunc = Modal.HideAsync;
            var saveGameFunc = SaveGame;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeEvent.DepartmentId), _departmentId},
                { nameof(ChangeEvent.CloseModalFunc), closeModalFunc },
                { nameof(ChangeEvent.SaveEventFunc), saveGameFunc },
                { nameof(ChangeEvent.Event), null }
            };
            await Modal.ShowAsync<ChangeEvent>(title: "Event bearbeiten", parameters: parameters);
        }

        private async Task SaveGame(string? eventId, string? groupId, string? eventCategoryId, DateTime gameDate, Dictionary<string, Tuple<int, DateTime, List<string>>> helpers, bool dateHasChanged)
        {
            var sendChangesFunc = async (bool removeMembers) =>
            {
                await _eventService.UpdateOrCreate(_departmentId, eventId, groupId, eventCategoryId, gameDate, helpers, removeMembers);
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
            _navigationManager.NavigateTo($"./{DepartmentUrl}/game/{@event.Id}");
        }

        public async Task EditGame(Event? @event)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteGameFunc = DeleteGame;
            var saveGameFunc = SaveGame;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeEvent.DepartmentId), _departmentId },
                { nameof(ChangeEvent.Event), @event },
                { nameof(ChangeEvent.CloseModalFunc), closeModalFunc },
                { nameof(ChangeEvent.DeleteGameFunc), deleteGameFunc },
                { nameof(ChangeEvent.SaveEventFunc), saveGameFunc }
            };
            await Modal.ShowAsync<ChangeEvent>(title: "Event bearbeiten", parameters: parameters);
        }

        private async Task DeleteGame(string gameId)
        {
            await _eventService.DeleteGame(_departmentId, gameId);
            await LoadEventData();
        }
    }
}
