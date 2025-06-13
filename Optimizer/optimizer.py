from ortools.math_opt.python import mathopt
from dataclasses import dataclass
from collections import defaultdict
from datetime import datetime, timezone

@dataclass
class Requirement:
    Id: str
    EventId : str
    RoleId : str
    LockingTime : datetime
    RequiredAmount : int
    LockedMembers : list[str]
    PreselectedMembers : list[str]
    AvailableMembers : list[str]
    RequiredQualifications : dict[str, int]

@dataclass
class Qualification:
    Id: str
    RoleId : str
    Members : list[str]

@dataclass
class Event:
    Id : str
    Date : datetime
    Requirements: list[Requirement]

@dataclass
class Updates:
    NewLockedMembers : list[str]
    NewPreselectedMembers : list[str]
    NewAvailableMembers : list[str]

def Optimize(events : list[Event], qualifications : list[Qualification]) -> list[(Requirement, Updates)]:
    lockedMemberAssignments = defaultdict[tuple[str, str], int](int)    
    for helper in [helper for event in events for helper in event.Requirements]:        
        for member in helper.LockedMembers:
            lockedMemberAssignments[(member, helper.RoleId)] = lockedMemberAssignments.get((member, helper.RoleId), 0) + 1

    return OptimizeOverfilled([event for event in events if event.Date > datetime.now(timezone.utc)], qualifications, lockedMemberAssignments)    

