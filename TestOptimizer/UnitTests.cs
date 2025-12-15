using Api.FirestoreModels;
using Api.Models;
using DTO;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace OptimizerTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test_Default()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),                    
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1", "M2" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var secondRequirement = new HelperDTO
            {
                Id = "2",
                EventId = "E1",
                RoleId = "R2",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1", "M2" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement };            

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            Assert.That(result[firstRequirement].NewPreselectedMembers.Count, Is.EqualTo(1));
            Assert.That(result[secondRequirement].NewPreselectedMembers.Count, Is.EqualTo(1));
            CollectionAssert.AreNotEquivalent(result[firstRequirement].NewPreselectedMembers, result[secondRequirement].NewPreselectedMembers);
        }

        [Test]
        public void Test_TrivialCaseInfluences()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 4,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1", "M2", "M3", "M4", "M5" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var secondRequirement = new HelperDTO
            {
                Id = "2",
                EventId = "E1",
                RoleId = "R2",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M3" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            Assert.That(result[firstRequirement].NewPreselectedMembers.Union(result[secondRequirement].NewPreselectedMembers).Count(), Is.EqualTo(5));
        }

        [Test]
        public void Test_LockedCaseInfluences()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 4,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1", "M2", "M3", "M4", "M5" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var secondRequirement = new HelperDTO
            {
                Id = "2",
                EventId = "E1",
                RoleId = "R2",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string> { "M3" },
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string>(),
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            Assert.That(result[firstRequirement].NewPreselectedMembers.Count, Is.EqualTo(4));
            CollectionAssert.DoesNotContain(result[firstRequirement].NewPreselectedMembers, 3);
        }

        [Test]
        public void Test_LockedHelpersAreFilledTrivial()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var requirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 6,
                LockedMembers = new List<string> { "M1", "M2" },
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M3", "M4", "M5" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { requirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            CollectionAssert.AreEquivalent(new List<string>() { "M3", "M4", "M5" }, result[requirement].NewPreselectedMembers);
        }

        [Test]
        public void Test_LockedHelpersAreFilledOptimized()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var requirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 4,
                LockedMembers = new List<string> { "M1", "M2" },
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M3", "M4", "M5" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { requirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            Assert.That(result[requirement].NewPreselectedMembers.Count, Is.EqualTo(2));
        }

        [Test]
        public void Test_AvoidChangesForEqualOptimizations()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string> { "M1" },
                AvailableMembers = new List<string> { "M2" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var secondRequirement = new HelperDTO
            {
                Id = "2",
                EventId = "E1",
                RoleId = "R2",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string> { "M2" },
                AvailableMembers = new List<string> { "M1" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            Assert.That(result[firstRequirement].NewPreselectedMembers.First(), Is.EqualTo("M1"));
            Assert.That(result[secondRequirement].NewPreselectedMembers.First(), Is.EqualTo("M2"));
        }

        [Test]
        public void Test_InsertsIntoLockedAfterLockingTime()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(1),
                },
                new EventDTO
                {
                    Id = "E2",
                    Date = now.AddDays(1),
                },
                new EventDTO
                {
                    Id = "E3",
                    Date = now.AddDays(1),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now,
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var secondRequirement = new HelperDTO
            {
                Id = "2",
                EventId = "E2",
                RoleId = "R2",
                LockingTime = now,
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string> { "M1" },
                AvailableMembers = new List<string>(),
                RequiredQualifications = new Dictionary<string, int>()
            };
            var thirdRequirement = new HelperDTO
            {
                Id = "3",
                EventId = "E3",
                RoleId = "R3",
                LockingTime = now,
                RequiredAmount = 2,
                LockedMembers = new List<string> { "M2" },
                PreselectedMembers = new List<string> { "M1" },
                AvailableMembers = new List<string>(),
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement, thirdRequirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            CollectionAssert.AreEquivalent(new List<string> { "M1" }, result[firstRequirement].NewLockedMembers);
            CollectionAssert.AreEquivalent(new List<string> { "M1" }, result[secondRequirement].NewLockedMembers);
            CollectionAssert.AreEquivalent(new List<string> { "M1", "M2" }, result[thirdRequirement].NewLockedMembers);
        }

        [Test]
        public void Test_SkipPastEvents()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(-1),
                },
                new EventDTO
                {
                    Id = "E2",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(-2),
                RequiredAmount = 1,
                LockedMembers = new List<string> { "M1" },
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string>(),
                RequiredQualifications = new Dictionary<string, int>()
            };
            var secondRequirement = new HelperDTO
            {
                Id = "2",
                EventId = "E2",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string> { "M1" },
                AvailableMembers = new List<string> { "M2" },
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            Assert.That(result.ContainsKey(firstRequirement), Is.False);

            CollectionAssert.IsEmpty(result[secondRequirement].NewLockedMembers);
            CollectionAssert.AreEquivalent(new List<string> { "M2" }, result[secondRequirement].NewPreselectedMembers);
            CollectionAssert.AreEquivalent(new List<string> { "M1" }, result[secondRequirement].NewAvailableMembers);
        }

        [Test]
        public void Test_OverfilledLockedHelpersUnchanged()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string> { "M1", "M2" },
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string>(),
                RequiredQualifications = new Dictionary<string, int>()
            };
            var requirements = new List<HelperDTO> { firstRequirement };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, new List<QualificationDTO>());

            CollectionAssert.AreEquivalent(new List<string> { "M1", "M2" }, result[firstRequirement].NewLockedMembers);
            CollectionAssert.IsEmpty(result[firstRequirement].NewPreselectedMembers);
            CollectionAssert.IsEmpty(result[firstRequirement].NewAvailableMembers);
        }

        [Test]
        public void Test_QualificationRequired()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1", "M2" },
                RequiredQualifications = new Dictionary<string, int> { { "Q1", 1 } }
            };
            var requirements = new List<HelperDTO> { firstRequirement };
            var qualifications = new List<QualificationDTO>
            {
                new QualificationDTO
                {
                    Id = "Q1",
                    RoleId = "R1",
                    MemberIds = new List<string> { "M1" }
                }
            };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, qualifications);

            CollectionAssert.AreEquivalent(new List<string> { "M1" }, result[firstRequirement].NewPreselectedMembers);
        }

        [Test]
        public void Test_QualificationInfluences()
        {
            var now = DateTime.UtcNow;
            var events = new List<EventDTO>
            {
                new EventDTO
                {
                    Id = "E1",
                    Date = now.AddDays(2),
                },
                new EventDTO
                {
                    Id = "E2",
                    Date = now.AddDays(2),
                }
            };

            var firstRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E1",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string> { "M2" },
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string>(),
                RequiredQualifications = new Dictionary<string, int> { { "Q1", 1 } }
            };
            var secondRequirement = new HelperDTO
            {
                Id = "1",
                EventId = "E2",
                RoleId = "R1",
                LockingTime = now.AddDays(1),
                RequiredAmount = 1,
                LockedMembers = new List<string>(),
                PreselectedMembers = new List<string>(),
                AvailableMembers = new List<string> { "M1", "M2" },
                RequiredQualifications = new Dictionary<string, int> { { "Q1", 1 } }
            };
            var requirements = new List<HelperDTO> { firstRequirement, secondRequirement };
            var qualifications = new List<QualificationDTO>
            {
                new QualificationDTO
                {
                    Id = "Q1",
                    RoleId = "R1",
                    MemberIds = new List<string> { "M2" }
                }
            };

            var result = Optimizer.Optimizer.OptimizeAvailableMembers(events, requirements, qualifications);

            CollectionAssert.AreEquivalent(new List<string> { "M2" }, result[secondRequirement].NewPreselectedMembers);
        }
    }
}