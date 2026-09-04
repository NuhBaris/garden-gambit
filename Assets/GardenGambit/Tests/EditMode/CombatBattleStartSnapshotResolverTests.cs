using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartSnapshotResolverTests
    {
        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var resolver =
                new CombatBattleStartSnapshotResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(null));
        }

        [Test]
        public void Resolve_WithEmptyBoards_ReturnsEmptySides()
        {
            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            Assert.That(
                snapshot.Player.Count,
                Is.EqualTo(0));

            Assert.That(
                snapshot.Enemy.Count,
                Is.EqualTo(0));

            Assert.That(
                snapshot.TotalCardCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Resolve_CapturesBothSidesAndCardValues()
        {
            var playerCard =
                CreateCard(
                    "player-card",
                    1,
                    rank: 4,
                    hpCapacity: 12,
                    currentHp: 7,
                    armor: 2,
                    attack: 5);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    101,
                    rank: 6,
                    hpCapacity: 15,
                    currentHp: 9,
                    armor: 3,
                    attack: 8);

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

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            var playerSnapshot =
                snapshot.Player.GetCard(
                    playerCard.InstanceId);

            var enemySnapshot =
                snapshot.Enemy.GetCard(
                    enemyCard.InstanceId);

            Assert.That(
                playerSnapshot.Position,
                Is.EqualTo(
                    playerPosition));

            Assert.That(
                playerSnapshot.Rank,
                Is.EqualTo(
                    playerCard.Rank));

            Assert.That(
                playerSnapshot.HpCapacity,
                Is.EqualTo(12));

            Assert.That(
                playerSnapshot.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                playerSnapshot.Armor,
                Is.EqualTo(2));

            Assert.That(
                playerSnapshot.Attack,
                Is.EqualTo(5));

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

            Assert.That(
                snapshot.TotalCardCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_OrdersByColumnThenFrontBeforeBack()
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

            var columnTwoBack =
                CreateCard(
                    "column-2-back",
                    4);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        columnOneFront,
                        columnOneBack,
                        columnTwoFront,
                        columnTwoBack
                    },
                    CreateSlot(
                        4,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            2),
                        columnTwoBack.InstanceId),
                    CreateSlot(
                        2,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        columnOneBack.InstanceId),
                    CreateSlot(
                        3,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            2),
                        columnTwoFront.InstanceId),
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

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            Assert.That(
                snapshot.Player.Cards[0].InstanceId,
                Is.EqualTo(
                    columnOneFront.InstanceId));

            Assert.That(
                snapshot.Player.Cards[1].InstanceId,
                Is.EqualTo(
                    columnOneBack.InstanceId));

            Assert.That(
                snapshot.Player.Cards[2].InstanceId,
                Is.EqualTo(
                    columnTwoFront.InstanceId));

            Assert.That(
                snapshot.Player.Cards[3].InstanceId,
                Is.EqualTo(
                    columnTwoBack.InstanceId));
        }

        [Test]
        public void Resolve_SkipsEmptySlots()
        {
            var card =
                CreateCard(
                    "occupied-card",
                    1);

            var occupiedPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var emptyPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
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
                        occupiedPosition,
                        card.InstanceId),
                    CreateSlot(
                        2,
                        emptyPosition));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            Assert.That(
                snapshot.Player.Count,
                Is.EqualTo(1));

            Assert.That(
                snapshot.Player.Cards[0].Position,
                Is.EqualTo(
                    occupiedPosition));

            Assert.Throws<KeyNotFoundException>(
                () => snapshot.Player.GetCardAt(
                    emptyPosition));
        }

        [Test]
        public void Resolve_ExcludesRegisteredCardNotOnBoard()
        {
            var placedCard =
                CreateCard(
                    "placed-card",
                    1);

            var unplacedCard =
                CreateCard(
                    "unplaced-card",
                    2);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        placedCard,
                        unplacedCard
                    },
                    CreateSlot(
                        1,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        placedCard.InstanceId));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            Assert.That(
                snapshot.Player.Count,
                Is.EqualTo(1));

            Assert.That(
                snapshot.Player.Cards[0].InstanceId,
                Is.EqualTo(
                    placedCard.InstanceId));

            Assert.Throws<KeyNotFoundException>(
                () => snapshot.Player.GetCard(
                    unplacedCard.InstanceId));
        }

        [Test]
        public void Resolve_WithZeroHpCard_CapturesDeathState()
        {
            var card =
                CreateCard(
                    "zero-hp-card",
                    1,
                    currentHp: 0);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        card
                    },
                    CreateSlot(
                        1,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        card.InstanceId));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            var cardSnapshot =
                snapshot.Player.Cards[0];

            Assert.That(
                cardSnapshot.CurrentHp,
                Is.EqualTo(0));

            Assert.That(
                cardSnapshot.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                cardSnapshot.WasAlive,
                Is.False);
        }

        [Test]
        public void Resolve_AfterCardHpChanges_SnapshotRemainsUnchanged()
        {
            var card =
                CreateCard(
                    "mutable-card",
                    1,
                    currentHp: 5);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        card
                    },
                    CreateSlot(
                        1,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        card.InstanceId));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            card.SetCurrentHpToZero();

            var cardSnapshot =
                snapshot.Player.Cards[0];

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(0));

            Assert.That(
                cardSnapshot.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                cardSnapshot.WasAlive,
                Is.True);

            Assert.That(
                cardSnapshot.WasAtDeathThreshold,
                Is.False);
        }

        [Test]
        public void Resolve_AfterCardRemoval_SnapshotKeepsCard()
        {
            var card =
                CreateCard(
                    "removed-card",
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

            var resolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                resolver.Resolve(state);

            var removedCard =
                player.RemoveCardFromCombat(
                    position);

            Assert.That(
                removedCard,
                Is.SameAs(card));

            Assert.That(
                player.Board.GetSlot(
                        position)
                    .IsOccupied,
                Is.False);

            Assert.That(
                snapshot.Player.Count,
                Is.EqualTo(1));

            Assert.That(
                snapshot.Player.GetCard(
                    card.InstanceId)
                    .Position,
                Is.EqualTo(position));
        }

        [Test]
        public void Resolve_DoesNotMutateLiveState()
        {
            var card =
                CreateCard(
                    "live-card",
                    1);

            var position =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var slot =
                CreateSlot(
                    1,
                    position,
                    card.InstanceId);

            var player =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        card
                    },
                    slot);

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new CombatBattleStartSnapshotResolver();

            resolver.Resolve(state);

            Assert.That(
                slot.IsOccupied,
                Is.True);

            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(
                    card.InstanceId));

            Assert.That(
                player.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                player.GetCardAt(position),
                Is.SameAs(card));
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
                int hpCapacity = 10,
                int currentHp = 5,
                int armor = 1,
                int attack = 3)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                hpCapacity,
                currentHp,
                armor,
                attack);
        }
    }
}