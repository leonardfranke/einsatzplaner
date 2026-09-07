using Api.Converter;
using Api.Models;
using DTO;
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
        private IMailjetClient _mailjetClient;
        private IGroupManager _groupManager;
        private IRoleManager _roleManager;
        private IDepartmentManager _departmentManager;
        private IMemberManager _memberManager;
        private IEventCategoryManager _eventCategoryManager;
        private IUserManager _userManager;

        public EventManager(IMailjetClient mailjetClient, IGroupManager groupManager, IRoleManager roleManager, IDepartmentManager departmentManager, IMemberManager memberManager, IEventCategoryManager eventCategoryManager, IUserManager userManager, Client supabaseClient)
        {
            _groupManager = groupManager;
            _roleManager = roleManager;
            _mailjetClient = mailjetClient;
            _departmentManager = departmentManager;
            _memberManager = memberManager;
            _eventCategoryManager = eventCategoryManager;
            _userManager = userManager;
            _supabaseClient = supabaseClient;
        }

        public async Task<string> CreateEvent(UpdateEventDTO updateEventDTO)
        {
            var newEvent = new Event
            {
                DepartmentId = updateEventDTO.DepartmentId,
                GroupId = updateEventDTO.GroupId,
                EventCategoryId = updateEventDTO.EventCategoryId,
                Date = updateEventDTO.Date,
            };

            if(!string.IsNullOrEmpty(updateEventDTO.LocationId))
            {
                newEvent.LocationId = updateEventDTO.LocationId;
            }
            else
            {
                newEvent.LocationText = updateEventDTO.LocationText;
                newEvent.LocationLatitude = updateEventDTO.Latitude;
                newEvent.LocationLongitude = updateEventDTO.Longitude;
            }

            var @event = await _supabaseClient.From<Event>().Insert(newEvent);
            return @event.Model!.Id;
        }

        public async Task UpdateEvent(UpdateEventDTO updateEventDTO)
        {
            var oldEvent = await GetEvent(updateEventDTO.DepartmentId, updateEventDTO.EventId);

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
            else if(oldEvent.Date.UtcTicks != updateEventDTO.Date.UtcTicks)
            {
                var enterings = await GetEnteringsOfEvent(updateEventDTO.DepartmentId, updateEventDTO.EventId, [EnteringType.Locked, EnteringType.Preselected]);
                var setMembers = enterings.Select(entering => entering.MemberId).Distinct().ToList();
                await CreateEventNotification(updateEventDTO.DepartmentId, updateEventDTO.EventId, oldEvent.Date, updateEventDTO.Date, setMembers);

            }
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

        public async Task DeleteEvent(string departmentId, string eventId)
        {
            var @event = await GetEvent(departmentId, eventId);
            if(@event == null)
                return;
            var requirements = await GetRequirements(departmentId, eventId, null).ToListAsync();
            var allInvolvedMembers = requirements.SelectMany(requirement => requirement.LockedMembers.Union(requirement.PreselectedMembers)).Distinct().ToList();

            await _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Event.Id), Operator.Equals, eventId)
                .Delete();

            if (@event != null)
                await CreateEventDeletionNotification(@event, allInvolvedMembers);
        }

        public async Task<List<Event>> GetAllEvents(string departmentId, DateTime fromDate, DateTime toDate)
        {
            var res = await _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Event.Date), Operator.GreaterThanOrEqual, fromDate.ToUniversalTime().ToString("O"))
                .Filter(nameof(Event.Date), Operator.LessThanOrEqual, toDate.ToUniversalTime().ToString("O"))
                .Get();
            return res.Models;
        }

        public Task<Event?> GetEvent(string departmentId, string eventId)
        {
            return _supabaseClient
                .From<Event>()
                .Filter(nameof(Event.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Event.Id), Operator.Equals, eventId).Single();
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

            var requirementsResult = await query.Get();

            var fetchedEventIds = requirementsResult.Models.Select(requirement => requirement.EventId).Distinct().ToList();
            var fetchedRoleIds = requirementsResult.Models.Select(requirement => requirement.RoleId).Distinct().ToList();
            var enteringsResult = await _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.In, fetchedEventIds)
                .Filter(nameof(Entering.RoleId), Operator.In, fetchedRoleIds)
                .Get();

            var qualiRequirementsResult = await _supabaseClient
                .From<QualificationRequirement>()
                .Filter(nameof(QualificationRequirement.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(QualificationRequirement.EventId), Operator.In, fetchedEventIds)
                .Filter(nameof(QualificationRequirement.RoleId), Operator.In, fetchedRoleIds)
                .Get();

            foreach (var requirement in requirementsResult.Models)
            {
                var enteringsOfRequirement = enteringsResult.Models
                    .Where(entering => entering.EventId == requirement.EventId && entering.RoleId == requirement.RoleId)
                    .ToList();
                var lockedMembers = enteringsOfRequirement
                    .Where(entering => entering.EnteringType == EnteringType.Locked)
                    .Select(entering => entering.MemberId)
                    .ToList();
                var preselectedMembers = enteringsOfRequirement
                    .Where(entering => entering.EnteringType == EnteringType.Preselected)
                    .Select(entering => entering.MemberId)
                    .ToList();
                var availableMembers = enteringsOfRequirement
                    .Where(entering => entering.EnteringType == EnteringType.Available)
                    .Select(entering => entering.MemberId)
                    .ToList();
                var recommendedMembers = enteringsOfRequirement
                    .Where(entering => entering.EnteringType == EnteringType.Recommended)
                    .Select(entering => entering.MemberId)
                    .ToList();
                var qualificationRequirements = qualiRequirementsResult.Models
                    .Where(qualiRequirement => qualiRequirement.EventId == requirement.EventId && qualiRequirement.RoleId == requirement.RoleId)
                    .ToDictionary(qualiRequirement => qualiRequirement.QualificationId, qualiRequirement => qualiRequirement.RequiredAmount);
                yield return RequirementConverter.Convert(requirement, lockedMembers ?? [], preselectedMembers ?? [], availableMembers ?? [], recommendedMembers ?? [], qualificationRequirements);
            }            
        }

        private Task<Entering?> GetEntering(string departmentId, string eventId, string roleId, string memberId)
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
                    await SetMembersEntering(departmentId, eventId, roleId, [memberId], EnteringType.Available);
                return;
            }

            if (entering.EnteringType == EnteringType.Locked 
                || (isAvailable && (entering.EnteringType == EnteringType.Preselected || entering.EnteringType == EnteringType.Available))
                || !isAvailable && entering.EnteringType == EnteringType.Recommended)
                return;

            if (isAvailable)
                await SetMembersEntering(departmentId, eventId, roleId, [memberId], EnteringType.Available);
            else
                await SetMembersEntering(departmentId, eventId, roleId, [memberId], null);
        }

        private async Task<List<Entering>> GetEnteringsOfEvent(string departmentId, string eventId, List<EnteringType> enteringTypes)
        {
            var currentEnteringsResult = await _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                .Filter(nameof(Entering.EnteringType), Operator.In, enteringTypes.Select(type => (int)type).ToList())
                .Get();

            return currentEnteringsResult.Models;
        }

        public async Task SetMembersEntering(string departmentId, string eventId, string roleId, List<string> memberIds, EnteringType? type)
        {
            if(memberIds == null || !memberIds.Any())
                return;

            var distinctMemberIds = memberIds.Distinct().ToList();
            var currentEnteringsResult = await _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                .Filter(nameof(Entering.RoleId), Operator.Equals, roleId)
                .Filter(nameof(Entering.MemberId), Operator.In, distinctMemberIds)
                .Get();
            var currentEnterings = currentEnteringsResult.Models.ToDictionary(entering => entering.MemberId, entering => (EnteringType?)entering.EnteringType);

            if (type == null)
            {
                await _supabaseClient
                    .From<Entering>()
                    .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                    .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                    .Filter(nameof(Entering.RoleId), Operator.Equals, roleId)
                    .Filter(nameof(Entering.MemberId), Operator.In, distinctMemberIds)
                    .Delete();
            }
            else
            {
                await _supabaseClient.From<Entering>().Upsert(distinctMemberIds.Select(memberId => new Entering
                {
                    DepartmentId = departmentId,
                    EventId = eventId,
                    RoleId = roleId,
                    MemberId = memberId,
                    EnteringType = type.Value
                }).ToList());
            }

            var changedMembersByStatus = distinctMemberIds
                .Select(memberId => new
                {
                    MemberId = memberId,
                    PreviousEnteringType = currentEnterings.TryGetValue(memberId, out var currentType) ? currentType : null,
                    NewEnteringType = type
                })
                .Where(change => change.PreviousEnteringType != change.NewEnteringType)
                .Select(change => new
                {
                    change.MemberId,
                    PreviousStatus = ConvertToHelperStatus(change.PreviousEnteringType),
                    NewStatus = ConvertToHelperStatus(change.NewEnteringType)
                })
                .Where(change => change.PreviousStatus.HasValue && change.NewStatus.HasValue)
                .GroupBy(change => new { PreviousStatus = change.PreviousStatus!.Value, NewStatus = change.NewStatus!.Value })
                .ToList();

            var updateTasks = changedMembersByStatus.Select(changeGroup => UpdateChangedStatus(
                departmentId,
                eventId,
                roleId,
                changeGroup.Select(change => change.MemberId),
                changeGroup.Key.PreviousStatus,
                changeGroup.Key.NewStatus));
            await Task.WhenAll(updateTasks);
        }

        private async Task RemoveMemberEnterings(string departmentId, string eventId)
        {
            var allEnteringsResult = await _supabaseClient
                .From<Entering>()
                .Filter(nameof(Entering.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(Entering.EventId), Operator.Equals, eventId)
                .Get();

            var removeTasks = allEnteringsResult.Models
                .GroupBy(entering => entering.RoleId)
                .Select(group => SetMembersEntering(departmentId, eventId, group.Key, group.Select(entering => entering.MemberId).Distinct().ToList(), null));
            await Task.WhenAll(removeTasks);
        }

        private static HelperStatus? ConvertToHelperStatus(EnteringType? enteringType)
        {
            if (enteringType == null)
                return HelperStatus.NotAvailable;

            return enteringType.Value switch
            {
                EnteringType.Locked => HelperStatus.Locked,
                EnteringType.Preselected => HelperStatus.Preselected,
                EnteringType.Available => HelperStatus.Available,
                _ => null
            };
        }

        public async Task UpdateChangedStatus(string departmentId, string eventId, string roleId, IEnumerable<string> memberIds, HelperStatus previousStatus, HelperStatus newStatus)
        {
            if (!memberIds.Any())
                return;

            var notification = await _supabaseClient
                .From<HelperNotification>()
                .Filter(nameof(HelperNotification.DepartmentId), Operator.Equals, departmentId)
                .Filter(nameof(HelperNotification.EventId), Operator.Equals, eventId)
                .Filter(nameof(HelperNotification.RoleId), Operator.Equals, roleId)
                .Single();

            if (notification == null)
            {
                notification = new HelperNotification
                {
                    DepartmentId = departmentId,
                    EventId = eventId,
                    RoleId = roleId,
                    PreviousStatus = new Dictionary<string, HelperStatus>(),
                    NewStatus = new Dictionary<string, HelperStatus>()
                };
            }

            foreach (var memberId in memberIds)
            {
                if (!notification.PreviousStatus.ContainsKey(memberId))
                    notification.PreviousStatus[memberId] = previousStatus;
                notification.NewStatus[memberId] = newStatus;
            }

            await _supabaseClient.From<HelperNotification>().Upsert(notification);
        }

        private async Task CreateEventDeletionNotification(Event @event, List<string> members)
        {
            if (!members.Any())
                return;
            
            var group = await _groupManager.GetById(@event.DepartmentId, @event.GroupId);
            var eventCategory = await _eventCategoryManager.GetById(@event.DepartmentId, @event.EventCategoryId);
            var notification = new DeletionNotification
            {
                DepartmentId = @event.DepartmentId,
                EventId = @event.Id,
                Group = group?.Id,
                EventCategory = eventCategory?.Id,
                Date = @event.Date.UtcDateTime,
                Members = members,
            };
            
            await _supabaseClient.From<DeletionNotification>().Insert(notification);
        }

        private async Task CreateEventNotification(string departmentId, string eventId, DateTimeOffset previousDate, DateTimeOffset newDate, List<string> members)
        {
            var notification = new EventNotification
            {
                DepartmentId = departmentId,
                EventId = eventId,
                PreviousDate = previousDate,
                NewDate = newDate,
                Members = members
            };

            await _supabaseClient.From<EventNotification>().Insert(notification);
        }

        public async Task SendHelperNotifications()
        {
            var departments = await _departmentManager.GetAll();
            foreach(var department in departments)
            {
                var requirementNotificationsResult = await _supabaseClient
                    .From<HelperNotification>()
                    .Filter(nameof(HelperNotification.DepartmentId), Operator.Equals, department.Id)
                    .Get();
                var eventNotificationsResult = await _supabaseClient
                    .From<EventNotification>()
                    .Filter(nameof(EventNotification.DepartmentId), Operator.Equals, department.Id)
                    .Get();
                var deletionNotificationsResult = await _supabaseClient
                    .From<DeletionNotification>()
                    .Filter(nameof(DeletionNotification.DepartmentId), Operator.Equals, department.Id)
                    .Get();

                var requirementNotifications = requirementNotificationsResult.Models;
                var eventNotifications = eventNotificationsResult.Models;
                var deletionNotifications = deletionNotificationsResult.Models;

                var releventEvents = new Dictionary<string, Event>();
                var relevantRoles = new Dictionary<string, Role>();

                var requirementNotificationDict = new Dictionary<string, Dictionary<string, List<(string, HelperStatus, HelperStatus)>>>();
                foreach (var notification in requirementNotifications)
                {
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

                var eventNotificationDict = new Dictionary<string, List<(string, DateTimeOffset, DateTimeOffset)>>();
                foreach (var notification in eventNotifications)
                {
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
                foreach (var notification in deletionNotifications)
                {
                    foreach (var memberId in notification.Members)
                    {
                        if (!deletionNotificationDict.ContainsKey(memberId))
                            deletionNotificationDict.Add(memberId, []);
                        deletionNotificationDict[memberId].Add((notification.Group, notification.EventCategory, notification.Date));
                        if (requirementNotificationDict.ContainsKey(memberId))
                        {
                            foreach (var requirementNotificationsOfRole in requirementNotificationDict[memberId].Values)
                            {
                                requirementNotificationsOfRole.RemoveAll(reqNotif => reqNotif.Item1 == notification.EventId);
                            }
                        }
                        if (eventNotificationDict.ContainsKey(memberId))
                        {
                            eventNotificationDict[memberId].RemoveAll(eventNotif => eventNotif.Item1 == notification.EventId);
                        }
                    }
                }

                var relevantGroupIds = releventEvents
                    .Where(pair => !string.IsNullOrEmpty(pair.Value?.GroupId))
                    .Select(pair => pair.Value.GroupId)
                    .Union(deletionNotifications.Where(notification => !string.IsNullOrEmpty(notification.Group)).Select(notification => notification.Group))
                    .Distinct()
                    .ToList();
                var relevantEventCategoryIds = releventEvents
                    .Where(pair => !string.IsNullOrEmpty(pair.Value?.EventCategoryId))
                    .Select(pair => pair.Value.EventCategoryId)
                    .Union(deletionNotifications.Where(notification => !string.IsNullOrEmpty(notification.EventCategory)).Select(notification => notification.EventCategory))
                    .Distinct()
                    .ToList();

                var relevantGroups = await Task.WhenAll(relevantGroupIds.Select(groupId => _groupManager.GetById(department.Id, groupId)));
                var relevantEventCategories = await Task.WhenAll(relevantEventCategoryIds.Select(eventCategoryId => _eventCategoryManager.GetById(department.Id, eventCategoryId)));

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
                        var changesText = new SortedList<DateTimeOffset, string>(Comparer<DateTimeOffset>.Create((a, b) => a == b ? 1 : a.CompareTo(b)));
                        foreach (var change in eventNotificationDict[memberId])
                        {
                            var @event = releventEvents.Values.First(e => e?.Id == change.Item1);

                            var group = string.IsNullOrEmpty(@event.GroupId) ? null : relevantGroups.FirstOrDefault(group => group.Id == @event.GroupId);
                            var eventCategory = string.IsNullOrEmpty(@event.EventCategoryId) ? null : relevantEventCategories.FirstOrDefault(eventCategory => eventCategory.Id == @event.EventCategoryId);

                            var previousDateInfo = change.Item2.ToString("dd.MM.yyyy HH:mm");
                            var newDateInfo = change.Item3.ToString("dd.MM.yyyy HH:mm");
                            var groupInfo = group == null ? "<i>Sonstiges</i>" : group.Name;
                            var eventCategoryInfo = eventCategory == null ? "<i>Sonstiges</i>" : eventCategory.Name;                            
                            changesText.Add(change.Item2, $"<li><a href=\"https://einsatzplaner.net/{department.URL}/event/{@event.Id}\" target=\"_blank\">{groupInfo} - {eventCategoryInfo}:</a> {previousDateInfo} -> {newDateInfo}</li>");
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

                            var group = string.IsNullOrEmpty(change.Item1) ? null : relevantGroups.FirstOrDefault(group => group?.Id == change.Item1);
                            var eventCategory = string.IsNullOrEmpty(change.Item2) ? null : relevantEventCategories.FirstOrDefault(eventCategory => eventCategory?.Id == change.Item2);

                            var dateInfo = localDateTime.ToString("dd.MM.yyyy HH:mm");
                            var groupInfo = group == null ? "<i>Sonstiges</i>" : group.Name;
                            var eventCategoryInfo = eventCategory == null ? "<i>Sonstiges</i>" : eventCategory.Name;
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

                if (requirementNotifications.Any())
                {
                    await _supabaseClient
                        .From<HelperNotification>()
                        .Filter(nameof(HelperNotification.DepartmentId), Operator.Equals, department.Id)
                        .Filter(nameof(HelperNotification.EventId), Operator.In, requirementNotifications.Select(n => n.EventId).Distinct().ToList())
                        .Delete();
                }
                if (eventNotifications.Any())
                {
                    await _supabaseClient
                        .From<EventNotification>()
                        .Filter(nameof(EventNotification.DepartmentId), Operator.Equals, department.Id)
                        .Filter(nameof(EventNotification.EventId), Operator.In, eventNotifications.Select(n => n.EventId).Distinct().ToList())
                        .Delete();
                }
                if (deletionNotifications.Any())
                {
                    await _supabaseClient
                        .From<DeletionNotification>()
                        .Filter(nameof(DeletionNotification.DepartmentId), Operator.Equals, department.Id)
                        .Filter(nameof(DeletionNotification.EventId), Operator.In, deletionNotifications.Select(n => n.EventId).Distinct().ToList())
                        .Delete();
                }
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
    }
}
