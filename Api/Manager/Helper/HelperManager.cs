using Api.Converter;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class HelperManager : IHelperManager
    {
        private FirestoreDb _firestoreDb;
        private IDepartmentManager _departmentManager;

        public HelperManager(IFirestoreManager firestoreManager, IDepartmentManager departmentManager)
        {
            _firestoreDb = firestoreManager.Database;
            _departmentManager = departmentManager;
        }

        public async Task<List<HelperDTO>> GetAll(string departmentId)
        {
            var snapshot = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).ListDocumentsAsync();

            var helpers = snapshot.SelectManyAwait(async eventRef =>
            {
                var helper = await GetAll(departmentId, eventRef.Id);
                return helper.ToAsyncEnumerable();
            });

            return await helpers.ToListAsync();
        }

        public async Task<List<HelperDTO>> GetAll(string departmentId, string eventID)
        {
            var helpersSnapshot = await _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventID)
                .Collection(Paths.HELPER).GetSnapshotAsync();

            var helpers = new List<Helper>();
            foreach (var document in helpersSnapshot)
            {
                var helper = document.ConvertTo<Helper>();
                if (helper == null)
                    continue;
                helpers.Add(helper);
            }
            return HelperConverter.Convert(helpers, eventID);
        }

        public async Task SetIsHelping(string departmentId, string eventId, string helperId, string memberId, bool isHelping)
        {
            var helperReference = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId)
                .Collection(Paths.HELPER).Document(helperId);

            var helperSnapshot = await helperReference.GetSnapshotAsync();
            if (!helperSnapshot.Exists)
                return;
            var helper = helperSnapshot.ConvertTo<Helper>();

            if (isHelping && !helper.SetMembers.Union(helper.QueuedMembers).Contains(memberId))
            {
                if(helper.SetMembers.Count < helper.RequiredAmount)
                    await helperReference.UpdateAsync(nameof(Helper.SetMembers), FieldValue.ArrayUnion(memberId), Precondition.MustExist);
                else
                    await helperReference.UpdateAsync(nameof(Helper.QueuedMembers), FieldValue.ArrayUnion(memberId), Precondition.MustExist);
            }
            else
            {
                if(helper.QueuedMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Helper.QueuedMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
                else if(helper.SetMembers.Contains(memberId) && helperSnapshot.ReadTime.ToDateTime() < helper.LockingTime.ToUniversalTime())
                {
                    await helperReference.UpdateAsync(nameof(Helper.SetMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
                    if(helper.QueuedMembers.Count == 1)
                    {
                        var fillingMember = helper.QueuedMembers.FirstOrDefault();
                        await helperReference.UpdateAsync(nameof(Helper.QueuedMembers), FieldValue.ArrayRemove(fillingMember), Precondition.MustExist);
                        await helperReference.UpdateAsync(nameof(Helper.SetMembers), FieldValue.ArrayUnion(fillingMember), Precondition.MustExist);
                    }
                }
            }
        }
    }
}
