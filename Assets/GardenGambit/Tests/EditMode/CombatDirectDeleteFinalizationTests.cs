using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDirectDeleteFinalizationTests
    {
        [Test]
        public void
            Drain_WithFrontDirectDelete_RunsQueuedTriggerBeforeAdvancingBackCard()
        {
            var environment =
                CreateTriggeredEnvironment(
                    deleteBackCard: false);

            environment.Engine.Drain(
                maximumPassCount: 10,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.Observer.CallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Observer.SawFrontEmpty,
                Is.True);

            Assert.That(
                environment.Observer.SawBackCard,
                Is.True);

            var frontSlot =
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.FrontPosition);

            var backSlot =
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.BackPosition);

            Assert.That(
                frontSlot.IsOccupied,
                Is.True);

            Assert.That(
                frontSlot.OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.BackCard.InstanceId));

            Assert.That(
                backSlot.IsOccupied,
                Is.False);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.SourceEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    DirectDeleteCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<
                    CardAdvancedCombatEvent>());

            var directDeleteEvent =
                (DirectDeleteCombatEvent)
                    environment.EventLog.Events[1];

            var advancementEvent =
                (CardAdvancedCombatEvent)
                    environment.EventLog.Events[2];

            Assert.That(
                directDeleteEvent.InstanceId,
                Is.EqualTo(
                    environment.FrontCard.InstanceId));

            Assert.That(
                advancementEvent.InstanceId,
                Is.EqualTo(
                    environment.BackCard.InstanceId));

            Assert.That(
                advancementEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    directDeleteEvent.Metadata.EventId));

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            Drain_WithBackDirectDelete_DoesNotAdvanceFrontCard()
        {
            var environment =
                CreateTriggeredEnvironment(
                    deleteBackCard: true);

            environment.Engine.Drain(
                maximumPassCount: 10,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            var frontSlot =
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.FrontPosition);

            var backSlot =
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.BackPosition);

            Assert.That(
                frontSlot.IsOccupied,
                Is.True);

            Assert.That(
                frontSlot.OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.FrontCard.InstanceId));

            Assert.That(
                backSlot.IsOccupied,
                Is.False);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Cards[0],
                Is.SameAs(
                    environment.FrontCard));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DirectDelete),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CardAdvanced),
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            Drain_WithDeathThenDirectDelete_AdvancesExactlyOnce()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var frontPosition =
                CreatePosition(
                    BoardRow.Front);

            var backPosition =
                CreatePosition(
                    BoardRow.Back);

            var deadFrontCard =
                CreateCard(
                    "dead-front-card",
                    1001,
                    currentHp: 0);

            var livingBackCard =
                CreateCard(
                    "living-back-card",
                    1002,
                    currentHp: 10);

            var playerSide =
                CreatePlayerSide(
                    frontPosition,
                    backPosition,
                    deadFrontCard,
                    livingBackCard);

            var state =
                new CombatState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var deathEvent =
                new DeathCombatEvent(
                    metadataFactory.CreateRoot(),
                    deadFrontCard.InstanceId,
                    frontPosition,
                    previousHp: 3,
                    currentHp: 0);

            eventLog.Append(
                deathEvent);

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    metadataFactory,
                    eventLog);

            var directDeleteEvent =
                directDeleteResolver
                    .ApplyDirectDelete(
                        state,
                        deathEvent,
                        frontPosition);

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var engine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    new CombatTriggerSourceRegistry(
                        Array.Empty<
                            ICombatTriggerSource>()));

            engine.Drain(
                maximumPassCount: 10,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.DirectDelete),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.DeathRemoval),
                Is.Zero);

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.CardAdvanced),
                Is.EqualTo(1));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(
                    deathEvent));

            Assert.That(
                eventLog.Events[1],
                Is.SameAs(
                    directDeleteEvent));

            var advancementEvent =
                eventLog.Events[2]
                    as CardAdvancedCombatEvent;

            Assert.That(
                advancementEvent,
                Is.Not.Null);

            Assert.That(
                advancementEvent.InstanceId,
                Is.EqualTo(
                    livingBackCard.InstanceId));

            Assert.That(
                advancementEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    directDeleteEvent.Metadata.EventId));

            var frontSlot =
                playerSide.Board.GetSlot(
                    frontPosition);

            var backSlot =
                playerSide.Board.GetSlot(
                    backPosition);

            Assert.That(
                frontSlot.OccupantInstanceId.Value,
                Is.EqualTo(
                    livingBackCard.InstanceId));

            Assert.That(
                backSlot.IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                engine.HasPendingWork,
                Is.False);
        }

        private static TestEnvironment
            CreateTriggeredEnvironment(
                bool deleteBackCard)
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var frontPosition =
                CreatePosition(
                    BoardRow.Front);

            var backPosition =
                CreatePosition(
                    BoardRow.Back);

            var frontCard =
                CreateCard(
                    "front-card",
                    1001,
                    currentHp: 10);

            var backCard =
                CreateCard(
                    "back-card",
                    1002,
                    currentHp: 10);

            var playerSide =
                CreatePlayerSide(
                    frontPosition,
                    backPosition,
                    frontCard,
                    backCard);

            var state =
                new CombatState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var sourceEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                sourceEvent);

            var deletePosition =
                deleteBackCard
                    ? backPosition
                    : frontPosition;

            var deleteHandler =
                new DirectDeleteHandler(
                    sourceEvent,
                    deletePosition,
                    metadataFactory,
                    eventLog);

            var observer =
                new BoardObserverHandler(
                    sourceEvent,
                    playerSide,
                    frontPosition,
                    backPosition,
                    backCard.InstanceId);

            var deleteSource =
                CreateHandlerSource(
                    deleteHandler,
                    horizontalOrder: 0);

            var observerSource =
                CreateHandlerSource(
                    observer,
                    horizontalOrder: 1);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        deleteSource,
                        observerSource
                    });

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var engine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            return new TestEnvironment
            {
                Engine =
                    engine,
                EventLog =
                    eventLog,
                SourceEvent =
                    sourceEvent,
                PlayerSide =
                    playerSide,
                FrontPosition =
                    frontPosition,
                BackPosition =
                    backPosition,
                FrontCard =
                    frontCard,
                BackCard =
                    backCard,
                Observer =
                    observer
            };
        }

        private static CombatTriggerHandlerSource
            CreateHandlerSource(
                ICombatTriggerHandler handler,
                int horizontalOrder)
        {
            return new CombatTriggerHandlerSource(
                new FixedCombatTriggerOrderKeyProvider(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        horizontalOrder,
                        verticalOrder: 0)),
                handler);
        }

        private static CombatSideState
            CreatePlayerSide(
                BoardPosition frontPosition,
                BoardPosition backPosition,
                CombatCardState frontCard,
                CombatCardState backCard)
        {
            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            frontPosition,
                            frontCard.InstanceId),
                        new CombatSlotState(
                            new SlotId(2),
                            backPosition,
                            backCard.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        frontCard,
                        backCard
                    }),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<
                        CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<
                        CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static BoardPosition
            CreatePosition(
                BoardRow row)
        {
            return new BoardPosition(
                CombatSide.Player,
                row,
                new BoardColumn(1));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: currentHp,
                armor: 0,
                attack: 2);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new
                    CombatSequenceNumberAllocator());
        }

        private static int CountEvents(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    count++;
                }
            }

            return count;
        }

        private sealed class DirectDeleteHandler :
            ICombatTriggerHandler
        {
            private readonly CombatEvent
                _expectedSourceEvent;

            private readonly BoardPosition
                _targetPosition;

            private readonly CombatDirectDeleteResolver
                _resolver;

            public DirectDeleteHandler(
                CombatEvent expectedSourceEvent,
                BoardPosition targetPosition,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
            {
                _expectedSourceEvent =
                    expectedSourceEvent;

                _targetPosition =
                    targetPosition;

                _resolver =
                    new CombatDirectDeleteResolver(
                        metadataFactory,
                        eventLog);
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                return ReferenceEquals(
                    sourceEvent,
                    _expectedSourceEvent);
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                _resolver.ApplyDirectDelete(
                    state,
                    sourceEvent,
                    _targetPosition);
            }
        }

        private sealed class BoardObserverHandler :
            ICombatTriggerHandler
        {
            private readonly CombatEvent
                _expectedSourceEvent;

            private readonly CombatSideState
                _playerSide;

            private readonly BoardPosition
                _frontPosition;

            private readonly BoardPosition
                _backPosition;

            private readonly InstanceId
                _expectedBackCardInstanceId;

            public BoardObserverHandler(
                CombatEvent expectedSourceEvent,
                CombatSideState playerSide,
                BoardPosition frontPosition,
                BoardPosition backPosition,
                InstanceId
                    expectedBackCardInstanceId)
            {
                _expectedSourceEvent =
                    expectedSourceEvent;

                _playerSide =
                    playerSide;

                _frontPosition =
                    frontPosition;

                _backPosition =
                    backPosition;

                _expectedBackCardInstanceId =
                    expectedBackCardInstanceId;
            }

            public int CallCount
            {
                get;
                private set;
            }

            public bool SawFrontEmpty
            {
                get;
                private set;
            }

            public bool SawBackCard
            {
                get;
                private set;
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                return ReferenceEquals(
                    sourceEvent,
                    _expectedSourceEvent);
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                CallCount++;

                var frontSlot =
                    _playerSide.Board.GetSlot(
                        _frontPosition);

                var backSlot =
                    _playerSide.Board.GetSlot(
                        _backPosition);

                SawFrontEmpty =
                    !frontSlot.IsOccupied;

                SawBackCard =
                    backSlot.IsOccupied &&
                    backSlot.OccupantInstanceId.Value ==
                    _expectedBackCardInstanceId;
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
            public CombatEventResolutionEngine Engine
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatEvent SourceEvent
            {
                get;
                set;
            }

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public BoardPosition FrontPosition
            {
                get;
                set;
            }

            public BoardPosition BackPosition
            {
                get;
                set;
            }

            public CombatCardState FrontCard
            {
                get;
                set;
            }

            public CombatCardState BackCard
            {
                get;
                set;
            }

            public BoardObserverHandler Observer
            {
                get;
                set;
            }
        }
    }
}