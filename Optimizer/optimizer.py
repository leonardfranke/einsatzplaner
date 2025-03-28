from ortools.math_opt.python import mathopt
from dataclasses import dataclass

@dataclass
class Category:
    EventId : str
    HelperCategoryId : str
    RequiredAmount : int
    LockedMembers : list[str]
    AvailableMembers : list[str]

@dataclass
class FilledCategory:
    EventId : str
    HelperCategoryId : str
    SetMembers : list[str]

def Optimize(categories : list[Category]) -> list[FilledCategory]:

    filledCategories : list[FilledCategory] = []
    fixedMemberAssignments : dict[tuple[str, str], int] = {}
    categoriesForOptimizer : list[Category] = []
    
    for category in categories:
        openRequiredAmount = category.RequiredAmount - len(category.LockedMembers)
        if len(category.AvailableMembers) <= openRequiredAmount:
            setMembers = category.LockedMembers.copy()
            setMembers.extend(category.AvailableMembers)
            
            filledCategories.append(FilledCategory(category.EventId, category.HelperCategoryId, setMembers))
            for member in setMembers:
                fixedMemberAssignments[(member, category.HelperCategoryId)] = fixedMemberAssignments.get((member, category.HelperCategoryId), 0) + 1
        elif category.RequiredAmount == 0:
            filledCategories.append(FilledCategory(category.EventId, category.HelperCategoryId, []))
        else:
            for member in category.LockedMembers:
                fixedMemberAssignments[(member, category.HelperCategoryId)] = fixedMemberAssignments.get((member, category.HelperCategoryId), 0) + 1
            categoriesForOptimizer.append(category)

    filledCategories.extend(OptimizeOverfilled(categoriesForOptimizer, fixedMemberAssignments))
    return filledCategories

def OptimizeOverfilled(categories : list[Category], fixedMemberAssignments : dict[tuple[str, str], int]) -> list[FilledCategory]: 
    model = mathopt.Model()
    groupedAssignments : dict[tuple[str, str], list[mathopt.Variable]] = {}
    categoryData : list[tuple[Category, list[tuple[mathopt.Variable, str]]]] = []
    
    for category in categories:
        for member in category.AvailableMembers:
            groupedAssignments.setdefault((member, category.HelperCategoryId), [])
    
    for category in categories:
        categoryVars : list[tuple[mathopt.Variable, str]] = []
        for availabledMember in category.AvailableMembers:
            X_mc = model.add_binary_variable(name=f"{category.EventId}: {availabledMember}")
            categoryVars.append((X_mc, availabledMember))

            groupedAssignments[(availabledMember, category.HelperCategoryId)].append(X_mc)
        
        openRequiredAmount = category.RequiredAmount - len(category.LockedMembers)
        model.add_linear_constraint(mathopt.LinearSum([var[0] for var in categoryVars]) == openRequiredAmount)
        categoryData.append((category, categoryVars))

    counts = [mathopt.LinearSum(variables) + fixedMemberAssignments.get(assignment, 0) for assignment, variables in groupedAssignments.items()]
    squaredCounts = [count*count for count in counts]
    squaredCountsSum = mathopt.QuadraticSum(squaredCounts)

    model.minimize_quadratic_objective(squaredCountsSum)
    result = mathopt.solve(model, mathopt.SolverType.GSCIP)

    filledCategories : list[FilledCategory] = []
    for category, variableTuples in categoryData:
        filledCategory = FilledCategory(category.EventId, category.LockedMembers.copy())
        for variableTuple in variableTuples:
            if result.variable_values(variableTuple[0]) == 1:
                filledCategory.SetMembers.append(variableTuple[1])
        filledCategories.append(filledCategory)

    return filledCategories