using Api.Converter;
using Api.FirestoreModels;
using Api.Models;
using DTO;
using Google.Cloud.Firestore;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using System.Text;

namespace Api.Manager
{
    public class EventManager : IEventManager
    {
        private FirestoreDb _firestoreDb;
        private IMailjetClient _mailjetClient;
        private IGroupManager _groupManager;
        private IRoleManager _roleManager;
        private IDepartmentManager _departmentManager;
        private IMemberManager _memberManager;
        private IEventCategoryManager _eventCategoryManager;
        private IUserManager _userManager;

        public EventManager(FirestoreDb firestoreDb, IMailjetClient mailjetClient, IGroupManager groupManager, IRoleManager roleManager, IDepartmentManager departmentManager, IMemberManager memberManager, IEventCategoryManager eventCategoryManager, IUserManager userManager)
        {
            _firestoreDb = firestoreDb;
            _groupManager = groupManager;
            _roleManager = roleManager;
            _mailjetClient = mailjetClient;
            _departmentManager = departmentManager;
            _memberManager = memberManager;
            _eventCategoryManager = eventCategoryManager;
            _userManager = userManager;
        }

        public async Task UpdateOrCreateEvent(UpdateEventDTO updateEventDTO)
        {
            var departmentId = updateEventDTO.DepartmentId;
            var eventId = updateEventDTO.EventId;
            var dateUTC = updateEventDTO.Date?.ToUniversalTime();
            var groupId = updateEventDTO.GroupId;
            var eventCategoryId = updateEventDTO.EventCategoryId;
            var updateHelpers = updateEventDTO.Helpers;
            var removeMembers = updateEventDTO.RemoveMembers;
            var locationId = updateEventDTO.LocationId;
            var locationText = updateEventDTO.LocationText;
            GeoPoint? location = updateEventDTO.Latitude != null && updateEventDTO.Longitude != null 
                ? new GeoPoint(updateEventDTO.Latitude.Value, updateEventDTO.Longitude.Value) : null;

            var eventsRef = GetEventsReference(departmentId);
            DocumentReference eventRef;

            DateTime? oldDate = null;
            var dataChangesTasks = new List<Task>();
            if (string.IsNullOrEmpty(eventId))
            {
                if (dateUTC == null)
                    throw new ArgumentException("Date must be present when creating a new event.");

                var newEvent = new Event
                {
                    GroupId = groupId,
                    EventCategoryId = eventCategoryId,
                    Date = (DateTime)dateUTC
                };
                if(string.IsNullOrEmpty(updateEventDTO.LocationId))
                {
                    newEvent.LocationText = locationText;
                    newEvent.Location = location;
                }
                else
                {
                    newEvent.LocationId = updateEventDTO.LocationId;
                }
                eventRef = await eventsRef.AddAsync(newEvent);
            }
            else
            {
                eventRef = eventsRef.Document(eventId);
                var valueToUpdate = new Dictionary<string, object>
                {
                    { nameof(Event.LocationId), locationId },
                    { nameof(Event.Location), location },
                    { nameof(Event.LocationText), locationText }
                };
                if(groupId != null)
                    valueToUpdate.Add(nameof(Event.GroupId), groupId);
                if (eventCategoryId != null)
                    valueToUpdate.Add(nameof(Event.EventCategoryId), eventCategoryId);
                if(dateUTC != null)
                {
                    valueToUpdate.Add(nameof(Event.Date), dateUTC);
                    var snapshot = await eventRef.GetSnapshotAsync();
                    var @event = snapshot.ConvertTo<Event>();
                    oldDate = @event.Date;
                }
                var updateTask = eventRef.UpdateAsync(valueToUpdate, Precondition.MustExist);
                dataChangesTasks.Add(updateTask);
            }

            var helpersRef = eventRef.Collection(Paths.HELPER);
            var currentHelpers = await GetRequirementsOfEvent(departmentId, eventId);

            var requirementsToDelete = currentHelpers.Where(requirement => !updateHelpers.Any(updateRequirement => updateRequirement.RoleId == requirement.RoleId));
            foreach(var requirement in requirementsToDelete)
            {
                var notificationTask = helpersRef.Document(requirement.Id).DeleteAsync().ContinueWith(async (task) =>
                {
                    await UpdateChangedStatus(departmentId, eventId, requirement.RoleId, requirement.AvailableMembers, HelperStatus.Available, HelperStatus.RequirementDeleted);
                    await UpdateChangedStatus(departmentId, eventId, requirement.RoleId, requirement.PreselectedMembers, HelperStatus.Preselected, HelperStatus.RequirementDeleted);
                    await UpdateChangedStatus(departmentId, eventId, requirement.RoleId, requirement.LockedMembers, HelperStatus.Locked, HelperStatus.RequirementDeleted);
                }, TaskContinuationOptions.NotOnFaulted);
                dataChangesTasks.Add(notificationTask);
            }

            var requirementsToAdd = updateHelpers.Where(requirement => !currentHelpers.Any(currentRequirement => currentRequirement.RoleId == requirement.RoleId));
            foreach(var requirement in requirementsToAdd)
            {
                var newHelper = new Requirement
                {
                    RoleId = requirement.RoleId,
                    RequiredAmount = requirement.RequiredAmount,
                    RequiredGroups = requirement.RequiredGroups,
                    LockingTime = requirement.LockingTime.Date.ToUniversalTime(),
                    LockedMembers = new(),
                    PreselectedMembers = new(),
                    AvailableMembers = new(),
                    RequiredQualifications = requirement.RequiredQualifications
                };
                var addTask = helpersRef.AddAsync(newHelper);
                dataChangesTasks.Add(addTask);
            }

            var existingRequirements = updateHelpers.Where(requirement => currentHelpers.Any(currentRequirement => currentRequirement.RoleId == requirement.RoleId));
            var dateInvolvedMembers = new List<string>();
            foreach (var existingRequirement in existingRequirements)
            {
                var currentRequirement = currentHelpers.First(requirement => requirement.RoleId == existingRequirement.RoleId);
                var updateDict = new Dictionary<string, object> {
                    { nameof(Requirement.RoleId), existingRequirement.RoleId },
                    { nameof(Requirement.RequiredAmount), existingRequirement.RequiredAmount },
                    { nameof(Requirement.RequiredGroups), existingRequirement.RequiredGroups },
                    { nameof(Requirement.LockingTime), existingRequirement.LockingTime.Date.ToUniversalTime() },
                    { nameof(Requirement.RequiredQualifications), existingRequirement.RequiredQualifications }
                };
                if (removeMembers)
                {
                    updateDict.Add(nameof(Requirement.LockedMembers), new List<string>());
                    updateDict.Add(nameof(Requirement.PreselectedMembers), new List<string>());
                    updateDict.Add(nameof(Requirement.AvailableMembers), new List<string>());
                }
                var helperRef = helpersRef.Document(currentRequirement.Id);
                var updateTask = helperRef.UpdateAsync(updateDict);
                dataChangesTasks.Add(updateTask);
                if (removeMembers)
                {
                    var notificationTask = updateTask.ContinueWith(async task =>
                    {
                        await UpdateChangedStatus(departmentId, eventId, existingRequirement.RoleId, currentRequirement.LockedMembers, HelperStatus.Locked, HelperStatus.NotAvailable);
                        await UpdateChangedStatus(departmentId, eventId, existingRequirement.RoleId, currentRequirement.PreselectedMembers, HelperStatus.Preselected, HelperStatus.NotAvailable);
                        await UpdateChangedStatus(departmentId, eventId, existingRequirement.RoleId, currentRequirement.AvailableMembers, HelperStatus.Available, HelperStatus.NotAvailable);
                    }, TaskContinuationOptions.NotOnFaulted).Unwrap();
                    dataChangesTasks.Add(notificationTask);
                }
                if(dateUTC != null)
                {
                    dateInvolvedMembers.AddRange(currentRequirement.LockedMembers);
                    dateInvolvedMembers.AddRange(currentRequirement.PreselectedMembers);
                    dateInvolvedMembers.AddRange(currentRequirement.AvailableMembers);
                }
            }

            if(dateInvolvedMembers.Any())
            {
                var notificationTask = CreateEventNotification(departmentId, eventId, oldDate.Value, dateUTC.Value, dateInvolvedMembers);
                dataChangesTasks.Add(notificationTask);
            }            
            
            await Task.WhenAll(dataChangesTasks);
        }

