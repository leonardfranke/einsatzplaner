using Google.Cloud.Firestore;
using Api.Models;
using Api.Converter;
using DTO;

namespace Api.Manager
{
    public class RoleManager : IRoleManager
    {
        private FirestoreDb _firestoreDb;

        public RoleManager(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
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

        public async Task<string> UpdateOrCreate(string departmentId, string? roleId, string name, int lockingPeriod)
        {
            var roleReference = GetRoleCollectionReference(departmentId);
            if (string.IsNullOrEmpty(roleId))
            {
                var newRole = new Role
                {
                    Name = name,
                    LockingPeriod = lockingPeriod
                };

                var newRoleRef = await roleReference.AddAsync(newRole);
                return newRoleRef.Id;
            }
            else
            {
                await roleReference.Document(roleId)
                .UpdateAsync(new Dictionary<string, object> {
                    { nameof(Role.Name), name },
                    { nameof(Role.LockingPeriod), lockingPeriod }
                }, Precondition.MustExist);
                return roleId;
            }
        }
    }
}
