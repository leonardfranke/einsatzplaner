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
        private ITaskManager _taskManager;

        public EventManager(FirestoreDb firestoreDb, IHelperManager helperManager, ITaskManager taskManager)
        {
            _firestoreDb = firestoreDb;
            _helperManager = helperManager;
            _taskManager = taskManager;
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

            var optimizerDatetimes = new HashSet<DateTime>();
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
                        var newHelper = new Helper
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
                            { nameof(Helper.RoleId), roleId },
                            { nameof(Helper.RequiredAmount), requiredAmount },
                            { nameof(Helper.LockingTime), lockingTimeUTC },
                            { nameof(Helper.RequiredGroups), requiredGroups },
                            { nameof(Helper.RequiredQualifications), requiredQualifications }
                        };
                        if (removeMembers)
                        {
                            updateDict.Add(nameof(Helper.LockedMembers), new List<string>());
                            updateDict.Add(nameof(Helper.PreselectedMembers), new List<string>());
                            updateDict.Add(nameof(Helper.AvailableMembers), new List<string>());
                        }
                        var helperRef = helpersRef.Document(currentHelper.Id);
                        var updateTask = helperRef.UpdateAsync(updateDict);
                        dataChangesTasks.Add(updateTask);
                    }
                    optimizerDatetimes.Add(lockingTime);
                }

                foreach (var helper in currentHelpers)
                {
                    var deleteTask = helpersRef.Document(helper.Id).DeleteAsync();
                    dataChangesTasks.Add(deleteTask);
                }
            }
            
            await Task.WhenAll(dataChangesTasks);  
            foreach(var lockingTime in optimizerDatetimes)
            {
                await _taskManager.TriggerRecalculation(departmentId, lockingTime);
            }
        }

        public async Task DeleteEvent(string departmentId, string eventId)
        {
            var eventRef = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId);
          
            var helpers = eventRef.Collection(Paths.HELPER).ListDocumentsAsync();
            await helpers.ForEachAwaitAsync(helper => helper.DeleteAsync());
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
