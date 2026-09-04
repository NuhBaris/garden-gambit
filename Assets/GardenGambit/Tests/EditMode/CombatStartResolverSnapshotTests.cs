using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatStartResolverSnapshotTests
    {
        [Test]
        public void Start_CreatesEventWithBattleStartSnapshot()
        {
            var state =
                CreateEmptyState();

            var eventLog =
                new CombatEventLog();

            var resolver =
                CreateResolver(
                    eventLog);

            var combatStartedEvent =
                resolver.Start(state);

            Assert.That(
                combatStartedEvent
                    .HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                combatStartedEvent
                    .BattleStartSnapshot,
                Is.Not.Null);

            Assert.That(
                combatStartedEvent
                    .BattleStartSnapshot
                    .TotalCardCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Start_CapturesPlayerAndEnemyCards()
        {
            var playerCard =
                CreateCard(
                    "player-card",
                    1,
                    rank: 4,
                    currentHp: 7);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    101,
                    rank: 6,
                    currentHp: 9);

            var playerPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    2);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    4);

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new[]
                        {
                            playerCard
                        },
                        CreateSlot(
                            1,
                            playerPosition,
                            playerCard.InstanceId)),
                    CreateSide(
                        CombatSide.Enemy,
                        new[]
                        {
                            enemyCard
                        },
                        CreateSlot(
                            101,
                            enemyPosition,
                            enemyCard.InstanceId)));

            var eventLog =
                new CombatEventLog();

            var resolver =
                CreateResolver(
                    eventLog);

            var combatStartedEvent =
                resolver.Start(state);

            var snapshot =
                combatStartedEvent
                    .BattleStartSnapshot;

            var playerSnapshot =
                snapshot.Player.GetCard(
                    playerCard.InstanceId);

            var enemySnapshot =
                snapshot.Enemy.GetCard(
                    enemyCard.InstanceId);

            Assert.That(
                snapshot.TotalCardCount,
                Is.EqualTo(2));

            Assert.That(
                playerSnapshot.Position,
                Is.EqualTo(
                    playerPosition));

            Assert.That(
                playerSnapshot.Rank,
                Is.EqualTo(
                    playerCard.Rank));

            Assert.That(
                playerSnapshot.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                enemySnapshot.Position,
                Is.EqualTo(
                    enemyPosition));

            Assert.That(
                enemySnapshot.Rank,
                Is.EqualTo(
                    enemyCard.Rank));

            Assert.That(
                enemySnapshot.CurrentHp,
                Is.EqualTo(9));
        }

        [Test]
        public void Start_AppendsExactSnapshotEventToLog()
        {
            var state =
                CreateEmptyState();

            var eventLog =
                new CombatEventLog();

            var resolver =
                CreateResolver(
                    eventLog);

            var combatStartedEvent =
                resolver.Start(state);

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(
                    combatStartedEvent));

            Assert.That(
                eventLog.GetEvent(
                    combatStartedEvent
                        .Metadata.EventId),
                Is.SameAs(
                    combatStartedEvent));

            Assert.That(
                ((CombatStartedCombatEvent)
                    eventLog.Events[0])
                    .BattleStartSnapshot,
                Is.SameAs(
                    combatStartedEvent
                        .BattleStartSnapshot));
        }

        [Test]
        public void Start_AfterCardHpChanges_SnapshotRemainsUnchanged()
        {
            var card =
                CreateCard(
                    "player-card",
                    1,
                    currentHp: 5);

            var position =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        card
                    },
                    CreateSlot(
                        1,
                        position,
                        card.InstanceId));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var eventLog =
                new CombatEventLog();

            var resolver =
                CreateResolver(
                    eventLog);

            var combatStartedEvent =
                resolver.Start(state);

            card.SetCurrentHpToZero();

            var cardSnapshot =
                combatStartedEvent
                    .BattleStartSnapshot
                    .Player
                    .GetCard(
                        card.InstanceId);

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(0));

            Assert.That(
                cardSnapshot.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                cardSnapshot.WasAlive,
                Is.True);
        }

        [Test]
        public void Start_AfterCardRemoval_SnapshotKeepsCard()
        {
            var card =
                CreateCard(
                    "player-card",
                    1);

            var position =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        card
                    },
                    CreateSlot(
                        1,
                        position,
                        card.InstanceId));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var eventLog =
                new CombatEventLog();

            var resolver =
                CreateResolver(
                    eventLog);

            var combatStartedEvent =
                resolver.Start(state);

            var removedCard =
                player.RemoveCardFromCombat(
                    position);

            var cardSnapshot =
                combatStartedEvent
                    .BattleStartSnapshot
                    .Player
                    .GetCard(
                        card.InstanceId);

            Assert.That(
                removedCard,
                Is.SameAs(card));

            Assert.That(
                player.Board.GetSlot(
                        position)
                    .IsOccupied,
                Is.False);

            Assert.That(
                cardSnapshot.InstanceId,
                Is.EqualTo(
                    card.InstanceId));

            Assert.That(
                cardSnapshot.Position,
                Is.EqualTo(position));
        }

        [Test]
        public void Start_UsesDeterministicSnapshotCardOrder()
        {
            var columnOneFront =
                CreateCard(
                    "column-1-front",
                    1);

            var columnOneBack =
                CreateCard(
                    "column-1-back",
                    2);

            var columnTwoFront =
                CreateCard(
                    "column-2-front",
                    3);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        columnOneFront,
                        columnOneBack,
                        columnTwoFront
                    },
                    CreateSlot(
                        3,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            2),
                        columnTwoFront.InstanceId),
                    CreateSlot(
                        2,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        columnOneBack.InstanceId),
                    CreateSlot(
                        1,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        columnOneFront.InstanceId));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var eventLog =
                new CombatEventLog();

            var resolver =
                CreateResolver(
                    eventLog);

            var combatStartedEvent =
                resolver.Start(state);

            var cards =
                combatStartedEvent
                    .BattleStartSnapshot
                    .Player
                    .Cards;

            Assert.That(
                cards[0].InstanceId,
                Is.EqualTo(
                    columnOneFront.InstanceId));

            Assert.That(
                cards[1].InstanceId,
                Is.EqualTo(
                    columnOneBack.InstanceId));

            Assert.That(
                cards[2].InstanceId,
                Is.EqualTo(
                    columnTwoFront.InstanceId));
        }

        private static CombatStartResolver
            CreateResolver(
                CombatEventLog eventLog)
        {
            return new CombatStartResolver(
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator()),
                eventLog);
        }

        private static CombatState
            CreateEmptyState()
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return CreateSide(
                side,
                new CombatCardState[0]);
        }

        private static CombatSideState
            CreateSide(
                CombatSide side,
                CombatCardState[] cards,
                params CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSlotState
            CreateSlot(
                int slotId,
                BoardPosition position,
                InstanceId? occupantInstanceId = null)
        {
            return new CombatSlotState(
                new SlotId(slotId),
                position,
                occupantInstanceId);
        }

        private static BoardPosition
            CreatePosition(
                CombatSide side,
                BoardRow row,
                int column)
        {
            return new BoardPosition(
                side,
                row,
                new BoardColumn(column));
        }

        private static CombatCardState
            CreateCard(
                string definitionId,
                long instanceId,
                int rank = 2,
                int currentHp = 5)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                hpCapacity: 10,
                currentHp: currentHp,
                armor: 1,
                attack: 3);
        }
    }
}