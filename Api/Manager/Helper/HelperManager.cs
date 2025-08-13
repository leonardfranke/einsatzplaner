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

            var helpers = new List<Requirement>();
            foreach (var document in helpersSnapshot)
            {
                var helper = document.ConvertTo<Requirement>();
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
            var helper = helperSnapshot.ConvertTo<Requirement>();

            if (isAvailable)
            {
                if (!helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Requirement.AvailableMembers), FieldValue.ArrayUnion(memberId), Precondition.MustExist);
            }
            else
            {
                if(helper.PreselectedMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
                if(helper.AvailableMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);                
            }
        }

        public async Task UpdateLockedMembers(string departmentId, string eventId, string helperId, UpdateMembersListDTO updateMembersList)
        {
            if ((updateMembersList.FormerMembers.Any() || updateMembersList.NewMembers.Any()) == false)
                return;

            var requirementReference = _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.EVENT).Document(eventId)
                .Collection(Paths.HELPER).Document(helperId);
            var requirementSnapshot = await requirementReference.GetSnapshotAsync();
            var previousRequirement = requirementSnapshot.ConvertTo<Requirement>();

            await requirementReference.UpdateAsync(nameof(Requirement.LockedMembers), FieldValue.ArrayRemove(updateMembersList.FormerMembers.ToArray()));
            await requirementReference.UpdateAsync(nameof(Requirement.LockedMembers), FieldValue.ArrayUnion(updateMembersList.NewMembers.ToArray()));
            await requirementReference.UpdateAsync(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(updateMembersList.NewMembers.ToArray()));
            await requirementReference.UpdateAsync(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(updateMembersList.NewMembers.ToArray()));

            await _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, updateMembersList.FormerMembers, FirestoreModels.HelperStatus.Locked, FirestoreModels.HelperStatus.NotAvailable);
            var previouslyAvailable = updateMembersList.NewMembers.Where(previousRequirement.AvailableMembers.Contains);
            var previouslyPreselected = updateMembersList.NewMembers.Where(previousRequirement.PreselectedMembers.Contains);
            var previouslyNotAvailable = updateMembersList.NewMembers.Except(previouslyAvailable).Except(previouslyPreselected);
            await _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, previouslyAvailable, FirestoreModels.HelperStatus.Available, FirestoreModels.HelperStatus.Locked);
            await _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, previouslyPreselected, FirestoreModels.HelperStatus.Preselected, FirestoreModels.HelperStatus.Locked);
            await _helperNotificationManager.UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, previouslyNotAvailable, FirestoreModels.HelperStatus.NotAvailable, FirestoreModels.HelperStatus.Locked);

        }
    }
}
