from ortools.math_opt.python import mathopt
from dataclasses import dataclass
from collections import defaultdict
from datetime import datetime, timezone


@dataclass
class Helper:
    Id: str
    EventId : str
    RoleId : str
    LockingTime : datetime
    RequiredAmount : int
    LockedMembers : list[str]
    PreselectedMembers : list[str]
    AvailableMembers : list[str]

@dataclass
class Event:
    Helpers: list[Helper]

@dataclass
class Updates:
    NewLockedMembers : list[str]
    NewPreselectedMembers : list[str]
    NewAvailableMembers : list[str]

def Optimize(events : list[Event]) -> list[(Helper, Updates)]:
    lockedMemberAssignments : dict[tuple[str, str], int] = {}    
    for helper in [helper for event in events for helper in event.Helpers]:        
        for member in helper.LockedMembers:
            lockedMemberAssignments[(member, helper.RoleId)] = lockedMemberAssignments.get((member, helper.RoleId), 0) + 1

    return OptimizeOverfilled(events, lockedMemberAssignments, None)
    

def OptimizeOverfilled(events : list[Event], lockedMemberAssignments : dict[tuple[str, str], int], max_V_er: float) -> list[(Helper, Updates)]: 
    model = mathopt.Model()
    
    X_me_dict = defaultdict[list[mathopt.Variable]](list)
    X_mr_dict = defaultdict[list[mathopt.Variable]](list)
    X_er_dict = defaultdict[list[mathopt.Variable]](list)
    V_er_sum = mathopt.LinearSum([])
    Deselected = []

    for event in events:
        eventLockedMembers = set().union(*[helper.LockedMembers for helper in event.Helpers])
        for helper in event.Helpers:
            X_er = []
            for member, was_preselected in [(member, True) for member in set(helper.PreselectedMembers) - eventLockedMembers] + [(member, False) for member in set(helper.AvailableMembers) - eventLockedMembers]:
                X = model.add_binary_variable(name=f"{helper.EventId}, {helper.RoleId}: {member}")
                X_er.append(X)
                X_me_dict[(member, helper.EventId)].append(X)
                X_mr_dict[(member, helper.RoleId)].append(X)
                X_er_dict[(helper.EventId, helper.RoleId)].append((X, member))
                if was_preselected:
                    Deselected.append(1 - X)

            openRequiredAmount = helper.RequiredAmount - len(helper.LockedMembers)
            V_er = openRequiredAmount - mathopt.LinearSum(X_er)
            V_er_sum += V_er
            model.add_linear_constraint(V_er >= 0)

    for X_me in X_me_dict.values():
        model.add_linear_constraint(mathopt.LinearSum(X_me) <= 1)

    model.minimize_quadratic_objective(V_er_sum)
    result = mathopt.solve(model, mathopt.SolverType.GSCIP)
    max_V_er = result.objective_value()         
        
    model.add_linear_constraint(V_er_sum == max_V_er)        
    E_mr_list = [mathopt.LinearSum(variables) + lockedMemberAssignments.get(assignment, 0) for assignment, variables in X_mr_dict.items()]
    squaredCounts = [E_mr*E_mr for E_mr in E_mr_list]
    squaredCountsSum = mathopt.QuadraticSum(squaredCounts)
    deselections = mathopt.LinearSum(Deselected)

    model.minimize((len(Deselected) + 1) * squaredCountsSum + deselections)
    result = mathopt.solve(model, mathopt.SolverType.GSCIP)

    filledHelpers : list[(Helper, Updates)] = []
    for helper in [helper for event in events for helper in event.Helpers]:
        lockedMembers = set(helper.LockedMembers)
        preselectedMembers = set(helper.PreselectedMembers)
        availableMembers = set(helper.AvailableMembers)
        X_er = X_er_dict[(helper.EventId, helper.RoleId)]
        for (X, member) in X_er:
            if result.variable_values(X) == 1:
                preselectedMembers.add(member)
                availableMembers.discard(member)
            else:
                preselectedMembers.discard(member)
                availableMembers.add(member)
        if datetime.now(timezone.utc) < helper.LockingTime:
            update = Updates(list(lockedMembers), list(preselectedMembers), list(availableMembers))
        else:
            update = Updates(list(lockedMembers.union(preselectedMembers)), [], list(availableMembers))
        filledHelpers.append((helper, update))

    return filledHelpers