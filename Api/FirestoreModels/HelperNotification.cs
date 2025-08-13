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
        Locked,
        Preselected,
        Available,
        NotAvailable
    }
}
