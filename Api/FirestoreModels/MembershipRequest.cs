using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class MembershipRequest : FirebaseDocument
    {        
        [FirestoreProperty]
        public string UserId { get; set; }
        [FirestoreProperty]
        public string UserName { get; set; }
    }
}
