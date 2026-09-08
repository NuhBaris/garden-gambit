using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatFinalRankContributionResolverTests
    {
        [Test]
        public void
            Constructor_WithNullRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatFinalRankContributionResolver(
                        null));
        }

        [Test]
        public void
            Constructor_ExposesExactRegistry()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var resolver =
                new CombatFinalRankContributionResolver(
                    registry);

            Assert.That(
                resolver.ModifierRegistry,
                Is.SameAs(
                    registry));
        }

        [Test]
        public void Resolve_WithNullCard_Throws()
        {
            var resolver =
                CreateResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(
                    null));
        }

        [Test]
        public void
            Resolve_WithInvalidInstanceId_Throws()
        {
            var resolver =
                CreateResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.Resolve(
                    default(InstanceId),
                    structuralRankContribution: 5));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void
            Resolve_WithNonPositiveStructuralContribution_Throws(
                int structuralRankContribution)
        {
            var resolver =
                CreateResolver();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => resolver.Resolve(
                    new InstanceId(100),
                    structuralRankContribution));
        }

        [Test]
        public void
            Resolve_WithoutModifier_ReturnsStructuralContribution()
        {
            var resolver =
                CreateResolver();

            var result =
                resolver.Resolve(
                    new InstanceId(100),
                    structuralRankContribution: 7);

            Assert.That(
                result,
                Is.EqualTo(7));
        }

        [Test]
        public void
            Resolve_WithPositiveModifier_AddsAfterStructuralContribution()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                4);

            var resolver =
                new CombatFinalRankContributionResolver(
                    registry);

            var result =
                resolver.Resolve(
                    cardInstanceId,
                    structuralRankContribution: 14);

            Assert.That(
                result,
                Is.EqualTo(18));
        }

        [Test]
        public void
            Resolve_WithNegativeModifier_SubtractsFromStructuralContribution()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                -2);

            var resolver =
                new CombatFinalRankContributionResolver(
                    registry);

            var result =
                resolver.Resolve(
                    cardInstanceId,
                    structuralRankContribution: 7);

            Assert.That(
                result,
                Is.EqualTo(5));
        }

        [Test]
        public void
            Resolve_WhenModifierReducesContributionToZero_Throws()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                -5);

            var resolver =
                new CombatFinalRankContributionResolver(
                    registry);

            Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(
                    cardInstanceId,
                    structuralRankContribution: 5));
        }

        [Test]
        public void
            Resolve_WhenFinalContributionOverflows_Throws()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                1);

            var resolver =
                new CombatFinalRankContributionResolver(
                    registry);

            Assert.Throws<OverflowException>(
                () => resolver.Resolve(
                    cardInstanceId,
                    structuralRankContribution:
                        int.MaxValue));
        }

        [Test]
        public void
            Resolve_WithCard_UsesRankAndDoesNotMutateCard()
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "test-card"),
                    new InstanceId(100),
                    new CardRank(7),
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: 0,
                    attack: 3);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                card.InstanceId,
                4);

            var resolver =
                new CombatFinalRankContributionResolver(
                    registry);

            var result =
                resolver.Resolve(
                    card);

            Assert.That(
                result,
                Is.EqualTo(11));

            Assert.That(
                card.Rank,
                Is.EqualTo(
                    new CardRank(7)));
        }

        private static
            CombatFinalRankContributionResolver
            CreateResolver()
        {
            return new
                CombatFinalRankContributionResolver(
                    new
                        CombatFinalRankModifierRegistry());
        }
    }
}