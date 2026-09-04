using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerPetBattleStartIntegrationTests
    {
        [Test]
        public void
            StartAndResolveCombat_WithPetTrigger_ResolvesPetBeforeCardAndColumns()
        {
            var resolutionOrder =
                new List<string>();

            var pet =
                CreatePet(
                    "pet.player",
                    101);

            var petHandler =
                new RecordingPetHandler(
                    CombatSide.Player,
                    pet.InstanceId,
                    "Pet",
                    resolutionOrder,
                    canTrigger: true);

            var cardHandler =
                new RecordingStageHandler(
                    CombatBattleStartStage.Card,
                    "Card",
                    resolutionOrder);

            var sources =
                new ICombatTriggerSource[]
                {
                    CreateCardSource(
                        cardHandler),

                    new CombatPetBattleStartTriggerSource(
                        petHandler)
                };

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        pet
                    },
                    new CombatPetState[0],
                    sources);

            var completedEvent =
                StartCombat(
                    environment.Runner);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                completedEvent.IsDraw,
                Is.True);

            Assert.That(
                resolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Pet",
                        "Card"
                    }));

            Assert.That(
                petHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                cardHandler.ResolveCallCount,
                Is.EqualTo(1));

            var petStageIndex =
                IndexOfStage(
                    environment.EventLog,
                    CombatBattleStartStage.Pet);

            var cardStageIndex =
                IndexOfStage(
                    environment.EventLog,
                    CombatBattleStartStage.Card);

            var firstColumnIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted);

            Assert.That(
                petStageIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                cardStageIndex,
                Is.GreaterThan(
                    petStageIndex));

            Assert.That(
                firstColumnIndex,
                Is.GreaterThan(
                    cardStageIndex));

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
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveCombat_WithMultiplePets_UsesSourceOrderAndPlayerTiePriority()
        {
            var resolutionOrder =
                new List<string>();

            var playerFirst =
                CreatePet(
                    "pet.player.first",
                    101);

            var playerSecond =
                CreatePet(
                    "pet.player.second",
                    102);

            var enemyFirst =
                CreatePet(
                    "pet.enemy.first",
                    201);

            var enemySecond =
                CreatePet(
                    "pet.enemy.second",
                    202);

            var playerFirstHandler =
                new RecordingPetHandler(
                    CombatSide.Player,
                    playerFirst.InstanceId,
                    "Player0",
                    resolutionOrder,
                    canTrigger: true);

            var playerSecondHandler =
                new RecordingPetHandler(
                    CombatSide.Player,
                    playerSecond.InstanceId,
                    "Player1",
                    resolutionOrder,
                    canTrigger: true);

            var enemyFirstHandler =
                new RecordingPetHandler(
                    CombatSide.Enemy,
                    enemyFirst.InstanceId,
                    "Enemy0",
                    resolutionOrder,
                    canTrigger: true);

            var enemySecondHandler =
                new RecordingPetHandler(
                    CombatSide.Enemy,
                    enemySecond.InstanceId,
                    "Enemy1",
                    resolutionOrder,
                    canTrigger: true);

            var sources =
                new ICombatTriggerSource[]
                {
                    new CombatPetBattleStartTriggerSource(
                        enemySecondHandler),

                    new CombatPetBattleStartTriggerSource(
                        playerSecondHandler),

                    new CombatPetBattleStartTriggerSource(
                        enemyFirstHandler),

                    new CombatPetBattleStartTriggerSource(
                        playerFirstHandler)
                };

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        playerFirst,
                        playerSecond
                    },
                    new[]
                    {
                        enemyFirst,
                        enemySecond
                    },
                    sources);

            StartCombat(
                environment.Runner);

            Assert.That(
                resolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Player0",
                        "Enemy0",
                        "Player1",
                        "Enemy1"
                    }));

            Assert.That(
                playerFirstHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                enemyFirstHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                playerSecondHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                enemySecondHandler.ResolveCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartAndResolveCombat_WhenPetConditionFails_StillCompletesStagesWithoutResolvingPet()
        {
            var resolutionOrder =
                new List<string>();

            var pet =
                CreatePet(
                    "pet.player",
                    101);

            var petHandler =
                new RecordingPetHandler(
                    CombatSide.Player,
                    pet.InstanceId,
                    "Pet",
                    resolutionOrder,
                    canTrigger: false);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        pet
                    },
                    new CombatPetState[0],
                    new ICombatTriggerSource[]
                    {
                        new
                            CombatPetBattleStartTriggerSource(
                                petHandler)
                    });

            var completedEvent =
                StartCombat(
                    environment.Runner);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                petHandler.ResolveCallCount,
                Is.EqualTo(0));

            Assert.That(
                resolutionOrder,
                Is.Empty);

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
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveCombat_WhenPetStageExhaustsBudget_DoesNotRepeatCompletedStagesOrPet()
        {
            var resolutionOrder =
                new List<string>();

            var pet =
                CreatePet(
                    "pet.player",
                    101);

            TestEnvironment environment =
                null;

            var petHandler =
                new RecordingPetHandler(
                    CombatSide.Player,
                    pet.InstanceId,
                    "Pet",
                    resolutionOrder,
                    canTrigger: true,
                    generatedEventCount: 2);

            environment =
                CreateEnvironment(
                    new[]
                    {
                        pet
                    },
                    new CombatPetState[0],
                    new ICombatTriggerSource[]
                    {
                        new
                            CombatPetBattleStartTriggerSource(
                                petHandler)
                    });

            petHandler.SetEventDependencies(
                environment.MetadataFactory,
                environment.EventLog);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 1,
                        maximumEventCountPerPass: 2,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveBattleStartResolution,
                Is.True);

            Assert.That(
                environment.Runner.HasActivePetStage,
                Is.True);

            Assert.That(
                environment.Runner
                    .NextBattleStartStage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                petHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                resolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Pet"
                    }));

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

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                petHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                resolutionOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Pet"
                    }));

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
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasActiveBattleStartResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingBattleStartResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatPetState[] playerPets,
                CombatPetState[] enemyPets,
                ICombatTriggerSource[] sources)
        {
            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            playerPets)),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            enemyPets)));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            return new TestEnvironment
            {
                MetadataFactory =
                    metadataFactory,

                EventLog =
                    eventLog,

                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatCompletedCombatEvent
            StartCombat(
                CombatResolutionRunner runner)
        {
            return runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);
        }

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
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

        private static ICombatTriggerSource
            CreateCardSource(
                ICombatTriggerHandler handler)
        {
            return new CombatTriggerHandlerSource(
                new FixedCombatTriggerOrderKeyProvider(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        0,
                        0)),
                handler);
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

        private static int IndexOfStage(
            CombatEventLog eventLog,
            CombatBattleStartStage stage)
        {
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
                    return index;
                }
            }

            return -1;
        }

        private static int IndexOfKind(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    return index;
                }
            }

            return -1;
        }

        private sealed class RecordingPetHandler :
            CombatPetBattleStartTriggerHandler
        {
            private readonly string
                _name;

            private readonly List<string>
                _resolutionOrder;

            private readonly bool
                _canTrigger;

            private readonly int
                _generatedEventCount;

            private CombatEventMetadataFactory
                _metadataFactory;

            private CombatEventLog
                _eventLog;

            public RecordingPetHandler(
                CombatSide side,
                InstanceId petInstanceId,
                string name,
                List<string> resolutionOrder,
                bool canTrigger,
                int generatedEventCount = 0)
                : base(
                    side,
                    petInstanceId)
            {
                _name = name;

                _resolutionOrder =
                    resolutionOrder;

                _canTrigger =
                    canTrigger;

                _generatedEventCount =
                    generatedEventCount;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public void SetEventDependencies(
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
            {
                if (metadataFactory == null)
                {
                    throw new ArgumentNullException(
                        nameof(metadataFactory));
                }

                if (eventLog == null)
                {
                    throw new ArgumentNullException(
                        nameof(eventLog));
                }

                _metadataFactory =
                    metadataFactory;

                _eventLog =
                    eventLog;
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

                _resolutionOrder.Add(
                    _name);

                if (_generatedEventCount == 0)
                {
                    return;
                }

                if (_metadataFactory == null ||
                    _eventLog == null)
                {
                    throw new InvalidOperationException(
                        "Generated-event dependencies " +
                        "were not configured.");
                }

                for (var index = 0;
                     index < _generatedEventCount;
                     index++)
                {
                    var metadata =
                        _metadataFactory.CreateChild(
                            sourceEvent.Metadata);

                    _eventLog.Append(
                        new TestCombatEvent(
                            metadata));
                }
            }
        }

        private sealed class RecordingStageHandler :
            ICombatTriggerHandler
        {
            private readonly CombatBattleStartStage
                _stage;

            private readonly string
                _name;

            private readonly List<string>
                _resolutionOrder;

            public RecordingStageHandler(
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

            public CombatResolutionRunner Runner
            {
                get;
                set;
            }
        }
    }
}