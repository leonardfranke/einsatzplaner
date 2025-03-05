using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class Department : FirebaseDocument
    {        
        [FirestoreProperty]
        public string Name { get; set; }
    }
}
