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

        public EventManager(IFirestoreManager firestoreManager, IHelperManager helperManager)
        {
            _firestoreDb = firestoreManager.Database;
            _helperManager = helperManager;
        }

        public async Task UpdateOrCreate(UpdateEventDTO updateEventDTO)
        {
            var departmentId = updateEventDTO.DepartmentId;
            var eventId = updateEventDTO.EventId;
            var dateUTC = updateEventDTO.Date?.ToUniversalTime();
            var groupId = updateEventDTO.GroupId;
            var eventCategoryId = updateEventDTO.EventCategoryId;
            var updateHelpers = updateEventDTO.Helpers;

            var eventsRef = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT);
            DocumentReference eventRef;


            var tasks = new List<Task>();
            if (string.IsNullOrEmpty(eventId))
            {
                if (dateUTC == null)
                    throw new ArgumentException("Date must be present when creating a new event.");

                var newEvent = new Event
                {
                    GroupId = groupId,
                    EventCategoryId = eventCategoryId,
                    Date = (DateTime)dateUTC
                };
                eventRef = await eventsRef.AddAsync(newEvent);

                var updateTask = eventRef.UpdateAsync(nameof(Event.IdProperty), eventRef.Id, Precondition.MustExist);
                tasks.Add(updateTask);
            }
            else
            {
                eventRef = eventsRef.Document(eventId);
                var valueToUpdate = new Dictionary<string, object>();
                if(groupId != null)
                    valueToUpdate.Add(nameof(Event.GroupId), groupId);
                if (eventCategoryId != null)
                    valueToUpdate.Add(nameof(Event.EventCategoryId), eventCategoryId);
                if(dateUTC != null)
                    valueToUpdate.Add(nameof(Event.Date), dateUTC);
                var updateTask = eventRef.UpdateAsync(valueToUpdate, Precondition.MustExist);
                tasks.Add(updateTask);
            }

            var helpersRef = eventRef.Collection(Paths.HELPER);

            List<HelperDTO> currentHelpers = new List<HelperDTO>();
            if(!string.IsNullOrEmpty(eventId))
                currentHelpers = await _helperManager.GetAll(departmentId, eventId);

            if(updateHelpers != null)
            {
                foreach (var updateHelper in updateHelpers)
                {
                    var currentHelper = currentHelpers.Find(helper => helper.HelperCategoryId == updateHelper.HelperCategoryId);
                    currentHelpers.Remove(currentHelper);

                    var helperCategoryId = updateHelper.HelperCategoryId;
                    var requiredAmount = updateHelper.RequiredAmount;
                    var requiredGroups = updateHelper.RequiredGroups;
                    var lockingTimeUTC = updateHelper.LockingTime.ToUniversalTime();
                    if (currentHelper == null)
                    {
                        var newHelper = new Helper
                        {
                            HelperCategoryId = helperCategoryId,
                            RequiredAmount = requiredAmount,
                            LockingTime = lockingTimeUTC,
                            RequiredGroups = requiredGroups,
                            QueuedMembers = new(),
                            SetMembers = new()
                        };
                        var addTask = helpersRef.AddAsync(newHelper);
                        tasks.Add(addTask);
                    }
                    else
                    {
                        var helperRef = helpersRef.Document(currentHelper.Id);
                        var updateTask = helperRef.UpdateAsync(new Dictionary<string, object> {
                        { nameof(Helper.HelperCategoryId), helperCategoryId },
                        { nameof(Helper.RequiredAmount), requiredAmount },
                        { nameof(Helper.LockingTime), lockingTimeUTC },
                        { nameof(Helper.RequiredGroups), requiredGroups }
                    }, Precondition.MustExist);
                        tasks.Add(updateTask);
                    }
                }

                foreach (var helper in currentHelpers)
                {
                    var deleteTask = helpersRef.Document(helper.Id).DeleteAsync();
                    tasks.Add(deleteTask);
                }
            }
            
            await Task.WhenAll(tasks);
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

        public async Task<EventDTO?> GetEvent(string eventId)
        {
            var snapshot = await _firestoreDb
                .CollectionGroup(Paths.EVENT).WhereEqualTo(nameof(Event.IdProperty), eventId)
                .Limit(1).GetSnapshotAsync();

            if (snapshot.Count == 0)
                return null;

            var eventModel = snapshot.First();
            var @event = eventModel.ConvertTo<Event>();
            var departmentId = eventModel.Reference.Parent.Parent.Id;
            return EventConverter.Convert(@event, departmentId);            
        }
    }
}
