using Google.Cloud.Firestore;

namespace Api.Models
{
    public abstract class FirebaseDocument
    {
        [FirestoreDocumentId]
        public string Id { get; set; }
    }
}
