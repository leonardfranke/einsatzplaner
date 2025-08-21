using Api.Converter;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class EventManager : IEventManager
    {
        private FirestoreDb _firestoreDb;
        private IHelperManager _helperManager;
        private IHelperNotificationManager _helperNotificationManager;

        public EventManager(FirestoreDb firestoreDb, IHelperManager helperManager, IHelperNotificationManager helperNotificationManager)
        {
            _firestoreDb = firestoreDb;
            _helperManager = helperManager;
            _helperNotificationManager = helperNotificationManager;
        }

        public async Task UpdateOrCreate(UpdateEventDTO updateEventDTO)
        {
            var departmentId = updateEventDTO.DepartmentId;
            var eventId = updateEventDTO.EventId;
            var dateUTC = updateEventDTO.Date?.ToUniversalTime();
            var groupId = updateEventDTO.GroupId;
            var eventCategoryId = updateEventDTO.EventCategoryId;
            var updateHelpers = updateEventDTO.Helpers;
            var removeMembers = updateEventDTO.RemoveMembers;
            GeoPoint? place = updateEventDTO.Place.HasValue ? new GeoPoint(updateEventDTO.Place.Value.Latitude, updateEventDTO.Place.Value.Longitude) : null;

            var eventsRef = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT);
            DocumentReference eventRef;


            var dataChangesTasks = new List<Task>();
            if (string.IsNullOrEmpty(eventId))
            {
                if (dateUTC == null)
                    throw new ArgumentException("Date must be present when creating a new event.");

                var newEvent = new Event
                {
                    GroupId = groupId,
                    EventCategoryId = eventCategoryId,
                    Date = (DateTime)dateUTC,
                    Place = place
                };
                eventRef = await eventsRef.AddAsync(newEvent);
            }
            else
            {
                eventRef = eventsRef.Document(eventId);
                var valueToUpdate = new Dictionary<string, object>
                {
                    { nameof(Event.Place), place }
                };
                if(groupId != null)
                    valueToUpdate.Add(nameof(Event.GroupId), groupId);
                if (eventCategoryId != null)
                    valueToUpdate.Add(nameof(Event.EventCategoryId), eventCategoryId);
                if(dateUTC != null)
                    valueToUpdate.Add(nameof(Event.Date), dateUTC);
                var updateTask = eventRef.UpdateAsync(valueToUpdate, Precondition.MustExist);
                dataChangesTasks.Add(updateTask);
            }

            var helpersRef = eventRef.Collection(Paths.HELPER);

            List<HelperDTO> currentHelpers = new List<HelperDTO>();
            if(!string.IsNullOrEmpty(eventId))
                currentHelpers = await _helperManager.GetAll(departmentId, eventId);

            if (updateHelpers != null)
            {
                foreach (var updateHelper in updateHelpers)
                {
                    var currentHelper = currentHelpers.Find(helper => helper.RoleId == updateHelper.RoleId);
                    currentHelpers.Remove(currentHelper);

                    var roleId = updateHelper.RoleId;
                    var requiredAmount = updateHelper.RequiredAmount;
                    var requiredGroups = updateHelper.RequiredGroups;
                    var lockingTime = updateHelper.LockingTime.Date;
                    var lockingTimeUTC = lockingTime.ToUniversalTime();
                    var requiredQualifications = updateHelper.RequiredQualifications;
                    if (currentHelper == null)
                    {
                        var newHelper = new Requirement
                        {
                            RoleId = roleId,
                            RequiredAmount = requiredAmount,
                            LockingTime = lockingTimeUTC,
                            RequiredGroups = requiredGroups,
                            LockedMembers = new(),
                            PreselectedMembers = new(),
                            AvailableMembers = new(),
                            RequiredQualifications = requiredQualifications
                        };
                        var addTask = helpersRef.AddAsync(newHelper);
                        dataChangesTasks.Add(addTask);
                    }
                    else
                    {
                        var updateDict = new Dictionary<string, object> {
                            { nameof(Requirement.RoleId), roleId },
                            { nameof(Requirement.RequiredAmount), requiredAmount },
                            { nameof(Requirement.LockingTime), lockingTimeUTC },
                            { nameof(Requirement.RequiredGroups), requiredGroups },
                            { nameof(Requirement.RequiredQualifications), requiredQualifications }
                        };
                        if (removeMembers)
                        {
                            updateDict.Add(nameof(Requirement.LockedMembers), new List<string>());
                            updateDict.Add(nameof(Requirement.PreselectedMembers), new List<string>());
                            updateDict.Add(nameof(Requirement.AvailableMembers), new List<string>());
                        }
                        var helperRef = helpersRef.Document(currentHelper.Id);
                        var updateTask = helperRef.UpdateAsync(updateDict);
                        dataChangesTasks.Add(updateTask);
                    }
                }

                foreach (var helper in currentHelpers)
                {
                    var deleteTask = helpersRef.Document(helper.Id).DeleteAsync();
                    var notificationTask = deleteTask.ContinueWith((task) =>
                    {
                        var availableTask = _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, helper.RoleId, helper.AvailableMembers, FirestoreModels.HelperStatus.Available, FirestoreModels.HelperStatus.RequirementDeleted);
                        var preselectedTask = _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, helper.RoleId, helper.PreselectedMembers, FirestoreModels.HelperStatus.Preselected, FirestoreModels.HelperStatus.RequirementDeleted);
                        var lockedTask = _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, helper.RoleId, helper.LockedMembers, FirestoreModels.HelperStatus.Locked, FirestoreModels.HelperStatus.RequirementDeleted);
                        return Task.WhenAll(availableTask, preselectedTask, lockedTask);
                    }, TaskContinuationOptions.NotOnFaulted);                    
                    dataChangesTasks.Add(deleteTask);
                    dataChangesTasks.Add(notificationTask);
                }
            }
            
            await Task.WhenAll(dataChangesTasks);
        }

        public async Task DeleteEvent(string departmentId, string eventId)
        {
            var eventRef = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId);
            var requirementsRef = eventRef.Collection(Paths.HELPER);
            var requirements = await _helperManager.GetAll(departmentId, eventId);

            var tasks = new List<Task>();
            foreach(var requirement in requirements)
            {
                var requirementRef = requirementsRef.Document(requirement.Id);
                var requirementTask = requirementRef.DeleteAsync().ContinueWith(task =>
                {
                    var availableTask = _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, requirement.RoleId, requirement.AvailableMembers, FirestoreModels.HelperStatus.Available, FirestoreModels.HelperStatus.EventDeleted);
                    var preselectedTask = _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, requirement.RoleId, requirement.PreselectedMembers, FirestoreModels.HelperStatus.Preselected, FirestoreModels.HelperStatus.EventDeleted);
                    var lockedTask = _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, requirement.RoleId, requirement.LockedMembers, FirestoreModels.HelperStatus.Locked, FirestoreModels.HelperStatus.EventDeleted);
                    return Task.WhenAll(availableTask, preselectedTask, lockedTask);
                });
                tasks.Add(requirementTask);
            }
            await Task.WhenAll(tasks);
            await eventRef.DeleteAsync();
        }

        public async Task<List<EventDTO>> GetAll(string departmentId)
        {
            var snapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).GetSnapshotAsync();
            var events = new List<Event>();
            foreach (var document in snapshot)
            {
                var @event = document.ConvertTo<Event>();
                if (@event == null)
                    continue;
                events.Add(@event);
            }
            return EventConverter.Convert(events, departmentId);
        }

        public async Task<EventDTO?> GetEvent(string departmentId, string eventId)
        {
            var snapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId).GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            var @event = snapshot.ConvertTo<Event>();
            return EventConverter.Convert(@event, departmentId);            
        }
    }
}
