using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideResultContributionFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatSideResultContributionResolver(
                            null));
        }

        [Test]
        public void
            Constructor_ExposesExactModifierRegistry()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var resolver =
                new
                    CombatSideResultContributionResolver(
                        registry);

            Assert.That(
                resolver.FinalRankModifierRegistry,
                Is.SameAs(
                    registry));
        }

        [Test]
        public void
            Resolve_WithDefaultResolver_PreservesBaseRankContribution()
        {
            var card =
                CreateCard(
                    instanceId: 100,
                    rank: 5,
                    currentHp: 10);

            var sideState =
                CreateSideWithOnePlacedCard(
                    card,
                    CombatSlotEnhanceKind.None);

            var resolver =
                new
                    CombatSideResultContributionResolver();

            var contribution =
                resolver.Resolve(
                    sideState);

            Assert.That(
                contribution.SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(5));

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(5));
        }

        [Test]
        public void
            Resolve_WithLivingCardModifier_AddsModifierWithoutMutatingRank()
        {
            var card =
                CreateCard(
                    instanceId: 100,
                    rank: 5,
                    currentHp: 10);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                card.InstanceId,
                4);

            var resolver =
                new
                    CombatSideResultContributionResolver(
                        registry);

            var contribution =
                resolver.Resolve(
                    CreateSideWithOnePlacedCard(
                        card,
                        CombatSlotEnhanceKind.None));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(9));

            Assert.That(
                card.Rank,
                Is.EqualTo(
                    new CardRank(5)));
        }

        [Test]
        public void
            Resolve_WithTwoModifiedSurvivors_SumsIndependentFinalRanks()
        {
            var firstCard =
                CreateCard(
                    instanceId: 100,
                    rank: 3,
                    currentHp: 10);

            var secondCard =
                CreateCard(
                    instanceId: 200,
                    rank: 5,
                    currentHp: 10);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                firstCard.InstanceId,
                4);

            registry.AddModifier(
                secondCard.InstanceId,
                1);

            registry.AddModifier(
                secondCard.InstanceId,
                1);

            var resolver =
                new
                    CombatSideResultContributionResolver(
                        registry);

            var contribution =
                resolver.Resolve(
                    CreateSideWithTwoPlacedCards(
                        firstCard,
                        secondCard));

            Assert.That(
                contribution.SurvivorCount,
                Is.EqualTo(2));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(14));

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(14));
        }

        [Test]
        public void
            Resolve_WithModifierForDeathThresholdCard_IgnoresModifier()
        {
            var card =
                CreateCard(
                    instanceId: 100,
                    rank: 5,
                    currentHp: 0);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                card.InstanceId,
                4);

            var resolver =
                new
                    CombatSideResultContributionResolver(
                        registry);

            var contribution =
                resolver.Resolve(
                    CreateSideWithOnePlacedCard(
                        card,
                        CombatSlotEnhanceKind.None));

            Assert.That(
                contribution.SurvivorCount,
                Is.Zero);

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.Zero);

            Assert.That(
                contribution.FinalResultContribution,
                Is.Zero);
        }

        [Test]
        public void
            Resolve_WithModifierForUnplacedCard_IgnoresModifier()
        {
            var placedCard =
                CreateCard(
                    instanceId: 100,
                    rank: 3,
                    currentHp: 10);

            var unplacedCard =
                CreateCard(
                    instanceId: 200,
                    rank: 10,
                    currentHp: 10);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                unplacedCard.InstanceId,
                4);

            var resolver =
                new
                    CombatSideResultContributionResolver(
                        registry);

            var contribution =
                resolver.Resolve(
                    CreateSideWithUnplacedCard(
                        placedCard,
                        unplacedCard));

            Assert.That(
                contribution.SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(3));
        }

        [Test]
        public void
            Resolve_WithWarBannerAndFinalRankModifier_AppliesMultiplierAfterRankModifier()
        {
            var card =
                CreateCard(
                    instanceId: 100,
                    rank: 3,
                    currentHp: 10);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                card.InstanceId,
                4);

            var resolver =
                new
                    CombatSideResultContributionResolver(
                        registry);

            var contribution =
                resolver.Resolve(
                    CreateSideWithOnePlacedCard(
                        card,
                        CombatSlotEnhanceKind.WarBanner));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                contribution
                    .FinalAttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(2)));

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(14));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                hpCapacity: 10,
                currentHp: currentHp,
                armor: 0,
                attack: 3);
        }

        private static CombatSideState
            CreateSideWithOnePlacedCard(
                CombatCardState card,
                CombatSlotEnhanceKind enhanceKind)
        {
            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            position,
                            card.InstanceId,
                            enhanceKind)
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
            CreateSideWithTwoPlacedCards(
                CombatCardState firstCard,
                CombatCardState secondCard)
        {
            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            new BoardPosition(
                                CombatSide.Player,
                                BoardRow.Front,
                                new BoardColumn(1)),
                            firstCard.InstanceId),

                        new CombatSlotState(
                            new SlotId(2),
                            new BoardPosition(
                                CombatSide.Player,
                                BoardRow.Front,
                                new BoardColumn(2)),
                            secondCard.InstanceId)
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
        }

        private static CombatSideState
            CreateSideWithUnplacedCard(
                CombatCardState placedCard,
                CombatCardState unplacedCard)
        {
            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            new BoardPosition(
                                CombatSide.Player,
                                BoardRow.Front,
                                new BoardColumn(1)),
                            placedCard.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        placedCard,
                        unplacedCard
                    }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}