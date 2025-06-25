using Api.Models;
using Google.Cloud.Firestore;

namespace Api.FirestoreModels
{
    [FirestoreData]
    public class HelperGroup : FirebaseDocument
    {
        [FirestoreProperty]
        public Dictionary<string, int> Requirements { get; set; }
    }
}
