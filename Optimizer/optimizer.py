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
    
    for event in events:
        lockedMembers = set().union(*[category.LockedMembers for category in event.Helpers])
        for category in event.Helpers:
            category.AvailableMembers = set(category.AvailableMembers).difference(lockedMembers)

    filledCategories : list[FilledHelper] = []
    fixedMemberAssignments : dict[tuple[str, str], int] = {}
    categoriesForOptimizer : list[Helper] = []
    
    for category in [category for event in events for category in event.Helpers]:
        openRequiredAmount = category.RequiredAmount - len(category.LockedMembers)
        if openRequiredAmount == 0 or len(category.AvailableMembers) == 0:
            filledCategories.append(FilledHelper(category.Id, category.EventId, category.LockedMembers, category.AvailableMembers))
        else:
            for member in category.LockedMembers:
                fixedMemberAssignments[(member, category.RoleId)] = fixedMemberAssignments.get((member, category.RoleId), 0) + 1
            categoriesForOptimizer.append(category)

    filledCategories.extend(OptimizeOverfilled(categoriesForOptimizer, fixedMemberAssignments, None))
    return filledCategories

def OptimizeOverfilled(categories : list[Helper], fixedMemberAssignments : dict[tuple[str, str], int], max_V_er: float) -> list[FilledHelper]: 
    model = mathopt.Model()

    X_mer_dict = defaultdict[list[mathopt.Variable]](list)
    X_me_dict = defaultdict[list[mathopt.Variable]](list)
    X_mr_dict = defaultdict[list[mathopt.Variable]](list)
    X_er_dict = defaultdict[list[mathopt.Variable]](list)
    V_er_sum = mathopt.LinearSum([])

    for category in categories:
        X_er = []
        for member in category.AvailableMembers:
            X = model.add_binary_variable(name=f"{category.EventId}, {category.RoleId}: {member}")
            X_er.append(X)
            X_mer_dict[(member, category.EventId, category.RoleId)].append(X)
            X_me_dict[(member, category.EventId)].append(X)
            X_mr_dict[(member, category.RoleId)].append(X)
            X_er_dict[(category.EventId, category.RoleId)].append((X, member))
        openRequiredAmount = category.RequiredAmount - len(category.LockedMembers)
        V_er = openRequiredAmount - mathopt.LinearSum(X_er)
        V_er_sum += V_er
        model.add_linear_constraint(V_er >= 0)

    for X_me in X_me_dict.values():
        model.add_linear_constraint(mathopt.LinearSum(X_me) <= 1)


    if max_V_er is None:
        model.minimize_quadratic_objective(V_er_sum)
        result = mathopt.solve(model, mathopt.SolverType.GSCIP)
        max_V_er = result.objective_value()
        return OptimizeOverfilled(categories, fixedMemberAssignments, max_V_er)
        
        
    model.add_linear_constraint(V_er_sum == max_V_er)        
    E_mr_list = [mathopt.LinearSum(variables) + fixedMemberAssignments.get(assignment, 0) for assignment, variables in X_mr_dict.items()]
    squaredCounts = [E_mr*E_mr for E_mr in E_mr_list]
    squaredCountsSum = mathopt.QuadraticSum(squaredCounts)
    model.minimize_quadratic_objective(squaredCountsSum)
    result = mathopt.solve(model, mathopt.SolverType.GSCIP)

    filledCategories : list[FilledHelper] = []
    for category in categories:
        filledCategory = FilledHelper(category.Id, category.EventId, category.LockedMembers.copy(), [])
        X_er = X_er_dict[(category.EventId, category.RoleId)]
        for (X, member) in X_er:
            if result.variable_values(X) == 1:
                filledCategory.SetMembers.append(member)
            else:
                filledCategory.RemainingMembers.append(member)
        filledCategories.append(filledCategory)

    return filledCategories