import unittest
from optimizer import Event, Requirement, Qualification, Optimize
from datetime import datetime, timezone, timedelta

class Tests(unittest.TestCase):
    def test_Default(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "R1", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["M1", "M2"], RequiredQualifications={}),
            Requirement("2", "E1", "R2", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["M1", "M2"], RequiredQualifications={})
        ])]
        filledHelpers = Optimize(categories, [])
        self.assertEqual(filledHelpers[0][0].Id, "1")
        self.assertEqual(filledHelpers[1][0].Id, "2")
        self.assertEqual(filledHelpers[0][0].EventId, "E1")
        self.assertEqual(filledHelpers[1][0].EventId, "E1")
        firstSetMembers = filledHelpers[0][1].NewPreselectedMembers
        secondSetMembers = filledHelpers[1][1].NewPreselectedMembers
        self.assertEqual(len(firstSetMembers), 1)
        self.assertEqual(len(secondSetMembers), 1)
        self.assertNotEqual(firstSetMembers[0], secondSetMembers[0])

    def test_TrivialCaseInfluences(self):
        events : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 4, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2", "3", "4", "5"], RequiredQualifications={}),
            Requirement("2", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["3"], RequiredQualifications={})
        ])]
        filledCategories = Optimize(events, [])
        allSetMembers = set(filledCategories[0][1].NewPreselectedMembers).union(filledCategories[1][1].NewPreselectedMembers)
        self.assertEqual(len(allSetMembers), 5)

    def test_LockedCaseInfluences(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 4, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2", "3", "4", "5"], RequiredQualifications={}),
            Requirement("2", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=["3"], PreselectedMembers=[], AvailableMembers=[], RequiredQualifications={})
        ])]
        filledCategories = Optimize(categories, [])
        firstPreselectedMembers = filledCategories[0][1].NewPreselectedMembers
        self.assertEqual(len(firstPreselectedMembers), 4)
        self.assertNotIn("3", firstPreselectedMembers)

    def test_LockedHelpersAreFilledTrivial(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 6, LockedMembers=["4", "5"], PreselectedMembers=[], AvailableMembers=["1", "2", "3"], RequiredQualifications={}),
        ])]
        filledCategories = Optimize(categories, [])        
        self.assertSetEqual(set(filledCategories[0][1].NewPreselectedMembers), set(["1", "2", "3"]))

    def test_LockedHelpersAreFilledOptimized(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 4, LockedMembers=["4", "5"], PreselectedMembers=[], AvailableMembers=["1", "2", "3"], RequiredQualifications={}),
        ])]
        filledCategories = Optimize(categories, [])   
        setMembers = filledCategories[0][1].NewPreselectedMembers
        self.assertEqual(len(setMembers), 2)

    def test_OnlyOneCategoryPerEvent(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"], RequiredQualifications={}),
            Requirement("2", "E1", "Y", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"], RequiredQualifications={}),
        ])]
        filledCategories = Optimize(categories, [])
        self.assertEqual(len(filledCategories[0][1].NewPreselectedMembers) + len(filledCategories[1][1].NewPreselectedMembers), 1)

    def test_OnlyOneCategoryPerEventAsLocked(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=["1"], PreselectedMembers=[], AvailableMembers=[], RequiredQualifications={}),
            Requirement("2", "E1", "Y", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"], RequiredQualifications={}),
        ])]
        filledCategories = Optimize(categories, [])
        self.assertEqual(len(filledCategories[0][1].NewPreselectedMembers) + len(filledCategories[0][1].NewAvailableMembers) + len(filledCategories[1][1].NewPreselectedMembers), 0)
        self.assertSetEqual(set(filledCategories[1][1].NewAvailableMembers), set("1"))

    def test_AvoidChangesForEqualOptimizations(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=["1"], AvailableMembers=["2"], RequiredQualifications={}),
            Requirement("2", "E1", "Y", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=["2"], AvailableMembers=["1"], RequiredQualifications={}),
        ])]
        filledCategories = Optimize(categories, [])
        firstPreselectedMembers = filledCategories[0][1].NewPreselectedMembers
        secondPreselectedMembers = filledCategories[1][1].NewPreselectedMembers
        self.assertSetEqual(set(firstPreselectedMembers), set("1"))
        self.assertSetEqual(set(secondPreselectedMembers), set("2"))

    def test_InsertsIntoLockedAfterLockingTime(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=1), Requirements=[
            Requirement("1", "E1", "R1", datetime.now(timezone.utc), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1"], RequiredQualifications={})
        ]),Event(Id="E2", Date=datetime.now(timezone.utc) + timedelta(days=1), Requirements=[
            Requirement("2", "E2", "R2", datetime.now(timezone.utc), 1, LockedMembers=[], PreselectedMembers=["1"], AvailableMembers=[], RequiredQualifications={})
        ]),Event(Id="E3", Date=datetime.now(timezone.utc) + timedelta(days=1), Requirements=[
            Requirement("3", "E3", "R3", datetime.now(timezone.utc), 2, LockedMembers=["2"], PreselectedMembers=["1"], AvailableMembers=[], RequiredQualifications={})
        ])]
        filledCategories = Optimize(categories, [])
        firstLockedMembers = filledCategories[0][1].NewLockedMembers
        secondLockedMembers = filledCategories[1][1].NewLockedMembers
        thirdLockedMembers = filledCategories[2][1].NewLockedMembers
        self.assertSetEqual(set(firstLockedMembers), set("1"))
        self.assertSetEqual(set(secondLockedMembers), set("1"))
        self.assertSetEqual(set(thirdLockedMembers), set(["1", "2"]))

    def test_SkipsPastEvents(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) - timedelta(days=1), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) - timedelta(days=2), 1, LockedMembers=["1"], PreselectedMembers=[], AvailableMembers=[], RequiredQualifications={})
        ]),Event(Id="E2", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("2", "E2", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=["1"], AvailableMembers=["2"], RequiredQualifications={})
        ])]
        filledCategories = Optimize(categories, [])
        self.assertEqual(len(filledCategories), 1)
        lockedMembers = filledCategories[0][1].NewLockedMembers
        preselectedMembers = filledCategories[0][1].NewPreselectedMembers
        availableMembers = filledCategories[0][1].NewAvailableMembers
        self.assertSetEqual(set(lockedMembers), set())
        self.assertSetEqual(set(preselectedMembers), set("2"))
        self.assertSetEqual(set(availableMembers), set("1"))

    def test_OverfilledLockedHelpersUnchanged(self):
        categories : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "Z", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=["1", "2"], PreselectedMembers=[], AvailableMembers=[], RequiredQualifications={})
        ]),Event(Id="E2", Date=datetime.now(timezone.utc) + timedelta(days=1), Requirements=[
            Requirement("2", "E2", "Z", datetime.now(timezone.utc), 1, LockedMembers=["1", "2"], PreselectedMembers=[], AvailableMembers=[], RequiredQualifications={})
        ])]
        filledCategories = Optimize(categories, [])
        for filledCategory in filledCategories:            
            lockedMembers = filledCategory[1].NewLockedMembers
            preselectedMembers = filledCategory[1].NewPreselectedMembers
            availableMembers = filledCategory[1].NewAvailableMembers
            self.assertSetEqual(set(lockedMembers), set(["1","2"]))
            self.assertSetEqual(set(preselectedMembers), set())
            self.assertSetEqual(set(availableMembers), set())
    
    def test_QualificationRequired(self):
        qualifications : list[Qualification] = [Qualification("Q1", "R1", ["2"])]
        events : list[Event] = [Event(Id="E1", Date=datetime.now(timezone.utc) + timedelta(days=2), Requirements=[
            Requirement("1", "E1", "R1", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=["2"], PreselectedMembers=[], AvailableMembers=[], RequiredQualifications={}),
            Requirement("2", "E1", "R1", datetime.now(timezone.utc) + timedelta(days=1), 1, LockedMembers=[], PreselectedMembers=[], AvailableMembers=["1", "2"], RequiredQualifications={"Q1":1})
        ])]
        filledCategories = Optimize(events, qualifications)
        self.assertEqual(filledCategories[1][1].NewPreselectedMembers[0], "2")


if __name__ == '__main__':
    unittest.main()