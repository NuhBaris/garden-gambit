using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideStatePermanentRemovalTests
    {
        [Test]
        public void RemoveCardFromCombat_RemovesCardFromBoardAndRegistry()
        {
            var environment =
                CreateEnvironment();

            var removedCard =
                environment.Side.RemoveCardFromCombat(
                    environment.FirstPosition);

            Assert.That(
                removedCard,
                Is.SameAs(environment.FirstCard));

            var removedSlot =
                environment.Side.Board.GetSlot(
                    environment.FirstPosition);

            Assert.That(
                removedSlot.IsOccupied,
                Is.False);

            Assert.That(
                removedSlot.OccupantInstanceId,
                Is.Null);

            Assert.That(
                environment.Side.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Side.Cards.Cards[0],
                Is.SameAs(environment.SecondCard));

            Assert.That(
                environment.Side.GetCardAt(
                    environment.SecondPosition),
                Is.SameAs(environment.SecondCard));

            Assert.Throws<KeyNotFoundException>(
                () => environment.Side.Cards.GetCard(
                    environment.FirstCard.InstanceId));
        }

        [Test]
        public void RemoveCardFromCombat_WithEmptySlot_ThrowsWithoutChangingRegistry()
        {
            var environment =
                CreateEnvironment();

            environment.Side.RemoveCard(
                environment.FirstPosition);

            Assert.That(
                environment.Side.Cards.Count,
                Is.EqualTo(2));

            Assert.Throws<InvalidOperationException>(
                () => environment.Side
                    .RemoveCardFromCombat(
                        environment.FirstPosition));

            Assert.That(
                environment.Side.Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Side.Board
                    .GetSlot(
                        environment.FirstPosition)
                    .IsOccupied,
                Is.False);
        }

        [Test]
        public void RemoveCardFromCombat_WhenRegistryEntryIsMissing_ThrowsWithoutClearingBoardSlot()
        {
            var environment =
                CreateEnvironment();

            environment.Side.Cards.RemoveCard(
                environment.FirstCard.InstanceId);

            Assert.Throws<KeyNotFoundException>(
                () => environment.Side
                    .RemoveCardFromCombat(
                        environment.FirstPosition));

            var slot =
                environment.Side.Board.GetSlot(
                    environment.FirstPosition);

            Assert.That(
                slot.IsOccupied,
                Is.True);

            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.FirstCard.InstanceId));

            Assert.That(
                environment.Side.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Side.Cards.Cards[0],
                Is.SameAs(environment.SecondCard));
        }

        [Test]
        public void RemoveCardFromCombat_WithInvalidPosition_ThrowsWithoutChangingSide()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Side
                    .RemoveCardFromCombat(
                        default(BoardPosition)));

            Assert.That(
                environment.Side.Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Side.Board
                    .GetSlot(
                        environment.FirstPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.FirstCard.InstanceId));

            Assert.That(
                environment.Side.Board
                    .GetSlot(
                        environment.SecondPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.SecondCard.InstanceId));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var firstPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var secondPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var firstCard =
                CreateCard(
                    "card.first",
                    100);

            var secondCard =
                CreateCard(
                    "card.second",
                    200);

            var firstSlot =
                new CombatSlotState(
                    new SlotId(1),
                    firstPosition,
                    firstCard.InstanceId);

            var secondSlot =
                new CombatSlotState(
                    new SlotId(2),
                    secondPosition,
                    secondCard.InstanceId);

            var side =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            firstSlot,
                            secondSlot
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            firstCard,
                            secondCard
                        }),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            return new TestEnvironment
            {
                Side = side,
                FirstCard = firstCard,
                SecondCard = secondCard,
                FirstPosition = firstPosition,
                SecondPosition = secondPosition
            };
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

        private sealed class TestEnvironment
        {
            public CombatSideState Side { get; set; }

            public CombatCardState FirstCard { get; set; }

            public CombatCardState SecondCard { get; set; }

            public BoardPosition FirstPosition { get; set; }

            public BoardPosition SecondPosition { get; set; }
        }
    }
}