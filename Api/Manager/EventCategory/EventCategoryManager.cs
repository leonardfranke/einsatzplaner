using Api.Converter;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class EventCategoryManager : IEventCategoryManager
    {
        private FirestoreDb _firestoreDb;

        public EventCategoryManager(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        private CollectionReference GetCollectionReference(string departmentId)
        {
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT_CATEGORY);
        }

        public async Task Delete(string departmentId, string eventCategoryId)
        {
            var groupCollectionReference = GetCollectionReference(departmentId);
            var groupRef = groupCollectionReference.Document(eventCategoryId);
            await groupRef.DeleteAsync();
        }

        public async Task<List<EventCategoryDTO>> GetAll(string departmentId)
        {
            var groupReference = GetCollectionReference(departmentId);
            var snapshot = await groupReference.GetSnapshotAsync();
            var eventCategories = snapshot.Select(doc => doc.ConvertTo<EventCategory>()).ToList();
            return EventCategoryConverter.Convert(eventCategories);
        }

        public Task UpdateOrCreate(string departmentId, string? eventCategoryId, string name)
        {
            var eventCategoryReference = GetCollectionReference(departmentId);
            if (string.IsNullOrEmpty(eventCategoryId))
            {
                var newEventCategory = new EventCategory
                {
                    Name = name
                };

                return eventCategoryReference.AddAsync(newEventCategory);
            }
            else
            {
                return eventCategoryReference.Document(eventCategoryId)
                .UpdateAsync(new Dictionary<string, object> {
                    { nameof(Group.Name), name }
                }, Precondition.MustExist);
            }
        }

        public async Task<EventCategoryDTO> GetById(string departmentId, string eventCategoryId)
        {
            if (string.IsNullOrEmpty(eventCategoryId))
                return null;
            var eventCategoryReference = GetCollectionReference(departmentId)
                .Document(eventCategoryId);
            var snapshot = await eventCategoryReference.GetSnapshotAsync();
            var eventCategory = snapshot.ConvertTo<EventCategory>();
            return EventCategoryConverter.Convert(eventCategory);
        }
    }
}