        public async Task DeleteEvent(string departmentId, string eventId)
        {
            var eventRef = GetEventReference(departmentId, eventId);
            var @event = await GetEvent(departmentId, eventId);
            var requirementsRef = GetRequirementsReference(departmentId, eventId);
            var requirements = await GetRequirementsOfEvent(departmentId, eventId);
            var allInvolvedMembers = requirements.SelectMany(requirement => requirement.LockedMembers.Union(requirement.PreselectedMembers)).Distinct().ToList();

            var tasks = new List<Task>();
            foreach(var requirement in requirements)
            {
                var requirementTask = requirementsRef.Document(requirement.Id).DeleteAsync();
                tasks.Add(requirementTask);
            }
            await Task.WhenAll(tasks);
            await eventRef.DeleteAsync().ContinueWith(task => CreateEventDeletionNotification(@event, allInvolvedMembers), TaskContinuationOptions.NotOnFaulted).Unwrap();
        }

        public async Task<List<EventDTO>> GetAllEvents(string departmentId, DateTime fromDate, DateTime toDate)
        {
            var snapshot = await GetEventsReference(departmentId)
                .WhereGreaterThanOrEqualTo(nameof(Event.Date), fromDate.ToUniversalTime())
                .WhereLessThanOrEqualTo(nameof(Event.Date), toDate.ToUniversalTime())
                .GetSnapshotAsync();
            var events = new List<Event>();
            foreach (var document in snapshot)
            {
                var @event = document.ConvertTo<Event>();
                if (@event == null)
                    continue;
                events.Add(@event);
            }
            return EventConverter.Convert(events, departmentId);
        }

