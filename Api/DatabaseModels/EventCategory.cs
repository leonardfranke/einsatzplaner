using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class EventCategory : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }
    }
}
