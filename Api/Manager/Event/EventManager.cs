using Api.Converter;
using Api.FirestoreModels;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Supabase;
using System.Text;
using static Supabase.Postgrest.Constants;

namespace Api.Manager
{
    public class EventManager : IEventManager
    {
        private Client _supabaseClient;
        private FirestoreDb _firestoreDb;
        private IMailjetClient _mailjetClient;
        private IGroupManager _groupManager;
        private IRoleManager _roleManager;
        private IDepartmentManager _departmentManager;
        private IMemberManager _memberManager;
        private IEventCategoryManager _eventCategoryManager;
        private IUserManager _userManager;

        public EventManager(FirestoreDb firestoreDb, IMailjetClient mailjetClient, IGroupManager groupManager, IRoleManager roleManager, IDepartmentManager departmentManager, IMemberManager memberManager, IEventCategoryManager eventCategoryManager, IUserManager userManager, Client supabaseClient)
        {
            _firestoreDb = firestoreDb;
            _groupManager = groupManager;
            _roleManager = roleManager;
            _mailjetClient = mailjetClient;
            _departmentManager = departmentManager;
            _memberManager = memberManager;
            _eventCategoryManager = eventCategoryManager;
            _userManager = userManager;
            _supabaseClient = supabaseClient;
        }

        public Task CreateEvent(UpdateEventDTO updateEventDTO)
        {
            var newEvent = new Event
            {
                DepartmentId = updateEventDTO.DepartmentId,
                GroupId = updateEventDTO.GroupId,
                EventCategoryId = updateEventDTO.EventCategoryId,
                Date = updateEventDTO.Date,
            };

            if(string.IsNullOrEmpty(updateEventDTO.LocationId))
            {
                newEvent.LocationId = updateEventDTO.LocationId;
            }
            else
            {
                newEvent.LocationText = updateEventDTO.LocationText;
                newEvent.LocationLatitude = updateEventDTO.Latitude;
                newEvent.LocationLongitude = updateEventDTO.Longitude;
            }

            return _supabaseClient.From<Event>().Insert(newEvent);
        }

        public async Task UpdateEvent(UpdateEventDTO updateEventDTO)
        {
            await _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, updateEventDTO.DepartmentId)
                .Filter(nameof(Event.Id), Operator.Equals, updateEventDTO.EventId)
                .Limit(1)
                .Set(@event => @event.GroupId, updateEventDTO.GroupId)
                .Set(@event => @event.EventCategoryId, updateEventDTO.EventCategoryId)
                .Set(@event => @event.Date, updateEventDTO.Date)
                .Set(@event => @event.LocationId, updateEventDTO.LocationId)
                .Set(@event => @event.LocationLatitude, updateEventDTO.Latitude)
                .Set(@event => @event.LocationLongitude, updateEventDTO.Longitude)
                .Set(@event => @event.LocationText, updateEventDTO.LocationText)
                .Update();

            if (updateEventDTO.RemoveMembers)
                await RemoveMemberEnterings(updateEventDTO.DepartmentId, updateEventDTO.EventId);
        }

        public Task CreateRequirement(UpdateRequirementDTO updateRequirementDTO)
        {
            return _supabaseClient.From<Requirement>().Insert(new Requirement
            {
                DepartmentId = updateRequirementDTO.DepartmentId,
                EventId = updateRequirementDTO.EventId,
                RoleId = updateRequirementDTO.RoleId,
                LockingTime = updateRequirementDTO.LockingTime.Value,
                RequiredAmount = updateRequirementDTO.RequiredAmount.Value,
                RecommendedGroups = updateRequirementDTO.RecommendedGroups
            });
        }

        public async Task UpdateRequirement(UpdateRequirementDTO updateRequirementDTO)
        {
            var query = _supabaseClient
                .From<Requirement>()
                .Filter(nameof(Requirement.DepartmentId), Operator.Equals, updateRequirementDTO.DepartmentId)
                .Filter(nameof(Requirement.EventId), Operator.Equals, updateRequirementDTO.EventId)
                .Filter(nameof(Requirement.RoleId), Operator.Equals, updateRequirementDTO.RoleId)
                .Limit(1);

            if(updateRequirementDTO.RequiredAmount != null)
                query = query.Set(requirement => requirement.RequiredAmount, updateRequirementDTO.RequiredAmount);
            if (updateRequirementDTO.LockingTime != null)
                query = query.Set(requirement => requirement.LockingTime, updateRequirementDTO.LockingTime);
            if (updateRequirementDTO.RecommendedGroups != null)
                query = query.Set(requirement => requirement.RecommendedGroups, updateRequirementDTO.RecommendedGroups);

            await query.Update();
        }

