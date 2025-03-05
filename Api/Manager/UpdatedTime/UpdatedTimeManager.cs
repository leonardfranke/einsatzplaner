using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class UpdatedTimeManager : IUpdatedTimeManager
    {
        private FirestoreDb _firestoreDb;
        private readonly string UpdateTimeField = "UpdateTime";
        private readonly string HelperCategoryKey = "HelperCategory";
        private readonly string HelperCategoryGroupKey = "HelperCategoryGroup";

        public UpdatedTimeManager(IFirestoreManager firestoreManager)
        {
            _firestoreDb = firestoreManager.Database;
        }

        private DocumentReference GetUpdateTimeReference(string departmentId, string key)
        {
            return _firestoreDb
                .Collection(Paths.DEPARTMENT).Document(departmentId)
                .Collection(Paths.UPDATETIME).Document(key);
        }

        public async Task<DateTime> GetHelperCategory(string depeartmentId)
        {
            var updateTimeObj = await GetUpdateTimeReference(depeartmentId, HelperCategoryKey).GetSnapshotAsync();
            if(updateTimeObj.TryGetValue<DateTime>(UpdateTimeField, out var updateTime))
                return updateTime;
            else
                return DateTime.MaxValue;
        }

        public async Task<DateTime> GetHelperCategoryGroup(string depeartmentId)
        {
            var updateTimeObj = await GetUpdateTimeReference(depeartmentId, HelperCategoryGroupKey).GetSnapshotAsync();
            if (updateTimeObj.TryGetValue<DateTime>(UpdateTimeField, out var updateTime))
                return updateTime;
            else
                return DateTime.MaxValue;
        }

        public async Task SetHelperCategory(string depeartmentId)
        {
            var updateTimeRef = GetUpdateTimeReference(depeartmentId, HelperCategoryKey);
            await updateTimeRef.UpdateAsync(UpdateTimeField, DateTime.Now);
        }

        public async Task SetHelperCategoryGroup(string depeartmentId)
        {
            var updateTimeRef = GetUpdateTimeReference(depeartmentId, HelperCategoryGroupKey);
            await updateTimeRef.UpdateAsync(UpdateTimeField, DateTime.Now);
        }
    }
}
