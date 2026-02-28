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
            var res = await _supabaseClient
                .From<MembershipRequest>()
                .Select(nameof(MembershipRequest.UserId))
                .Where(request => request.DepartmentId == departmentId && request.UserId == userId)
                .Single();
            return res != null;
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
                DepartmentId = departmentId,
                UserId = userId,
                UserName = user.DisplayName,                
            };

            await _supabaseClient.From<MembershipRequest>().Insert(request);
            return true;
        }

        public async Task<List<MembershipRequestDTO>> MembershipRequests(string departmentId)
        {
            var res = await _supabaseClient.From<MembershipRequest>().Where(request => request.DepartmentId == departmentId).Get();            
            return MembershipRequestConverter.Convert(res.Models);
        }

        public async Task AnswerRequest(string departmentId, string userId, bool accept)
        {
            if(await MembershipRequested(departmentId, userId) == false)
                return;

            await _supabaseClient.From<MembershipRequest>().Where(request => request.DepartmentId == departmentId && request.UserId == userId).Delete();

            if (accept)
            {
                var user = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
                if (user == null)
                    return;
                
                await _memberManager.CreateMember(departmentId, userId, user.DisplayName, false);
            }
        }

        public Task RemoveMember(string departmentId, string memberId)
        {
            return _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId && member.Id == memberId).Delete();
        }
    }
}
