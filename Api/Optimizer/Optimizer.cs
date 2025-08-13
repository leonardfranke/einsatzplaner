using Api.FirestoreModels;
using Api.Models;
using DTO;
using Google.OrTools.Sat;

namespace Optimizer
{
    public class Optimizer
    {
        public class Updates
        {
            public List<string> NewLockedMembers { get; set; }
            public List<string> NewPreselectedMembers { get; set; }
            public List<string> NewAvailableMembers { get; set; }
        }

        public static Dictionary<HelperDTO, Updates> Optimize(List<EventDTO> events, List<HelperDTO> requirements, List<QualificationDTO> qualifications) 
        {
            var lockedMemberAssignments = new Dictionary<(string, string), int>();
            foreach (var requirement in requirements) 
            {
                foreach (var member in requirement.LockedMembers)
                {
                    lockedMemberAssignments[(member, requirement.RoleId)] = lockedMemberAssignments.GetValueOrDefault((member, requirement.RoleId)) + 1;
                }
            }

            var eventsToOptimize = events.Where(@event => @event.Date > DateTime.UtcNow);
            var requirementsToOptimize = requirements.Where(requirement => eventsToOptimize.Any(@event => @event.Id == requirement.EventId));
            return OptimizeOverfilled(eventsToOptimize.ToList(), requirementsToOptimize.ToList(), qualifications, lockedMemberAssignments);
        }

        private static Dictionary<HelperDTO, Updates> OptimizeOverfilled(List<EventDTO> events, List<HelperDTO> requirements, List<QualificationDTO> qualifications, Dictionary<(string, string), int> lockedMemberAssignments)
        {
            var F_mr_dict = lockedMemberAssignments;
            var B_erq_dict = new Dictionary<(string, string, string), int>();
            var A_mer_dict = new Dictionary<(string, string, string), bool>();
            var P_mer_dict = new Dictionary<(string, string, string), bool>();

            foreach(var @event in events)
            {
                var requirementsOfEvent = requirements.Where(requirement => requirement.EventId == @event.Id);
                var lockedMembersOfEvent = requirementsOfEvent.SelectMany(requirement => requirement.LockedMembers);
                var lockedMembersOfEventCount = lockedMembersOfEvent.Count();
                foreach(var requirement in requirementsOfEvent)
                {
                    foreach (var member in requirement.AvailableMembers.Except(lockedMembersOfEvent))
                        A_mer_dict[(member, @event.Id, requirement.RoleId)] = true;
                    foreach (var member in requirement.PreselectedMembers.Except(lockedMembersOfEvent))
                    {
                        A_mer_dict[(member, @event.Id, requirement.RoleId)] = true;
                        P_mer_dict[(member, @event.Id, requirement.RoleId)] = true;
                    }

                    var openRequiredAmountForRole = requirement.RequiredAmount - requirement.LockedMembers.Count;
                    foreach(var (qualificationId, requiredAmount) in requirement.RequiredQualifications)
                    {
                        B_erq_dict[(@event.Id, requirement.RoleId, qualificationId)] = requiredAmount;
                        openRequiredAmountForRole -= requiredAmount;
                    }
                    if(openRequiredAmountForRole > 0)
                        B_erq_dict[(@event.Id, requirement.RoleId, null)] = openRequiredAmountForRole;
                }
            }

            var S_mrq_dict = new Dictionary<(string, string, string), bool>();
            foreach(var qualification in qualifications)
            {
                foreach(var member in qualification.MemberIds)
                {
                    S_mrq_dict[(member, qualification.RoleId, qualification.Id)] = true;
                }
            }

            var X_erq_m_dict = new Dictionary<(string, string, string), List<IntVar>>();
            var X_rm_e_dict = new Dictionary<string, Dictionary<string, List<IntVar>>>();
            var X_mer_dict = new Dictionary<(string, string, string), IntVar>();
            var X_er_m_dict = new Dictionary<(string, string), List<(string, IntVar)>>();


            var model = new CpModel();
            foreach (var @event in events)
            {
                var X_me_r_dict = new Dictionary<string, List<IntVar>>();

                var requirementsOfEvent = requirements.Where(requirement => requirement.EventId == @event.Id);
                var lockedMembersOfEvent = requirementsOfEvent.SelectMany(requirement => requirement.LockedMembers);
                foreach(var requirement in requirementsOfEvent)
                {
                    foreach(var member in requirement.AvailableMembers.Union(requirement.PreselectedMembers).Except(lockedMembersOfEvent))
                    {
                        var X_mer = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}: {member}");
                        model.Add(X_mer <= (A_mer_dict[(member, @event.Id, requirement.RoleId)] ? 1 : 0));
                        if (X_me_r_dict.ContainsKey(member))
                            X_me_r_dict[member].Add(X_mer);
                        else
                            X_me_r_dict[member] = new() {X_mer};
                        if (X_rm_e_dict.ContainsKey(requirement.RoleId))
                        {
                            if (X_rm_e_dict[requirement.RoleId].ContainsKey(member))
                                X_rm_e_dict[requirement.RoleId][member].Add(X_mer);
                            else
                                X_rm_e_dict[requirement.RoleId][member] = new (){ X_mer };
                        }                            
                        else
                        {
                            X_rm_e_dict[requirement.RoleId] = new (){ { member, new List<IntVar>() { X_mer } } };
                        }
                        X_mer_dict[(member, @event.Id, requirement.RoleId)] = X_mer;
                        if (X_er_m_dict.ContainsKey((@event.Id, requirement.RoleId)))
                            X_er_m_dict[(@event.Id, requirement.RoleId)].Add((member, X_mer));
                        else
                            X_er_m_dict[(@event.Id, requirement.RoleId)] = new() { (member, X_mer) };
                        var X_mer_q_list = new List<IntVar>();
                        IntVar X_merq;
                        foreach (var qualification in requirement.RequiredQualifications.Keys)
                        {
                            X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, {qualification}: {member}");
                            X_mer_q_list.Add(X_merq);
                            model.Add(X_merq <= (S_mrq_dict.GetValueOrDefault((member, requirement.RoleId, qualification), false) ? 1 : 0));
                            if (X_erq_m_dict.ContainsKey((@event.Id, requirement.RoleId, qualification)))
                                X_erq_m_dict[(@event.Id, requirement.RoleId, qualification)].Add(X_merq);
                            else
                                X_erq_m_dict[(@event.Id, requirement.RoleId, qualification)] = new() { X_merq };                            
                        }
                        X_merq = model.NewBoolVar($"{requirement.EventId}, {requirement.RoleId}, null: {member}");
                        X_mer_q_list.Add(X_merq);
                        if (X_erq_m_dict.ContainsKey((@event.Id, requirement.RoleId, null)))
                            X_erq_m_dict[(@event.Id, requirement.RoleId, null)].Add(X_merq);
                        else
                            X_erq_m_dict[(@event.Id, requirement.RoleId, null)] = new() { X_merq };
                        model.Add(X_mer == LinearExpr.Sum(X_mer_q_list));
                    }
                }
                foreach(var X_me in X_me_r_dict.Values)
                {
                    model.Add(LinearExpr.Sum(X_me) <= 1);
                }
            }

