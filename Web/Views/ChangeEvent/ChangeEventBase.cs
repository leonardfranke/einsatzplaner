﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class ChangeEventBase : ComponentBase
    {
        [Parameter]
        public string DepartmentId { get; set; }

        [Inject]
        private IJSRuntime js { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private IRequirementGroupService _requirementGroupService { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Parameter]
        public Event? Event { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteGameFunc { get; set; }

        public List<Group> Groups { get; private set; }

        public List<Role> Roles { get; private set; }

        public List<EventCategory> EventCategories { get; private set; }

        public List<RequirementGroup> RequirementGroups { get; private set; }

        [Parameter]
        public Func<string?, string?, string?, DateTime, Dictionary<string, Tuple<int, DateTime, List<string>>>, Task> SaveEventFunc { get; set; }

        public ElementReference GroupSelect;

        public ElementReference EventCategorySelect;

        [SupplyParameterFromForm]
        public FormModel EventData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;

        protected override void OnParametersSet()
        {
            CreateFormContext();
        }

        protected override async Task OnParametersSetAsync()
        {
            Roles = await _roleService.GetAll(DepartmentId);
            Groups = await _groupService.GetAll(DepartmentId);
            Roles = await _roleService.GetAll(DepartmentId);
            EventCategories = await _eventCategoryService.GetAll(DepartmentId);
            RequirementGroups = await _requirementGroupService.GetAll(DepartmentId);
            
            IsUpdate = Event != null;
            if (IsUpdate)
            {
                EventData.GroupId = Event.GroupId;
                EventData.EventCategoryId = Event.EventCategoryId;
                EventData.Date = Event.GameDate;
                var helpers = await _helperService.GetAll(Event.DepartmentId, Event.Id);
                    
                foreach (var helper in helpers)
                {
                    var lockingTime = helper.LockingTime;
                    var lockingPeriod = Event.GameDate.Subtract(lockingTime).Days;
                    AddCategoryToGame(helper.RoleId, helper.RequiredAmount, lockingPeriod, helper.RequiredGroups);
                }
            }
            else
            {
                EventData.Date = DateTime.Today.AddDays(1);
            }
        }

        private void CreateFormContext()
        {
            EventData = new();
            EditContext = new(EventData);
            _messageStore = new(EditContext);
        }

        public async Task SaveGame()
        {
            var categoryData = new Dictionary<string, Tuple<int, DateTime, List<string>>>();
            foreach (var helper in EventData.Helpers)
            {
                var lockingTime = EventData.Date.AddDays(-helper.LockingPeriod);
                categoryData.Add(helper.HelperCategoryId, new(helper.RequiredAmount, lockingTime, helper.RequiredGroups));
            }

            await SaveEventFunc(Event?.Id, EventData.GroupId, EventData.EventCategoryId, EventData.Date, categoryData);
            await CloseModal();
        }

        public async Task DeleteGame()
        {
            await DeleteGameFunc(Event.Id);
            await CloseModal();
        }

        public Task CloseModal() => CloseModalFunc();

        public Role GetCategoryById(string id) => Roles.Find(category => category.Id == id);

        public string GetGroupDisplayText(RequirementGroup helperCategoryGroup)
        {
            var names = helperCategoryGroup.Requirements.Select(requirement =>
            {
                var category = GetCategoryById(requirement.Key);
                return $"{requirement.Value}x {category?.Name ?? "Kein Name"}";
            });
            return string.Join(Environment.NewLine, names);
        }

        public void SetHelperGroup(RequirementGroup helperCategoryGroup)
        {
            ClearCategories();
            foreach (var requirement in helperCategoryGroup.Requirements)
            {
                AddCategoryToGame(requirement.Key, (int)requirement.Value);
            }
        }

        public void ClearCategories()
        {
            EventData.Helpers.Clear();
        }

        public void AddCategoryToGame(string categoryId, int requiredAmount, int? lockingPeriod = null, List<string> requiredGroups = null)
        {
            var category = GetCategoryById(categoryId);
            var defaultLockingPeriod = category.LockingPeriod;
            EventData.Helpers.Add(new HelperFormModel
            {
                HelperCategoryId = categoryId,
                RequiredAmount = requiredAmount,
                LockingPeriod = lockingPeriod ?? defaultLockingPeriod,
                RequiredGroups = requiredGroups ?? new()
            });
        }

        public void RemoveCategoryFromGame(HelperFormModel helperForm)
        {
            EventData.Helpers.Remove(helperForm);
        }

        private HelperFormModel GetHelperFormModel(string categoryId)
        {
            return EventData.Helpers.FirstOrDefault(helper => helper.HelperCategoryId == categoryId);
        }

        public void SetLockingPeriod(HelperFormModel helperForm, string value)
        {
            if (!int.TryParse(value, out int period))
                period = 1;
            helperForm.LockingPeriod = period;
        }

        public void SetRequiredAmount(HelperFormModel helperForm, string value)
        {
            if (!int.TryParse(value, out int amount))
                amount = 1;
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
            public DateTime Date { 
                get => _date.ToLocalTime(); 
                set 
                {
                    var date = value;
                    if(date.Kind == DateTimeKind.Unspecified)
                        date = new DateTime(date.Ticks, DateTimeKind.Local);
                    _date = date;
                } }
            public List<HelperFormModel> Helpers { get; set; } = new();
        }

        public class HelperFormModel
        {
            public string HelperCategoryId { get; set; }
            public int LockingPeriod { get; set; }
            public int RequiredAmount { get; set; }
            public List<string> RequiredGroups { get; set; } = new();
        }
    }
}
