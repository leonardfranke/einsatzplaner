using Api.Converter;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class RequirementGroupManager : IRequirementGroupManager
    {
        private FirestoreDb _firestoreDb;

        public RequirementGroupManager(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        private CollectionReference GetCategoryGroupCollectionReference(string departmentId)
        {
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.REQUIREMENT_GROUP);
        }

        public Task DeleteGroup(string departmentId, string helperCategoryGroupId)
        {
            var categoryReference = GetCategoryGroupCollectionReference(departmentId);
            return categoryReference.Document(helperCategoryGroupId)
                .DeleteAsync();
        }

        public async Task<List<RequirementGroupDTO>> GetAllGroups(string departmentId)
        {
            var categoryReference = GetCategoryGroupCollectionReference(departmentId);
            var snapshot = await categoryReference.GetSnapshotAsync();
            var groups = snapshot.Select(doc => doc.ConvertTo<HelperCategoryGroup>()).ToList();
            return RequirementGroupConverter.Convert(groups);
        }

        public Task UpdateOrCreateGroup(string departmentId, string? helperCategoryGroupId, Dictionary<string, uint> requirements)
        {
            var categoryReference = GetCategoryGroupCollectionReference(departmentId);
            if (string.IsNullOrEmpty(helperCategoryGroupId))
            {
                var newGroup = new HelperCategoryGroup
                {
                    Requirements = requirements
                };

                return categoryReference.AddAsync(newGroup);
            }
            else
            {
                return categoryReference.Document(helperCategoryGroupId)
                .UpdateAsync(new Dictionary<string, object> {
                    { nameof(HelperCategoryGroup.Requirements), requirements}
                }, Precondition.MustExist);
            }
        }
    }
}
