using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDeathChainCompletionResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathChainCompletionResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathChainCompletionResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void CompleteDeathChain_WithDeadFront_RemovesAndAdvancesBackCard()
        {
            var environment =
                CreateEnvironment();

            var removalEvent =
                environment.Resolver.CompleteDeathChain(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                removalEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0].Kind,
                Is.EqualTo(
                    CombatEventKind.Death));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

            Assert.That(
                environment.EventLog.Events[2].Kind,
                Is.EqualTo(
                    CombatEventKind.CardAdvanced));

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
                playerSide.Cards.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void CompleteDeathChain_WithNoBackCard_RemovesWithoutAdvancement()
        {
            var environment =
                CreateEnvironment(
                    hasRemainingCard: false);

            var removalEvent =
                environment.Resolver.CompleteDeathChain(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

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
        }

        [Test]
        public void CompleteDeathChain_WhenCardWasRescued_DoesNotRemoveOrAdvance()
        {
            var environment =
                CreateEnvironment();

            var rescueResolver =
                new CombatRescueResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            rescueResolver.ApplyRescue(
                environment.State,
                environment.DeathEvent);

            var removalEvent =
                environment.Resolver.CompleteDeathChain(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.DeadCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.Rescue));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FrontPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.DeadCard.InstanceId));

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.BackPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.RemainingCard.InstanceId));
        }

        [Test]
        public void CompleteDeathChain_WhenDeadFrontWasDirectDeleted_AdvancesBackCard()
        {
            var environment =
                CreateEnvironment();

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            directDeleteResolver.ApplyDirectDelete(
                environment.State,
                environment.DeathEvent,
                environment.FrontPosition);

            var removalEvent =
                environment.Resolver.CompleteDeathChain(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                environment.EventLog.Events[2].Kind,
                Is.EqualTo(
                    CombatEventKind.CardAdvanced));

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
                playerSide.Cards.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void CompleteDeathChain_WhenDifferentCardWasDirectDeleted_StillRemovesAndAdvancesDeadCard()
        {
            var environment =
                CreateEnvironment();

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            directDeleteResolver.ApplyDirectDelete(
                environment.State,
                environment.DeathEvent,
                enemyPosition);

            var removalEvent =
                environment.Resolver.CompleteDeathChain(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                environment.EventLog.Events[2].Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

            Assert.That(
                environment.EventLog.Events[3].Kind,
                Is.EqualTo(
                    CombatEventKind.CardAdvanced));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);
        }

        [Test]
        public void CompleteDeathChain_WithDeadBack_RemovesWithoutAdvancement()
        {
            var environment =
                CreateEnvironment(
                    deathRow: BoardRow.Back);

            var removalEvent =
                environment.Resolver.CompleteDeathChain(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

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
                playerSide.Cards.Count,
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                BoardRow deathRow = BoardRow.Front,
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

            var deadCard =
                CreateCard(
                    "card.dead",
                    100,
                    0);

            var remainingCard =
                hasRemainingCard
                    ? CreateCard(
                        "card.remaining",
                        200,
                        5)
                    : null;

            CombatCardState frontCard;
            CombatCardState backCard;

            if (deathRow == BoardRow.Front)
            {
                frontCard = deadCard;
                backCard = remainingCard;
            }
            else
            {
                frontCard = remainingCard;
                backCard = deadCard;
            }

            var playerCards =
                new List<CombatCardState>
                {
                    deadCard
                };

            if (remainingCard != null)
            {
                playerCards.Add(
                    remainingCard);
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

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var enemyBackPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    new BoardColumn(1));

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    300,
                    5);

            var enemySide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Enemy,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(3),
                                enemyPosition,
                                enemyCard.InstanceId),
                            new CombatSlotState(
                                new SlotId(4),
                                enemyBackPosition)
                        }),
                    new CombatCardRegistry(
                        new[] { enemyCard }),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var deathPosition =
                deathRow == BoardRow.Front
                    ? frontPosition
                    : backPosition;

            var deathEvent =
                new DeathCombatEvent(
                    metadataFactory.CreateRoot(),
                    deadCard.InstanceId,
                    deathPosition,
                    3,
                    0);

            eventLog.Append(
                deathEvent);

            return new TestEnvironment
            {
                State = state,
                DeadCard = deadCard,
                RemainingCard = remainingCard,
                FrontPosition = frontPosition,
                BackPosition = backPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                DeathEvent = deathEvent,
                Resolver =
                    new CombatDeathChainCompletionResolver(
                        metadataFactory,
                        eventLog)
            };
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

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public CombatCardState DeadCard { get; set; }

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

            public DeathCombatEvent DeathEvent { get; set; }

            public CombatDeathChainCompletionResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}