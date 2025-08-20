using Api.Models;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class OptimizationManager : IOptimizationManager
    {
        private FirestoreDb _firestoreDb;
        private IEventManager _eventManager;
        private IHelperManager _helperManager;
        private IQualificationManager _qualificationManager;
        private IHelperNotificationManager _helperNotificationManager;

        public OptimizationManager(FirestoreDb firestoreDb, IEventManager eventManager, IHelperManager helperManager, IQualificationManager qualificationManager, IHelperNotificationManager helperNotificationManager) 
        {
            _firestoreDb = firestoreDb;
            _eventManager = eventManager;
            _helperManager = helperManager;
            _qualificationManager = qualificationManager;
            _helperNotificationManager = helperNotificationManager;
        }

        public async Task OptimizeDepartment(string departmentId)
        {
            var eventsTask = _eventManager.GetAll(departmentId);
            var requirementsTask = _helperManager.GetAll(departmentId);
            var qualificationsTask = _qualificationManager.GetAll(departmentId);

            var events = await eventsTask;
            var requirements = await requirementsTask;
            var qualifications = await qualificationsTask;

            var optimizerDict = Optimizer.Optimizer.Optimize(events, requirements, qualifications);

            var batch = _firestoreDb.StartBatch();
            foreach (var (requirement, update) in optimizerDict)
            {
                var requirementRef = _firestoreDb
                    .Collection(Paths.DEPARTMENT).Document(departmentId)
                    .Collection(Paths.EVENT).Document(requirement.EventId)
                    .Collection(Paths.HELPER).Document(requirement.Id);

                var newLockedMembers = update.NewLockedMembers;
                var newPreselectedMembers = update.NewPreselectedMembers;
                var newAvailableMembers = update.NewAvailableMembers;

                var oldLockedMembers = requirement.LockedMembers;
                var oldPreselectedMembers = requirement.PreselectedMembers;
                var oldAvailableMembers = requirement.AvailableMembers;

                var lockedMembersToRemove = oldLockedMembers.Except(newLockedMembers);
                var preselectedMembersToRemove = oldPreselectedMembers.Except(newPreselectedMembers);
                var availableMembersToRemove = oldAvailableMembers.Except(newAvailableMembers);

                var updates = new Dictionary<string, object>();
                if (lockedMembersToRemove.Any())
                    updates.Add(nameof(Requirement.LockedMembers), FieldValue.ArrayRemove(lockedMembersToRemove.ToArray()));
                if (preselectedMembersToRemove.Any())
                    updates.Add(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(preselectedMembersToRemove.ToArray()));
                if (availableMembersToRemove.Any())
                    updates.Add(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(availableMembersToRemove.ToArray()));
                batch.Update(requirementRef, updates, Precondition.MustExist);

                var lockedMembersToAdd = newLockedMembers.Except(oldLockedMembers);
                var preselectedMembersToAdd = newPreselectedMembers.Except(oldPreselectedMembers);
                var availableMembersToAdd = newAvailableMembers.Except(oldAvailableMembers);

                updates.Clear();
                if (lockedMembersToAdd.Any())
                    updates.Add(nameof(Requirement.LockedMembers), FieldValue.ArrayUnion(lockedMembersToAdd.ToArray()));
                if (preselectedMembersToAdd.Any())
                    updates.Add(nameof(Requirement.PreselectedMembers), FieldValue.ArrayUnion(preselectedMembersToAdd.ToArray()));
                if (availableMembersToAdd.Any())
                    updates.Add(nameof(Requirement.AvailableMembers), FieldValue.ArrayUnion(availableMembersToAdd.ToArray()));
                batch.Update(requirementRef, updates, Precondition.MustExist);
            }
            await batch.CommitAsync();

            var updateTasks = new List<Task>();
            foreach (var (requirement, update) in optimizerDict)
            {
                var requirementRef = _firestoreDb
                    .Collection(Paths.DEPARTMENT).Document(departmentId)
                    .Collection(Paths.EVENT).Document(requirement.EventId)
                    .Collection(Paths.HELPER).Document(requirement.Id);

                var newLockedMembers = update.NewLockedMembers;
                var newPreselectedMembers = update.NewPreselectedMembers;
                var newAvailableMembers = update.NewAvailableMembers;

                var oldLockedMembers = requirement.LockedMembers;
                var oldPreselectedMembers = requirement.PreselectedMembers;
                var oldAvailableMembers = requirement.AvailableMembers;

                var lockedMembersToRemove = oldLockedMembers.Except(newLockedMembers);
                var preselectedMembersToRemove = oldPreselectedMembers.Except(newPreselectedMembers);
                var availableMembersToRemove = oldAvailableMembers.Except(newAvailableMembers);

                var lockedMembersToAdd = newLockedMembers.Except(oldLockedMembers);
                var preselectedMembersToAdd = newPreselectedMembers.Except(oldPreselectedMembers);
                var availableMembersToAdd = newAvailableMembers.Except(oldAvailableMembers);

                updateTasks.Add(_helperNotificationManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    lockedMembersToAdd.Except(oldPreselectedMembers),
                    FirestoreModels.HelperStatus.Available,
                    FirestoreModels.HelperStatus.Locked));
                updateTasks.Add(_helperNotificationManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    lockedMembersToAdd.Except(oldAvailableMembers),
                    FirestoreModels.HelperStatus.Preselected,
                    FirestoreModels.HelperStatus.Locked));
                updateTasks.Add(_helperNotificationManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    preselectedMembersToAdd,
                    FirestoreModels.HelperStatus.Available,
                    FirestoreModels.HelperStatus.Preselected));
                updateTasks.Add(_helperNotificationManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    availableMembersToAdd,
                    FirestoreModels.HelperStatus.Preselected,
                    FirestoreModels.HelperStatus.Available));
            }
            await Task.WhenAll(updateTasks);
        }
    }
}
