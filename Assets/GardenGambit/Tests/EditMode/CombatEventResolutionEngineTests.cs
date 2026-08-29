using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatEventResolutionEngineTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventResolutionEngine(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventResolutionEngine(
                        environment.State,
                        null,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventResolutionEngine(
                        environment.State,
                        environment.MetadataFactory,
                        null,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventResolutionEngine(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullSourceRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventResolutionEngine(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        null));
        }

        [Test]
        public void Drain_WithInvalidBudgets_ThrowsWithoutProcessingWork()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendTestEvent(environment);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Engine.Drain(
                    0,
                    1,
                    1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Engine.Drain(
                    1,
                    0,
                    1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Engine.Drain(
                    1,
                    1,
                    0));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Engine.PendingEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Engine.UnscannedEventCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Drain_WithNoWork_ReturnsZero()
        {
            var environment =
                CreateEnvironment();

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    1,
                    1);

            Assert.That(
                processedEventCount,
                Is.Zero);

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);

            Assert.That(
                environment.Engine.PendingEventCount,
                Is.Zero);

            Assert.That(
                environment.Engine.UnscannedEventCount,
                Is.Zero);
        }

        [Test]
        public void Drain_WithNonDeathEvent_ProcessesAndScansEventInOnePass()
        {
            var environment =
                CreateEnvironment();

            AppendTestEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    1,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);

            Assert.That(
                environment.Engine.UnscannedEventCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Drain_WithDeathEvent_ProcessesRemovalInSecondPass()
        {
            var environment =
                CreateEnvironment();

            AppendDeathEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    2,
                    2,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.TypeOf<DeathCombatEvent>());

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            var deathRemovalTombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                deathRemovalTombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DeathRemoval));

            Assert.That(
                deathRemovalTombstone.CurrentHp,
                Is.Zero);

            Assert.That(
                deathRemovalTombstone
                    .RemovalMetadata.EventId,
                Is.EqualTo(
                    environment.EventLog.Events[1]
                        .Metadata.EventId));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        [Test]
        public void Drain_WhenPassBudgetIsExhausted_PreservesGeneratedRemovalForRetry()
        {
            var environment =
                CreateEnvironment();

            AppendDeathEvent(environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Engine.Drain(
                    1,
                    2,
                    1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.Engine.PendingEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Engine.UnscannedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.True);

            var processedOnRetry =
                environment.Engine.Drain(
                    1,
                    1,
                    1);

            Assert.That(
                processedOnRetry,
                Is.EqualTo(1));

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Drain_WhenHandlerCreatesDeath_ProcessesEntireChainAcrossPasses()
        {
            var handler =
                new TestTriggerHandler();

            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var rootEvent =
                AppendTestEvent(environment);

            DeathCombatEvent deathEvent = null;

            source.DiscoverAction =
                (state, combatEvent) =>
                {
                    if (ReferenceEquals(
                            combatEvent,
                            rootEvent))
                    {
                        return new[]
                        {
                            CreateCandidate(handler)
                        };
                    }

                    return EmptyCandidates();
                };

            handler.ResolveAction =
                (state, sourceEvent) =>
                {
                    deathEvent =
                        new DeathCombatEvent(
                            environment.MetadataFactory
                                .CreateChild(
                                    sourceEvent.Metadata),
                            environment.PlayerCard.InstanceId,
                            environment.PlayerFrontPosition,
                            3,
                            environment.PlayerCard.CurrentHp);

                    environment.EventLog.Append(
                        deathEvent);
                };

            var processedEventCount =
                environment.Engine.Drain(
                    2,
                    3,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(3));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(3));

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    rootEvent.Metadata.EventId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(rootEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(deathEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        [Test]
        public void Drain_WhenDeathTriggerRescuesCard_SkipsRemovalAndKeepsCardAtOneHp()
        {
            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var rescueHandler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog);

            source.DiscoverAction =
                (state, combatEvent) =>
                {
                    var deathEvent =
                        combatEvent as DeathCombatEvent;

                    if (deathEvent != null &&
                        rescueHandler.CanTrigger(
                            state,
                            deathEvent))
                    {
                        return new[]
                        {
                                        CreateCandidate(
                                            rescueHandler)
                        };
                    }

        return EmptyCandidates();
    };

            var deathEvent =
                AppendDeathEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    3,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                rescueHandler.CanRescueCallCount,
                Is.EqualTo(1));

            Assert.That(
                rescueHandler.ReceivedState,
                Is.SameAs(environment.State));

            Assert.That(
                rescueHandler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(deathEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<RescueCombatEvent>());

            Assert.That(
                environment.EventLog.Events,
                Has.None.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        [Test]
        public void Drain_WhenDirectDeletePrecedesRescue_SkipsRescueAndDeathRemoval()
        {
            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var directDeleteHandler =
                new TestDirectDeleteTriggerHandler(
                    directDeleteResolver,
                    environment.PlayerFrontPosition);

            var rescueHandler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog);

            source.DiscoverAction =
                (state, combatEvent) =>
                {
                    var deathEvent =
                        combatEvent as DeathCombatEvent;

                    if (deathEvent != null &&
                        directDeleteHandler.CanTrigger(
                            state,
                            deathEvent) &&
                        rescueHandler.CanTrigger(
                            state,
                            deathEvent))
                    {
                        return new[]
                        {
                                        CreateCandidate(
                                            directDeleteHandler),
                                        CreateCandidate(
                                            rescueHandler)
                        };
                    }

                    return EmptyCandidates();
                };

            var deathEvent =
                AppendDeathEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    3,
                    2);

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                directDeleteHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                rescueHandler.CanRescueCallCount,
                Is.EqualTo(1));

            Assert.That(
                directDeleteHandler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));

            Assert.That(
                rescueHandler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));

            Assert.That(
                directDeleteHandler.DirectDeleteEvent,
                Is.Not.Null);

            Assert.That(
                directDeleteHandler
                    .DirectDeleteEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                directDeleteHandler
                    .DirectDeleteEvent.Position,
                Is.EqualTo(
                    environment.PlayerFrontPosition));

            Assert.That(
                directDeleteHandler
                    .DirectDeleteEvent.HpAtDeletion,
                Is.EqualTo(0));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(deathEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    directDeleteHandler
                        .DirectDeleteEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<DirectDeleteCombatEvent>());

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            var directDeleteTombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                directDeleteTombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));

            Assert.That(
                directDeleteTombstone.CurrentHp,
                Is.Zero);

            Assert.That(
                directDeleteTombstone
                    .RemovalMetadata.EventId,
                Is.EqualTo(
                    environment.EventLog.Events[1]
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Events,
                Has.None.TypeOf<RescueCombatEvent>());

            Assert.That(
                environment.EventLog.Events,
                Has.None.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        [Test]
        public void Drain_WhenRescuePrecedesDirectDelete_DirectDeleteStillRemovesCard()
        {
            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var rescueHandler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog);

            var directDeleteHandler =
                new TestDirectDeleteTriggerHandler(
                    directDeleteResolver,
                    environment.PlayerFrontPosition);

            source.DiscoverAction =
                (state, combatEvent) =>
                {
                    var deathEvent =
                        combatEvent as DeathCombatEvent;

                    if (deathEvent != null &&
                        rescueHandler.CanTrigger(
                            state,
                            deathEvent) &&
                        directDeleteHandler.CanTrigger(
                            state,
                            deathEvent))
                    {
                        return new[]
                        {
                            CreateCandidate(
                                rescueHandler),
                            CreateCandidate(
                                directDeleteHandler)
                        };
                    }

                    return EmptyCandidates();
                };

            var deathEvent =
                AppendDeathEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    4,
                    2);

            Assert.That(
                processedEventCount,
                Is.EqualTo(3));

            Assert.That(
                rescueHandler.CanRescueCallCount,
                Is.EqualTo(1));

            Assert.That(
                directDeleteHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                rescueHandler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));

            Assert.That(
                directDeleteHandler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));

            Assert.That(
                directDeleteHandler.DirectDeleteEvent,
                Is.Not.Null);

            Assert.That(
                directDeleteHandler
                    .DirectDeleteEvent.HpAtDeletion,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(deathEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<RescueCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    directDeleteHandler
                        .DirectDeleteEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<DirectDeleteCombatEvent>());

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            var rescuedDeleteTombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                rescuedDeleteTombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));

            Assert.That(
                rescuedDeleteTombstone.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                rescuedDeleteTombstone
                    .RemovalMetadata.EventId,
                Is.EqualTo(
                    environment.EventLog.Events[2]
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Events,
                Has.None.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                environment.Engine.HasPendingWork,
                Is.False);
        }

        private static TestEnvironment CreateEnvironment(
            params ICombatTriggerSource[] sources)
        {
            var playerFrontPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerBackPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var enemyFrontPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var enemyBackPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    new BoardColumn(1));

            var playerCard =
                new CombatCardState(
                    new DefinitionId("player-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    7,
                    0,
                    0,
                    3);

            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerFrontPosition,
                    playerBackPosition,
                    new SlotId(1),
                    new SlotId(2),
                    playerCard);

            var enemySide =
                CreateEmptySideState(
                    CombatSide.Enemy,
                    enemyFrontPosition,
                    enemyBackPosition,
                    new SlotId(3),
                    new SlotId(4));

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                PlayerCard = playerCard,
                PlayerFrontPosition =
                    playerFrontPosition,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry =
                    sourceRegistry,
                Engine =
                    new CombatEventResolutionEngine(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatSideState CreateSideState(
            CombatSide side,
            BoardPosition frontPosition,
            BoardPosition backPosition,
            SlotId frontSlotId,
            SlotId backSlotId,
            CombatCardState card)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition,
                            card.InstanceId),
                        new CombatSlotState(
                            backSlotId,
                            backPosition)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSideState
            CreateEmptySideState(
                CombatSide side,
                BoardPosition frontPosition,
                BoardPosition backPosition,
                SlotId frontSlotId,
                SlotId backSlotId)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition),
                        new CombatSlotState(
                            backSlotId,
                            backPosition)
                    }),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static TestCombatEvent AppendTestEvent(
            TestEnvironment environment)
        {
            var combatEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                combatEvent);

            return combatEvent;
        }

        private static DeathCombatEvent AppendDeathEvent(
            TestEnvironment environment)
        {
            var deathEvent =
                new DeathCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    environment.PlayerCard.InstanceId,
                    environment.PlayerFrontPosition,
                    3,
                    environment.PlayerCard.CurrentHp);

            environment.EventLog.Append(
                deathEvent);

            return deathEvent;
        }

        private static CombatTriggerCandidate<
            ICombatTriggerHandler> CreateCandidate(
                ICombatTriggerHandler handler)
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        0,
                        0),
                    handler);
        }

        private static IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            EmptyCandidates()
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>[0];
        }

        private sealed class TestTriggerSource :
            ICombatTriggerSource
        {
            public Func<
                CombatState,
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>>
                DiscoverAction
            {
                get;
                set;
            }

            public int DiscoveryCallCount
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

                if (DiscoverAction == null)
                {
                    return EmptyCandidates();
                }

                return DiscoverAction(
                    state,
                    sourceEvent);
            }
        }

        private sealed class TestTriggerHandler :
            ICombatTriggerHandler
        {
            public Action<CombatState, CombatEvent>
                ResolveAction
            {
                get;
                set;
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
                return true;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;

                if (ResolveAction != null)
                {
                    ResolveAction(
                        state,
                        sourceEvent);
                }
            }
        }

        private sealed class
            TestDirectDeleteTriggerHandler :
            CombatEventTriggerHandler<
                DeathCombatEvent>
        {
            private readonly CombatDirectDeleteResolver
                _directDeleteResolver;

            private readonly BoardPosition
                _targetPosition;

            public TestDirectDeleteTriggerHandler(
                CombatDirectDeleteResolver
                    directDeleteResolver,
                BoardPosition targetPosition)
            {
                if (directDeleteResolver == null)
                {
                    throw new ArgumentNullException(
                        nameof(directDeleteResolver));
                }

                if (!targetPosition.IsValid)
                {
                    throw new ArgumentException(
                        "A valid target position is required.",
                        nameof(targetPosition));
                }

                _directDeleteResolver =
                    directDeleteResolver;

                _targetPosition =
                    targetPosition;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public DeathCombatEvent ReceivedDeathEvent
            {
                get;
                private set;
            }

            public DirectDeleteCombatEvent
                DirectDeleteEvent
            {
                get;
                private set;
            }

            protected override bool CanTriggerTyped(
                CombatState state,
                DeathCombatEvent sourceEvent)
            {
                return true;
            }

            protected override void ResolveTyped(
                CombatState state,
                DeathCombatEvent sourceEvent)
            {
                ResolveCallCount++;

                ReceivedDeathEvent =
                    sourceEvent;

                DirectDeleteEvent =
                    _directDeleteResolver
                        .ApplyDirectDelete(
                            state,
                            sourceEvent,
                            _targetPosition);
            }
        }

        private sealed class
            TestRescueTriggerHandler :
            CombatRescueTriggerHandler
        {
            public TestRescueTriggerHandler(
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
                : base(
                    metadataFactory,
                    eventLog)
            {
            }

            public int CanRescueCallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public DeathCombatEvent ReceivedDeathEvent
            {
                get;
                private set;
            }

            protected override bool CanRescue(
                CombatState state,
                DeathCombatEvent sourceEvent)
            {
                CanRescueCallCount++;
                ReceivedState = state;
                ReceivedDeathEvent = sourceEvent;

                return true;
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

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatCardState PlayerCard
            {
                get;
                set;
            }

            public BoardPosition PlayerFrontPosition
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

            public CombatEventQueue EventQueue
            {
                get;
                set;
            }

            public CombatTriggerSourceRegistry
                SourceRegistry
            {
                get;
                set;
            }

            public CombatEventResolutionEngine Engine
            {
                get;
                set;
            }
        }
    }
}