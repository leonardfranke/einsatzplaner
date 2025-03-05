using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class Role : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public int LockingPeriod { get; set; }
    }
}
