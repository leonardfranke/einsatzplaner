using Google.Cloud.Firestore;
using Api.Models;
using Api.Converter;
using DTO;

namespace Api.Manager
{
    public class GroupManager : IGroupManager
    {
        private FirestoreDb _firestoreDb;
        private IMemberManager _memberManager;

        public GroupManager(FirestoreDb firestoreDb, IMemberManager memberManager)
        {
            _firestoreDb = firestoreDb;
            _memberManager = memberManager;
        }

        private CollectionReference GetGroupCollectionReference(string departmentId)
        {
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.GROUP);
        }

        public async Task Delete(string departmentId, string groupId)
        {
            var groupCollectionReference = GetGroupCollectionReference(departmentId);
            var groupRef = groupCollectionReference.Document(groupId);
            await groupRef.DeleteAsync();
        }

        public async Task<List<GroupDTO>> GetAll(string departmentId)
        {
            var groupReference = GetGroupCollectionReference(departmentId);
            var snapshot = await groupReference.GetSnapshotAsync();
            var groups = snapshot.Select(doc => doc.ConvertTo<Group>()).ToList();
            return GroupConverter.Convert(groups);
        }

        public async Task<string> UpdateOrCreate(string departmentId, string? groupId, string name)
        {
            var groupReference = GetGroupCollectionReference(departmentId);
            if (string.IsNullOrEmpty(groupId))
            {
                var newGroup = new Group
                {
                    Name = name
                };

                var newGroupReference = await groupReference.AddAsync(newGroup);
                return newGroupReference.Id;
            }
            else
            {
                await groupReference.Document(groupId)
                .UpdateAsync(new Dictionary<string, object> {
                    { nameof(Group.Name), name }
                }, Precondition.MustExist);
                return groupId;
            }
        }
    }
}
