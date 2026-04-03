using DTO;
using Google.Cloud.Firestore;

namespace Api.Manager
{
    public class OptimizationManager : IOptimizationManager
    {
        private IEventManager _eventManager;
        private IQualificationManager _qualificationManager;
        private IRoleManager _roleManager;
        private IGroupManager _groupManager;

        public OptimizationManager(IEventManager eventManager, IQualificationManager qualificationManager, IRoleManager roleManager, IGroupManager groupManager) 
        {
            _eventManager = eventManager;
            _qualificationManager = qualificationManager;
            _roleManager = roleManager;
            _groupManager = groupManager;
        }

        public async Task OptimizeDepartment(string departmentId)
        {
            var eventsTask = _eventManager.GetAllEvents(departmentId, DateTime.MinValue, DateTime.MaxValue);
            var requirementsTask = _eventManager.GetRequirements(departmentId, null, null);
            var groupsTask = _groupManager.GetAll(departmentId);
            var rolesTask = _roleManager.GetAll(departmentId);
            var qualificationsTask = _qualificationManager.GetAll(departmentId);

            var events = await eventsTask;
            var requirements = await requirementsTask.ToListAsync();
            var groups = await groupsTask.ToListAsync();
            var roles = await rolesTask.ToListAsync();
            var qualifications = await qualificationsTask.ToListAsync();

            var optimizerDict = Optimizer.Optimizer.OptimizeAssignments(events, requirements, roles, groups, qualifications);

            foreach (var (requirement, update) in optimizerDict)
            {
                var newLockedMembers = update.LockedMembers;
                var newPreselectedMembers = update.PreselectedMembers;
                var newAvailableMembers = update.AvailableMembers;
                var newFillMembers = update.FillMembers;

                var membersToRemove = requirement.LockedMembers.Union(requirement.PreselectedMembers).Union(requirement.AvailableMembers).Union(requirement.FillMembers)
                    .Except(newLockedMembers).Except(newPreselectedMembers).Except(newAvailableMembers).Except(newFillMembers).ToList();

                var task1 = _eventManager.SetMembersEntering(departmentId, requirement.EventId, requirement.RoleId, membersToRemove, null);
                var task2 = _eventManager.SetMembersEntering(departmentId, requirement.EventId, requirement.RoleId, newLockedMembers, EnteringType.Locked);
                var task3 = _eventManager.SetMembersEntering(departmentId, requirement.EventId, requirement.RoleId, newPreselectedMembers, EnteringType.Preselected);
                var task4 = _eventManager.SetMembersEntering(departmentId, requirement.EventId, requirement.RoleId, newAvailableMembers, EnteringType.Available);
                var task5 = _eventManager.SetMembersEntering(departmentId, requirement.EventId, requirement.RoleId, newFillMembers, EnteringType.Recommended);
                await Task.WhenAll(task1, task2, task3, task4, task5);
            }

            //TODO Save notifications
            //var updateTasks = new List<Task>();
            //foreach (var (requirement, update) in optimizerDict)
            //{
            //    var requirementRef = _firestoreDb
            //        .Collection(Paths.DEPARTMENT).Document(departmentId)
            //        .Collection(Paths.EVENT).Document(requirement.EventId)
            //        .Collection(Paths.HELPER).Document(requirement.Id);

            //    var newLockedMembers = update.LockedMembers;
            //    var newPreselectedMembers = update.PreselectedMembers;
            //    var newAvailableMembers = update.AvailableMembers;

            //    var oldLockedMembers = requirement.LockedMembers;
            //    var oldPreselectedMembers = requirement.PreselectedMembers;
            //    var oldAvailableMembers = requirement.AvailableMembers;

            //    var lockedMembersToRemove = oldLockedMembers.Except(newLockedMembers);
            //    var preselectedMembersToRemove = oldPreselectedMembers.Except(newPreselectedMembers);
            //    var availableMembersToRemove = oldAvailableMembers.Except(newAvailableMembers);

            //    var lockedMembersToAdd = newLockedMembers.Except(oldLockedMembers);
            //    var preselectedMembersToAdd = newPreselectedMembers.Except(oldPreselectedMembers);
            //    var availableMembersToAdd = newAvailableMembers.Except(oldAvailableMembers);

            //    updateTasks.Add(_eventManager.UpdateChangedStatus(
            //        departmentId,
            //        requirement.EventId,
            //        requirement.RoleId,
            //        lockedMembersToAdd.Except(oldPreselectedMembers),
            //        FirestoreModels.HelperStatus.Available,
            //        FirestoreModels.HelperStatus.Locked));
            //    updateTasks.Add(_eventManager.UpdateChangedStatus(
            //        departmentId,
            //        requirement.EventId,
            //        requirement.RoleId,
            //        lockedMembersToAdd.Except(oldAvailableMembers),
            //        FirestoreModels.HelperStatus.Preselected,
            //        FirestoreModels.HelperStatus.Locked));
            //    updateTasks.Add(_eventManager.UpdateChangedStatus(
            //        departmentId,
            //        requirement.EventId,
            //        requirement.RoleId,
            //        preselectedMembersToAdd,
            //        FirestoreModels.HelperStatus.Available,
            //        FirestoreModels.HelperStatus.Preselected));
            //    updateTasks.Add(_eventManager.UpdateChangedStatus(
            //        departmentId,
            //        requirement.EventId,
            //        requirement.RoleId,
            //        availableMembersToAdd,
            //        FirestoreModels.HelperStatus.Preselected,
            //        FirestoreModels.HelperStatus.Available));
            //}
            //await Task.WhenAll(updateTasks);
        }
    }
}
