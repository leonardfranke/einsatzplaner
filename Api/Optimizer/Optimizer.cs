using DTO;
using Google.OrTools.Sat;
using NuGet.Packaging;

namespace Optimizer
{
    public static class Optimizer
    {
        public class OptimizedAssignments
        {
            public List<string> LockedMembers { get; set; }
            public List<string> PreselectedMembers { get; set; }
            public List<string> FillMembers { get; set; }
            public List<string> AvailableMembers { get; set; }
        }

        public static Dictionary<HelperDTO, OptimizedAssignments> OptimizeAssignments(List<EventDTO> allEvents, List<HelperDTO> allRequirements, List<RoleDTO> roles, List<QualificationDTO> qualifications)
        {
            var eventsToOptimize = allEvents.Where(@event => @event.Date > DateTime.UtcNow);
            var requirementsToOptimize = allRequirements.Where(requirement => eventsToOptimize.Any(@event => @event.Id == requirement.EventId));

            var F_mr_dict = new Dictionary<(string, string), int>();
            foreach (var requirement in allRequirements)
            {
                foreach (var member in requirement.LockedMembers)
                {
                    F_mr_dict[(member, requirement.RoleId)] = F_mr_dict.GetValueOrDefault((member, requirement.RoleId)) + 1;
                }
            }

            var B_erq_dict = new Dictionary<(string, string, string), int>();
            foreach (var @event in eventsToOptimize)
            {
                var requirementsOfEvent = allRequirements.Where(requirement => requirement.EventId == @event.Id);
                foreach (var requirement in requirementsOfEvent)
                {
                    var openRequiredAmountForRole = requirement.RequiredAmount - requirement.LockedMembers.Count;
                    foreach (var (qualificationId, requiredAmount) in requirement.RequiredQualifications)
                    {
                        B_erq_dict[(@event.Id, requirement.RoleId, qualificationId)] = requiredAmount;
                        openRequiredAmountForRole -= requiredAmount;
                    }
                    if (openRequiredAmountForRole > 0)
                        B_erq_dict[(@event.Id, requirement.RoleId, string.Empty)] = openRequiredAmountForRole;
                }
            }

            var P_mer_list = new List<(string, string, string)>();
            foreach (var @event in eventsToOptimize)
            {
                var requirementsOfEvent = allRequirements.Where(requirement => requirement.EventId == @event.Id);
                var lockedMembersOfEvent = requirementsOfEvent.SelectMany(requirement => requirement.LockedMembers);
                foreach (var requirement in requirementsOfEvent)
                {
                    foreach (var member in requirement.PreselectedMembers.Except(lockedMembersOfEvent))
                    {
                        P_mer_list.Add((member, @event.Id, requirement.RoleId));
                    }
                }
            }

            var S_mrq_dict = GetQualificationAssignments(qualifications);
                       
            var X_mer_dict_available = new Dictionary<(string memberId, string eventId, string roleId), IntVar>();
            var X_merq_dict_available = new Dictionary<(string memberId, string eventId, string roleId, string qualificationId), IntVar>();
            var X_mer_dict_additional = new Dictionary<(string memberId, string eventId, string roleId), IntVar>();
            var X_merq_dict_additional = new Dictionary<(string memberId, string eventId, string roleId, string qualificationId), IntVar>();

            var model = new CpModel();
            foreach (var @event in eventsToOptimize)
            {
                var requirementsOfEvent = allRequirements.Where(requirement => requirement.EventId == @event.Id);
                var lockedMembersOfEvent = requirementsOfEvent.SelectMany(requirement => requirement.LockedMembers);
                foreach(var requirement in requirementsOfEvent)
                {
                    var role = roles.Find(role => role.Id == requirement.RoleId);
                    var availableMembers = requirement.PreselectedMembers.Union(requirement.AvailableMembers);
                    foreach (var member in availableMembers.Except(lockedMembersOfEvent))
                    {
                        var X_mer = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}: {member}");
                        X_mer_dict_available[(member, requirement.EventId, requirement.RoleId)] = X_mer;
                        BoolVar X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, null: {member}");
                        X_merq_dict_available[(member, requirement.EventId, requirement.RoleId, string.Empty)] = X_merq;
                        var X_merq_list = new List<BoolVar> { X_merq };
                        foreach (var qualification in requirement.RequiredQualifications.Keys)
                        {
                            X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, {qualification}: {member}");
                            X_merq_dict_available[(member, requirement.EventId, requirement.RoleId, qualification)] = X_merq;
                            X_merq_list.Add(X_merq);
                            model.Add(X_merq <= (S_mrq_dict.GetValueOrDefault((member, requirement.RoleId, qualification), false) ? 1 : 0));
                        }
                        model.Add(X_mer == LinearExpr.Sum(X_merq_list));
                    }
                    if(role?.MemberIds?.Count > 0)
                    {
                        foreach (var member in role.MemberIds.Except(availableMembers).Except(lockedMembersOfEvent))
                        {
                            var X_mer = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}: {member}");
                            X_mer_dict_additional[(member, requirement.EventId, requirement.RoleId)] = X_mer;
                            BoolVar X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, null: {member}");
                            X_merq_dict_additional[(member, requirement.EventId, requirement.RoleId, string.Empty)] = X_merq;
                            var X_merq_list = new List<BoolVar> { X_merq };
                            foreach (var qualification in requirement.RequiredQualifications.Keys)
                            {
                                X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, {qualification}: {member}");
                                X_merq_dict_additional[(member, requirement.EventId, requirement.RoleId, qualification)] = X_merq;
                                X_merq_list.Add(X_merq);
                                model.Add(X_merq <= (S_mrq_dict.GetValueOrDefault((member, requirement.RoleId, qualification), false) ? 1 : 0));
                            }
                            model.Add(X_mer == LinearExpr.Sum(X_merq_list));
                        }
                    }
                }
            }

