using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class Event : FirebaseDocument
    {
        [FirestoreProperty]
        public string? GroupId { get; set; }
        [FirestoreProperty]
        public string? EventCategoryId { get; set; }
        [FirestoreProperty]
        public DateTime Date { get; set; }
        [FirestoreProperty]
        public string? LocationId { get; set; }
        [FirestoreProperty]
        public GeoPoint? Location { get; set; }
        [FirestoreProperty]
        public string? LocationText { get; set; }
    }
}
