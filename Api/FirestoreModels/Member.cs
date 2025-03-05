using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class Member : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public List<string> GroupIds { get; set; }

        [FirestoreProperty]
        public List<string> RoleIds { get; set; }

        [FirestoreProperty]
        public bool IsAdmin { get; set; }
    }
}