        public Task DeleteRequirement(string departmentId, string eventId, string roleId)
        {
            return _supabaseClient
                .From<Requirement>()
                .Filter(nameof(Requirement.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Requirement.EventId), Operator.Equals, eventId)
                .Filter(nameof(Requirement.RoleId), Operator.Equals, roleId)
                .Delete();
        }

        public Task UpdateOrCreateQualificationRequirement(UpdateQualificationRequirementDTO updateQualificationRequirementDTO)
        {
            return _supabaseClient.From<QualificationRequirement>().Upsert(new QualificationRequirement
            {
                DepartmentId = updateQualificationRequirementDTO.DepartmentId,
                EventId = updateQualificationRequirementDTO.EventId,
                RoleId = updateQualificationRequirementDTO.RoleId,
                QualificationId = updateQualificationRequirementDTO.QualificationId,
                RequiredAmount = updateQualificationRequirementDTO.RequiredAmount
            });
        }

        public Task DeleteQualificationRequirement(string departmentId, string eventId, string roleId, string qualificationId)
        {
            return _supabaseClient
                .From<QualificationRequirement>()
                .Filter(nameof(QualificationRequirement.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(QualificationRequirement.EventId), Operator.Equals, eventId)
                .Filter(nameof(QualificationRequirement.RoleId), Operator.Equals, roleId)
                .Filter(nameof(QualificationRequirement.QualificationId), Operator.Equals, qualificationId)
                .Delete();
        }

        public Task DeleteEvent(string departmentId, string eventId)
        {
            return _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Event.Id), Operator.Equals, eventId)
                .Delete();

