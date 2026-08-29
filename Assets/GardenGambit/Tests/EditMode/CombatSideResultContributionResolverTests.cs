using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideResultContributionResolverTests
    {
        [Test]
        public void Resolve_WithNullSideState_Throws()
        {
            var resolver =
                new CombatSideResultContributionResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(null));
        }

        [Test]
        public void Resolve_WithEmptyBoard_ReturnsZeroContribution()
        {
            var sideState =
                CreateSide(
                    CombatSide.Enemy,
                    new CombatCardState[0],
                    new CombatCardState[0],
                    3);

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                result.IsValid,
                Is.True);

            Assert.That(
                result.Side,
                Is.EqualTo(CombatSide.Enemy));

            Assert.That(
                result.SurvivorCount,
                Is.Zero);

            Assert.That(
                result
                    .TotalSurvivorRankContribution,
                Is.Zero);

            Assert.That(
                result.FinalAttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(3)));

            Assert.That(
                result.FinalResultContribution,
                Is.Zero);

            Assert.That(
                result.HasSurvivors,
                Is.False);
        }

        [Test]
        public void Resolve_WithOneLivingCard_UsesRankAndMultiplier()
        {
            var card =
                CreateCard(
                    100,
                    7,
                    6);

            var sideState =
                CreateSide(
                    CombatSide.Player,
                    new[] { card },
                    new[] { card },
                    2);

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                result.Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(
                result.SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                result.FinalResultContribution,
                Is.EqualTo(14));

            Assert.That(
                result.HasSurvivors,
                Is.True);

            Assert.That(
                result.HasPositiveContribution,
                Is.True);
        }

        [Test]
        public void Resolve_WithMultipleLivingCards_SumsRanksBeforeMultiplying()
        {
            var firstCard =
                CreateCard(
                    100,
                    2,
                    4);

            var secondCard =
                CreateCard(
                    101,
                    10,
                    7);

            var thirdCard =
                CreateCard(
                    102,
                    14,
                    1);

            var cards =
                new[]
                {
                    firstCard,
                    secondCard,
                    thirdCard
                };

            var sideState =
                CreateSide(
                    CombatSide.Enemy,
                    cards,
                    cards,
                    3);

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                result.SurvivorCount,
                Is.EqualTo(3));

            Assert.That(
                result
                    .TotalSurvivorRankContribution,
                Is.EqualTo(26));

            Assert.That(
                result.FinalResultContribution,
                Is.EqualTo(78));
        }

        [Test]
        public void Resolve_WithZeroHpCard_IgnoresDeathThresholdCard()
        {
            var deathThresholdCard =
                CreateCard(
                    100,
                    14,
                    0);

            var livingCard =
                CreateCard(
                    101,
                    5,
                    3);

            var cards =
                new[]
                {
                    deathThresholdCard,
                    livingCard
                };

            var sideState =
                CreateSide(
                    CombatSide.Player,
                    cards,
                    cards,
                    2);

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                result.SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result
                    .TotalSurvivorRankContribution,
                Is.EqualTo(5));

            Assert.That(
                result.FinalResultContribution,
                Is.EqualTo(10));
        }

        [Test]
        public void Resolve_WithNegativeHpCard_IgnoresDeathThresholdCard()
        {
            var card =
                CreateCard(
                    100,
                    14,
                    -3);

            var sideState =
                CreateSide(
                    CombatSide.Player,
                    new[] { card },
                    new[] { card },
                    2);

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                result.IsValid,
                Is.True);

            Assert.That(
                result.SurvivorCount,
                Is.Zero);

            Assert.That(
                result
                    .TotalSurvivorRankContribution,
                Is.Zero);

            Assert.That(
                result.FinalResultContribution,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithUnplacedRegistryCard_IgnoresUnplacedCard()
        {
            var placedCard =
                CreateCard(
                    100,
                    4,
                    5);

            var unplacedCard =
                CreateCard(
                    101,
                    14,
                    10);

            var sideState =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        placedCard,
                        unplacedCard
                    },
                    new[]
                    {
                        placedCard
                    },
                    2);

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                sideState.Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                result.SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result
                    .TotalSurvivorRankContribution,
                Is.EqualTo(4));

            Assert.That(
                result.FinalResultContribution,
                Is.EqualTo(8));
        }

        [Test]
        public void Resolve_WithLivingCard_DoesNotChangeBoardOrCard()
        {
            var card =
                CreateCard(
                    100,
                    9,
                    6);

            var sideState =
                CreateSide(
                    CombatSide.Player,
                    new[] { card },
                    new[] { card },
                    2);

            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var previousHp =
                card.CurrentHp;

            var previousRank =
                card.Rank.Value;

            var previousOccupant =
                sideState.Board
                    .GetSlot(position)
                    .OccupantInstanceId;

            var resolver =
                new CombatSideResultContributionResolver();

            var result =
                resolver.Resolve(sideState);

            Assert.That(
                result.IsValid,
                Is.True);

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(previousHp));

            Assert.That(
                card.Rank.Value,
                Is.EqualTo(previousRank));

            Assert.That(
                sideState.Board
                    .GetSlot(position)
                    .OccupantInstanceId,
                Is.EqualTo(previousOccupant));

            Assert.That(
                sideState.GetCardAt(position),
                Is.SameAs(card));

            Assert.That(
                sideState.Cards.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WhenFinalContributionOverflows_ThrowsWithoutChangingState()
        {
            var card =
                CreateCard(
                    100,
                    14,
                    10);

            var sideState =
                CreateSide(
                    CombatSide.Player,
                    new[] { card },
                    new[] { card },
                    int.MaxValue);

            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var resolver =
                new CombatSideResultContributionResolver();

            Assert.Throws<OverflowException>(
                () => resolver.Resolve(
                    sideState));

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                card.Rank.Value,
                Is.EqualTo(14));

            Assert.That(
                sideState.GetCardAt(position),
                Is.SameAs(card));

            Assert.That(
                sideState.Cards.Count,
                Is.EqualTo(1));
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            IEnumerable<CombatCardState>
                registeredCards,
            IReadOnlyList<CombatCardState>
                placedCards,
            int attackMultiplier)
        {
            var slots =
                new List<CombatSlotState>();

            var nextSlotId = 1;

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                 BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

                var frontPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        column);

                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        column);

                var placedCardIndex =
                    columnValue -
                    BoardColumn.MinimumValue;

                if (placedCardIndex <
                    placedCards.Count)
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition,
                            placedCards[
                                placedCardIndex]
                                .InstanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition));
                }

                nextSlotId++;

                slots.Add(
                    new CombatSlotState(
                        new SlotId(
                            nextSlotId),
                        backPosition));

                nextSlotId++;
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    registeredCards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    attackMultiplier));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    "result-card-" +
                    instanceId),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                10,
                currentHp,
                0,
                3);
        }
    }
}