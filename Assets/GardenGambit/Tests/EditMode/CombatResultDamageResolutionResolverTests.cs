using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultDamageResolutionResolverTests
    {
        [Test]
        public void Resolve_WithValidCalculation_ReturnsUnmodifiedBaselineDamage()
        {
            var state =
                CreateState();

            var calculation =
                CreateCalculation(
                    baseDamageToPlayer: 4,
                    baseDamageToEnemy: 6);

            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    state,
                    calculation);

            Assert.That(
                resolution.IsValid,
                Is.True);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.Zero);

            Assert.That(
                resolution.EnemyDamageDelta,
                Is.Zero);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void Resolve_WithAsymmetricDamage_PreservesEachSideIndependently()
        {
            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    CreateState(),
                    CreateCalculation(
                        baseDamageToPlayer: 0,
                        baseDamageToEnemy: 6));

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void Resolve_WithZeroDamage_ReturnsValidZeroResolution()
        {
            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    CreateState(),
                    CreateCalculation(
                        baseDamageToPlayer: 0,
                        baseDamageToEnemy: 0));

            Assert.That(
                resolution.IsValid,
                Is.True);

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.Zero);

            Assert.That(
                resolution.PreventedDamageForEnemy,
                Is.Zero);

            Assert.That(
                resolution.AddedDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution.AddedDamageToEnemy,
                Is.Zero);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void Resolve_DoesNotChangeCombatState()
        {
            var state =
                CreateState();

            var playerHealthBefore =
                state.GetSide(CombatSide.Player)
                    .BattleHealth;

            var enemyHealthBefore =
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth;

            var playerMultiplierBefore =
                state.GetSide(CombatSide.Player)
                    .AttackMultiplier;

            var enemyMultiplierBefore =
                state.GetSide(CombatSide.Enemy)
                    .AttackMultiplier;

            var resolver =
                new CombatResultDamageResolutionResolver();

            resolver.Resolve(
                state,
                CreateCalculation(
                    baseDamageToPlayer: 4,
                    baseDamageToEnemy: 6));

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    playerHealthBefore));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    enemyHealthBefore));

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .AttackMultiplier,
                Is.EqualTo(
                    playerMultiplierBefore));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .AttackMultiplier,
                Is.EqualTo(
                    enemyMultiplierBefore));
        }

        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var resolver =
                new CombatResultDamageResolutionResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(
                    null,
                    CreateCalculation(
                        baseDamageToPlayer: 4,
                        baseDamageToEnemy: 6)));
        }

        [Test]
        public void Resolve_WithInvalidCalculation_Throws()
        {
            var resolver =
                new CombatResultDamageResolutionResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.Resolve(
                    CreateState(),
                    default(
                        CombatResultDamageCalculation)));
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player),
                CreateSideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState CreateSideState(
            CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatResultDamageCalculation
            CreateCalculation(
                int baseDamageToPlayer,
                int baseDamageToEnemy)
        {
            return new CombatResultDamageCalculation(
                CreateContribution(
                    CombatSide.Player,
                    baseDamageToEnemy),
                CreateContribution(
                    CombatSide.Enemy,
                    baseDamageToPlayer));
        }

        private static CombatSideResultContribution
            CreateContribution(
                CombatSide side,
                int contribution)
        {
            var survivorCount =
                contribution > 0
                    ? 1
                    : 0;

            return new CombatSideResultContribution(
                side,
                survivorCount,
                contribution,
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}