using Api.FirestoreModels;
using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class HelperNotificationManager : IHelperNotificationManager
    {
        private FirestoreDb _firestoreDb;

        public HelperNotificationManager(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task UpdateChangedStatus(string departmentId, string eventId, string roleId, IEnumerable<string> memberIds, HelperStatus previousStatus, HelperStatus newStatus)
        {
            if (!memberIds.Any())
                return;

            var notificationCollection = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.HelperNotification);

            var notificationSnapshots = await notificationCollection
                .WhereEqualTo(nameof(HelperNotification.EventId), eventId)
                .WhereEqualTo(nameof(HelperNotification.RoleId), roleId).GetSnapshotAsync();
            var notificationSnapshot = notificationSnapshots?.FirstOrDefault();
            var notificationReference = notificationSnapshot?.Reference;   
            
            var previousStatusDb = new Dictionary<string, HelperStatus>();
            if(notificationSnapshot != null)
            {
                var notification = notificationSnapshot.ConvertTo<HelperNotification>();
                previousStatusDb = notification.PreviousStatus;
            }

            var updates = new Dictionary<string, object>();
            if(notificationReference == null)
            {
                notificationReference = notificationCollection.Document();
                await notificationReference.CreateAsync(new HelperNotification
                {
                    EventId = eventId,
                    RoleId = roleId                    
                });
            }
            foreach (var memberId in memberIds)
            {
                if (!previousStatusDb.ContainsKey(memberId))
                    updates.Add($"{nameof(HelperNotification.PreviousStatus)}.{memberId}", previousStatus);
                updates.Add($"{nameof(HelperNotification.NewStatus)}.{memberId}", newStatus);
            }
            await notificationReference.UpdateAsync(updates);
        }
    }
}
