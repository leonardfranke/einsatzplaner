﻿using Api.Converter;
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
                    await TriggerRecalculation(departmentId);
                }
            }
            else
            {
                if(helper.PreselectedMembers.Contains(memberId))
                {
                    await helperReference.UpdateAsync(nameof(Helper.PreselectedMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
                    await TriggerRecalculation(departmentId);
                }
                if (helper.AvailableMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Helper.AvailableMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);                
            }
        }

        private async Task TriggerRecalculation(string departmentId)
        {

        }
    }
}
