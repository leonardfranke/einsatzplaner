using Api.Converter;
using Api.Models;
using DTO;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class DepartmentManager : IDepartmentManager
    {
        private FirestoreDb _firestoreDb;
        private IMemberManager _memberManager;
        private string path = "Department";

        public DepartmentManager(FirestoreDb firestoreDb, IMemberManager memberManager)
        {
            _firestoreDb = firestoreDb;
            _memberManager = memberManager;
        }

        public async Task<List<DepartmentDTO>> GetAll()
        {
            var snapshot = await _firestoreDb.Collection(path).GetSnapshotAsync();
            var departments = new List<Department>();
            foreach (var document in snapshot)
            {
                var department = document.ConvertTo<Department>();
                if (department == null)
                    continue;
                departments.Add(department);
            }
            return DepartmentConverter.Convert(departments);                      
        }

        public async Task<DepartmentDTO> GetById(string departmentId)
        {
            var snapshot = await _firestoreDb.Collection(path)
                .Document(departmentId).GetSnapshotAsync();
            var department = snapshot.ConvertTo<Department>();
            return DepartmentConverter.Convert(department);
        }

        public async Task<DepartmentDTO> GetByUrl(string departmentUrl)
        {
            var snapshots = await _firestoreDb.Collection(path).WhereEqualTo(nameof(Department.URL), departmentUrl).Limit(1).GetSnapshotAsync();
            if (!snapshots.Any())
                return null;
            var department = snapshots.First().ConvertTo<Department>();
            return DepartmentConverter.Convert(department);
        }

        public async Task<bool> IsMemberInDepartment(string memberId, string departmentId)
        {
            var memberSnapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).Document(memberId).GetSnapshotAsync();

            return memberSnapshot.Exists;
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

            var memberCount = await _memberManager.MemberCount(departmentId);
            if (memberCount == 0)
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
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.MEMBER).Document(memberId).DeleteAsync();
        }
    }
}
