using Api.FirestoreModels;

namespace Api.Manager
{
    public interface IHelperNotificationManager
    {
        public Task UpdateChangedStatus(string departmentId, string eventId, string roleId, IEnumerable<string> memberIds, HelperStatus previousStatus, HelperStatus newStatus);
    }
}
