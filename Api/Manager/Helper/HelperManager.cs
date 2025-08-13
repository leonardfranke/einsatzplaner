using Api.Converter;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class HelperManager : IHelperManager
    {
        private FirestoreDb _firestoreDb;
        private IHelperNotificationManager _helperNotificationManager;

        public HelperManager(FirestoreDb firestoreDb, IHelperNotificationManager helperNotificationManager)
        {
            _firestoreDb = firestoreDb;
            _helperNotificationManager = helperNotificationManager;
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

        public async Task SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isAvailable)
        {
            var helperReference = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId)
                .Collection(Paths.HELPER).Document(helperId);

            var helperSnapshot = await helperReference.GetSnapshotAsync();
            if (!helperSnapshot.Exists)
                return;
            var helper = helperSnapshot.ConvertTo<Helper>();

            if (isAvailable)
            {
                if (!helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Contains(memberId))
                {
                    await helperReference.UpdateAsync(nameof(Helper.AvailableMembers), FieldValue.ArrayUnion(memberId), Precondition.MustExist);
                }
            }
            else
            {
                if(helper.PreselectedMembers.Contains(memberId))
                {
                    await helperReference.UpdateAsync(nameof(Helper.PreselectedMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
                    await helperReference.UpdateAsync(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);                
                }
                if (helper.AvailableMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Helper.AvailableMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);                
            }
        }

        public async Task UpdateLockedMembers(string departmentId, string eventId, string helperId, UpdateMembersListDTO updateMembersList)
        {
            if ((updateMembersList.FormerMembers.Any() || updateMembersList.NewMembers.Any()) == false)
                return; 

            var helperReference = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId)
                .Collection(Paths.HELPER).Document(helperId);


            await helperReference.UpdateAsync(nameof(Helper.LockedMembers), FieldValue.ArrayRemove(updateMembersList.FormerMembers.ToArray()));
            await helperReference.UpdateAsync(nameof(Helper.LockedMembers), FieldValue.ArrayUnion(updateMembersList.NewMembers.ToArray()));
            await helperReference.UpdateAsync(nameof(Helper.PreselectedMembers), FieldValue.ArrayRemove(updateMembersList.NewMembers.ToArray()));
            await helperReference.UpdateAsync(nameof(Helper.AvailableMembers), FieldValue.ArrayRemove(updateMembersList.NewMembers.ToArray()));
        }
    }
}
