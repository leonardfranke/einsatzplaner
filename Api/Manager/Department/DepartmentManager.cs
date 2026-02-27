using Api.Converter;
using Api.Models;
using DTO;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Supabase;

namespace Api.Manager
{
    public class DepartmentManager : IDepartmentManager
    {
        private Client _supabaseClient;
        private FirestoreDb _firestoreDb;
        private IMemberManager _memberManager;

        public DepartmentManager(FirestoreDb firestoreDb, IMemberManager memberManager, Client supabaseClient)
        {
            _firestoreDb = firestoreDb;
            _memberManager = memberManager;
            _supabaseClient = supabaseClient;
        }

        public async Task<List<DepartmentDTO>> GetAll()
        {
            var res = await _supabaseClient.From<Department>().Get();
            return DepartmentConverter.Convert(res.Models);                      
        }

        public async Task<DepartmentDTO> GetById(string departmentId)
        {
            var res = await _supabaseClient.From<Department>().Where(dep => dep.Id == departmentId).Single();
            return DepartmentConverter.Convert(res);
        }

        public async Task<DepartmentDTO> GetByUrl(string departmentUrl)
        {
            var res = await _supabaseClient.From<Department>().Where(dep => dep.URL == departmentUrl).Single();
            return DepartmentConverter.Convert(res);
        }

        public async Task<bool> IsMemberInDepartment(string memberId, string departmentId)
        {
            var res = await _supabaseClient.From<Member>().Select(nameof(Member.Id)).Where(member => member.DepartmentId == departmentId && member.Id == memberId).Single();
            return res != null;
        }

        public async Task<bool> MembershipRequested(string departmentId, string userId)
        {
            var snapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBERSHIP_REQUEST).WhereEqualTo(nameof(MembershipRequest.UserId), userId).GetSnapshotAsync();
            return snapshot.Count > 0;
        }

        public async Task<bool> AddRequest(string departmentId, string userId)
        {
            var user = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
            if (user == null)
                return false;

            var hasMembers = await _memberManager.HasMembers(departmentId);
            if (!hasMembers)
            {  
                await _memberManager.CreateMember(departmentId, userId, user.DisplayName, true);
                return true;
            }

            var membershipAlreadyRequested = await MembershipRequested(departmentId, userId);
            if (membershipAlreadyRequested)
                return false;            

            var request = new MembershipRequest
            {
                UserId = userId,
                UserName = user.DisplayName
            };

            await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBERSHIP_REQUEST).AddAsync(request);
            return true;
        }

        public async Task<List<MembershipRequestDTO>> MembershipRequests(string departmentId)
        {
            var documents = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBERSHIP_REQUEST).GetSnapshotAsync();
            var memberships = documents.Select(doc => doc.ConvertTo<MembershipRequest>()).ToList();
            return MembershipRequestConverter.Convert(memberships);
        }

        public async Task RemoveRequest(string departmentId, string requestId)
        {
            var reference = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBERSHIP_REQUEST).Document(requestId);
            await reference.DeleteAsync();
        }

        public async Task AnswerRequest(string departmentId, string requestId, bool accept)
        {
            var snapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBERSHIP_REQUEST).Document(requestId).GetSnapshotAsync();
            var request = snapshot.ConvertTo<MembershipRequest>();
            if (request == null)
                return;

            await RemoveRequest(departmentId, requestId);

            if (accept)
            {
                var user = await FirebaseAuth.DefaultInstance.GetUserAsync(request.UserId);
                if (user == null)
                    return;
                
                await _memberManager.CreateMember(departmentId, request.UserId, user.DisplayName, false);

            }
        }

        public Task RemoveMember(string departmentId, string memberId)
        {
            return _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId && member.Id == memberId).Delete();
        }
    }
}