        public async Task<EventDTO?> GetEvent(string departmentId, string eventId)
        {
            var snapshot = await GetEventReference(departmentId, eventId).GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            var @event = snapshot.ConvertTo<Event>();
            return EventConverter.Convert(@event, departmentId);            
        }

        public async Task<List<HelperDTO>> GetAllRequirements(string departmentId)
        {
            var snapshot = await GetEventsReference(departmentId).GetSnapshotAsync();

            var helpersList = snapshot.Select(eventSnapshot =>
            {
                return GetRequirementsOfEvent(departmentId, eventSnapshot.Id);
            });
            var helpers = await Task.WhenAll(helpersList);
            return helpers.SelectMany(requirements => requirements).ToList();
        }

        public async Task<List<HelperDTO>> GetRequirementsOfEvent(string departmentId, string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return new();

            var helpersSnapshot = await GetRequirementsReference(departmentId, eventId).GetSnapshotAsync();

            var helpers = new List<Requirement>();
            foreach (var document in helpersSnapshot)
            {
                var helper = document.ConvertTo<Requirement>();
                if (helper == null)
                    continue;
                helpers.Add(helper);
            }
            return HelperConverter.Convert(helpers, eventId);
        }

        public async Task SetIsAvailable(string departmentId, string eventId, string helperId, string memberId, bool isAvailable)
        {
            var helperReference = GetRequirementReference(departmentId, eventId, helperId);

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
                if (helper.PreselectedMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
                if (helper.AvailableMembers.Contains(memberId))
                    await helperReference.UpdateAsync(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(memberId), Precondition.MustExist);
            }
        }

