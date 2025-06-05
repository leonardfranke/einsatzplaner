using Api.Models;
using Google.Cloud.Firestore;

namespace Api.FirestoreModels
{
    [FirestoreData]
    public class Qualification : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string RoleId { get; set; }

        [FirestoreProperty]
        public List<string> MemberIds { get; set; }
    }
}
