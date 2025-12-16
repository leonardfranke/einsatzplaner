using Api.Models;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class OptimizationManager : IOptimizationManager
    {
        private FirestoreDb _firestoreDb;
        private IEventManager _eventManager;
        private IQualificationManager _qualificationManager;

        public OptimizationManager(FirestoreDb firestoreDb, IEventManager eventManager, IQualificationManager qualificationManager) 
        {
            _firestoreDb = firestoreDb;
            _eventManager = eventManager;
            _qualificationManager = qualificationManager;
        }

        public async Task OptimizeDepartment(string departmentId)
        {
            var eventsTask = _eventManager.GetAllEvents(departmentId, DateTime.MinValue, DateTime.MaxValue);
            var requirementsTask = _eventManager.GetAllRequirements(departmentId);
            var qualificationsTask = _qualificationManager.GetAll(departmentId);

            var events = await eventsTask;
            var requirements = await requirementsTask;
            var qualifications = await qualificationsTask;

            var optimizerDict = Optimizer.Optimizer.OptimizeAssignments(events, requirements, qualifications);

            var batch = _firestoreDb.StartBatch();
            foreach (var (requirement, update) in optimizerDict)
            {
                var requirementRef = _firestoreDb
                    .Collection(Paths.DEPARTMENT).Document(departmentId)
                    .Collection(Paths.EVENT).Document(requirement.EventId)
                    .Collection(Paths.HELPER).Document(requirement.Id);

                var newLockedMembers = update.LockedMembers;
                var newPreselectedMembers = update.PreselectedMembers;
                var newAvailableMembers = update.AvailableMembers;

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
                if (updates.Any())
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
                if(updates.Any())
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

                var newLockedMembers = update.LockedMembers;
                var newPreselectedMembers = update.PreselectedMembers;
                var newAvailableMembers = update.AvailableMembers;

                var oldLockedMembers = requirement.LockedMembers;
                var oldPreselectedMembers = requirement.PreselectedMembers;
                var oldAvailableMembers = requirement.AvailableMembers;

                var lockedMembersToRemove = oldLockedMembers.Except(newLockedMembers);
                var preselectedMembersToRemove = oldPreselectedMembers.Except(newPreselectedMembers);
                var availableMembersToRemove = oldAvailableMembers.Except(newAvailableMembers);

                var lockedMembersToAdd = newLockedMembers.Except(oldLockedMembers);
                var preselectedMembersToAdd = newPreselectedMembers.Except(oldPreselectedMembers);
                var availableMembersToAdd = newAvailableMembers.Except(oldAvailableMembers);

                updateTasks.Add(_eventManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    lockedMembersToAdd.Except(oldPreselectedMembers),
                    FirestoreModels.HelperStatus.Available,
                    FirestoreModels.HelperStatus.Locked));
                updateTasks.Add(_eventManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    lockedMembersToAdd.Except(oldAvailableMembers),
                    FirestoreModels.HelperStatus.Preselected,
                    FirestoreModels.HelperStatus.Locked));
                updateTasks.Add(_eventManager.UpdateChangedStatus(
                    departmentId,
                    requirement.EventId,
                    requirement.RoleId,
                    preselectedMembersToAdd,
                    FirestoreModels.HelperStatus.Available,
                    FirestoreModels.HelperStatus.Preselected));
                updateTasks.Add(_eventManager.UpdateChangedStatus(
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
