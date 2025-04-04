from ortools.math_opt.python import mathopt
from dataclasses import dataclass
from collections import defaultdict


@dataclass
class Helper:
    Id: str
    EventId : str
    RoleId : str
    RequiredAmount : int
    LockedMembers : list[str]
    AvailableMembers : list[str]

@dataclass
class Event:
    Helpers: list[Helper]

@dataclass
class FilledHelper:
    Id: str
    EventId : str
    SetMembers : list[str]
    RemainingMembers : list[str]

def Optimize(events : list[Event]) -> list[FilledHelper]:
    lockedMemberAssignments : dict[tuple[str, str], int] = {}    
    for helper in [helper for event in events for helper in event.Helpers]:        
        for member in helper.LockedMembers:
            lockedMemberAssignments[(member, helper.RoleId)] = lockedMemberAssignments.get((member, helper.RoleId), 0) + 1

    return OptimizeOverfilled(events, lockedMemberAssignments, None)
    

def OptimizeOverfilled(events : list[Event], lockedMemberAssignments : dict[tuple[str, str], int], max_V_er: float) -> list[FilledHelper]: 
    model = mathopt.Model()

    X_mer_dict = defaultdict[list[mathopt.Variable]](list)
    X_me_dict = defaultdict[list[mathopt.Variable]](list)
    X_mr_dict = defaultdict[list[mathopt.Variable]](list)
    X_er_dict = defaultdict[list[mathopt.Variable]](list)
    V_er_sum = mathopt.LinearSum([])

    for event in events:
        eventLockedMembers = set().union(*[helper.LockedMembers for helper in event.Helpers])
        for helper in event.Helpers:
            X_er = []
            for member in set(helper.AvailableMembers) - eventLockedMembers:
                X = model.add_binary_variable(name=f"{helper.EventId}, {helper.RoleId}: {member}")
                X_er.append(X)
                X_mer_dict[(member, helper.EventId, helper.RoleId)].append(X)
                X_me_dict[(member, helper.EventId)].append(X)
                X_mr_dict[(member, helper.RoleId)].append(X)
                X_er_dict[(helper.EventId, helper.RoleId)].append((X, member))
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
    model.minimize_quadratic_objective(squaredCountsSum)
    result = mathopt.solve(model, mathopt.SolverType.GSCIP)

    filledCategories : list[FilledHelper] = []
    for helper in [helper for event in events for helper in event.Helpers]:
        filledHelper = FilledHelper(helper.Id, helper.EventId, helper.LockedMembers.copy(), helper.AvailableMembers.copy())
        X_er = X_er_dict[(helper.EventId, helper.RoleId)]
        for (X, member) in X_er:
            if result.variable_values(X) == 1:
                filledHelper.SetMembers.append(member)
                filledHelper.RemainingMembers.remove(member)
        filledCategories.append(filledHelper)

    return filledCategories