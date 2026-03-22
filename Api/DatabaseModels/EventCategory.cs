using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class EventCategoryOld : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }
    }
}
