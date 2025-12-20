using Api.Models;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class OptimizationManager : IOptimizationManager
    {
        private FirestoreDb _firestoreDb;
        private IEventManager _eventManager;
        private IQualificationManager _qualificationManager;
        private IRoleManager _roleManager;
        private IGroupManager _groupManager;

        public OptimizationManager(FirestoreDb firestoreDb, IEventManager eventManager, IQualificationManager qualificationManager, IRoleManager roleManager, IGroupManager groupManager) 
        {
            _firestoreDb = firestoreDb;
            _eventManager = eventManager;
            _qualificationManager = qualificationManager;
            _roleManager = roleManager;
            _groupManager = groupManager;
        }

        public async Task OptimizeDepartment(string departmentId)
        {
            var eventsTask = _eventManager.GetAllEvents(departmentId, DateTime.MinValue, DateTime.MaxValue);
            var requirementsTask = _eventManager.GetAllRequirements(departmentId);
            var groupsTask = _groupManager.GetAll(departmentId);
            var rolesTask = _roleManager.GetAll(departmentId);
            var qualificationsTask = _qualificationManager.GetAll(departmentId);

            var events = await eventsTask;
            var requirements = await requirementsTask;
            var groups = await groupsTask;
            var roles = await rolesTask;
            var qualifications = await qualificationsTask;

            var optimizerDict = Optimizer.Optimizer.OptimizeAssignments(events, requirements, roles, groups, qualifications);

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
                var newFillMembers = update.FillMembers;

                var oldLockedMembers = requirement.LockedMembers;
                var oldPreselectedMembers = requirement.PreselectedMembers;
                var oldAvailableMembers = requirement.AvailableMembers;
                var oldFillMembers = requirement.FillMembers;

                var lockedMembersToRemove = oldLockedMembers.Except(newLockedMembers);
                var preselectedMembersToRemove = oldPreselectedMembers.Except(newPreselectedMembers);
                var availableMembersToRemove = oldAvailableMembers.Except(newAvailableMembers);
                var fillMembersToRemove = oldFillMembers.Except(newFillMembers);

                var updates = new Dictionary<string, object>();
                if (lockedMembersToRemove.Any())
                    updates.Add(nameof(Requirement.LockedMembers), FieldValue.ArrayRemove(lockedMembersToRemove.ToArray()));
                if (preselectedMembersToRemove.Any())
                    updates.Add(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(preselectedMembersToRemove.ToArray()));
                if (availableMembersToRemove.Any())
                    updates.Add(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(availableMembersToRemove.ToArray()));
                if(fillMembersToRemove.Any())
                    updates.Add(nameof(Requirement.FillMembers), FieldValue.ArrayRemove(fillMembersToRemove.ToArray()));
                if (updates.Any())
                    batch.Update(requirementRef, updates, Precondition.MustExist);

                var lockedMembersToAdd = newLockedMembers.Except(oldLockedMembers);
                var preselectedMembersToAdd = newPreselectedMembers.Except(oldPreselectedMembers);
                var availableMembersToAdd = newAvailableMembers.Except(oldAvailableMembers);
                var fillMembersToAdd = newFillMembers.Except(oldFillMembers);

                updates.Clear();
                if (lockedMembersToAdd.Any())
                    updates.Add(nameof(Requirement.LockedMembers), FieldValue.ArrayUnion(lockedMembersToAdd.ToArray()));
                if (preselectedMembersToAdd.Any())
                    updates.Add(nameof(Requirement.PreselectedMembers), FieldValue.ArrayUnion(preselectedMembersToAdd.ToArray()));
                if (availableMembersToAdd.Any())
                    updates.Add(nameof(Requirement.AvailableMembers), FieldValue.ArrayUnion(availableMembersToAdd.ToArray()));
                if (fillMembersToAdd.Any())
                    updates.Add(nameof(Requirement.FillMembers), FieldValue.ArrayUnion(fillMembersToAdd.ToArray()));
                if (updates.Any())
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
