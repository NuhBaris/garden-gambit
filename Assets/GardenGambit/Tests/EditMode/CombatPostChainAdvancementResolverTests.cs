using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPostChainAdvancementResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPostChainAdvancementResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPostChainAdvancementResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void TryAdvanceAfterChain_WithFrontDeathRemoval_AdvancesBackCard()
        {
            var environment =
                CreateEnvironment();

            var advancedEvent =
                environment.Resolver
                    .TryAdvanceAfterChain(
                        environment.State,
                        environment.RemovalEvent);

            Assert.That(
                advancedEvent,
                Is.Not.Null);

            Assert.That(
                advancedEvent.InstanceId,
                Is.EqualTo(
                    environment.RemainingCard.InstanceId));

            Assert.That(
                advancedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.RemovalEvent
                        .Metadata.EventId));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FrontPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.RemainingCard.InstanceId));

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(advancedEvent));
        }

        [Test]
        public void TryAdvanceAfterChain_WithFrontDirectDelete_AdvancesBackCard()
        {
            var environment =
                CreateEnvironment(
                    useDirectDelete: true);

            var advancedEvent =
                environment.Resolver
                    .TryAdvanceAfterChain(
                        environment.State,
                        environment.RemovalEvent);

            Assert.That(
                environment.RemovalEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                advancedEvent,
                Is.Not.Null);

            Assert.That(
                advancedEvent.InstanceId,
                Is.EqualTo(
                    environment.RemainingCard.InstanceId));

            Assert.That(
                advancedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.RemovalEvent
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void TryAdvanceAfterChain_WithBackRemoval_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    removalRow: BoardRow.Back);

            var advancedEvent =
                environment.Resolver
                    .TryAdvanceAfterChain(
                        environment.State,
                        environment.RemovalEvent);

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
                    environment.RemainingCard.InstanceId));

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvanceAfterChain_WithEmptyBack_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    hasRemainingCard: false);

            var advancedEvent =
                environment.Resolver
                    .TryAdvanceAfterChain(
                        environment.State,
                        environment.RemovalEvent);

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
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdvanceAfterChain_WithUnsupportedEvent_ThrowsWithoutChangingBoard()
        {
            var environment =
                CreateEnvironment();

            var unsupportedEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                unsupportedEvent);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryAdvanceAfterChain(
                        environment.State,
                        unsupportedEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.BackPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.RemainingCard.InstanceId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void TryAdvanceAfterChain_WithUnloggedRemovalEvent_ThrowsWithoutChangingBoard()
        {
            var environment =
                CreateEnvironment();

            var unloggedRemovalEvent =
                new DeathRemovalCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    new InstanceId(999),
                    environment.FrontPosition,
                    0);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryAdvanceAfterChain(
                        environment.State,
                        unloggedRemovalEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.BackPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.RemainingCard.InstanceId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool useDirectDelete = false,
                BoardRow removalRow = BoardRow.Front,
                bool hasRemainingCard = true)
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

            var remainingCard =
                hasRemainingCard
                    ? CreateCard(
                        "card.remaining",
                        200)
                    : null;

            InstanceId? frontOccupant = null;
            InstanceId? backOccupant = null;

            if (remainingCard != null)
            {
                if (removalRow == BoardRow.Front)
                {
                    backOccupant =
                        remainingCard.InstanceId;
                }
                else
                {
                    frontOccupant =
                        remainingCard.InstanceId;
                }
            }

            var playerCards =
                remainingCard == null
                    ? Array.Empty<CombatCardState>()
                    : new[] { remainingCard };

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

            var vacatedPosition =
                removalRow == BoardRow.Front
                    ? frontPosition
                    : backPosition;

            CombatEvent removalEvent;

            if (useDirectDelete)
            {
                removalEvent =
                    new DirectDeleteCombatEvent(
                        metadataFactory.CreateRoot(),
                        new InstanceId(100),
                        vacatedPosition,
                        5);
            }
            else
            {
                removalEvent =
                    new DeathRemovalCombatEvent(
                        metadataFactory.CreateRoot(),
                        new InstanceId(100),
                        vacatedPosition,
                        0);
            }

            eventLog.Append(
                removalEvent);

            return new TestEnvironment
            {
                State = state,
                RemainingCard = remainingCard,
                FrontPosition = frontPosition,
                BackPosition = backPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                RemovalEvent = removalEvent,
                Resolver =
                    new CombatPostChainAdvancementResolver(
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
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                5,
                5,
                0,
                1);
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

            public CombatCardState RemainingCard { get; set; }

            public BoardPosition FrontPosition { get; set; }

            public BoardPosition BackPosition { get; set; }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog { get; set; }

            public CombatEvent RemovalEvent { get; set; }

            public CombatPostChainAdvancementResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}