using Google.Cloud.Firestore;

namespace Api.Manager
{
    public interface IFirestoreManager
    {
        public FirestoreDb Database { get; }
    }
}
