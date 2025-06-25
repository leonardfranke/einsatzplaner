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
            var groups = snapshot.Select(doc => doc.ConvertTo<RequirementGroup>()).ToList();
            return RequirementGroupConverter.Convert(groups);
        }

        public Task UpdateOrCreateGroup(string departmentId, UpdateRequirementGroupDTO updateRequirementGroupDTO)
        {
            var categoryReference = GetCategoryGroupCollectionReference(departmentId);
            if (string.IsNullOrEmpty(updateRequirementGroupDTO.Id))
            {
                var newGroup = new RequirementGroup
                {
                    RequirementsRoles = updateRequirementGroupDTO.NewRequirementsRole,
                    RequirementsQualifications = updateRequirementGroupDTO.NewRequirementsQualifications
                };

                return categoryReference.AddAsync(newGroup);
            }
            else
            {
                var updateDict = new Dictionary<string, object>();
                foreach(var formerRole in updateRequirementGroupDTO.FormerRequirementsRole)
                {
                    updateDict.Add($"{nameof(RequirementGroup.RequirementsRoles)}.{formerRole}", FieldValue.Delete);
                }
                foreach (var newRole in updateRequirementGroupDTO.NewRequirementsRole)
                {
                    updateDict.Add($"{nameof(RequirementGroup.RequirementsRoles)}.{newRole.Key}", newRole.Value);
                }
                foreach (var formerQualification in updateRequirementGroupDTO.FormerRequirementsQualifications)
                {
                    updateDict.Add($"{nameof(RequirementGroup.RequirementsQualifications)}.{formerQualification}", FieldValue.Delete);
                }
                foreach (var newQualification in updateRequirementGroupDTO.NewRequirementsQualifications)
                {
                    updateDict.Add($"{nameof(RequirementGroup.RequirementsQualifications)}.{newQualification.Key}", newQualification.Value);
                }

                return categoryReference.Document(updateRequirementGroupDTO.Id).UpdateAsync(updateDict, Precondition.MustExist);
            }
        }
    }
}
