using Api.Converter;
using Api.Models;
using DTO;
using Supabase;

namespace Api.Manager
{
    public class MemberManager : IMemberManager
    {
        private Client _supabaseClient;

        public MemberManager(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<MemberDTO>> GetAll(string departmentId)
        {
            var res = await _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId).Get();
            return MemberConverter.Convert(res.Models);
        }

        public Task UpdateMember(string departmentId, UpdateMemberDTO updateMemberDTO)
        {
            var query = _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId && member.Id == updateMemberDTO.Id);

            var updateDict = new Dictionary<string, object>();
            if (updateMemberDTO.Name != null)
                query.Set(member => member.Name, updateMemberDTO.Name);
            if (updateMemberDTO.IsAdmin != null)
                query.Set(member => member.IsAdmin, updateMemberDTO.IsAdmin);

            return query.Update();
        }

        public async Task<string> CreateDummyMember(string departmentId)
        {
            var newMember = new Member
            {
                DepartmentId = departmentId,
                Name = "Dummy-Nutzer",
                IsAdmin = false,
                IsDummy = true,
                EmailNotificationActive = false
            };

            var res = await _supabaseClient.From<Member>().Insert(newMember);
            return res.Model.Id;
        }

        public Task CreateMember(string departmentId, string id, string name, bool isAdmin)
        {
            var newMember = new Member
            {
                DepartmentId= departmentId,
                Id = id,
                Name = name,
                IsAdmin = isAdmin,
                IsDummy = false,
                EmailNotificationActive = true
            };

            return _supabaseClient.From<Member>().Insert(newMember);
        }

        public async Task<MemberDTO> GetMember(string departmentId, string memberId)
        {
            var res = await _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId && member.Id == memberId).Single();            
            return MemberConverter.Convert(res);
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

        public async Task<bool> HasMembers(string departmentId)
        {
            var res = await _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId).Limit(1).Single();
            return res != null;
        }
    }
}
