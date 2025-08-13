
using Api.Models;
using Google.Cloud.Firestore;
using Optimizer;

namespace Api.Manager
{
    public class OptimizationManager : IOptimizationManager
    {
        private FirestoreDb _firestoreDb;
        private IEventManager _eventManager;
        private IHelperManager _helperManager;
        private IQualificationManager _qualificationManager;

        public OptimizationManager(FirestoreDb firestoreDb, IEventManager eventManager, IHelperManager helperManager, IQualificationManager qualificationManager) 
        {
            _firestoreDb = firestoreDb;
            _eventManager = eventManager;
            _helperManager = helperManager;
            _qualificationManager = qualificationManager;
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

            var updateTasks = new List<Task>();
            foreach (var (requirement, update) in optimizerDict)
            {
                var newLockedMembers = update.NewLockedMembers;
                var newPreselectedMembers = update.NewPreselectedMembers;
                var newAvailableMembers = update.NewAvailableMembers;

                var oldLockedMembers = requirement.LockedMembers;
                var oldPreselectedMembers = requirement.PreselectedMembers;
                var oldAvailableMembers = requirement.AvailableMembers;

                var lockedMembersToAdd = newLockedMembers.Except(oldLockedMembers);
                var preselectedMembersToAdd = newPreselectedMembers.Except(oldPreselectedMembers);
                var availableMembersToAdd = newAvailableMembers.Except(oldAvailableMembers);

                var lockedMembersToRemove = oldLockedMembers.Except(newLockedMembers);
                var preselectedMembersToRemove = oldPreselectedMembers.Except(newPreselectedMembers);
                var availableMembersToRemove = oldAvailableMembers.Except(newAvailableMembers);

                var updates = new Dictionary<string, object>();
                if (lockedMembersToAdd.Any())
                    updates.Add(nameof(Requirement.LockedMembers), FieldValue.ArrayUnion(lockedMembersToAdd.ToArray()));
                if(preselectedMembersToAdd.Any())
                    updates.Add(nameof(Requirement.PreselectedMembers), FieldValue.ArrayUnion(preselectedMembersToAdd.ToArray()));
                if(availableMembersToAdd.Any())
                    updates.Add(nameof(Requirement.AvailableMembers), FieldValue.ArrayUnion(availableMembersToAdd.ToArray()));
                if (lockedMembersToRemove.Any())
                    updates.Add(nameof(Requirement.LockedMembers), FieldValue.ArrayRemove(lockedMembersToRemove.ToArray()));
                if (preselectedMembersToRemove.Any())
                    updates.Add(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(preselectedMembersToRemove.ToArray()));
                if (availableMembersToRemove.Any())
                    updates.Add(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(availableMembersToRemove.ToArray()));

                if(updates.Any())
                {
                    var requirementRef = _firestoreDb
                        .Collection(Paths.DEPARTMENT).Document(departmentId)
                        .Collection(Paths.EVENT).Document(requirement.EventId)
                        .Collection(Paths.HELPER).Document(requirement.Id);
                    updateTasks.Add(requirementRef.UpdateAsync(updates,Precondition.MustExist));
                }
            }

            await Task.WhenAll(updateTasks);
        }
    }
}
