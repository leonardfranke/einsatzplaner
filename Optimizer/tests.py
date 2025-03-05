import unittest
from optimizer import Category, Optimize

class Tests(unittest.TestCase):
    def test_Default(self):
        categories : list[Category] = [
            Category("A", "1", 1, LockedMembers=[], AvailableMembers=["1", "2"]),
            Category("B", "1", 1, LockedMembers=[], AvailableMembers=["1", "2"])
        ]
        filledCategories = Optimize(categories)
        firstSetMembers = filledCategories[0].SetMembers
        secondSetMembers = filledCategories[1].SetMembers
        self.assertEqual(len(firstSetMembers), 1)
        self.assertEqual(len(secondSetMembers), 1)
        self.assertNotEqual(firstSetMembers[0], secondSetMembers[0])

    def test_TrivialCaseInfluences(self):
        categories : list[Category] = [
            Category("A", "1", 4, LockedMembers=[], AvailableMembers=["1", "2", "3", "4", "5"]),
            Category("B", "1", 1, LockedMembers=[], AvailableMembers=["3"])
        ]
        filledCategories = Optimize(categories)
        allSetMembers = set(filledCategories[0].SetMembers).union(filledCategories[1].SetMembers)
        self.assertEqual(len(allSetMembers), 5)

    def test_LockedCaseInfluences(self):
        categories : list[Category] = [
            Category("A", "1", 4, LockedMembers=[], AvailableMembers=["1", "2", "3", "4", "5"]),
            Category("B", "1", 1, LockedMembers=["3"], AvailableMembers=[])
        ]
        filledCategories = Optimize(categories)
        allSetMembers = set(filledCategories[0].SetMembers).union(filledCategories[1].SetMembers)
        self.assertEqual(len(allSetMembers), 5)

    def test_LockedHelpersAreFilledTrivial(self):
        categories : list[Category] = [
            Category("A", "1", 6, LockedMembers=["4", "5"], AvailableMembers=["1", "2", "3"]),
        ]
        filledCategories = Optimize(categories)        
        self.assertSetEqual(set(filledCategories[0].SetMembers), set(["1", "2", "3", "4", "5"]))

    def test_LockedHelpersAreFilledOptimized(self):
        categories : list[Category] = [
            Category("A", "1", 4, LockedMembers=["4", "5"], AvailableMembers=["1", "2", "3"]),
        ]
        filledCategories = Optimize(categories)   
        setMembers = filledCategories[0].SetMembers
        self.assertIn("4", setMembers)     
        self.assertIn("5", setMembers)   
        self.assertEqual(len(setMembers), 4)

if __name__ == '__main__':
    unittest.main()