            foreach(var group in X_mer_dict_available.GroupBy(pair => (pair.Key.memberId, pair.Key.eventId)))
            {
                model.Add(LinearExpr.Sum(group.Select(pair => pair.Value)) <= 1);
            }
            foreach (var group in X_mer_dict_additional.GroupBy(pair => (pair.Key.memberId, pair.Key.eventId)))
            {
                model.Add(LinearExpr.Sum(group.Select(pair => pair.Value)) <= 1);
            }

            foreach(var (merq, X_merq) in X_merq_dict_available)
            {
                model.Add(X_merq <= B_erq_dict.GetValueOrDefault((merq.eventId, merq.roleId, merq.qualificationId), 0));
            }
            foreach (var (merq, X_merq) in X_merq_dict_additional)
            {
                model.Add(X_merq <= B_erq_dict.GetValueOrDefault((merq.eventId, merq.roleId, merq.qualificationId), 0));
            }

            var V_erq_available_list = new List<LinearExpr>();
            var V_erq_additional_list = new List<LinearExpr>();
            foreach (var ((eventId, roleId, qualificationId), B_erq) in B_erq_dict)
            {
                var V_erq_available = B_erq - LinearExpr.Sum(X_merq_dict_available
                    .Where(pair => pair.Key.eventId == eventId && pair.Key.roleId == roleId && pair.Key.qualificationId == qualificationId)
                    .Select(pair => pair.Value));
                model.Add(V_erq_available >= 0);
                V_erq_available_list.Add(V_erq_available);
                var V_erq_additional = V_erq_available - LinearExpr.Sum(X_merq_dict_additional
                    .Where(pair => pair.Key.eventId == eventId && pair.Key.roleId == roleId && pair.Key.qualificationId == qualificationId)
                    .Select(pair => pair.Value));
                model.Add(V_erq_additional >= 0);
                V_erq_additional_list.Add(V_erq_additional);
            }

            var E_mr_dict_available = X_mer_dict_available
                .GroupBy(pair => (pair.Key.memberId, pair.Key.roleId))
                .ToDictionary(group => group.Key, group => F_mr_dict.GetValueOrDefault(group.Key, 0) + LinearExpr.Sum(group.Select(pair => pair.Value)));
            var E_mr_dict_additional = X_mer_dict_additional
                .GroupBy(pair => (pair.Key.memberId, pair.Key.roleId))
                .ToDictionary(group => group.Key, group => E_mr_dict_available.GetValueOrDefault(group.Key, LinearExpr.Constant(0)) + LinearExpr.Sum(group.Select(pair => pair.Value)));

            var D_mmr_list_available = new List<IntVar>();
            var E_mr_list_available = E_mr_dict_available.ToList();
            for (var i = 0; i < E_mr_list_available.Count; i++)
            {
                var ((memberId, roleId), E_mr) = E_mr_list_available[i];
                for (var j = i+1; j < E_mr_list_available.Count; j++)
                {
                    var ((otherMemberId, otherRoleId), otherE_mr) = E_mr_list_available[j];
                    if(roleId == otherRoleId && memberId != otherMemberId)
                    {
                        var D_mmr = model.NewIntVar(0, allRequirements.Count, "Diff");
                        model.AddAbsEquality(D_mmr, E_mr - otherE_mr);
                        D_mmr_list_available.Add(D_mmr);
                    }
                }
            }
            var max_D_available = allRequirements.Count * D_mmr_list_available.Count + 1;

