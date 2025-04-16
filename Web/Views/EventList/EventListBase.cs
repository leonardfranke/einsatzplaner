using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class EventListBase : ComponentBase
    {

        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter] 
        public string DepartmentId { get; set; }

        [Parameter]
        public IEnumerable<Event> Events { get; set; }

        public IEnumerable<Models.Helper> Helpers { private get; set; }

        public IEnumerable<Group> Groups { private get; set; } = new List<Group>();

        public IEnumerable<EventCategory> EventCategories { private get; set; } = new List<EventCategory>();

        public IEnumerable<Role> Roles { private get; set; }

        [Parameter]
        public Func<Task> ReloadPage { private get; set; }

        [Parameter]
        public Member Member { get; set; }

        [Parameter]
        public bool ShowGroup { get; set; }

        [Parameter]
        public bool ShowEventCategory { get; set; }

        public string HoveredEventId { get; set; }

        [Inject]
        private NavigationManager _navigationManager { get; set; }

        [Inject]
        private IEventService _eventService { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IRoleService _rolesService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Roles = await _rolesService.GetAll(DepartmentId);
            Helpers = await _helperService.GetAll(DepartmentId);
            Groups = await _groupService.GetAll(DepartmentId);
            EventCategories = await _eventCategoryService.GetAll(DepartmentId);
        }

        protected Role GetCategory(Models.Helper helper) => Roles.FirstOrDefault(category => category.Id == helper.RoleId);

        protected List<Models.Helper> GetHelpers(Event @event)
        {
            if (@event == null)
                return null;
            if (Helpers == null)
                return new();
            return Helpers.Where(helper => helper.EventId == @event.Id).ToList();
        }

        protected Group GetGroupById(string groupId) => Groups.FirstOrDefault(group => group.Id == groupId);

        protected EventCategory GetEventCategoryById(string categoryId) => EventCategories.FirstOrDefault(category => category.Id == categoryId);

        protected void OpenGame(Models.Event @event)
        {
            _navigationManager.NavigateTo($"./game/{@event.Id}");
        }

        public async Task EditGame(Event? @event)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteGameFunc = DeleteGame;
            var saveGameFunc = SaveGame;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeEvent.ChangeEvent.DepartmentId), DepartmentId },
                { nameof(ChangeEvent.ChangeEvent.Event), @event },
                { nameof(ChangeEvent.ChangeEvent.CloseModalFunc), closeModalFunc },
                { nameof(ChangeEvent.ChangeEvent.DeleteGameFunc), deleteGameFunc },
                { nameof(ChangeEvent.ChangeEvent.SaveEventFunc), saveGameFunc }
            };
            await Modal.ShowAsync<ChangeEvent.ChangeEvent>(title: "Event bearbeiten", parameters: parameters);
        }

        private async Task DeleteGame(string gameId)
        {
            await _eventService.DeleteGame(DepartmentId, gameId);
            await ReloadPage();
        }

        private async Task SaveGame(string? eventId, string? groupId, string? eventCategoryId, DateTime gameDate, Dictionary<string, Tuple<int, DateTime, List<string>>> helpers)
        {
            //await _eventService.UpdateOrCreate(DepartmentId, eventId, groupId, eventCategoryId, gameDate, helpers);
            await ReloadPage();
        }
    }
}
