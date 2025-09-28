using DTO;
using LeafletForBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class ChangeEventBase : ComponentBase
    {
        [Parameter]
        public string DepartmentId { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        [Inject]
        private IRequirementGroupService _requirementGroupService { get; set; }

        [Parameter]
        public Event? Event { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteGameFunc { get; set; }

        [Parameter]
        public List<Role> Roles { get;  set; }

        [Parameter]
        public List<EventCategory> EventCategories { get;  set; }

        [Parameter]
        public List<Group> Groups { get; set; }

        public List<RequirementGroup> RequirementGroups { get; set; }

        [Parameter]
        public List<Qualification> Qualifications { get; set; }

        public RealTimeMap EventMap { get; set; }

        public bool IsEventSaving { get; set; }
        public bool IsEventDeleting { get; set; }
        public bool IsEventLoading { get; set; }

        [Parameter]
        public Func<string?, string?, string?, DateTime?, string?, double?, double?, string?, Dictionary<string, Tuple<int, DateTime, List<string>, Dictionary<string, int>>>, bool, Task> SaveEventFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel EventData { get; set; }
        public EditContext EditContext { get; set; }
        public bool IsUpdate { get; set; }

        protected override void OnParametersSet()
        {
            EventData = new();
        }

        protected override async Task OnParametersSetAsync()
        {
            IsEventLoading = true;
            var requirementGroupsTask = _requirementGroupService.GetAll(DepartmentId);
            IsUpdate = Event != null;
            if (IsUpdate)
            {
                EventData.GroupId = Event.GroupId;
                EventData.EventCategoryId = Event.EventCategoryId;
                EventData.Date = Event.EventDate.Date;
                EventData.Begin = Event.EventDate.TimeOfDay;
                var helpers = await _helperService.GetAll(Event.DepartmentId, Event.Id);
                    
                foreach (var helper in helpers)
                {
                    var lockingTime = helper.LockingTime;
                    var lockingPeriod = (int)Event.EventDate.Subtract(lockingTime).Days;
                    AddCategoryToGame(helper.RoleId, (int)helper.RequiredAmount, lockingPeriod, helper.RequiredGroups, helper.RequiredQualifications);
                }
            }
            else
            {
                EventData.Date = DateTime.Today.AddDays(1);
            }
            RequirementGroups = await requirementGroupsTask;
            IsEventLoading = false;
        }

        public async Task SaveGame()
        {
            IsEventSaving = true;
            var categoryData = new Dictionary<string, Tuple<int, DateTime, List<string>, Dictionary<string, int>>>();
            var beginDatetimeToSave = EventData.Date.Value.Date + EventData.Begin.Value;
            foreach (var helper in EventData.Helpers)
            {
                var lockingTime = beginDatetimeToSave.AddDays(-helper.LockingPeriod);
                var requiredGroups = helper.RestrictGroups ? helper.RequiredGroups : new();
                categoryData.Add(helper.RoleId, new(helper.RequiredAmount, lockingTime, requiredGroups, helper.RequiredQualifications));
            }

            var dateHasChanged = beginDatetimeToSave != Event?.EventDate;
            var showRemoveMembersModal = IsUpdate && dateHasChanged;
            await SaveEventFunc(
                Event?.Id, 
                EventData.GroupId, 
                EventData.EventCategoryId, 
                dateHasChanged ? beginDatetimeToSave : null,
                EventData.LocationId,
                EventData.Latitude,
                EventData.Longitude,
                EventData.LocationText,
                categoryData,
                showRemoveMembersModal);
            if(!showRemoveMembersModal)
                await CloseModal();
            IsEventSaving = false;
        }

        public async Task DeleteGame()
        {
            IsEventDeleting = true;
            await DeleteGameFunc(Event.Id);
            await CloseModal();
            IsEventDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public Role GetRoleById(string id) => Roles.Find(role => role.Id == id);

        public void SetHelperGroup(RequirementGroup helperCategoryGroup)
        {
            ClearCategories();
            var groupedQualificationRequirements = helperCategoryGroup.RequirementsQualifications
                .ToDictionary(pair => Qualifications.First(qualification => qualification.Id == pair.Key), pair => pair.Value)
                .GroupBy(pair => pair.Key.RoleId);
            foreach (var requirement in helperCategoryGroup.RequirementsRoles)
            {
                AddCategoryToGame(requirement.Key, requirement.Value, requiredQualifications: groupedQualificationRequirements.FirstOrDefault(group => group.Key == requirement.Key)?.ToDictionary(pair => pair.Key.Id, pair => pair.Value));
            }
        }

        public void ClearCategories()
        {
            EventData.Helpers.Clear();
        }

        public void AddCategoryToGame(string categoryId, int requiredAmount, int? lockingPeriod = null, List<string> requiredGroups = null, Dictionary<string, int> requiredQualifications = null)
        {
            var category = GetRoleById(categoryId);
            var defaultLockingPeriod = category?.LockingPeriod ?? 0;
            var newRequirement = new HelperFormModel
            {
                RoleId = categoryId,
                RequiredAmount = requiredAmount,
                LockingPeriod = lockingPeriod ?? defaultLockingPeriod,
                RequiredGroups = requiredGroups ?? new(),
                RequiredQualifications = requiredQualifications?.ToDictionary(pair => pair.Key, pair => (int)pair.Value) ?? new()
            };
            newRequirement.RestrictGroups = newRequirement.RequiredGroups.Any();
            EventData.Helpers.Add(newRequirement);
        }

        public void AddQualificationToRole(HelperFormModel helperForm, string qualificationId)
        {
            helperForm.RequiredQualifications.Add(qualificationId, 1);
        }

        public void RemoveQualificationFromRole(HelperFormModel helperForm, string qualificationId)
        {
            helperForm.RequiredQualifications.Remove(qualificationId);
        }        

        public void RemoveCategoryFromGame(HelperFormModel helperForm)
        {
            EventData.Helpers.Remove(helperForm);
        }

        public IEnumerable<Qualification> GetQualificationsOfRole(string roleId)
        {
            return Qualifications.Where(qualification => qualification.RoleId == roleId);
        }

        public void SetLockingPeriod(HelperFormModel helperForm, int period)
        {
            helperForm.LockingPeriod = period;
        }

        public void SetRequiredAmount(HelperFormModel helperForm, int amount)
        {
            helperForm.RequiredAmount = amount;
        }

        public void SetRequiredGroup(HelperFormModel helperForm, string groupId, bool toAdd)
        {            
            if (toAdd)
                helperForm.RequiredGroups.Add(groupId);
            else
                helperForm.RequiredGroups.Remove(groupId);
        }

        public bool IsGroupSetForCategory(HelperFormModel helperForm, string groupId)
        {
            return helperForm.RequiredGroups.Contains(groupId);
        }

        public class FormModel
        {
            private DateTime _date = DateTime.Now.Date;
            public string GroupId { get; set; } = "";
            public string EventCategoryId { get; set; } = "";
            public DateTime? Date { 
                get => _date.ToLocalTime(); 
                set 
                {
                    var date = value;
                    if(date.Value.Kind == DateTimeKind.Unspecified)
                        date = new DateTime(date.Value.Ticks, DateTimeKind.Local);
                    _date = date.Value;
                } }

            public TimeSpan? Begin { get; set; }
            public TimeSpan? End { get; set; }
            public string? LocationId { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string? LocationText { get; set; }
            public List<HelperFormModel> Helpers { get; set; } = new();
        }

        public class HelperFormModel
        {
            public string RoleId { get; set; }
            public int LockingPeriod { get; set; }
            public int RequiredAmount { get; set; }
            public bool RestrictGroups { get; set; }
            public List<string> RequiredGroups { get; set; } = new();
            public Dictionary<string, int> RequiredQualifications { get; set; } = new();
        }
    }
}
