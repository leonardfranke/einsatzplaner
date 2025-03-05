using Api.Models;
using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class HelperCategoryGroup : FirebaseDocument
    {
        [FirestoreProperty]
        public Dictionary<string, uint> Requirements { get; set; }
    }
}
