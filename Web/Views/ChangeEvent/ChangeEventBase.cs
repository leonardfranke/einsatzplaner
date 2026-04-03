using DTO;
using LeafletForBlazor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Models;
using Web.Services;
using Web.Views.BasicModals;

namespace Web.Views
{
    public class ChangeEventBase : ComponentBase
    {
        [CascadingParameter]
        public IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string DepartmentId { get; set; }

        [Inject]
        private IEventService _eventService { get; set; }

        [Inject]
        private IRequirementService _requirementService { get; set; }

        [Inject]
        private IRequirementGroupService _requirementGroupService { get; set; }

        [Inject]
        private IDialogService _dialogService { get; set; }

        [Parameter]
        public Event? Event { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteGameFunc { get; set; }

        [Parameter]
        public List<Models.Location> Locations { get; set; }

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

        public string? GroupId { get; set; }
        public string? EventCategoryId { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Begin { get; set; }
        public string? LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationText { get; set; }
        public List<RequirementForm> RequirementForms { get; set; } = new();
        public List<RequirementForm> OldRequirementForms { get; set; } = new();

        public bool IsUpdate => Event != null;

        protected override async Task OnParametersSetAsync()
        {
            IsEventLoading = true;
            var requirementGroupsTask = _requirementGroupService.GetAll(DepartmentId);
            if (IsUpdate)
            {
                GroupId = Event.GroupId;
                EventCategoryId = Event.EventCategoryId;
                Date = Event.EventDate.Date;
                Begin = Event.EventDate.TimeOfDay;
                Latitude = Event.Latitude;
                Longitude = Event.Longitude;
                LocationId = Event.LocationId;
                LocationText = Event.LocationText;
                var requirements = await _requirementService.GetAll(Event.DepartmentId, Event.Id);
                    
                foreach (var requirement in requirements)
                {
                    var lockingTime = requirement.LockingTime;
                    var lockingPeriod = Event.EventDate.Subtract(lockingTime).Days;
                    AddRequirementToEvent(false, requirement.RoleId, requirement.RequiredAmount, lockingPeriod, requirement.RequiredGroups, requirement.RequiredQualifications);
                    AddRequirementToEvent(true, requirement.RoleId, requirement.RequiredAmount, lockingPeriod, requirement.RequiredGroups, requirement.RequiredQualifications);
                }
            }
            else
            {
                Date = DateTime.Today.AddDays(1);
            }
            RequirementGroups = await requirementGroupsTask;
            IsEventLoading = false;
        }

        public void PositionChanged(object value)
        {
            if (value is LocationInfo locationInfo)
            {
                LocationText = locationInfo.Text;
                Latitude = locationInfo.Latitude;
                Longitude = locationInfo.Longitude;
                LocationId = null;
            } 
            else if(value is Models.Location location)
            {
                LocationText = null;
                Latitude = null;
                Longitude = null;
                LocationId = location.Id;
            }
        }

        public async Task SaveGame()
        {
            IsEventSaving = true;

            var beginDateTime = Date.Value.Date + Begin.Value;
            var dateHasChanged = beginDateTime != Event?.EventDate;
            var showRemoveMembersModal = IsUpdate && dateHasChanged;
            
            var removeMembers = false;
            if (dateHasChanged)
            {
                var parameter = new DialogParameters<YesNoModal>()
                {
                    { nameof(YesNoModal.Text), "Das Datum des Events wird verändert. Sollen die Eintragungen zu verfügbaren und fest eingetragenen Helfern entfernt werden?" },
                    { nameof(YesNoModal.TrueButtonText), "Entfernen" },
                    { nameof(YesNoModal.FalseButtonText), "Einträge belassen" },
                };
                var dialog = await _dialogService.ShowAsync<YesNoModal>(title: "Eintragungen entfernen", parameter);
                var res = await dialog.Result;
                removeMembers = (bool)res.Data;
            }            

            await _eventService.CreateOrUpdate(new UpdateEventDTO
            {
                DepartmentId = DepartmentId,
                EventId = Event?.Id,
                GroupId = GroupId,
                EventCategoryId = EventCategoryId,
                Date = beginDateTime,
                LocationId = LocationId,
                Latitude = Latitude,
                Longitude = Longitude,
                LocationText = LocationText,
                RemoveMembers = removeMembers
            });

            var currentRequiredRoles = RequirementForms.Select(requirement => requirement.RoleId);
            var oldRequiredRoles = OldRequirementForms.Select(requirement => requirement.RoleId);
            var rolesToAdd = currentRequiredRoles.Except(oldRequiredRoles);
            var rolesToUpdate = currentRequiredRoles.Intersect(oldRequiredRoles);
            var rolesToRemove = oldRequiredRoles.Except(currentRequiredRoles);

            foreach(var requirement in RequirementForms.Where(requirement => rolesToAdd.Contains(requirement.RoleId)))
            {
                await _requirementService.CreateRequirement(new UpdateRequirementDTO
                {
                    DepartmentId = DepartmentId,
                    EventId = Event.Id,
                    RoleId = requirement.RoleId,
                    RequiredAmount = requirement.RequiredAmount,
                    RecommendedGroups = requirement.RequiredGroups,
                    LockingTime = beginDateTime.AddDays(-requirement.LockingPeriod)
                });

                foreach(var qualification in requirement.RequiredQualifications)
                {
                    await _requirementService.CreateOrUpdateQualificationRequirement(new UpdateQualificationRequirementDTO
                    {
                        DepartmentId = DepartmentId,
                        EventId = Event.Id,
                        RoleId = requirement.RoleId,
                        QualificationId = qualification.Key,
                        RequiredAmount = qualification.Value
                    });
                }
            }

            foreach (var requirement in RequirementForms.Where(requirement => rolesToRemove.Contains(requirement.RoleId)))
            {
                await _requirementService.DeleteRequirement(DepartmentId, Event.Id, requirement.RoleId);
            }

            foreach (var requirement in RequirementForms.Where(requirement => rolesToUpdate.Contains(requirement.RoleId)))
            {
                var oldRequirement = OldRequirementForms.FirstOrDefault(r => r.RoleId == requirement.RoleId);

                var updateRequirementDTO = new UpdateRequirementDTO
                {
                    DepartmentId = DepartmentId,
                    EventId = Event.Id,
                    RoleId = requirement.RoleId
                };
                if (requirement.RequiredAmount != oldRequirement.RequiredAmount)
                    updateRequirementDTO.RequiredAmount = requirement.RequiredAmount;
                if (requirement.LockingPeriod != oldRequirement.LockingPeriod)
                    updateRequirementDTO.LockingTime = beginDateTime.AddDays(-requirement.LockingPeriod);
                if (!requirement.RequiredGroups.ToHashSet().SetEquals(oldRequirement.RequiredGroups))
                    updateRequirementDTO.RecommendedGroups = requirement.RequiredGroups;

                if(updateRequirementDTO.RequiredAmount != null || updateRequirementDTO.LockingTime != null || updateRequirementDTO.RecommendedGroups != null)
                    await _requirementService.UpdateRequirement(updateRequirementDTO);

                var currentRequiredQualifications = requirement.RequiredQualifications.Keys;
                var oldRequiredQualifications = oldRequirement.RequiredQualifications.Keys;
                var qualificationsToAdd = currentRequiredQualifications.Except(oldRequiredQualifications);
                var qualificationsToUpdate = currentRequiredQualifications.Intersect(oldRequiredQualifications);
                var qualificationsToRemove = oldRequiredQualifications.Except(currentRequiredQualifications);

                foreach (var qualificationId in qualificationsToAdd)
                {
                    var requiredAmount = requirement.RequiredQualifications[qualificationId];
                    await _requirementService.CreateOrUpdateQualificationRequirement(new UpdateQualificationRequirementDTO
                    {
                        DepartmentId = DepartmentId,
                        EventId = Event.Id,
                        RoleId = requirement.RoleId,
                        QualificationId = qualificationId,
                        RequiredAmount = requiredAmount
                    });
                }

                foreach (var qualificationId in qualificationsToRemove)
                {
                    await _requirementService.DeleteQualificationRequirement(DepartmentId, Event.Id, requirement.RoleId, qualificationId);
                }

                foreach (var qualificationId in qualificationsToUpdate)
                {
                    var newRequiredAmount = requirement.RequiredQualifications[qualificationId];
                    var oldRequiredAmount = oldRequirement.RequiredQualifications[qualificationId];
                    if (newRequiredAmount != oldRequiredAmount)
                    {
                        await _requirementService.CreateOrUpdateQualificationRequirement(new UpdateQualificationRequirementDTO
                        {
                            DepartmentId = DepartmentId,
                            EventId = Event.Id,
                            RoleId = requirement.RoleId,
                            QualificationId = qualificationId,
                            RequiredAmount = newRequiredAmount
                        });
                    }
                }
            }                        
            IsEventSaving = false;
            MudDialog.Close(removeMembers);
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
                AddRequirementToEvent(false, requirement.Key, requirement.Value, requiredQualifications: groupedQualificationRequirements.FirstOrDefault(group => group.Key == requirement.Key)?.ToDictionary(pair => pair.Key.Id, pair => pair.Value));
            }
        }

        public void ClearCategories()
        {
            RequirementForms.Clear();
        }        

        public void AddRequirementToEvent(bool saveAsOld, string roleId, int requiredAmount, int? lockingPeriod = null, List<string> requiredGroups = null, Dictionary<string, int> requiredQualifications = null)
        {
            var role = GetRoleById(roleId);
            var newRequirement = new RequirementForm
            {
                RoleId = roleId,
                RequiredAmount = requiredAmount,
                LockingPeriod = lockingPeriod ?? role?.LockingPeriod ?? 0,
                RequiredGroups = requiredGroups ?? new(),
                RequiredQualifications = requiredQualifications?.ToDictionary(pair => pair.Key, pair => (int)pair.Value) ?? new()
            };
            newRequirement.RestrictGroups = newRequirement.RequiredGroups.Any();

            if(saveAsOld)
                OldRequirementForms.Add(newRequirement);
            else
                RequirementForms.Add(newRequirement);
        }

        public void AddQualificationToRole(RequirementForm helperForm, string qualificationId)
        {
            helperForm.RequiredQualifications.Add(qualificationId, 1);
        }

        public void RemoveQualificationFromRole(RequirementForm helperForm, string qualificationId)
        {
            helperForm.RequiredQualifications.Remove(qualificationId);
        }        

        public void RemoveCategoryFromGame(RequirementForm helperForm)
        {
            RequirementForms.Remove(helperForm);
        }

        public IEnumerable<Qualification> GetQualificationsOfRole(string roleId)
        {
            return Qualifications.Where(qualification => qualification.RoleId == roleId);
        }

        public void SetLockingPeriod(RequirementForm helperForm, int period)
        {
            helperForm.LockingPeriod = period;
        }

        public void SetRequiredAmount(RequirementForm helperForm, int amount)
        {
            helperForm.RequiredAmount = amount;
        }

        public void SetRequiredGroup(RequirementForm helperForm, string groupId, bool toAdd)
        {            
            if (toAdd)
                helperForm.RequiredGroups.Add(groupId);
            else
                helperForm.RequiredGroups.Remove(groupId);
        }

        public bool IsGroupSetForCategory(RequirementForm helperForm, string groupId)
        {
            return helperForm.RequiredGroups.Contains(groupId);
        }

        public class RequirementForm
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
