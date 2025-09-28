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

        [FirestoreProperty]
        public bool IsFree { get; set; }

        [FirestoreProperty]
        public List<string> MemberIds { get; set; }
    }
}