            var V_erq_list = new List<LinearExpr>();
            foreach(var ((eventId, roleId, qualificationId), X_erq_m) in X_erq_m_dict)
            {
                var V_erq = B_erq_dict.GetValueOrDefault((eventId, roleId, qualificationId), 0) - LinearExpr.Sum(X_erq_m);
                model.Add(V_erq >= 0);
                V_erq_list.Add(V_erq);
            }

            var E_diffs = new List<IntVar>();
            var max_E = 0;
            foreach(var (roleId, X_m_e_dict) in X_rm_e_dict)
            {
                LinearExpr last_E_mr = null;
                foreach(var (memberId, X_m_e) in X_m_e_dict)
                {
                    var F_mr = F_mr_dict.GetValueOrDefault((memberId, roleId));
                    max_E = Math.Max(max_E, F_mr + X_m_e.Count);
                    var E_mr = F_mr + LinearExpr.Sum(X_m_e);
                    if(last_E_mr != null)
                    {
                        var diffVar = model.NewIntVar(0, 10000, "Diff");
                        model.Add(diffVar >= last_E_mr - E_mr);
                        model.Add(diffVar >= E_mr - last_E_mr);
                        E_diffs.Add(diffVar);
                    }
                    last_E_mr = E_mr;
                }
            }
            var maxDiffSum = max_E * E_diffs.Count + 1;

            var D_mer_list = new List<IntVar>();
            foreach(var ((memberId, eventId, roleId), P_mer) in P_mer_dict)
            {
                if(P_mer)
                {
                    var D_mer = model.NewBoolVar($"Deselected {eventId}, {roleId}: {memberId}");
                    model.Add(D_mer >= (P_mer ? 1 : 0) - X_mer_dict[(memberId, eventId, roleId)]);
                    D_mer_list.Add(D_mer);
                }
            }
            var max_D = D_mer_list.Count + 1;

            model.Minimize(max_D * (LinearExpr.Sum(E_diffs) + maxDiffSum * LinearExpr.Sum(V_erq_list)) + LinearExpr.Sum(D_mer_list));
            var solver = new CpSolver();
            var status = solver.Solve(model);

            var filledHelpers = new Dictionary<HelperDTO, Updates>();
            foreach(var requirement in requirements)
            {
                var lockedMembers = new HashSet<string>(requirement.LockedMembers);
                var preselectedMembers = new HashSet<string>(requirement.PreselectedMembers);
                var availableMembers = new HashSet<string>(requirement.AvailableMembers);
                foreach(var (member, X_erm) in X_er_m_dict.GetValueOrDefault((requirement.EventId, requirement.RoleId), new()))
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
                        new Updates { 
                            NewLockedMembers = [.. lockedMembers], 
                            NewPreselectedMembers = [.. preselectedMembers], 
                            NewAvailableMembers = [.. availableMembers] 
                        });
                }
                else
                {
                    filledHelpers.Add(
                        requirement,
                        new Updates
                        {
                            NewLockedMembers = [.. lockedMembers.Union(preselectedMembers)],
                            NewPreselectedMembers = [],
                            NewAvailableMembers = [.. availableMembers]
                        });
                }
            }
            return filledHelpers;
        }
    }
}
