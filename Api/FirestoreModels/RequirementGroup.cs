using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class RequirementGroup : FirebaseDocument
    {
        [FirestoreProperty]
        public Dictionary<string, int> RequirementsRoles { get; set; }
        [FirestoreProperty]
        public Dictionary<string, int> RequirementsQualifications { get; set; }
    }
}