        public async Task UpdateLockedMembers(string departmentId, string eventId, string helperId, UpdateMembersListDTO updateMembersList)
        {
            if ((updateMembersList.FormerMembers.Any() || updateMembersList.NewMembers.Any()) == false)
                return;

            var requirementReference = GetRequirementReference(departmentId, eventId, helperId);
            var requirementSnapshot = await requirementReference.GetSnapshotAsync();
            var previousRequirement = requirementSnapshot.ConvertTo<Requirement>();

            await requirementReference.UpdateAsync(nameof(Requirement.LockedMembers), FieldValue.ArrayRemove(updateMembersList.FormerMembers.ToArray()));
            await requirementReference.UpdateAsync(nameof(Requirement.LockedMembers), FieldValue.ArrayUnion(updateMembersList.NewMembers.ToArray()));
            await requirementReference.UpdateAsync(nameof(Requirement.PreselectedMembers), FieldValue.ArrayRemove(updateMembersList.NewMembers.ToArray()));
            await requirementReference.UpdateAsync(nameof(Requirement.AvailableMembers), FieldValue.ArrayRemove(updateMembersList.NewMembers.ToArray()));

            await UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, updateMembersList.FormerMembers, HelperStatus.Locked, HelperStatus.NotAvailable);
            var previouslyAvailable = updateMembersList.NewMembers.Where(previousRequirement.AvailableMembers.Contains);
            var previouslyPreselected = updateMembersList.NewMembers.Where(previousRequirement.PreselectedMembers.Contains);
            var previouslyNotAvailable = updateMembersList.NewMembers.Except(previouslyAvailable).Except(previouslyPreselected);
            await UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, previouslyAvailable, HelperStatus.Available, HelperStatus.Locked);
            await UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, previouslyPreselected, HelperStatus.Preselected, HelperStatus.Locked);
            await UpdateChangedStatus(departmentId, eventId, previousRequirement.RoleId, previouslyNotAvailable, HelperStatus.NotAvailable, HelperStatus.Locked);

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
                Date = @event.Date,
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
                                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(@event.Date.ToUniversalTime(), timeZoneOfMember);

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

        public async Task<Dictionary<string, Tuple<int, int>>> GetStats(string departmentId, string roleId, DateTime fromDate, DateTime toDate)
        {
            var eventsInRange = await GetAllEvents(departmentId, fromDate, toDate);
            var requirementsInRange = await Task.WhenAll(eventsInRange.Select(@event => GetRequirementsOfEvent(departmentId, @event.Id)));
            var requirementsOfRole = requirementsInRange.SelectMany(l => l).Where(requirement => requirement.RoleId == roleId);
                        
            var countsTotal = requirementsOfRole.SelectMany(requirement =>
            {
                return requirement.LockedMembers.Concat(requirement.PreselectedMembers).Concat(requirement.AvailableMembers);
            }).GroupBy(memberId => memberId).ToDictionary(group => group.Key, group => group.Count());
            var countsFixed = requirementsOfRole.SelectMany(requirement =>
            {
                return requirement.LockedMembers.Concat(requirement.PreselectedMembers);
            }).GroupBy(memberId => memberId).ToDictionary(group => group.Key, group => group.Count());
            var counts = countsTotal.ToDictionary(pair => pair.Key, pair => Tuple.Create(pair.Value, countsFixed.GetValueOrDefault(pair.Key, 0)));
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

        private CollectionReference GetEventsReference(string departmentId)
        {
            return GetDepartmentReference(departmentId).Collection(Paths.EVENT);
        }

        private DocumentReference GetEventReference(string departmentId, string eventId)
        {
            return GetEventsReference(departmentId).Document(eventId);
        }

        private CollectionReference GetRequirementsReference(string departmentId, string eventId)
        {
            return GetEventReference(departmentId, eventId).Collection(Paths.HELPER);
        }

        private DocumentReference GetRequirementReference(string departmentId, string eventId, string requirementId)
        {
            return GetRequirementsReference(departmentId, eventId).Document(requirementId);
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
