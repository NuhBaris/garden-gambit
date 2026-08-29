using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideResultContributionTests
    {
        [Test]
        public void Constructor_WithValidValues_CalculatesFinalContribution()
        {
            var contribution =
                new CombatSideResultContribution(
                    CombatSide.Player,
                    3,
                    30,
                    new AttackMultiplier(2));

            Assert.That(
                contribution.Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(
                contribution.SurvivorCount,
                Is.EqualTo(3));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(30));

            Assert.That(
                contribution.FinalAttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(2)));

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(60));

            Assert.That(
                contribution.HasSurvivors,
                Is.True);

            Assert.That(
                contribution.HasPositiveContribution,
                Is.True);

            Assert.That(
                contribution.IsValid,
                Is.True);
        }

        [Test]
        public void Constructor_WithNoSurvivors_AllowsZeroContribution()
        {
            var contribution =
                new CombatSideResultContribution(
                    CombatSide.Enemy,
                    0,
                    0,
                    new AttackMultiplier(4));

            Assert.That(
                contribution.Side,
                Is.EqualTo(CombatSide.Enemy));

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

            Assert.That(
                contribution.HasSurvivors,
                Is.False);

            Assert.That(
                contribution.HasPositiveContribution,
                Is.False);

            Assert.That(
                contribution.IsValid,
                Is.True);
        }

        [Test]
        public void Constructor_WithMaximumSurvivorCount_AllowsValue()
        {
            var totalRankContribution =
                CombatBoardState.MaximumSlotCount *
                14;

            var contribution =
                new CombatSideResultContribution(
                    CombatSide.Player,
                    CombatBoardState.MaximumSlotCount,
                    totalRankContribution,
                    new AttackMultiplier(3));

            Assert.That(
                contribution.SurvivorCount,
                Is.EqualTo(
                    CombatBoardState.MaximumSlotCount));

            Assert.That(
                contribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(totalRankContribution));

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(
                    totalRankContribution * 3));

            Assert.That(
                contribution.IsValid,
                Is.True);
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSideResultContribution(
                        default(CombatSide),
                        1,
                        2,
                        new AttackMultiplier(1)));
        }

        [Test]
        public void Constructor_WithNegativeSurvivorCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        -1,
                        0,
                        new AttackMultiplier(1)));
        }

        [Test]
        public void Constructor_WithSurvivorCountAboveBoardLimit_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        CombatBoardState
                            .MaximumSlotCount + 1,
                        22,
                        new AttackMultiplier(1)));
        }

        [Test]
        public void Constructor_WithNegativeRankContribution_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        1,
                        -1,
                        new AttackMultiplier(1)));
        }

        [Test]
        public void Constructor_WithoutSurvivorsAndPositiveRankContribution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        0,
                        2,
                        new AttackMultiplier(1)));
        }

        [Test]
        public void Constructor_WithSurvivorsAndZeroRankContribution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        1,
                        0,
                        new AttackMultiplier(1)));
        }

        [Test]
        public void Constructor_WithInvalidAttackMultiplier_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        1,
                        2,
                        default(AttackMultiplier)));
        }

        [Test]
        public void Constructor_WhenFinalContributionOverflows_Throws()
        {
            Assert.Throws<OverflowException>(
                () => _ =
                    new CombatSideResultContribution(
                        CombatSide.Player,
                        1,
                        int.MaxValue,
                        new AttackMultiplier(2)));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var contribution =
                default(
                    CombatSideResultContribution);

            Assert.That(
                contribution.IsValid,
                Is.False);

            Assert.That(
                contribution.HasSurvivors,
                Is.False);

            Assert.That(
                contribution.HasPositiveContribution,
                Is.False);
        }
    }
}