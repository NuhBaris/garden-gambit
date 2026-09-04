using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSlotBattleStartRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSlotBattleStartRunner(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.Engine));
        }

        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSlotBattleStartRunner(
                        environment.State,
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
                    new CombatSlotBattleStartRunner(
                        environment.State,
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
                    new CombatSlotBattleStartRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void
            StartAndResolveSlotStage_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveSlotStage(
                        null,
                        100,
                        100,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartAndResolveSlotStage_WithInvalidPassBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSlotStage(
                        environment
                            .CombatStartedEvent,
                        0,
                        100,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveSlotStage_WithInvalidEventBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSlotStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        0,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveSlotStage_WithInvalidTriggerBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSlotStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        0));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveSlotStage_WithEmptyBoards_CompletesWithoutAltars()
        {
            var environment =
                CreateEnvironment();

            var activationCount =
                environment.Runner
                    .StartAndResolveSlotStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100);

            Assert.That(
                activationCount,
                Is.EqualTo(0));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            var slotStageEvent =
                environment.EventLog.Events[1]
                    as
                    BattleStartStageStartedCombatEvent;

            Assert.That(
                slotStageEvent,
                Is.Not.Null);

            Assert.That(
                slotStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                slotStageEvent.IsSlotStage,
                Is.True);

            Assert.That(
                slotStageEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveSlotStageEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedActivationCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            ResumeActiveSlotStage_WithoutActiveStage_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveSlotStage(
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            ResumeActiveSlotStage_AfterFinalDrainBudgetExhaustion_DoesNotRepeatStageOrTrigger()
        {
            var environment =
                CreateEnvironment(
                    appendEventFromSlotTrigger: true);

            environment.Engine.Drain(
                100,
                100,
                100);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveSlotStage(
                        environment
                            .CombatStartedEvent,
                        1,
                        1,
                        100));

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveSlotStageEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.False);

            Assert.That(
                environment.SlotTriggerHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountSlotStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            var activeSlotStageEvent =
                environment.Runner
                    .ActiveSlotStageEvent;

            var activationCount =
                environment.Runner
                    .ResumeActiveSlotStage(
                        100,
                        100,
                        100);

            Assert.That(
                activationCount,
                Is.EqualTo(0));

            Assert.That(
                environment.SlotTriggerHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountSlotStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.ContainsEvent(
                    activeSlotStageEvent
                        .Metadata.EventId),
                Is.True);

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool appendEventFromSlotTrigger = false)
        {
            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var slotTriggerHandler =
                new TestSlotTriggerHandler(
                    appendEventFromSlotTrigger,
                    metadataFactory,
                    eventLog);

            ICombatTriggerSource[] sources;

            if (appendEventFromSlotTrigger)
            {
                sources =
                    new ICombatTriggerSource[]
                    {
                        new CombatTriggerHandlerSource(
                            new
                                FixedCombatTriggerOrderKeyProvider(
                                    new CombatTriggerOrderKey(
                                        CombatTriggerSourceKind
                                            .Slot,
                                        CombatSide.Player,
                                        0,
                                        0)),
                            slotTriggerHandler)
                    };
            }
            else
            {
                sources =
                    new ICombatTriggerSource[0];
            }

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

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

            return new TestEnvironment
            {
                State = state,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                Engine = engine,
                CombatStartedEvent =
                    combatStartedEvent,
                SlotTriggerHandler =
                    slotTriggerHandler,
                Runner =
                    new CombatSlotBattleStartRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        engine)
            };
        }

        private static int CountSlotStageEvents(
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
                    stageEvent.IsSlotStage)
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
            TestSlotTriggerHandler :
            ICombatTriggerHandler
        {
            private readonly bool
                _appendEvent;

            private readonly CombatEventMetadataFactory
                _metadataFactory;

            private readonly CombatEventLog
                _eventLog;

            public TestSlotTriggerHandler(
                bool appendEvent,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
            {
                _appendEvent =
                    appendEvent;

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

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                var stageEvent =
                    sourceEvent
                        as
                        BattleStartStageStartedCombatEvent;

                return stageEvent != null &&
                       stageEvent.IsSlotStage;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;

                if (!_appendEvent)
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

            public TestSlotTriggerHandler
                SlotTriggerHandler
            {
                get;
                set;
            }

            public CombatSlotBattleStartRunner Runner
            {
                get;
                set;
            }
        }
    }
}