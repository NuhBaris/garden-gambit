using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatTriggerSourceRegistryTests
    {
        [Test]
        public void Constructor_WithNullSources_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerSourceRegistry(
                        null));
        }

        [Test]
        public void Constructor_WithNullSource_Throws()
        {
            var sources =
                new ICombatTriggerSource[]
                {
                    new TestTriggerSource(),
                    null
                };

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatTriggerSourceRegistry(
                        sources));
        }

        [Test]
        public void Constructor_WithEmptySources_AllowsRegistry()
        {
            var registry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[0]);

            var candidates =
                new List<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>(
                    registry.DiscoverTriggers(
                        CreateState(),
                        CreateSourceEvent()));

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.Sources,
                Is.Empty);

            Assert.That(
                candidates,
                Is.Empty);
        }

        [Test]
        public void Constructor_CopiesSourceCollection()
        {
            var firstSource =
                new TestTriggerSource();

            var secondSource =
                new TestTriggerSource();

            var sourceList =
                new List<ICombatTriggerSource>
                {
                    firstSource
                };

            var registry =
                new CombatTriggerSourceRegistry(
                    sourceList);

            sourceList.Add(secondSource);

            Assert.That(
                sourceList.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Sources[0],
                Is.SameAs(firstSource));
        }

        [Test]
        public void DiscoverTriggers_WithNullState_ThrowsWithoutQueryingSource()
        {
            var source =
                new TestTriggerSource();

            var registry =
                new CombatTriggerSourceRegistry(
                    new[]
                    {
                        source
                    });

            Assert.Throws<ArgumentNullException>(
                () => registry.DiscoverTriggers(
                    null,
                    CreateSourceEvent()));

            Assert.That(
                source.DiscoveryCallCount,
                Is.Zero);
        }

        [Test]
        public void DiscoverTriggers_WithNullEvent_ThrowsWithoutQueryingSource()
        {
            var source =
                new TestTriggerSource();

            var registry =
                new CombatTriggerSourceRegistry(
                    new[]
                    {
                        source
                    });

            Assert.Throws<ArgumentNullException>(
                () => registry.DiscoverTriggers(
                    CreateState(),
                    null));

            Assert.That(
                source.DiscoveryCallCount,
                Is.Zero);
        }

        [Test]
        public void DiscoverTriggers_AggregatesSourcesAndCandidatesInRegistrationOrder()
        {
            var state =
                CreateState();

            var sourceEvent =
                CreateSourceEvent();

            var firstCandidate =
                CreateCandidate(
                    "First",
                    0);

            var secondCandidate =
                CreateCandidate(
                    "Second",
                    1);

            var thirdCandidate =
                CreateCandidate(
                    "Third",
                    2);

            var firstSource =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            firstCandidate,
                            secondCandidate
                        }
                };

            var secondSource =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            thirdCandidate
                        }
                };

            var registry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        firstSource,
                        secondSource
                    });

            var discoveredCandidates =
                new List<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>(
                    registry.DiscoverTriggers(
                        state,
                        sourceEvent));

            Assert.That(
                discoveredCandidates.Count,
                Is.EqualTo(3));

            Assert.That(
                discoveredCandidates[0],
                Is.SameAs(firstCandidate));

            Assert.That(
                discoveredCandidates[1],
                Is.SameAs(secondCandidate));

            Assert.That(
                discoveredCandidates[2],
                Is.SameAs(thirdCandidate));

            AssertSourceReceived(
                firstSource,
                state,
                sourceEvent);

            AssertSourceReceived(
                secondSource,
                state,
                sourceEvent);
        }

        [Test]
        public void DiscoverTriggers_WhenSourceReturnsNull_Throws()
        {
            var source =
                new TestTriggerSource
                {
                    Candidates = null
                };

            var registry =
                new CombatTriggerSourceRegistry(
                    new[]
                    {
                        source
                    });

            Assert.Throws<InvalidOperationException>(
                () => registry.DiscoverTriggers(
                    CreateState(),
                    CreateSourceEvent()));

            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void DiscoverTriggers_WhenSourceContainsNullCandidate_Throws()
        {
            var source =
                new TestTriggerSource
                {
                    Candidates =
                        new CombatTriggerCandidate<
                            ICombatTriggerHandler>[]
                        {
                            CreateCandidate(
                                "Valid",
                                0),
                            null
                        }
                };

            var registry =
                new CombatTriggerSourceRegistry(
                    new[]
                    {
                        source
                    });

            Assert.Throws<InvalidOperationException>(
                () => registry.DiscoverTriggers(
                    CreateState(),
                    CreateSourceEvent()));

            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void DiscoverTriggers_WhenSourceThrows_StopsWithoutQueryingLaterSources()
        {
            var firstSource =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            CreateCandidate(
                                "First",
                                0)
                        }
                };

            var failingSource =
                new TestTriggerSource
                {
                    ThrowDuringDiscovery = true
                };

            var laterSource =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            CreateCandidate(
                                "Later",
                                1)
                        }
                };

            var registry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        firstSource,
                        failingSource,
                        laterSource
                    });

            Assert.Throws<InvalidOperationException>(
                () => registry.DiscoverTriggers(
                    CreateState(),
                    CreateSourceEvent()));

            Assert.That(
                firstSource.DiscoveryCallCount,
                Is.EqualTo(1));

            Assert.That(
                failingSource.DiscoveryCallCount,
                Is.EqualTo(1));

            Assert.That(
                laterSource.DiscoveryCallCount,
                Is.Zero);
        }

        private static void AssertSourceReceived(
            TestTriggerSource source,
            CombatState expectedState,
            CombatEvent expectedEvent)
        {
            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(1));

            Assert.That(
                source.ReceivedState,
                Is.SameAs(expectedState));

            Assert.That(
                source.ReceivedEvent,
                Is.SameAs(expectedEvent));
        }

        private static CombatTriggerCandidate<
            ICombatTriggerHandler> CreateCandidate(
                string name,
                int horizontalOrder)
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        horizontalOrder,
                        0),
                    new TestTriggerHandler(name));
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player),
                CreateSideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState CreateSideState(
            CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static TestCombatEvent
            CreateSourceEvent()
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            return new TestCombatEvent(
                metadataFactory.CreateRoot());
        }

        private sealed class TestTriggerSource :
            ICombatTriggerSource
        {
            public TestTriggerSource()
            {
                Candidates =
                    new CombatTriggerCandidate<
                        ICombatTriggerHandler>[0];
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                Candidates
            {
                get;
                set;
            }

            public bool ThrowDuringDiscovery
            {
                get;
                set;
            }

            public int DiscoveryCallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public CombatEvent ReceivedEvent
            {
                get;
                private set;
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                DiscoverTriggers(
                    CombatState state,
                    CombatEvent sourceEvent)
            {
                DiscoveryCallCount++;

                ReceivedState = state;
                ReceivedEvent = sourceEvent;

                if (ThrowDuringDiscovery)
                {
                    throw new InvalidOperationException(
                        "Test discovery failure.");
                }

                return Candidates;
            }
        }

        private sealed class TestTriggerHandler :
            ICombatTriggerHandler
        {
            public TestTriggerHandler(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                return true;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
            }
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }
    }
}