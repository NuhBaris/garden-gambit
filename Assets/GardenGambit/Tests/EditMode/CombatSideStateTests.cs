using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSideStateTests
    {
        [Test]
        public void Constructor_WithValidDependencies_SetsProperties()
        {
            var board =
                CreateEmptyBoard(CombatSide.Player);

            var cards = new CombatCardRegistry(
                new CombatCardState[0]);

            var battleHealth =
                new BattleHealth(20);

            var attackMultiplier =
                new AttackMultiplier(1);

            var state = new CombatSideState(
                board,
                cards,
                battleHealth,
                attackMultiplier);

            Assert.That(
                state.Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(state.Board, Is.SameAs(board));
            Assert.That(state.Cards, Is.SameAs(cards));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(battleHealth));

            Assert.That(
                state.AttackMultiplier,
                Is.EqualTo(attackMultiplier));
        }

        [Test]
        public void Constructor_WithEnemyBoard_ReportsEnemySide()
        {
            var state = new CombatSideState(
                CreateEmptyBoard(CombatSide.Enemy),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(20),
                new AttackMultiplier(1));

            Assert.That(
                state.Side,
                Is.EqualTo(CombatSide.Enemy));
        }

        [Test]
        public void Constructor_WithNullBoard_Throws()
        {
            var cards = new CombatCardRegistry(
                new CombatCardState[0]);

            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new CombatSideState(
                    null,
                    cards,
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            });
        }

        [Test]
        public void Constructor_WithNullCardRegistry_Throws()
        {
            var board =
                CreateEmptyBoard(CombatSide.Player);

            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new CombatSideState(
                    board,
                    null,
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            });
        }

        [Test]
        public void Constructor_WithInvalidAttackMultiplier_Throws()
        {
            var board =
                CreateEmptyBoard(CombatSide.Player);

            var cards = new CombatCardRegistry(
                new CombatCardState[0]);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatSideState(
                    board,
                    cards,
                    new BattleHealth(20),
                    default(AttackMultiplier));
            });
        }

        [Test]
        public void Constructor_WhenBoardOccupantIsMissingFromRegistry_Throws()
        {
            var occupant = new InstanceId(100);

            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1,
                occupant);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var cards = new CombatCardRegistry(
                new CombatCardState[0]);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatSideState(
                    board,
                    cards,
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            });
        }

        [Test]
        public void Constructor_WhenBoardOccupantExistsInRegistry_CreatesState()
        {
            var card = CreateCard(100);

            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1,
                card.InstanceId);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var cards = new CombatCardRegistry(
                new[] { card });

            var state = new CombatSideState(
                board,
                cards,
                new BattleHealth(20),
                new AttackMultiplier(1));

            Assert.That(
                state.Board.GetSlotContaining(
                    card.InstanceId),
                Is.SameAs(slot));

            Assert.That(
                state.Cards.GetCard(card.InstanceId),
                Is.SameAs(card));
        }

        [Test]
        public void Constructor_WithUnplacedRegisteredCard_AllowsState()
        {
            var card = CreateCard(100);

            var state = new CombatSideState(
                CreateEmptyBoard(CombatSide.Player),
                new CombatCardRegistry(
                    new[] { card }),
                new BattleHealth(20),
                new AttackMultiplier(1));

            Assert.That(state.Cards.Count, Is.EqualTo(1));
            Assert.That(state.Board.SlotCount, Is.Zero);
        }

        private static CombatSideState CreateSideState(
            int battleHealth,
            int attackMultiplier =
                AttackMultiplier.BaseValue)
        {
            return new CombatSideState(
                CreateEmptyBoard(CombatSide.Player),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(battleHealth),
                new AttackMultiplier(attackMultiplier));
        }

        private static CombatSideState CreateSideState(
            CombatCardState[] cards,
            CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    slots),
                new CombatCardRegistry(cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSlotState CreateEmptySlot(
            long slotId,
            BoardRow row,
            int column)
        {
            return new CombatSlotState(
                new SlotId(slotId),
                new BoardPosition(
                    CombatSide.Player,
                    row,
                    new BoardColumn(column)));
        }

        private static CombatBoardState CreateEmptyBoard(
            CombatSide side)
        {
            return new CombatBoardState(
                side,
                new CombatSlotState[0]);
        }

        private static CombatSlotState CreateSlot(
            long slotId,
            CombatSide side,
            BoardRow row,
            int column,
            InstanceId occupantInstanceId)
        {
            return new CombatSlotState(
                new SlotId(slotId),
                new BoardPosition(
                    side,
                    row,
                    new BoardColumn(column)),
                occupantInstanceId);
        }

        [Test]
        public void ApplyBattleHealthDamage_UpdatesAndReturnsBattleHealth()
        {
            var state = CreateSideState(20);

            var result =
                state.ApplyBattleHealthDamage(25);

            Assert.That(
                result,
                Is.EqualTo(new BattleHealth(-5)));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(new BattleHealth(-5)));
        }

        [Test]
        public void ApplyBattleHealthDamage_WithNegativeAmount_ThrowsWithoutChangingState()
        {
            var state = CreateSideState(20);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.ApplyBattleHealthDamage(-1));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(new BattleHealth(20)));
        }

        [Test]
        public void ApplyBattleHealthDamage_WhenValueWouldUnderflow_ThrowsWithoutChangingState()
        {
            var state =
                CreateSideState(int.MinValue);

            Assert.Throws<OverflowException>(
                () => state.ApplyBattleHealthDamage(1));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(
                    new BattleHealth(int.MinValue)));
        }

        [Test]
        public void ApplyBattleHealthGain_UpdatesAndReturnsBattleHealth()
        {
            var state = CreateSideState(-5);

            var result =
                state.ApplyBattleHealthGain(10);

            Assert.That(
                result,
                Is.EqualTo(new BattleHealth(5)));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(new BattleHealth(5)));
        }

        [Test]
        public void ApplyBattleHealthGain_WithNegativeAmount_ThrowsWithoutChangingState()
        {
            var state = CreateSideState(20);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.ApplyBattleHealthGain(-1));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(new BattleHealth(20)));
        }

        [Test]
        public void ApplyBattleHealthGain_WhenValueWouldOverflow_ThrowsWithoutChangingState()
        {
            var state =
                CreateSideState(int.MaxValue);

            Assert.Throws<OverflowException>(
                () => state.ApplyBattleHealthGain(1));

            Assert.That(
                state.BattleHealth,
                Is.EqualTo(
                    new BattleHealth(int.MaxValue)));
        }

        [Test]
        public void ApplyAttackMultiplierGain_WithPositiveAmount_UpdatesAndReturnsMultiplier()
        {
            var state = CreateSideState(20);

            var result =
                state.ApplyAttackMultiplierGain(2);

            Assert.That(
                result,
                Is.EqualTo(new AttackMultiplier(3)));

            Assert.That(
                state.AttackMultiplier,
                Is.EqualTo(new AttackMultiplier(3)));
        }

        [Test]
        public void ApplyAttackMultiplierGain_WithZeroAmount_DoesNotChangeMultiplier()
        {
            var state = CreateSideState(20);

            var result =
                state.ApplyAttackMultiplierGain(0);

            Assert.That(
                result,
                Is.EqualTo(
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue)));

            Assert.That(
                state.AttackMultiplier,
                Is.EqualTo(result));
        }

        [Test]
        public void ApplyAttackMultiplierGain_WithNegativeAmount_ThrowsWithoutChangingState()
        {
            var state = CreateSideState(20);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.ApplyAttackMultiplierGain(-1));

            Assert.That(
                state.AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue)));
        }

        [Test]
        public void ApplyAttackMultiplierGain_WhenValueWouldOverflow_ThrowsWithoutChangingState()
        {
            var state = CreateSideState(
                20,
                int.MaxValue);

            Assert.Throws<OverflowException>(
                () => state.ApplyAttackMultiplierGain(1));

            Assert.That(
                state.AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(int.MaxValue)));
        }

        [Test]
        public void GetCardAt_WithOccupiedSlot_ReturnsRegisteredCard()
        {
            var card = CreateCard(100);

            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1,
                card.InstanceId);

            var state = CreateSideState(
                new[] { card },
                new[] { slot });

            var result =
                state.GetCardAt(slot.Position);

            Assert.That(result, Is.SameAs(card));
        }

        [Test]
        public void GetCardAt_WithEmptySlot_Throws()
        {
            var slot = CreateEmptySlot(
                1,
                BoardRow.Front,
                1);

            var state = CreateSideState(
                new CombatCardState[0],
                new[] { slot });

            Assert.Throws<InvalidOperationException>(
                () => state.GetCardAt(
                    slot.Position));
        }

        [Test]
        public void PlaceCard_WithRegisteredCard_OccupiesSlotAndReturnsCard()
        {
            var card = CreateCard(100);

            var slot = CreateEmptySlot(
                1,
                BoardRow.Front,
                1);

            var state = CreateSideState(
                new[] { card },
                new[] { slot });

            var result = state.PlaceCard(
                slot.Position,
                card.InstanceId);

            Assert.That(result, Is.SameAs(card));
            Assert.That(slot.IsOccupied, Is.True);

            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(card.InstanceId));
        }

        [Test]
        public void PlaceCard_WithUnregisteredCard_ThrowsWithoutChangingBoard()
        {
            var slot = CreateEmptySlot(
                1,
                BoardRow.Front,
                1);

            var state = CreateSideState(
                new CombatCardState[0],
                new[] { slot });

            Assert.Throws<KeyNotFoundException>(
                () => state.PlaceCard(
                    slot.Position,
                    new InstanceId(999)));

            Assert.That(slot.IsOccupied, Is.False);
        }

        [Test]
        public void MoveCard_ToEmptySlot_MovesAndReturnsCard()
        {
            var card = CreateCard(100);

            var sourceSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1,
                card.InstanceId);

            var destinationSlot = CreateEmptySlot(
                2,
                BoardRow.Front,
                2);

            var state = CreateSideState(
                new[] { card },
                new[] { sourceSlot, destinationSlot });

            var result = state.MoveCard(
                sourceSlot.Position,
                destinationSlot.Position);

            Assert.That(result, Is.SameAs(card));
            Assert.That(sourceSlot.IsOccupied, Is.False);
            Assert.That(destinationSlot.IsOccupied, Is.True);

            Assert.That(
                destinationSlot.OccupantInstanceId.Value,
                Is.EqualTo(card.InstanceId));
        }

        [Test]
        public void MoveCard_ToOccupiedSlot_ThrowsWithoutChangingBoard()
        {
            var sourceCard = CreateCard(100);
            var destinationCard = CreateCard(101);

            var sourceSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1,
                sourceCard.InstanceId);

            var destinationSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Front,
                2,
                destinationCard.InstanceId);

            var state = CreateSideState(
                new[] { sourceCard, destinationCard },
                new[] { sourceSlot, destinationSlot });

            Assert.Throws<InvalidOperationException>(
                () => state.MoveCard(
                    sourceSlot.Position,
                    destinationSlot.Position));

            Assert.That(
                sourceSlot.OccupantInstanceId.Value,
                Is.EqualTo(sourceCard.InstanceId));

            Assert.That(
                destinationSlot.OccupantInstanceId.Value,
                Is.EqualTo(destinationCard.InstanceId));
        }

        [Test]
        public void RemoveCard_FromOccupiedSlot_RemovesAndReturnsCard()
        {
            var card = CreateCard(100);

            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1,
                card.InstanceId);

            var state = CreateSideState(
                new[] { card },
                new[] { slot });

            var result =
                state.RemoveCard(slot.Position);

            Assert.That(result, Is.SameAs(card));
            Assert.That(slot.IsOccupied, Is.False);

            Assert.That(
                state.Cards.GetCard(card.InstanceId),
                Is.SameAs(card));
        }

        [Test]
        public void RemoveCard_FromEmptySlot_ThrowsWithoutRemovingRegisteredCard()
        {
            var card = CreateCard(100);

            var slot = CreateEmptySlot(
                1,
                BoardRow.Front,
                1);

            var state = CreateSideState(
                new[] { card },
                new[] { slot });

            Assert.Throws<InvalidOperationException>(
                () => state.RemoveCard(
                    slot.Position));

            Assert.That(slot.IsOccupied, Is.False);

            Assert.That(
                state.Cards.GetCard(card.InstanceId),
                Is.SameAs(card));
        }


        private static CombatCardState CreateCard(
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId("card.test"),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                7,
                2,
                3);
        }
    }
}