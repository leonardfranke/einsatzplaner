import unittest
from optimizer import Event, Helper, Optimize

class Tests(unittest.TestCase):
    def test_Default(self):
        categories : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2"]),
            Helper("2", "B", "Z", 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2"])
        ])]
        filledCategories = Optimize(categories)
        self.assertEqual(filledCategories[0].Id, "1")
        self.assertEqual(filledCategories[1].Id, "2")
        self.assertEqual(filledCategories[0].EventId, "A")
        self.assertEqual(filledCategories[1].EventId, "B")
        firstSetMembers = filledCategories[0].PreselectedMembers
        secondSetMembers = filledCategories[1].PreselectedMembers
        self.assertEqual(len(firstSetMembers), 1)
        self.assertEqual(len(secondSetMembers), 1)
        self.assertNotEqual(firstSetMembers[0], secondSetMembers[0])

    def test_TrivialCaseInfluences(self):
        events : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 4, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2", "3", "4", "5"]),
            Helper("2", "B", "Z", 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["3"])
        ])]
        filledCategories = Optimize(events)
        allSetMembers = set(filledCategories[0].PreselectedMembers).union(filledCategories[1].PreselectedMembers)
        self.assertEqual(len(allSetMembers), 5)

    def test_LockedCaseInfluences(self):
        categories : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 4, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2", "3", "4", "5"]),
            Helper("2", "B", "Z", 1, LockedMembers=["3"], PreselectedMembers=[], AvailableMembers=[])
        ])]
        filledCategories = Optimize(categories)
        firstPreselectedMembers = filledCategories[0].PreselectedMembers
        self.assertEqual(len(firstPreselectedMembers), 4)
        self.assertNotIn("3", firstPreselectedMembers)

    def test_LockedHelpersAreFilledTrivial(self):
        categories : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 6, LockedMembers=["4", "5"], PreselectedMembers=[], AvailableMembers=["1", "2", "3"]),
        ])]
        filledCategories = Optimize(categories)        
        self.assertSetEqual(set(filledCategories[0].PreselectedMembers), set(["1", "2", "3"]))

    def test_LockedHelpersAreFilledOptimized(self):
        categories : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 4, LockedMembers=["4", "5"], PreselectedMembers=[], AvailableMembers=["1", "2", "3"]),
        ])]
        filledCategories = Optimize(categories)   
        setMembers = filledCategories[0].PreselectedMembers
        self.assertEqual(len(setMembers), 2)

    def test_OnlyOneCategoryPerEvent(self):
        categories : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"]),
            Helper("2", "A", "Y", 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"]),
        ])]
        filledCategories = Optimize(categories)
        self.assertEqual(len(filledCategories[0].PreselectedMembers) + len(filledCategories[1].PreselectedMembers), 1)

    def test_OnlyOneCategoryPerEventAsLocked(self):
        categories : list[Event] = [Event(Helpers=[
            Helper("1", "A", "Z", 1, LockedMembers=["1"], PreselectedMembers=[], AvailableMembers=[]),
            Helper("2", "A", "Y", 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"]),
        ])]
        filledCategories = Optimize(categories)
        print(filledCategories)
        self.assertEqual(len(filledCategories[0].PreselectedMembers) + len(filledCategories[0].AvailableMembers) + len(filledCategories[1].PreselectedMembers), 0)
        self.assertSetEqual(set(filledCategories[1].AvailableMembers), set("1"))


if __name__ == '__main__':
    unittest.main()