using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardAdvancementResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardAdvancementResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardAdvancementResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void TryAdvance_WithEmptyFrontAndOccupiedBack_MovesCardAndLogsEvent()
        {
            var environment =
                CreateEnvironment();

            var advancedEvent =
                environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    new BoardColumn(1));

            Assert.That(
                advancedEvent,
                Is.Not.Null);

            Assert.That(
                advancedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CardAdvanced));

            Assert.That(
                advancedEvent.InstanceId,
                Is.EqualTo(
                    environment.BackCard.InstanceId));

            Assert.That(
                advancedEvent.SourcePosition,
                Is.EqualTo(
                    environment.BackPosition));

            Assert.That(
                advancedEvent.DestinationPosition,
                Is.EqualTo(
                    environment.FrontPosition));

            Assert.That(
                advancedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                advancedEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.TriggerRootId));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FrontPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.BackCard.InstanceId));

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                playerSide.Cards.Cards[0],
                Is.SameAs(environment.BackCard));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(advancedEvent));
        }

        [Test]
        public void TryAdvance_WhenFrontIsOccupied_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    frontOccupied: true);

            var advancedEvent =
                environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    new BoardColumn(1));

            Assert.That(
                advancedEvent,
                Is.Null);

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FrontPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.FrontCard.InstanceId));

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.BackCard.InstanceId));

            Assert.That(
                playerSide.Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WhenBackIsEmpty_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    backOccupied: false);

            var advancedEvent =
                environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    new BoardColumn(1));

            Assert.That(
                advancedEvent,
                Is.Null);

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WhenBackCardIsAtDeathThreshold_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    backCardHp: 0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    new BoardColumn(1)));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.BackCard.InstanceId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WithInvalidSide_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    default(CombatSide),
                    new BoardColumn(1)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WithInvalidColumn_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    default(BoardColumn)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WithUnloggedParent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var unloggedParent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.TryAdvance(
                    environment.State,
                    unloggedParent,
                    CombatSide.Player,
                    new BoardColumn(1)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.BackPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WithDifferentParentObjectUsingLoggedId_Throws()
        {
            var environment =
                CreateEnvironment();

            var differentParent =
                new TestCombatEvent(
                    environment.ParentEvent.Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.TryAdvance(
                    environment.State,
                    differentParent,
                    CombatSide.Player,
                    new BoardColumn(1)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.BackPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvance_WhenAlreadyAdvancedForParent_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment();

            var firstAdvancedEvent =
                environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    new BoardColumn(1));

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.TryAdvance(
                    environment.State,
                    environment.ParentEvent,
                    CombatSide.Player,
                    new BoardColumn(1)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstAdvancedEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool frontOccupied = false,
                bool backOccupied = true,
                int backCardHp = 5)
        {
            var frontPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var backPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var frontCard =
                frontOccupied
                    ? CreateCard(
                        "card.front",
                        100,
                        5)
                    : null;

            var backCard =
                backOccupied
                    ? CreateCard(
                        "card.back",
                        200,
                        backCardHp)
                    : null;

            var playerCards =
                new List<CombatCardState>();

            if (frontCard != null)
            {
                playerCards.Add(
                    frontCard);
            }

            if (backCard != null)
            {
                playerCards.Add(
                    backCard);
            }

            InstanceId? frontOccupant = null;
            InstanceId? backOccupant = null;

            if (frontCard != null)
            {
                frontOccupant =
                    frontCard.InstanceId;
            }

            if (backCard != null)
            {
                backOccupant =
                    backCard.InstanceId;
            }

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                frontPosition,
                                frontOccupant),
                            new CombatSlotState(
                                new SlotId(2),
                                backPosition,
                                backOccupant)
                        }),
                    new CombatCardRegistry(
                        playerCards),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var enemySide =
                CreateEmptyEnemySide();

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var parentEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                parentEvent);

            return new TestEnvironment
            {
                State = state,
                FrontCard = frontCard,
                BackCard = backCard,
                FrontPosition = frontPosition,
                BackPosition = backPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                ParentEvent = parentEvent,
                Resolver =
                    new CombatCardAdvancementResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatSideState
            CreateEmptyEnemySide()
        {
            var frontPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var backPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    new BoardColumn(1));

            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Enemy,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(3),
                            frontPosition),
                        new CombatSlotState(
                            new SlotId(4),
                            backPosition)
                    }),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                0,
                3);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
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
            public CombatState State { get; set; }

            public CombatCardState FrontCard { get; set; }

            public CombatCardState BackCard { get; set; }

            public BoardPosition FrontPosition { get; set; }

            public BoardPosition BackPosition { get; set; }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog { get; set; }

            public CombatEvent ParentEvent { get; set; }

            public CombatCardAdvancementResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}