            //TODO Save notifications
            //var requirements = await GetRequirementsOfEvent(departmentId, eventId);
            //var allInvolvedMembers = requirements.SelectMany(requirement => requirement.LockedMembers.Union(requirement.PreselectedMembers)).Distinct().ToList();
            //.ContinueWith(task => CreateEventDeletionNotification(@event, allInvolvedMembers), TaskContinuationOptions.NotOnFaulted).Unwrap();
        }

        public async Task<List<EventDTO>> GetAllEvents(string departmentId, DateTime fromDate, DateTime toDate)
        {
            var res = await _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Event.Date), Operator.GreaterThanOrEqual, fromDate.ToUniversalTime().ToString("O"))
                .Filter(nameof(Event.Date), Operator.LessThanOrEqual, toDate.ToUniversalTime().ToString("O"))
                .Get();            
            return EventConverter.Convert(res.Models, departmentId);
        }

        public async Task<EventDTO?> GetEvent(string departmentId, string eventId)
        {
            var @event = await _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Event.Id), Operator.Equals, eventId).Single();            
            return EventConverter.Convert(@event, departmentId);            
        }

        public async IAsyncEnumerable<RequirementDTO> GetEnteredMemberRequirements(string departmentId, string memberId)
        {
            var requirements = GetRequirements(departmentId, null, null);
            await foreach (var requirement in requirements)
            {
                if(requirement.PreselectedMembers.Contains(memberId) || requirement.LockedMembers.Contains(memberId))
                    yield return requirement;
            }
        }

        public async IAsyncEnumerable<RequirementDTO> GetRequirements(string departmentId, string? eventId, string? roleId)
        {
            var query = _supabaseClient.From<Requirement>().Where(requirement => requirement.DepartmentId == departmentId);
            if(eventId != null)
                query = query.Where(requirement => requirement.EventId == eventId);
            if (roleId != null)
                query = query.Where(requirement => requirement.RoleId == roleId);

            var res = await query.Get();
            foreach(var requirement in res.Models)
            {
                var enterings = await GetEnteringsOfRequirement(departmentId, requirement.EventId, requirement.RoleId);
                var groupedEnterings = enterings.GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => group.Select(pair => pair.Key).ToList());
                groupedEnterings.TryGetValue(EnteringType.Locked, out var lockedMembers);
                groupedEnterings.TryGetValue(EnteringType.Preselected, out var preselectedMembers);
                groupedEnterings.TryGetValue(EnteringType.Available, out var availableMembers);
                groupedEnterings.TryGetValue(EnteringType.Recommended, out var recommendedMembers);
                var qualificationRequirements = await GetQualificationRequirements(departmentId, requirement.EventId, requirement.RoleId);
                yield return RequirementConverter.Convert(requirement, lockedMembers ?? [], preselectedMembers ?? [], availableMembers ?? [], recommendedMembers ?? [], qualificationRequirements);
            }            
        }

        private async Task<Dictionary<string, int>> GetQualificationRequirements(string departmentId, string eventId, string roleId)
        {
            var res = await _supabaseClient
                .From<QualificationRequirement>()
                .Select($"{nameof(QualificationRequirement.QualificationId)}, {nameof(QualificationRequirement.RequiredAmount)}")
                .Filter(nameof(QualificationRequirement.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(QualificationRequirement.EventId), Operator.Equals, eventId)
                .Filter(nameof(QualificationRequirement.RoleId), Operator.Equals, roleId)
                .Get();
            return res.Models.ToDictionary(qr => qr.QualificationId, qr => qr.RequiredAmount);
        }

        private async Task<Dictionary<string, EnteringType>> GetEnteringsOfRequirement(string departmentId, string eventId, string roleId)
        {
            var res = await _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                .Filter(nameof(Entering.RoleId), Operator.Equals, roleId)
                .Select($"{nameof(Entering.MemberId)}, {nameof(Entering.EnteringType)}")
                .Get();
            return res.Models.ToDictionary(entering => entering.MemberId, entering => entering.EnteringType);
        }

        private Task<Entering> GetEntering(string departmentId, string eventId, string roleId, string memberId)
        {
            return _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                .Filter(nameof(Entering.RoleId), Operator.Equals, roleId)
                .Filter(nameof(Entering.MemberId), Operator.Equals, memberId)
                .Single();
        }

        public async Task SetIsAvailable(string departmentId, string eventId, string roleId, string memberId, bool isAvailable)
        {
            var entering = await GetEntering(departmentId, eventId, roleId, memberId);
            if(entering == null)
            {
                if(isAvailable)
                {
                    var newEntering = new Entering
                    {
                        DepartmentId = departmentId,
                        EventId = eventId,
                        RoleId = roleId,
                        MemberId = memberId,
                        EnteringType = EnteringType.Available
                    };

                    await _supabaseClient.From<Entering>().Insert(newEntering);
                }
                return;
            }

            if (entering.EnteringType == EnteringType.Locked 
                || (isAvailable && (entering.EnteringType == EnteringType.Preselected || entering.EnteringType == EnteringType.Available))
                || !isAvailable && entering.EnteringType == EnteringType.Recommended)
                return;

            
            if (isAvailable)
            {
                await SetMembersEntering(departmentId, eventId, roleId, [memberId], EnteringType.Available);
            }
            else
            {
                await SetMembersEntering(departmentId, eventId, roleId, [memberId], null);
            }
        }

        public Task SetMembersEntering(string departmentId, string eventId, string roleId, List<string> memberIds, EnteringType? type)
        {
            if(type == null)
            {
                return _supabaseClient
                    .From<Entering>()
                    .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                    .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                    .Filter(nameof(Entering.RoleId), Operator.Equals, roleId)
                    .Filter(nameof(Entering.MemberId), Operator.In, memberIds)
                    .Delete();
            }
            else
            {
                return _supabaseClient.From<Entering>().Upsert(memberIds.Select(memberId => new Entering
                {
                    DepartmentId = departmentId,
                    EventId = eventId,
                    RoleId = roleId,
                    MemberId = memberId,
                    EnteringType = type.Value
                }).ToList());
            }
        }

        private Task RemoveMemberEnterings(string departmentId, string eventId)
        {
            return _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                .Delete();
        }

        public async Task UpdateChangedStatus(string departmentId, string eventId, string roleId, IEnumerable<string> memberIds, HelperStatus previousStatus, HelperStatus newStatus)
        {
            if (!memberIds.Any())
                return;

            var notificationCollection = GetNotificationsReference(departmentId);

            var notificationSnapshots = await notificationCollection
                .WhereEqualTo(nameof(HelperNotification.EventId), eventId)
                .WhereEqualTo(nameof(HelperNotification.RoleId), roleId).GetSnapshotAsync();
            var notificationSnapshot = notificationSnapshots?.FirstOrDefault();
            var notificationReference = notificationSnapshot?.Reference;

            var previousStatusDb = new Dictionary<string, HelperStatus>();
            if (notificationSnapshot != null)
            {
                var notification = notificationSnapshot.ConvertTo<HelperNotification>();
                previousStatusDb = notification.PreviousStatus;
            }

            var updates = new Dictionary<string, object>();
            if (notificationReference == null)
            {
                notificationReference = notificationCollection.Document();
                await notificationReference.CreateAsync(new HelperNotification
                {
                    EventId = eventId,
                    RoleId = roleId
                });
            }
            foreach (var memberId in memberIds)
            {
                if (!previousStatusDb.ContainsKey(memberId))
                    updates.Add($"{nameof(HelperNotification.PreviousStatus)}.{memberId}", previousStatus);
                updates.Add($"{nameof(HelperNotification.NewStatus)}.{memberId}", newStatus);
            }
            await notificationReference.UpdateAsync(updates);
        }

        public async Task CreateEventDeletionNotification(EventDTO @event, List<string> members)
        {
            if (!members.Any())
                return;
            
            var group = await _groupManager.GetById(@event.DepartmentId, @event.GroupId);
            var eventCategory = await _eventCategoryManager.GetById(@event.DepartmentId, @event.EventCategoryId);
            var notification = new DeletionNotification
            {
                EventId = @event.Id,
                GroupName = group?.Name,
                EventCategoryName = eventCategory?.Name,
                Date = @event.Date.UtcDateTime,
                Members = members,
            };
            
            var deletionReference = GetDeletionNotificationsReference(@event.DepartmentId).Document();
            await deletionReference.CreateAsync(notification);
        }

        public async Task CreateEventNotification(string departmentId, string eventId, DateTime previousDate, DateTime newDate, List<string> members)
        {
            var notification = new EventNotification
            {
                EventId = eventId,
                PreviousDate = previousDate,
                NewDate = newDate,
                Members = members
            };

            var notificationReference = GetEventNotificationsReference(departmentId).Document();
            await notificationReference.CreateAsync(notification);
        }

        public async Task SendHelperNotifications()
        {
            var departments = await _departmentManager.GetAll();
            foreach(var department in departments)
            {
                var requirementNotificationSnapshots = await GetNotificationsReference(department.Id).GetSnapshotAsync();
                var eventNotificationSnapshots = await GetEventNotificationsReference(department.Id).GetSnapshotAsync();
                var deletionNotificationSnapshots = await GetDeletionNotificationsReference(department.Id).GetSnapshotAsync();

                var releventEvents = new Dictionary<string, EventDTO>();
                var relevantRoles = new Dictionary<string, Role>();

                var requirementNotificationDict = new Dictionary<string, Dictionary<string, List<(string, HelperStatus, HelperStatus)>>>();
                foreach (var snapshot in requirementNotificationSnapshots)
                {
                    var notification = snapshot.ConvertTo<HelperNotification>();
                    if (!releventEvents.ContainsKey(notification.EventId))
                        releventEvents[notification.EventId] = await GetEvent(department.Id, notification.EventId);
                    if (releventEvents[notification.EventId] == null)
                        continue;
                    if (!relevantRoles.ContainsKey(notification.RoleId))
                            relevantRoles[notification.RoleId] = await _roleManager.GetRole(department.Id, notification.RoleId);

                    foreach (var (memberId, newHelperStatus) in notification.NewStatus)
                    {
                        if (!requirementNotificationDict.ContainsKey(memberId))
                            requirementNotificationDict.Add(memberId, []);
                        if (!requirementNotificationDict[memberId].ContainsKey(notification.RoleId))
                            requirementNotificationDict[memberId].Add(notification.RoleId, []);
                        var previousHelperStatus = notification.PreviousStatus[memberId];
                        if (previousHelperStatus != newHelperStatus)
                            requirementNotificationDict[memberId][notification.RoleId].Add((notification.EventId, previousHelperStatus, newHelperStatus));
                    }
                }

                var eventNotificationDict = new Dictionary<string, List<(string, DateTime, DateTime)>>();
                foreach (var snapshot in eventNotificationSnapshots)
                {
                    var notification = snapshot.ConvertTo<EventNotification>();
                    if (!releventEvents.ContainsKey(notification.EventId))
                        releventEvents[notification.EventId] = await GetEvent(department.Id, notification.EventId);
                    if (releventEvents[notification.EventId] == null)
                        continue;

                    foreach (var memberId in notification.Members)
                    {
                        if (!eventNotificationDict.ContainsKey(memberId))
                            eventNotificationDict.Add(memberId, []);
                        if (notification.PreviousDate != notification.NewDate)
                            eventNotificationDict[memberId].Add((notification.EventId, notification.PreviousDate.Value, notification.NewDate.Value));
                    }
                }

                var deletionNotificationDict = new Dictionary<string, List<(string, string, DateTime)>>();
                foreach (var snapshot in deletionNotificationSnapshots)
                {
                    var notification = snapshot.ConvertTo<DeletionNotification>();
                    foreach (var memberId in notification.Members)
                    {
                        if (!deletionNotificationDict.ContainsKey(memberId))
                            deletionNotificationDict.Add(memberId, []);
                        deletionNotificationDict[memberId].Add((notification.GroupName, notification.EventCategoryName, notification.Date));
                        if (requirementNotificationDict.ContainsKey(memberId))
                        {
                            foreach (var requirementNotifications in requirementNotificationDict[memberId].Values)
                            {
                                requirementNotifications.RemoveAll(reqNotif => reqNotif.Item1 == notification.EventId);
                            }
                        }
                        if (eventNotificationDict.ContainsKey(memberId))
                        {
                            eventNotificationDict[memberId].RemoveAll(eventNotif => eventNotif.Item1 == notification.EventId);
                        }
                    }
                }

                var relevantGroups = await Task.WhenAll(releventEvents.Where(pair => !string.IsNullOrEmpty(pair.Value?.GroupId)).Select(pair => _groupManager.GetById(department.Id, pair.Value.GroupId)));
                var relevantEventCategories = await Task.WhenAll(releventEvents.Where(pair => !string.IsNullOrEmpty(pair.Value?.EventCategoryId)).Select(pair => _eventCategoryManager.GetById(department.Id, pair.Value.EventCategoryId)));

                var emails = new List<TransactionalEmail>();
                foreach(var memberId in requirementNotificationDict.Keys.Union(eventNotificationDict.Keys).Union(deletionNotificationDict.Keys).Distinct())
                {
                    var member = await _memberManager.GetMember(department.Id, memberId);
                    if (member?.EmailNotificationActive != true)
                        continue;
                    var user = await _userManager.GetUserData(memberId);
                    if (!TimeZoneInfo.TryFindSystemTimeZoneById("Europe/Berlin", out var timeZoneOfMember))
                    {
                        timeZoneOfMember = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    }
                    var emailBuilder = new TransactionalEmailBuilder()
                        .WithFrom(new SendContact("noreply@einsatzplaner.net", "Einsatzplaner"))
                        .WithTo(new SendContact(user.Email, user.Name))
                        .WithSubject("Änderungen Einsatzplaner")
                        .WithBcc(new SendContact("leonard.franke@t-online.de"));
                    var text = new StringBuilder($"Hallo {user.Name},<br /><br />folgende Änderungen wurden vom System oder den Administratoren im Einsatzplaner eingetragen:<br /><br />");
                    
                    if(requirementNotificationDict.ContainsKey(memberId))
                    {
                        foreach (var (roleId, changes) in requirementNotificationDict[memberId])
                        {
                            var role = relevantRoles.First(pair => pair.Value.Id == roleId).Value;
                            text.Append($"{role.Name}:<br /><ul>");
                            var changesText = new SortedList<DateTime, string>(Comparer<DateTime>.Create((a,b) => a == b ? 1 : a.CompareTo(b)));
                            foreach (var change in changes)
                            {
                                var @event = releventEvents.Values.First(e => e?.Id == change.Item1);
                                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(@event.Date.UtcDateTime, timeZoneOfMember);

                                var group = string.IsNullOrEmpty(@event.GroupId) ? null : relevantGroups.FirstOrDefault(group => group.Id == @event.GroupId);
                                var eventCategory = string.IsNullOrEmpty(@event.EventCategoryId) ? null : relevantEventCategories.FirstOrDefault(eventCategory => eventCategory.Id == @event.EventCategoryId);

                                var dateInfo = localDateTime.ToString("dd.MM.yyyy HH:mm");
                                var groupInfo = group == null ? "<i>Sonstiges</i>" : group.Name;
                                var eventCategoryInfo = eventCategory == null ? "<i>Sonstiges</i>" : eventCategory.Name;
                                var helperStatusInfo = (change.Item2, change.Item3) switch
                                {
                                    (_, HelperStatus.Locked) => "Fest eingeplant",
                                    (_, HelperStatus.Preselected) => "Vorausgewählt",
                                    (_, HelperStatus.Available) => "Verfügbar",
                                    (_, HelperStatus.NotAvailable) => "Eintragung entfernt",
                                    (_, HelperStatus.RequirementDeleted) => "Bedarf an dieser Rolle entfernt",
                                    _ => "<i>Unbekannte Änderung</i>"
                                };
                                changesText.Add(localDateTime, $"<li><a href=\"https://einsatzplaner.net/{department.URL}/event/{@event.Id}\" target=\"_blank\">{dateInfo} - {groupInfo} - {eventCategoryInfo}:</a> {helperStatusInfo}</li>");
                            }
                            text.AppendJoin(string.Empty, changesText.Values);
                            text.Append("</ul> <br /><br />");
                        }
                    }
                    
                    if(eventNotificationDict.ContainsKey(memberId))
                    {
                        text.Append("Verschiebungen:<br /><ul>");
                        var changesText = new SortedList<DateTime, string>(Comparer<DateTime>.Create((a, b) => a == b ? 1 : a.CompareTo(b)));
                        foreach (var change in eventNotificationDict[memberId])
                        {
                            var @event = releventEvents.Values.First(e => e?.Id == change.Item1);
                            var previousLocalDateTime = TimeZoneInfo.ConvertTimeFromUtc(change.Item2, timeZoneOfMember);
                            var newLocalDateTime = TimeZoneInfo.ConvertTimeFromUtc(change.Item3, timeZoneOfMember);

                            var group = string.IsNullOrEmpty(@event.GroupId) ? null : relevantGroups.FirstOrDefault(group => group.Id == @event.GroupId);
                            var eventCategory = string.IsNullOrEmpty(@event.EventCategoryId) ? null : relevantEventCategories.FirstOrDefault(eventCategory => eventCategory.Id == @event.EventCategoryId);

                            var previousDateInfo = previousLocalDateTime.ToString("dd.MM.yyyy HH:mm");
                            var newDateInfo = newLocalDateTime.ToString("dd.MM.yyyy HH:mm");
                            var groupInfo = group == null ? "<i>Sonstiges</i>" : group.Name;
                            var eventCategoryInfo = eventCategory == null ? "<i>Sonstiges</i>" : eventCategory.Name;                            
                            changesText.Add(previousLocalDateTime, $"<li><a href=\"https://einsatzplaner.net/{department.URL}/event/{@event.Id}\" target=\"_blank\">{groupInfo} - {eventCategoryInfo}:</a> {previousDateInfo} -> {newDateInfo}</li>");
                        }
                        text.AppendJoin(string.Empty, changesText.Values);
                        text.Append("</ul> <br /><br />");
                    }

                    if(deletionNotificationDict.ContainsKey(memberId))
                    {
                        text.Append("Abgesagte Veranstaltungen:<br /><ul>");
                        var changesText = new SortedList<DateTime, string>(Comparer<DateTime>.Create((a, b) => a == b ? 1 : a.CompareTo(b)));
                        foreach (var change in deletionNotificationDict[memberId])
                        {
                            var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(change.Item3, timeZoneOfMember);

                            var dateInfo = localDateTime.ToString("dd.MM.yyyy HH:mm");
                            var groupInfo = string.IsNullOrEmpty(change.Item1) ? "<i>Sonstiges</i>" : change.Item1;
                            var eventCategoryInfo = string.IsNullOrEmpty(change.Item2) ? "<i>Sonstiges</i>" : change.Item2;
                            changesText.Add(localDateTime, $"<li>{groupInfo} - {eventCategoryInfo}: {dateInfo}</li>");
                        }
                        text.AppendJoin(string.Empty, changesText.Values);
                        text.Append("</ul> <br /><br />");
                    }

                    emailBuilder.WithHtmlPart(text.ToString());
                    var email = emailBuilder.Build();
                    emails.Add(email);
                }

                if (!emails.Any())
                    return;

                var responses = await _mailjetClient.SendTransactionalEmailsAsync(emails);
                foreach (var response in responses.Messages)
                {
                    Console.Error.WriteLine(response.Status);
                    if(response.Errors != null)
                    {
                        foreach (var error in response.Errors)
                        {
                            Console.Error.WriteLine($"{error.ErrorCode}: {error.ErrorMessage}");
                        }
                    }
                }

                await Task.WhenAll(requirementNotificationSnapshots.Select(notification => notification.Reference.DeleteAsync()));
                await Task.WhenAll(eventNotificationSnapshots.Select(notification => notification.Reference.DeleteAsync()));
                await Task.WhenAll(deletionNotificationSnapshots.Select(notification => notification.Reference.DeleteAsync()));
            }
        }

        public async Task<IEnumerable<StatDTO>> GetStats(string departmentId, string roleId, DateTime fromDate, DateTime toDate)
        {
            var eventsInRange = await GetAllEvents(departmentId, fromDate, toDate);
            var requirements = new List<RequirementDTO>();
            foreach(var @event in eventsInRange)
            {
                var requirementsOfEvent = await GetRequirements(departmentId, @event.Id, roleId).ToListAsync();
                requirements.AddRange(requirementsOfEvent);
            }
            var role = await _roleManager.GetRole(departmentId, roleId);

            var countsTotal = requirements.SelectMany(requirement =>
            {
                return requirement.LockedMembers.Concat(requirement.PreselectedMembers).Concat(requirement.AvailableMembers);
            }).GroupBy(memberId => memberId).ToDictionary(group => group.Key, group => group.Count());
            var countsFixed = requirements.SelectMany(requirement =>
            {
                return requirement.LockedMembers.Concat(requirement.PreselectedMembers);
            }).GroupBy(memberId => memberId).ToDictionary(group => group.Key, group => group.Count());
            var countsRecommendations = requirements.SelectMany(requirement =>
            {
                return requirement.FillMembers.Except(requirement.LockedMembers.Union(requirement.PreselectedMembers).Union(requirement.AvailableMembers));
            }).GroupBy(memberId => memberId).ToDictionary(group => group.Key, group => group.Count());
            var counts = countsTotal.Keys.Union(countsFixed.Keys).Union(countsRecommendations.Keys).Select(memberId => 
                new StatDTO(memberId, 
                countsFixed.GetValueOrDefault(memberId, 0), 
                countsTotal.GetValueOrDefault(memberId, 0), 
                countsRecommendations.GetValueOrDefault(memberId, 0)));
            return counts;
        }

        private DocumentReference GetDepartmentReference(string departmentId)
        {
            return _firestoreDb.Collection(Paths.DEPARTMENT).Document(departmentId);
        }

        private CollectionReference GetNotificationsReference(string departmentId)
        {
            return GetDepartmentReference(departmentId).Collection(Paths.HelperNotification);
        }

        private CollectionReference GetDeletionNotificationsReference(string departmentId)
        {
            return GetDepartmentReference(departmentId).Collection(Paths.DeletionNotification);
        }

        private CollectionReference GetEventNotificationsReference(string departmentId)
        {
            return GetDepartmentReference(departmentId).Collection(Paths.EventNotification);
        }
    }
}
