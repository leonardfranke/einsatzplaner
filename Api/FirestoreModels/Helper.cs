using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class Helper : FirebaseDocument
    {
        [FirestoreProperty]
        public string HelperCategoryId { get; set; }
        [FirestoreProperty]
        public DateTime LockingTime { get; set; }
        [FirestoreProperty]
        public int RequiredAmount { get; set; }
        [FirestoreProperty]
        public List<string> RequiredGroups { get; set; }
        [FirestoreProperty]
        public List<string> SetMembers { get; set; }
        [FirestoreProperty]
        public List<string> QueuedMembers { get; set; }

    }
}
