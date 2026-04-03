using Api.Models;
using DTO;
using Google.Cloud.Firestore;
using Supabase;

namespace Api.DataMigrations
{
    public class Migration
    {
        private Client _supabaseClient;
        private FirestoreDb _firestoreDb;

        public Migration(FirestoreDb firestoreDb, Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
            _firestoreDb = firestoreDb;
        }

        public async Task MigrateRoles()
        {
            var departmentId = "v87avboSu7Dc74ZJpJFk";
            var res = await _supabaseClient.From<Member>().Where(member => member.DepartmentId == departmentId).Get();
            var memberIds = res.Models.Select(member => member.Id);

            var eventsSnap = await _firestoreDb.Collection("Department").Document(departmentId).Collection("Event").GetSnapshotAsync();
            foreach (var eventDoc in eventsSnap)
            {
                var @event = eventDoc.ConvertTo<EventOld>();
                if (@event == null)
                    continue;

                var newEvent = new Event
                {
                    DepartmentId = departmentId,
                    Id = @event.Id,
                    GroupId = string.IsNullOrWhiteSpace(@event.GroupId) ? null : @event.GroupId,
                    EventCategoryId = @event.EventCategoryId,
                    LocationId = @event.LocationId,
                    LocationLatitude = @event.Location?.Latitude,
                    LocationLongitude = @event.Location?.Longitude,
                    LocationText = @event.LocationText,
                    Date = @event.Date
                };
                await _supabaseClient.From<Event>().Insert(newEvent);
                

                var requirementsSnap = await eventDoc.Reference
                    .Collection("Helper")
                    .GetSnapshotAsync();
                foreach(var requirementDoc in requirementsSnap)
                {
                    var requirement = requirementDoc.ConvertTo<RequirementOld>();
                    if (requirement == null)
                        continue;

                    var newRequirement = new Requirement
                    {
                        DepartmentId = departmentId,
                        EventId = @event.Id,
                        RoleId = requirement.RoleId,
                        LockingTime = requirement.LockingTime,
                        RequiredAmount = requirement.RequiredAmount,
                        RecommendedGroups = requirement.RequiredGroups,
                    };
                    await _supabaseClient.From<Requirement>().Insert(newRequirement);

                    foreach(var requiredQuali in requirement.RequiredQualifications)
                    {
                        var newQualiReq = new QualificationRequirement
                        {
                            DepartmentId = departmentId,
                            EventId = @event.Id,
                            RoleId = requirement.RoleId,
                            QualificationId = requiredQuali.Key,
                            RequiredAmount = requiredQuali.Value,
                        };
                        await _supabaseClient.From<QualificationRequirement>().Insert(newQualiReq);
                    }

                    if (requirement.LockedMembers?.Any() == true)
                    {
                        var lockedEnterings = requirement.LockedMembers?.Intersect(memberIds)?.Select(memberId =>
                        {
                            return new Entering
                            {
                                DepartmentId = departmentId,
                                EventId = @event.Id,
                                RoleId = requirement.RoleId,
                                MemberId = memberId,
                                EnteringType = EnteringType.Locked
                            };
                        }).ToList();
                        if(lockedEnterings.Any())
                            await _supabaseClient.From<Entering>().Insert(lockedEnterings);
                    }

                    if (requirement.PreselectedMembers?.Any() == true)
                    {
                        var preselectedEnterings = requirement.PreselectedMembers.Intersect(memberIds).Select(memberId =>
                        {
                            return new Entering
                            {
                                DepartmentId = departmentId,
                                EventId = @event.Id,
                                RoleId = requirement.RoleId,
                                MemberId = memberId,
                                EnteringType = EnteringType.Preselected
                            };
                        }).ToList();
                        if (preselectedEnterings.Any())
                            await _supabaseClient.From<Entering>().Insert(preselectedEnterings);
                    }


                    if (requirement.AvailableMembers?.Any() == true)
                    {
                        var availableEnterings = requirement.AvailableMembers.Intersect(memberIds).Select(memberId =>
                        {
                            return new Entering
                            {
                                DepartmentId = departmentId,
                                EventId = @event.Id,
                                RoleId = requirement.RoleId,
                                MemberId = memberId,
                                EnteringType = EnteringType.Available
                            };
                        }).ToList();
                        if (availableEnterings.Any())
                            await _supabaseClient.From<Entering>().Insert(availableEnterings);
                    }

                    var recommendedEnterings = requirement.FillMembers?.Intersect(memberIds)?.Except(requirement.LockedMembers ?? [])?.Select(memberId =>
                    {
                        return new Entering
                        {
                            DepartmentId = departmentId,
                            EventId = @event.Id,
                            RoleId = requirement.RoleId,
                            MemberId = memberId,
                            EnteringType = EnteringType.Recommended
                        };
                    }).ToList();
                    if (recommendedEnterings?.Any() == true)
                        await _supabaseClient.From<Entering>().Insert(recommendedEnterings);
                    
                }
            }
        }
    }
}