def OptimizeOverfilled(events : list[Event], qualifications : list[Qualification], lockedMemberAssignments : defaultdict[tuple[str, str], int]) -> list[(Requirement, Updates)]: 
    model = mathopt.Model()

    F_mr = lockedMemberAssignments
    B_erq_dict = defaultdict[tuple[str, str, str], int](int)
    A_mer_dict = defaultdict[tuple[str, str, str], bool](bool)
    P_mer_dict = defaultdict[tuple[str, str, str], bool](bool)
    for event in events:   
        lockedMembersOfEvent = set().union(*[requirement.LockedMembers for requirement in event.Requirements])     
        for requirementOfEvent in event.Requirements:
            for member in set(requirementOfEvent.AvailableMembers) - lockedMembersOfEvent:
                A_mer_dict[(member, event.Id, requirementOfEvent.RoleId)] = True
            for member in set(requirementOfEvent.PreselectedMembers) - lockedMembersOfEvent:
                A_mer_dict[(member, event.Id, requirementOfEvent.RoleId)] = True
                P_mer_dict[(member, event.Id, requirementOfEvent.RoleId)] = True                

            openRequiredAmountForRole = requirementOfEvent.RequiredAmount - len(requirementOfEvent.LockedMembers)
            for qualificationId, requiredAmountForQualification in requirementOfEvent.RequiredQualifications.items():
                B_erq_dict[(event.Id, requirementOfEvent.RoleId, qualificationId)] = requiredAmountForQualification
                openRequiredAmountForRole -= requiredAmountForQualification
            if openRequiredAmountForRole > 0:
                B_erq_dict[(event.Id, requirementOfEvent.RoleId, None)] = openRequiredAmountForRole
    
    S_mrq_dict = defaultdict[tuple[str, str, str], bool](bool)
    for qualification in qualifications:
        for member in qualification.Members:
            S_mrq_dict[(member, qualification.RoleId, qualification.Id)] = True

    # print(F_mr)
    # print(B_erq_dict)
    # print(A_mer_dict)
    # print(P_mer_dict)
    print(S_mrq_dict)
    
    X_erq_m_dict = defaultdict[tuple[str, str, str], list[mathopt.Variable]](list)
    X_mr_e_dict = defaultdict[tuple[str, str], list[mathopt.Variable]](list)
    X_mer_dict : dict[tuple[str, str, str], mathopt.Variable] = {}
    X_er_m_dict = defaultdict[tuple[str, str], list[tuple[str, mathopt.Variable]]](list)
    for event in events:   
        X_me_r_dict = defaultdict[str, list[mathopt.Variable]](list)
        lockedMembersOfEvent = set().union(*[requirement.LockedMembers for requirement in event.Requirements])     
        for requirementOfEvent in event.Requirements:              
            for member in set(requirementOfEvent.AvailableMembers).union(requirementOfEvent.PreselectedMembers) - lockedMembersOfEvent:
                X_mer = model.add_binary_variable(name=f"{requirementOfEvent.EventId}, {requirementOfEvent.RoleId}: {member}")
                model.add_linear_constraint(X_mer <= A_mer_dict[(member, event.Id, requirementOfEvent.RoleId)])
                X_me_r_dict[(member)].append(X_mer)
                X_mr_e_dict[(member, requirementOfEvent.RoleId)].append(X_mer)
                X_mer_dict[(member, event.Id, requirementOfEvent.RoleId)] = X_mer
                X_er_m_dict[(event.Id, requirementOfEvent.RoleId)].append((member, X_mer))
                X_mer_q_list = []
                for requiredQualificationOfEvent in requirementOfEvent.RequiredQualifications.keys():
                    X_merq = model.add_binary_variable(name=f"{requirementOfEvent.EventId}, {requirementOfEvent.RoleId}, {requiredQualificationOfEvent}: {member}")
                    X_mer_q_list.append(X_merq)
                    model.add_linear_constraint(X_merq <= S_mrq_dict[(member, requirementOfEvent.RoleId, requiredQualificationOfEvent)])
                    X_erq_m_dict[(event.Id, requirementOfEvent.RoleId, requiredQualificationOfEvent)].append(X_merq)
                X_merq = model.add_binary_variable(name=f"{requirementOfEvent.EventId}, {requirementOfEvent.RoleId}, None: {member}")
                X_mer_q_list.append(X_merq)
                X_erq_m_dict[(event.Id, requirementOfEvent.RoleId, None)].append(X_merq)
                model.add_linear_constraint(X_mer == mathopt.LinearSum(X_mer_q_list))
        for X_me in X_me_r_dict.values():
            model.add_linear_constraint(mathopt.LinearSum(X_me) <= 1)

    # print(X_erq_m_dict)
    print(X_mr_e_dict)
    # print(X_mer_dict)
    # print(X_er_m_dict)

    V_erq_list = []
    for ((eventId, roleId, qualificationId), X_erq_m) in X_erq_m_dict.items():
        V_erq = B_erq_dict[(eventId, roleId, qualificationId)] - mathopt.LinearSum(X_erq_m)
        model.add_linear_constraint(V_erq >= 0)
        V_erq_list.append(V_erq)

    E_mr_squared_list = []
    max_E_mr = 0
    for ((memberId, roleId), X_mr_e) in X_mr_e_dict.items():
        max_E_mr = max(max_E_mr, F_mr[(memberId, roleId)] + len(X_mr_e))
        E_mr = F_mr[(memberId, roleId)] + mathopt.LinearSum(X_mr_e)
        E_mr_squared_list.append(E_mr * E_mr)  
    max_E = len(E_mr_squared_list) * max_E_mr**2 + 1
    
    D_mer_list = []
    for ((memberId, eventId, roleId), P_mer) in P_mer_dict.items():
        if P_mer:
            D_mer = model.add_binary_variable(name=f"Deselected {eventId}, {roleId}: {memberId}")
            model.add_linear_constraint(D_mer >= P_mer - X_mer_dict[(memberId, eventId, roleId)])
            D_mer_list.append(D_mer)       
    max_D = len(D_mer_list) + 1

    
    # print(V_erq_list)
    # print(E_mr_squared_list)
    # print(D_mer_list)

    # print(max_E)
    # print(max_D)
               
    model.minimize(max_D * (max_E * mathopt.LinearSum(V_erq_list) + mathopt.QuadraticSum(E_mr_squared_list)) + mathopt.LinearSum(D_mer_list))
    result = mathopt.solve(model, mathopt.SolverType.GSCIP)

    print(result)

    filledHelpers : list[(Requirement, Updates)] = []
    for requirement in [requirement for event in events for requirement in event.Requirements]:
        lockedMembers = set(requirement.LockedMembers)
        preselectedMembers = set(requirement.PreselectedMembers)
        availableMembers = set(requirement.AvailableMembers)
        for member, X_erm in X_er_m_dict[(requirement.EventId, requirement.RoleId)]:            
            if result.variable_values(X_erm) == 1:
                preselectedMembers.add(member)
                availableMembers.discard(member)
            else:
                preselectedMembers.discard(member)
                availableMembers.add(member)
        if datetime.now(timezone.utc) < requirement.LockingTime:
            update = Updates(list(lockedMembers), list(preselectedMembers), list(availableMembers))
        else:
            update = Updates(list(lockedMembers.union(preselectedMembers)), [], list(availableMembers))
        filledHelpers.append((requirement, update))

    return filledHelpers