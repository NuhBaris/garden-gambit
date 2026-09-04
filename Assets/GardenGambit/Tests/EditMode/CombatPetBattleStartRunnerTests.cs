using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartRunnerTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleStartRunner(
                        null,
                        environment.EventLog,
                        environment.Engine));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleStartRunner(
                        environment.MetadataFactory,
                        null,
                        environment.Engine));
        }

        [Test]
        public void Constructor_WithNullEngine_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleStartRunner(
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void
            StartAndResolvePetStage_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolvePetStage(
                        null,
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            StartAndResolvePetStage_WithInvalidPassBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        0,
                        100,
                        100));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolvePetStage_WithInvalidEventBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        0,
                        100));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolvePetStage_WithInvalidTriggerBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        0));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolvePetStage_WithoutSlotStage_Throws()
        {
            var environment =
                CreateEnvironment(
                    appendSlotStage: false);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolvePetStage_WithTrigger_ProcessesPetStage()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true);

            var petStageEvent =
                environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100);

            Assert.That(
                petStageEvent,
                Is.Not.Null);

            Assert.That(
                petStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                petStageEvent.IsPetStage,
                Is.True);

            Assert.That(
                environment.Handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Handler.LastSourceEvent,
                Is.SameAs(
                    petStageEvent));

            Assert.That(
                environment.Handler.LastPet,
                Is.SameAs(
                    environment.Pet));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);

            Assert.That(
                CountPetStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartAndResolvePetStage_WhenConditionFails_LogsStageWithoutResolving()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: false);

            var petStageEvent =
                environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100);

            Assert.That(
                petStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                environment.Handler.ResolveCallCount,
                Is.EqualTo(0));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);

            Assert.That(
                CountPetStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));
        }

        [Test]
        public void
            ResumeActivePetStage_WithoutActiveStage_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .ResumeActivePetStage(
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            ResumeActivePetStage_AfterBudgetExhaustion_DoesNotRepeatPetTrigger()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true,
                    appendEventOnResolve: true);

            environment.Engine.Drain(
                100,
                100,
                100);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolvePetStage(
                        environment
                            .CombatStartedEvent,
                        1,
                        1,
                        100));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActivePetStageEvent,
                Is.Not.Null);

            Assert.That(
                environment.Handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountPetStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            var activePetStageEvent =
                environment.Runner
                    .ActivePetStageEvent;

            var completedPetStageEvent =
                environment.Runner
                    .ResumeActivePetStage(
                        100,
                        100,
                        100);

            Assert.That(
                completedPetStageEvent,
                Is.SameAs(
                    activePetStageEvent));

            Assert.That(
                environment.Handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountPetStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool appendSlotStage = true,
                bool canTrigger = true,
                bool appendEventOnResolve = false)
        {
            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "pet.player"),
                    new InstanceId(101));

            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                pet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new CombatPetState[0])));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    pet.InstanceId,
                    canTrigger,
                    appendEventOnResolve,
                    metadataFactory,
                    eventLog);

            var source =
                new CombatPetBattleStartTriggerSource(
                    handler);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        source
                    });

            var engine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            if (appendSlotStage)
            {
                var stageResolver =
                    new CombatBattleStartStageResolver(
                        metadataFactory,
                        eventLog);

                stageResolver.StartStage(
                    combatStartedEvent,
                    CombatBattleStartStage.Slot);
            }

            return new TestEnvironment
            {
                State = state,
                Pet = pet,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                Engine = engine,
                CombatStartedEvent =
                    combatStartedEvent,
                Handler = handler,
                Runner =
                    new CombatPetBattleStartRunner(
                        metadataFactory,
                        eventLog,
                        engine)
            };
        }

        private static int CountPetStageEvents(
            CombatEventLog eventLog)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var stageEvent =
                    eventLog.Events[index]
                        as
                        BattleStartStageStartedCombatEvent;

                if (stageEvent != null &&
                    stageEvent.IsPetStage)
                {
                    count++;
                }
            }

            return count;
        }

        private static CombatSideState
            CreateEmptySide(
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

        private sealed class
            TestBattleStartHandler :
            CombatPetBattleStartTriggerHandler
        {
            private readonly bool
                _canTrigger;

            private readonly bool
                _appendEventOnResolve;

            private readonly CombatEventMetadataFactory
                _metadataFactory;

            private readonly CombatEventLog
                _eventLog;

            public TestBattleStartHandler(
                CombatSide side,
                InstanceId petInstanceId,
                bool canTrigger,
                bool appendEventOnResolve,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
                : base(
                    side,
                    petInstanceId)
            {
                _canTrigger =
                    canTrigger;

                _appendEventOnResolve =
                    appendEventOnResolve;

                _metadataFactory =
                    metadataFactory;

                _eventLog =
                    eventLog;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public CombatEvent LastSourceEvent
            {
                get;
                private set;
            }

            public CombatPetState LastPet
            {
                get;
                private set;
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                return _canTrigger;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                ResolveCallCount++;

                LastSourceEvent =
                    sourceEvent;

                LastPet =
                    pet;

                if (!_appendEventOnResolve)
                {
                    return;
                }

                var metadata =
                    _metadataFactory.CreateChild(
                        sourceEvent.Metadata);

                _eventLog.Append(
                    new TestCombatEvent(
                        metadata));
            }
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.HpGain)
            {
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatPetState Pet
            {
                get;
                set;
            }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatEventResolutionEngine Engine
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public TestBattleStartHandler Handler
            {
                get;
                set;
            }

            public CombatPetBattleStartRunner Runner
            {
                get;
                set;
            }
        }
    }
}