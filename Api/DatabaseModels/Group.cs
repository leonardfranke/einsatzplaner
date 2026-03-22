using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class GroupOld : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public List<string> MemberIds { get; set; }
    }
}
