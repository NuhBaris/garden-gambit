using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardBattleStartRunnerTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardBattleStartRunner(
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
                    new CombatCardBattleStartRunner(
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
                    new CombatCardBattleStartRunner(
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void
            StartAndResolveCardStage_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveCardStage(
                        null,
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            StartAndResolveCardStage_WithInvalidPassBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveCardStage(
                        environment
                            .CombatStartedEvent,
                        0,
                        100,
                        100));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveCardStage_WithInvalidEventBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveCardStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        0,
                        100));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveCardStage_WithInvalidTriggerBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveCardStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        0));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveCardStage_WithoutPetStage_Throws()
        {
            var environment =
                CreateEnvironment(
                    appendPetStage: false);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCardStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));

            Assert.That(
                environment.Runner.HasActiveCardStage,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveCardStage_WithTrigger_ProcessesCardStage()
        {
            var environment =
                CreateEnvironment();

            var cardStageEvent =
                environment.Runner
                    .StartAndResolveCardStage(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100);

            Assert.That(
                cardStageEvent,
                Is.Not.Null);

            Assert.That(
                cardStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            Assert.That(
                cardStageEvent.IsCardStage,
                Is.True);

            Assert.That(
                environment.CardTriggerHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.CardTriggerHandler
                    .LastSourceEvent,
                Is.SameAs(
                    cardStageEvent));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCardStage,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCardStageEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveCardStage_WithoutActiveStage_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCardStage(
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            ResumeActiveCardStage_AfterBudgetExhaustion_DoesNotRepeatStageOrTrigger()
        {
            var environment =
                CreateEnvironment(
                    appendEventOnResolve: true);

            environment.Engine.Drain(
                100,
                100,
                100);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCardStage(
                        environment
                            .CombatStartedEvent,
                        1,
                        1,
                        100));

            Assert.That(
                environment.Runner.HasActiveCardStage,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCardStageEvent,
                Is.Not.Null);

            Assert.That(
                environment.CardTriggerHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            var activeCardStageEvent =
                environment.Runner
                    .ActiveCardStageEvent;

            var completedCardStageEvent =
                environment.Runner
                    .ResumeActiveCardStage(
                        100,
                        100,
                        100);

            Assert.That(
                completedCardStageEvent,
                Is.SameAs(
                    activeCardStageEvent));

            Assert.That(
                environment.CardTriggerHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountCardStageEvents(
                    environment.EventLog),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCardStage,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool appendPetStage = true,
                bool appendEventOnResolve = false)
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

            var cardTriggerHandler =
                new TestCardTriggerHandler(
                    appendEventOnResolve,
                    metadataFactory,
                    eventLog);

            var source =
                new CombatTriggerHandlerSource(
                    new
                        FixedCombatTriggerOrderKeyProvider(
                            new CombatTriggerOrderKey(
                                CombatTriggerSourceKind.Card,
                                CombatSide.Player,
                                0,
                                0)),
                    cardTriggerHandler);

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

            var stageResolver =
                new CombatBattleStartStageResolver(
                    metadataFactory,
                    eventLog);

            stageResolver.StartStage(
                combatStartedEvent,
                CombatBattleStartStage.Slot);

            if (appendPetStage)
            {
                stageResolver.StartStage(
                    combatStartedEvent,
                    CombatBattleStartStage.Pet);
            }

            return new TestEnvironment
            {
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                Engine = engine,
                CombatStartedEvent =
                    combatStartedEvent,
                CardTriggerHandler =
                    cardTriggerHandler,
                Runner =
                    new CombatCardBattleStartRunner(
                        metadataFactory,
                        eventLog,
                        engine)
            };
        }

        private static int CountCardStageEvents(
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
                    stageEvent.IsCardStage)
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
            TestCardTriggerHandler :
            ICombatTriggerHandler
        {
            private readonly bool
                _appendEventOnResolve;

            private readonly CombatEventMetadataFactory
                _metadataFactory;

            private readonly CombatEventLog
                _eventLog;

            public TestCardTriggerHandler(
                bool appendEventOnResolve,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
            {
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

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                var stageEvent =
                    sourceEvent
                        as
                        BattleStartStageStartedCombatEvent;

                return stageEvent != null &&
                       stageEvent.IsCardStage;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;

                LastSourceEvent =
                    sourceEvent;

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

            public TestCardTriggerHandler
                CardTriggerHandler
            {
                get;
                set;
            }

            public CombatCardBattleStartRunner Runner
            {
                get;
                set;
            }
        }
    }
}