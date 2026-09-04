using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatBattleStartRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartRunner(
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
                    new CombatBattleStartRunner(
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
                    new CombatBattleStartRunner(
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
                    new CombatBattleStartRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void
            StartAndResolveBattleStart_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        null,
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            StartAndResolveBattleStart_WithInvalidPassBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        environment
                            .CombatStartedEvent,
                        0,
                        100,
                        100));

            Assert.That(
                CountAllStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveBattleStart_WithInvalidEventBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        environment
                            .CombatStartedEvent,
                        100,
                        0,
                        100));

            Assert.That(
                CountAllStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveBattleStart_WithInvalidTriggerBudget_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        0));

            Assert.That(
                CountAllStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveBattleStart_WithUnloggedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    appendCombatStartedEvent: false);

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveBattleStart_WithImpostorEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            var impostorEvent =
                new CombatStartedCombatEvent(
                    environment
                        .CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        impostorEvent,
                        100,
                        100,
                        100));

            Assert.That(
                CountAllStageEvents(
                    environment.EventLog),
                Is.EqualTo(0));
        }

        [Test]
        public void
            StartAndResolveBattleStart_ProcessesSlotPetCardOrder()
        {
            var environment =
                CreateEnvironment();

            var altarActivationCount =
                environment.Runner
                    .StartAndResolveBattleStart(
                        environment
                            .CombatStartedEvent,
                        100,
                        100,
                        100);

            Assert.That(
                altarActivationCount,
                Is.EqualTo(0));

            Assert.That(
                environment.ResolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Slot",
                        "Pet",
                        "Card"
                    }));

            Assert.That(
                environment.SlotHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.CardHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Slot),
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Pet),
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Card),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    BattleStartStageStartedCombatEvent>());

            Assert.That(
                ((BattleStartStageStartedCombatEvent)
                    environment.EventLog.Events[1])
                    .Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                ((BattleStartStageStartedCombatEvent)
                    environment.EventLog.Events[2])
                    .Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                ((BattleStartStageStartedCombatEvent)
                    environment.EventLog.Events[3])
                    .Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveStage,
                Is.False);

            Assert.That(
                environment.Runner.ActiveStageEvent,
                Is.Null);

            Assert.That(
                environment.Runner.NextStage,
                Is.EqualTo(
                    CombatBattleStartStage
                        .Unspecified));

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveBattleStart_WithoutActiveResolution_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveBattleStart(
                        100,
                        100,
                        100));
        }

        [Test]
        public void
            ResumeActiveBattleStart_AfterPetBudgetExhaustion_DoesNotRepeatCompletedStages()
        {
            var environment =
                CreateEnvironment(
                    appendEventFromPet: true);

            environment.Engine.Drain(
                100,
                100,
                100);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveBattleStart(
                        environment
                            .CombatStartedEvent,
                        1,
                        1,
                        100));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.True);

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.True);

            Assert.That(
                environment.Runner.NextStage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                environment.ResolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Slot",
                        "Pet"
                    }));

            Assert.That(
                environment.SlotHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.CardHandler
                    .ResolveCallCount,
                Is.EqualTo(0));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Slot),
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Pet),
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Card),
                Is.EqualTo(0));

            var altarActivationCount =
                environment.Runner
                    .ResumeActiveBattleStart(
                        100,
                        100,
                        100);

            Assert.That(
                altarActivationCount,
                Is.EqualTo(0));

            Assert.That(
                environment.ResolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Slot",
                        "Pet",
                        "Card"
                    }));

            Assert.That(
                environment.SlotHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.CardHandler
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Slot),
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Pet),
                Is.EqualTo(1));

            Assert.That(
                CountStageEvents(
                    environment.EventLog,
                    CombatBattleStartStage.Card),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool appendCombatStartedEvent = true,
                bool appendEventFromPet = false)
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

            var resolutionOrder =
                new List<string>();

            var slotHandler =
                new TestStageHandler(
                    CombatBattleStartStage.Slot,
                    "Slot",
                    resolutionOrder);

            var petHandler =
                new TestPetHandler(
                    pet.InstanceId,
                    resolutionOrder,
                    appendEventFromPet,
                    metadataFactory,
                    eventLog);

            var cardHandler =
                new TestStageHandler(
                    CombatBattleStartStage.Card,
                    "Card",
                    resolutionOrder);

            var slotSource =
                new CombatTriggerHandlerSource(
                    new
                        FixedCombatTriggerOrderKeyProvider(
                            new CombatTriggerOrderKey(
                                CombatTriggerSourceKind.Slot,
                                CombatSide.Player,
                                0,
                                0)),
                    slotHandler);

            var petSource =
                new CombatPetBattleStartTriggerSource(
                    petHandler);

            var cardSource =
                new CombatTriggerHandlerSource(
                    new
                        FixedCombatTriggerOrderKeyProvider(
                            new CombatTriggerOrderKey(
                                CombatTriggerSourceKind.Card,
                                CombatSide.Player,
                                0,
                                0)),
                    cardHandler);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        cardSource,
                        petSource,
                        slotSource
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

            if (appendCombatStartedEvent)
            {
                eventLog.Append(
                    combatStartedEvent);
            }

            return new TestEnvironment
            {
                State = state,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                Engine = engine,
                CombatStartedEvent =
                    combatStartedEvent,
                ResolutionOrder =
                    resolutionOrder,
                SlotHandler = slotHandler,
                PetHandler = petHandler,
                CardHandler = cardHandler,
                Runner =
                    new CombatBattleStartRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        engine)
            };
        }

        private static int CountAllStageEvents(
            CombatEventLog eventLog)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index] is
                    BattleStartStageStartedCombatEvent)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountStageEvents(
            CombatEventLog eventLog,
            CombatBattleStartStage stage)
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
                    stageEvent.Stage == stage)
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

        private sealed class TestStageHandler :
            ICombatTriggerHandler
        {
            private readonly CombatBattleStartStage
                _stage;

            private readonly string
                _name;

            private readonly List<string>
                _resolutionOrder;

            public TestStageHandler(
                CombatBattleStartStage stage,
                string name,
                List<string> resolutionOrder)
            {
                _stage = stage;
                _name = name;
                _resolutionOrder =
                    resolutionOrder;
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
                       stageEvent.Stage == _stage;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;

                _resolutionOrder.Add(
                    _name);
            }
        }

        private sealed class TestPetHandler :
            CombatPetBattleStartTriggerHandler
        {
            private readonly List<string>
                _resolutionOrder;

            private readonly bool
                _appendEvent;

            private readonly CombatEventMetadataFactory
                _metadataFactory;

            private readonly CombatEventLog
                _eventLog;

            public TestPetHandler(
                InstanceId petInstanceId,
                List<string> resolutionOrder,
                bool appendEvent,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
                : base(
                    CombatSide.Player,
                    petInstanceId)
            {
                _resolutionOrder =
                    resolutionOrder;

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

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                return true;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                ResolveCallCount++;

                _resolutionOrder.Add(
                    "Pet");

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

            public List<string> ResolutionOrder
            {
                get;
                set;
            }

            public TestStageHandler SlotHandler
            {
                get;
                set;
            }

            public TestPetHandler PetHandler
            {
                get;
                set;
            }

            public TestStageHandler CardHandler
            {
                get;
                set;
            }

            public CombatBattleStartRunner Runner
            {
                get;
                set;
            }
        }
    }
}