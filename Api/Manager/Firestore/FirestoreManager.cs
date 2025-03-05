using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class FirestoreManager : IFirestoreManager
    {
        public FirestoreDb Database { get; }

        public FirestoreManager()
        {
            Database = FirestoreDb.Create("heimspielplaner");
        }
    }
}
