using Api.Models;
using Google.Cloud.Firestore;

namespace Api.FirestoreModels
{
    [FirestoreData]
    public class HelperNotification : FirebaseDocument
    {
        [FirestoreProperty]
        public string EventId { get; set; }
        [FirestoreProperty]
        public string RoleId { get; set; }
        [FirestoreProperty]
        public Dictionary<string, HelperStatus> PreviousStatus { get; set; }
        [FirestoreProperty]
        public Dictionary<string, HelperStatus> NewStatus { get; set; }
    }

    public enum HelperStatus : int
    {
        Locked = 0,
        Preselected = 1,
        Available = 2,
        NotAvailable = 3,
        RequirementDeleted = 5
    }

    [FirestoreData]
    public class EventNotification : FirebaseDocument
    {
        [FirestoreProperty]
        public string EventId { get; set; }
        [FirestoreProperty]
        public DateTime? PreviousDate { get; set; }
        [FirestoreProperty]
        public DateTime? NewDate { get; set; }
        [FirestoreProperty]
        public string? PreviousLocationText { get; set; }
        [FirestoreProperty]
        public string? NewLocationText { get; set; }
        [FirestoreProperty]
        public GeoPoint? PreviousLocation { get; set; }
        [FirestoreProperty]
        public GeoPoint? NewLocation { get; set; }
        [FirestoreProperty]
        public List<string> Members { get; set; }
    }

    [FirestoreData]
    public class DeletionNotification : FirebaseDocument
    {
        [FirestoreProperty]
        public string EventId { get; set; }
        [FirestoreProperty]
        public string GroupName { get; set; }
        [FirestoreProperty]
        public string EventCategoryName { get; set; }
        [FirestoreProperty]
        public DateTime Date { get; set; }
        [FirestoreProperty]
        public List<string> Members { get; set; }
    }
}
