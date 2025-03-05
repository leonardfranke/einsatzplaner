using Api.Models;
using Google.Cloud.Firestore;

namespace Api.FirestoreModels
{
    [FirestoreData]
    public class HelperGroup : FirebaseDocument
    {
        [FirestoreProperty]
        public Dictionary<string, uint> Requirements { get; set; }
    }
}
