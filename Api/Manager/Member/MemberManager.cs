using Api.Converter;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class MemberManager : IMemberManager
    {
        private FirestoreDb _firestoreDb;

        public MemberManager(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<List<MemberDTO>> GetAll(string departmentId)
        {
            var snapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).GetSnapshotAsync();

            var members = new List<Member>();
            foreach (var document in snapshot)
            {
                var member = document.ConvertTo<Member>();
                if (member == null)
                    continue;
                members.Add(member);
            }
            return MemberConverter.Convert(members);
        }

        public async Task UpdateMember(string departmentId, UpdateMemberDTO updateMemberDTO)
        {
            var snapshot = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).Document(updateMemberDTO.Id);

            var updateDict = new Dictionary<string, object>();
            if (updateMemberDTO.Name != null)
                updateDict.Add(nameof(Member.Name), updateMemberDTO.Name);
            if (updateMemberDTO.IsAdmin != null)
                updateDict.Add(nameof(Member.IsAdmin), updateMemberDTO.IsAdmin);
                        
            await snapshot.UpdateAsync(updateDict, Precondition.MustExist);
        }

        public async Task<string> CreateDummyMember(string departmentId)
        {
            var newMember = new Member
            {
                Name = "Dummy-Nutzer",
                IsAdmin = false,
                IsDummy = true,
                EmailNotificationActive = false
            };

            var document = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).Document();
                
            await document.CreateAsync(newMember);
            return document.Id;
        }

        public Task CreateMember(string departmentId, string id, string name, bool isAdmin)
        {
            var newMember = new Member
            {
                Name = name,
                IsAdmin = isAdmin,
                IsDummy = false,
                EmailNotificationActive = true
            };

            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).Document(id).CreateAsync(newMember);
        }

        public async Task<MemberDTO> GetMember(string departmentId, string memberId)
        {
            var snapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).Document(memberId).GetSnapshotAsync();

            var member = snapshot.ConvertTo<Member>();
            return MemberConverter.Convert(member);
        }

        /*
        public Task AddGroupMembers(string departmentId, string groupId, List<string> members)
        {
            return AddOrRemoveGroupOrRoleMembers(departmentId, groupId, members, true, true);
        }

        public Task AddRoleMembers(string departmentId, string roleId, List<string> members)
        {
            return AddOrRemoveGroupOrRoleMembers(departmentId, roleId, members, false, true);
        }

        public Task RemoveGroupMembers(string departmentId, string groupId, List<string> members)
        {
            return AddOrRemoveGroupOrRoleMembers(departmentId, groupId, members, true, false);
        }

        public Task RemoveRoleMembers(string departmentId, string roleId, List<string> members)
        {
            return AddOrRemoveGroupOrRoleMembers(departmentId, roleId, members, false, false);
        }

        private Task AddOrRemoveGroupOrRoleMembers(string departmentId, string id, List<string> members, bool isGroupNotRole, bool isAddNotRemove)
        {
            var tasks = new List<Task>();
            foreach (var member in members)
            {
                var memberRef = GetMemberReference(departmentId, member);
                var task = UpdateMember(memberRef, id, isGroupNotRole, isAddNotRemove);
                tasks.Add(task);
            }
            return Task.WhenAll(tasks);
        }

        public async Task RemoveAllGroupMembers(string departmentId, string groupId)
        {
            var membersRef = GetMembersReference(departmentId);
            var tasks = new List<Task>();
            await foreach (var memberRef in membersRef.ListDocumentsAsync())
            {
                var task = UpdateMember(memberRef, groupId, true, false);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        public async Task RemoveAllRoleMembers(string departmentId, string roleId)
        {
            var membersRef = GetMembersReference(departmentId);
            var tasks = new List<Task>();
            await foreach (var memberRef in membersRef.ListDocumentsAsync())
            {
                var task = UpdateMember(memberRef, roleId, false, false);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        private Task UpdateMember(DocumentReference memberRef, string id, bool isGroupNotRole, bool isAddNotRemove)
        {
            return memberRef.UpdateAsync(new Dictionary<string, object> { {
                        isGroupNotRole ? nameof(Member.GroupIds) : nameof(Member.RoleIds),
                        isAddNotRemove ? FieldValue.ArrayUnion(id) : FieldValue.ArrayRemove(id) } });
        }*/

        public async Task<long?> MemberCount(string departmentId)
        {
            var result = await GetMembersReference(departmentId).Count().GetSnapshotAsync();
            return result.Count;
        }

        private DocumentReference GetMemberReference(string departmentId, string memberId)
        {
            return GetMembersReference(departmentId).Document(memberId);
        }

        private CollectionReference GetMembersReference(string departmentId)
        {
            return _firestoreDb.Collection(Paths.DEPARTMENT).Document(departmentId).Collection(Paths.MEMBER);
        }
    }
}
