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
                    Name = name,
                    MemberIds = new()
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

        public async Task UpdateGroupMembers(string departmentId, string groupId, UpdateMembersListDTO updateMembersList)
        {
            var groupCollectionReference = GetGroupCollectionReference(departmentId);
            var groupRef = groupCollectionReference.Document(groupId);
            await groupRef.UpdateAsync(nameof(Group.MemberIds), FieldValue.ArrayRemove(updateMembersList.FormerMembers.ToArray()));
            await groupRef.UpdateAsync(nameof(Group.MemberIds), FieldValue.ArrayUnion(updateMembersList.NewMembers.ToArray()));
        }

        public async Task<Group> GetById(string departmentId, string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return null;
            var groupReference = GetGroupCollectionReference(departmentId).Document(groupId);
            var snapshot = await groupReference.GetSnapshotAsync();
            return snapshot.ConvertTo<Group>();
        }
    }
}
