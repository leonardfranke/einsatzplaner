using Google.Cloud.Firestore;
using Api.Models;
using Api.Converter;
using DTO;

namespace Api.Manager
{
    public class RoleManager : IRoleManager
    {
        private FirestoreDb _firestoreDb;
        private IQualificationManager _qualificationManager;

        public RoleManager(FirestoreDb firestoreDb, IQualificationManager qualificationManager)
        {
            _firestoreDb = firestoreDb;
            _qualificationManager = qualificationManager;
        }

        private CollectionReference GetRoleCollectionReference(string departmentId)
        {
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.ROLE);
        }

        public async Task Delete(string departmentId, string roleId)
        {
            var roleCollectionReference = GetRoleCollectionReference(departmentId);
            var roleRef = roleCollectionReference.Document(roleId);
            await roleRef.DeleteAsync();
        }

        public async Task<List<RoleDTO>> GetAll(string departmentId)
        {
            var roleReference = GetRoleCollectionReference(departmentId);
            var snapshot = await roleReference.GetSnapshotAsync();
            var roles = snapshot.Select(doc => doc.ConvertTo<Role>()).ToList();
            return RoleConverter.Convert(roles);
        }

        public async Task<string> UpdateOrCreate(string departmentId, string? roleId, string? newName, int? newLockingPeriod, bool? newIsFree)
        {
            var roleReference = GetRoleCollectionReference(departmentId);
            if (string.IsNullOrEmpty(roleId))
            {
                if(newName == null || newLockingPeriod == null || newIsFree == null)
                    throw new ArgumentNullException("Name, LockingPeriod or IsFree were null when creating a new role");
                var newRole = new Role
                {
                    Name = newName,
                    LockingPeriod = newLockingPeriod.Value,
                    IsFree = newIsFree.Value,
                    MemberIds = new()
                };

                var newRoleRef = await roleReference.AddAsync(newRole);
                return newRoleRef.Id;
            }
            else
            {
                var updates = new Dictionary<string, object>();
                if (newName != null)
                    updates.Add(nameof(Role.Name), newName);
                if (newLockingPeriod != null)
                    updates.Add(nameof(Role.LockingPeriod), newLockingPeriod);
                if (newIsFree != null)
                {
                    updates.Add(nameof(Role.IsFree), newIsFree);
                    if (newIsFree == true)
                        updates.Add(nameof(Role.MemberIds), new List<string>());
                    else
                        await _qualificationManager.RemoveMembersFromQualifications(departmentId, roleId, null);
                }

                await roleReference.Document(roleId).UpdateAsync(updates, Precondition.MustExist);
                return roleId;
            }
        }

        public async Task UpdateRoleMembers(string departmentId, string roleId, UpdateMembersListDTO updateMembersList)
        {
            var roleCollectionReference = GetRoleCollectionReference(departmentId);
            var roleRef = roleCollectionReference.Document(roleId);
            await roleRef.UpdateAsync(nameof(Role.MemberIds), FieldValue.ArrayRemove(updateMembersList.FormerMembers.ToArray()));
            await roleRef.UpdateAsync(nameof(Role.MemberIds), FieldValue.ArrayUnion(updateMembersList.NewMembers.ToArray()));

            if (updateMembersList.FormerMembers.Any())
                await _qualificationManager.RemoveMembersFromQualifications(departmentId, roleId, updateMembersList.FormerMembers);
        }

        public async Task<Role> GetRole(string departmentId, string roleId)
        {

            var rolesReference = GetRoleCollectionReference(departmentId);
            var roleReference = rolesReference.Document(roleId);
            var snapshot = await roleReference.GetSnapshotAsync();
            return snapshot.ConvertTo<Role>();
        }
    }
}
