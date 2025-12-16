using DTO;
using Google.OrTools.Sat;

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

        public static Dictionary<HelperDTO, OptimizedAssignments> OptimizeAssignments(List<EventDTO> allEvents, List<HelperDTO> allRequirements, List<QualificationDTO> qualifications)
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
                        B_erq_dict[(@event.Id, requirement.RoleId, null)] = openRequiredAmountForRole;
                }
            }

            var P_mer_dict = new Dictionary<(string, string, string), bool>();
            foreach (var @event in eventsToOptimize)
            {
                var requirementsOfEvent = allRequirements.Where(requirement => requirement.EventId == @event.Id);
                var lockedMembersOfEvent = requirementsOfEvent.SelectMany(requirement => requirement.LockedMembers);
                foreach (var requirement in requirementsOfEvent)
                {
                    foreach (var member in requirement.PreselectedMembers.Except(lockedMembersOfEvent))
                    {
                        P_mer_dict[(member, @event.Id, requirement.RoleId)] = true;
                    }
                }
            }

            var S_mrq_dict = GetQualificationAssignments(qualifications);

            var X_merq_by_erq = new Dictionary<(string, string, string), List<IntVar>>();
            var X_mer_by_rm_dict = new Dictionary<(string, string), List<(string, IntVar)>>();
            var X_mer_by_er_dict = new Dictionary<(string, string), List<(string, IntVar)>>();
            var X_mer_dict = new Dictionary<(string, string, string), IntVar>();

            var model = new CpModel();
            foreach (var @event in eventsToOptimize)
            {
                var X_mr_by_m = new Dictionary<string, List<IntVar>>();

                var requirementsOfEvent = allRequirements.Where(requirement => requirement.EventId == @event.Id);
                var lockedMembersOfEvent = requirementsOfEvent.SelectMany(requirement => requirement.LockedMembers);
                foreach(var requirement in requirementsOfEvent)
                {
                    foreach(var member in requirement.AvailableMembers.Union(requirement.PreselectedMembers).Except(lockedMembersOfEvent))
                    {
                        var X_mer = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}: {member}");
                        X_mr_by_m.TryAppend(member, X_mer);
                        X_mer_by_rm_dict.TryAppend((requirement.RoleId, member), (member, X_mer));
                        X_mer_by_er_dict.TryAppend((@event.Id, requirement.RoleId), (member, X_mer));
                        X_mer_dict[(member, @event.Id, requirement.RoleId)] = X_mer;
                        var X_mer_q_list = new List<IntVar>();
                        IntVar X_merq;
                        foreach (var qualification in requirement.RequiredQualifications.Keys)
                        {
                            X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, {qualification}: {member}");
                            X_mer_q_list.Add(X_merq);
                            model.Add(X_merq <= (S_mrq_dict.GetValueOrDefault((member, requirement.RoleId, qualification), false) ? 1 : 0));
                            X_merq_by_erq.TryAppend((@event.Id, requirement.RoleId, qualification), X_merq);                           
                        }
                        X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, null: {member}");
                        X_mer_q_list.Add(X_merq);
                        X_merq_by_erq.TryAppend((@event.Id, requirement.RoleId, null), X_merq);
                        model.Add(X_mer == LinearExpr.Sum(X_mer_q_list));
                    }
                }
                foreach(var X_mer in X_mr_by_m.Values)
                {
                    model.Add(LinearExpr.Sum(X_mer) <= 1);
                }
            }

            var V_erq_list = new List<LinearExpr>();
            foreach(var ((eventId, roleId, qualificationId), X_merq) in X_merq_by_erq)
            {
                var V_erq = B_erq_dict.GetValueOrDefault((eventId, roleId, qualificationId), 0) - LinearExpr.Sum(X_merq);
                model.Add(V_erq >= 0);
                V_erq_list.Add(V_erq);
            }

            var D_mmr_list = CreateDmmr(model, F_mr_dict, X_mer_by_rm_dict);
            var max_D = 0;
            foreach (var ((roleId, memberId), X_mer) in X_mer_by_rm_dict)
            {
                var F_mr = F_mr_dict.GetValueOrDefault((memberId, roleId));
                max_D = Math.Max(max_D, F_mr + X_mer.Count);
            }
            var maxDiffSum = max_D * D_mmr_list.Count + 1;

            var D_mer_list = new List<LinearExpr>();
            foreach(var ((memberId, eventId, roleId), P_mer) in P_mer_dict)
            {
                if(P_mer)
                {
                    D_mer_list.Add(1 - X_mer_dict[(memberId, eventId, roleId)]);
                }
            }
            var max_De = D_mer_list.Count + 1;

            model.Minimize(max_De * (maxDiffSum * LinearExpr.Sum(V_erq_list) + LinearExpr.Sum(D_mmr_list)) + LinearExpr.Sum(D_mer_list));
            var solver = new CpSolver();
            var status = solver.Solve(model);

            

            var filledHelpers = new Dictionary<HelperDTO, OptimizedAssignments>();
            foreach(var requirement in requirementsToOptimize)
            {
                var lockedMembers = new HashSet<string>(requirement.LockedMembers);
                var preselectedMembers = new HashSet<string>(requirement.PreselectedMembers);
                var availableMembers = new HashSet<string>(requirement.AvailableMembers);
                foreach(var (member, X_erm) in X_mer_by_er_dict.GetValueOrDefault((requirement.EventId, requirement.RoleId), new()))
                {
                    if(solver.Value(X_erm) == 1)
                    {
                        preselectedMembers.Add(member);
                        availableMembers.Remove(member);
                    }
                    else
                    {
                        preselectedMembers.Remove(member);
                        availableMembers.Add(member);
                    }
                }
                if (DateTime.Now < requirement.LockingTime)
                {
                    filledHelpers.Add(
                        requirement,
                        new OptimizedAssignments { 
                            LockedMembers = [.. lockedMembers], 
                            PreselectedMembers = [.. preselectedMembers], 
                            AvailableMembers = [.. availableMembers] 
                        });
                }
                else
                {
                    filledHelpers.Add(
                        requirement,
                        new OptimizedAssignments
                        {
                            LockedMembers = [.. lockedMembers.Union(preselectedMembers)],
                            PreselectedMembers = [],
                            AvailableMembers = [.. availableMembers]
                        });
                }
            }

            return filledHelpers;
        }

        private static List<IntVar> CreateDmmr(CpModel model, Dictionary<(string, string), int>  F_mr_dict, Dictionary<(string, string), List<(string, IntVar)>>  X_mer_by_rm_dict)
        {
            var D_mmr_list = new List<IntVar>();
            var X_mer_by_rm_list = X_mer_by_rm_dict.ToList();
            for (var i = 0; i < X_mer_by_rm_list.Count; i++)
            {
                var ((roleId, memberId), X_mer) = X_mer_by_rm_list[i];
                var F_mr = F_mr_dict.GetValueOrDefault((memberId, roleId));
                var E_mr = F_mr + LinearExpr.Sum(X_mer.Select(pair => pair.Item2));
                foreach (var ((otherRoleId, otherMemberId), otherX_mer) in X_mer_by_rm_dict)
                {
                    var otherF_mr = F_mr_dict.GetValueOrDefault((otherRoleId, otherMemberId));
                    var otherE_mr = otherF_mr + LinearExpr.Sum(otherX_mer.Select(pair => pair.Item2));
                    if (roleId == otherRoleId && memberId != otherMemberId)
                    {
                        var D_mmr = model.NewIntVar(0, 10000, "Diff");
                        model.AddAbsEquality(D_mmr, E_mr - otherE_mr);
                        D_mmr_list.Add(D_mmr);
                    }
                }
            }
            return D_mmr_list;
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
