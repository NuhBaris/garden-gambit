using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultCalculatedCombatEventResolutionTests
    {
        [Test]
        public void ResolutionConstructor_WithUnmodifiedDamage_SetsCompleteSnapshot()
        {
            var metadata =
                CreateMetadata();

            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 4,
                        baseDamageToEnemy: 6),
                    4,
                    6);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    metadata,
                    resolution);

            Assert.That(
                resultEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .CombatResultCalculated));

            Assert.That(
                resultEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                resultEvent.Resolution.IsValid,
                Is.True);

            Assert.That(
                resultEvent.Calculation.IsValid,
                Is.True);

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resultEvent.PlayerDamageDelta,
                Is.Zero);

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.Zero);

            Assert.That(
                resultEvent.HasAnyDamageModification,
                Is.False);

            Assert.That(
                resultEvent.HasResolvedDamageToPlayer,
                Is.True);

            Assert.That(
                resultEvent.HasResolvedDamageToEnemy,
                Is.True);

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
                Is.True);
        }

        [Test]
        public void ResolutionConstructor_WithReductions_ExposesPreventedDamage()
        {
            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    new CombatResultDamageResolution(
                        CreateCalculation(
                            baseDamageToPlayer: 20,
                            baseDamageToEnemy: 10),
                        19,
                        8));

            Assert.That(
                resultEvent.PlayerDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.EqualTo(-2L));

            Assert.That(
                resultEvent.PreventedDamageForPlayer,
                Is.EqualTo(1L));

            Assert.That(
                resultEvent.PreventedDamageForEnemy,
                Is.EqualTo(2L));

            Assert.That(
                resultEvent.AddedDamageToPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.AddedDamageToEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.HasAnyDamageModification,
                Is.True);
        }

        [Test]
        public void ResolutionConstructor_WithIncreases_ExposesAddedDamage()
        {
            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    new CombatResultDamageResolution(
                        CreateCalculation(
                            baseDamageToPlayer: 4,
                            baseDamageToEnemy: 6),
                        7,
                        9));

            Assert.That(
                resultEvent.PlayerDamageDelta,
                Is.EqualTo(3L));

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.EqualTo(3L));

            Assert.That(
                resultEvent.AddedDamageToPlayer,
                Is.EqualTo(3L));

            Assert.That(
                resultEvent.AddedDamageToEnemy,
                Is.EqualTo(3L));

            Assert.That(
                resultEvent.PreventedDamageForPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.PreventedDamageForEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.HasAnyDamageModification,
                Is.True);
        }

        [Test]
        public void ResolutionConstructor_WithZeroDamage_SetsDamageFlagsFalse()
        {
            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    new CombatResultDamageResolution(
                        CreateCalculation(
                            baseDamageToPlayer: 0,
                            baseDamageToEnemy: 0),
                        0,
                        0));

            Assert.That(
                resultEvent
                    .HasResolvedDamageToPlayer,
                Is.False);

            Assert.That(
                resultEvent
                    .HasResolvedDamageToEnemy,
                Is.False);

            Assert.That(
                resultEvent
                    .HasMutualResolvedDamage,
                Is.False);

            Assert.That(
                resultEvent
                    .HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void ResolutionConstructor_WithInvalidMetadata_Throws()
        {
            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 4,
                        baseDamageToEnemy: 6),
                    4,
                    6);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultCalculatedCombatEvent(
                        default(CombatEventMetadata),
                        resolution));
        }

        [Test]
        public void ResolutionConstructor_WithInvalidResolution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultCalculatedCombatEvent(
                        CreateMetadata(),
                        default(
                            CombatResultDamageResolution)));
        }

        [Test]
        public void LegacyConstructor_CreatesEquivalentResolutionSnapshot()
        {
            var calculation =
                CreateCalculation(
                    baseDamageToPlayer: 20,
                    baseDamageToEnemy: 10);

            var legacyEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    calculation,
                    19,
                    8);

            Assert.That(
                legacyEvent.Resolution.IsValid,
                Is.True);

            Assert.That(
                legacyEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                legacyEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(10));

            Assert.That(
                legacyEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(19));

            Assert.That(
                legacyEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(8));

            Assert.That(
                legacyEvent.PlayerDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                legacyEvent.EnemyDamageDelta,
                Is.EqualTo(-2L));

            Assert.That(
                legacyEvent.PreventedDamageForPlayer,
                Is.EqualTo(1L));

            Assert.That(
                legacyEvent.PreventedDamageForEnemy,
                Is.EqualTo(2L));

            Assert.That(
                legacyEvent.HasAnyDamageModification,
                Is.True);
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

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }
    }
}