            var D_mmr_list_additional = new List<IntVar>();
            var E_mr_list_additional = E_mr_dict_additional.ToList();
            for (var i = 0; i < E_mr_list_additional.Count; i++)
            {
                var ((roleId, memberId), E_mr) = E_mr_list_additional[i];
                for (var j = i + 1; j < E_mr_list_additional.Count; j++)
                {
                    var ((otherRoleId, otherMemberId), otherE_mr) = E_mr_list_additional[j];
                    if (roleId == otherRoleId && memberId != otherMemberId)
                    {
                        var D_mmr = model.NewIntVar(0, allRequirements.Count, "");
                        model.AddAbsEquality(D_mmr, E_mr - otherE_mr);
                        D_mmr_list_available.Add(D_mmr);
                    }
                }
            }
            var max_D_additional = allRequirements.Count * D_mmr_list_additional.Count + 1;

            var D_mer_list = P_mer_list.Select(mer => 1 - X_mer_dict_available[mer]);
            var max_De = D_mer_list.Count() + 1;

            model.Minimize(max_De * (max_D_available * LinearExpr.Sum(V_erq_available_list) + LinearExpr.Sum(D_mmr_list_available)) + LinearExpr.Sum(D_mer_list));
            var solver_avaialble = new CpSolver();
            solver_avaialble.Solve(model);  
            
            foreach(var X_mer in X_mer_dict_available.Values)
            {
                model.Add(X_mer == solver_avaialble.Value(X_mer));
            }

            foreach (var X_merq in X_merq_dict_available.Values)
            {
                model.Add(X_merq == solver_avaialble.Value(X_merq));
            }

            model.Minimize(max_D_additional * LinearExpr.Sum(V_erq_additional_list) + LinearExpr.Sum(D_mmr_list_additional));
            var solver_additional = new CpSolver();
            solver_additional.Solve(model);

            var filledHelpers = new Dictionary<HelperDTO, OptimizedAssignments>();
            var X_mer_by_er_available = X_mer_dict_available.GroupBy(pair => (pair.Key.eventId, pair.Key.roleId));
            var X_mer_by_er_additional = X_mer_dict_additional .GroupBy(pair => (pair.Key.eventId, pair.Key.roleId));
            foreach (var requirement in requirementsToOptimize)
            {
                var lockedMembers = new HashSet<string>(requirement.LockedMembers);
                var preselectedMembers = new List<string>();
                var availableMembers = new List<string>();
                var fillMembers = new List<string>();

                var groupAvailable = X_mer_by_er_available.FirstOrDefault(group => group.Key == (requirement.EventId, requirement.RoleId));
                if(groupAvailable != null)
                {
                    foreach (var ((member, _, _), X_erm) in groupAvailable)
                    {
                        if(solver_additional.Value(X_erm) == 1)
                            preselectedMembers.Add(member);
                        else
                            availableMembers.Add(member);
                    }
                }

                var groupAdditional = X_mer_by_er_additional.FirstOrDefault(group => group.Key == (requirement.EventId, requirement.RoleId));
                if(groupAdditional != null)
                {
                    foreach (var ((member, _, _), X_erm) in groupAdditional)
                    {
                        if (solver_additional.Value(X_erm) == 1)
                            fillMembers.Add(member);
                    }
                }

                if (DateTime.Now < requirement.LockingTime)
                {
                    filledHelpers.Add(
                        requirement,
                        new OptimizedAssignments { 
                            LockedMembers = [.. lockedMembers], 
                            PreselectedMembers = [.. preselectedMembers], 
                            AvailableMembers = [.. availableMembers],
                            FillMembers = [.. fillMembers]
                        });
                }
                else
                {
                    filledHelpers.Add(
                        requirement,
                        new OptimizedAssignments
                        {
                            LockedMembers = [.. lockedMembers.Union(preselectedMembers).Union(fillMembers)],
                            PreselectedMembers = [],
                            AvailableMembers = [.. availableMembers],
                            FillMembers = []
                        });
                }
            }

            return filledHelpers;
        }

        private static Dictionary<(string, string, string), bool> GetQualificationAssignments(IEnumerable<QualificationDTO> qualifications)
        {
            var S_mrq_dict = new Dictionary<(string, string, string), bool>();
            foreach (var qualification in qualifications)
            {
                foreach (var member in qualification.MemberIds)
                {
                    S_mrq_dict[(member, qualification.RoleId, qualification.Id)] = true;
                }
            }
            return S_mrq_dict;
        }
    }
}
