using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        BattleHealthChangedCombatEventTests
    {
        [Test]
        public void Constructor_WithDamage_SetsNegativeDelta()
        {
            var metadata =
                CreateMetadata();

            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    metadata,
                    CombatSide.Player,
                    new BattleHealth(20),
                    new BattleHealth(-5));

            Assert.That(
                changeEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .BattleHealthChanged));

            Assert.That(
                changeEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                changeEvent.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                changeEvent.PreviousBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                changeEvent.CurrentBattleHealth,
                Is.EqualTo(
                    new BattleHealth(-5)));

            Assert.That(
                changeEvent.Delta,
                Is.EqualTo(-25L));

            Assert.That(
                changeEvent.ChangedAmount,
                Is.EqualTo(25L));

            Assert.That(
                changeEvent.IsDamage,
                Is.True);

            Assert.That(
                changeEvent.IsGain,
                Is.False);
        }

        [Test]
        public void Constructor_WithGain_SetsPositiveDelta()
        {
            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    CreateMetadata(),
                    CombatSide.Enemy,
                    new BattleHealth(-5),
                    new BattleHealth(10));

            Assert.That(
                changeEvent.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                changeEvent.Delta,
                Is.EqualTo(15L));

            Assert.That(
                changeEvent.ChangedAmount,
                Is.EqualTo(15L));

            Assert.That(
                changeEvent.IsGain,
                Is.True);

            Assert.That(
                changeEvent.IsDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithDamageCrossingZero_AllowsNegativeCurrentHealth()
        {
            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    CreateMetadata(),
                    CombatSide.Player,
                    new BattleHealth(3),
                    new BattleHealth(-7));

            Assert.That(
                changeEvent.PreviousBattleHealth.Value,
                Is.EqualTo(3));

            Assert.That(
                changeEvent.CurrentBattleHealth.Value,
                Is.EqualTo(-7));

            Assert.That(
                changeEvent.Delta,
                Is.EqualTo(-10L));

            Assert.That(
                changeEvent.ChangedAmount,
                Is.EqualTo(10L));

            Assert.That(
                changeEvent.IsDamage,
                Is.True);
        }

        [Test]
        public void Constructor_WithMaximumPositiveDelta_UsesLongWithoutOverflow()
        {
            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    CreateMetadata(),
                    CombatSide.Player,
                    new BattleHealth(int.MinValue),
                    new BattleHealth(int.MaxValue));

            Assert.That(
                changeEvent.Delta,
                Is.EqualTo(4294967295L));

            Assert.That(
                changeEvent.ChangedAmount,
                Is.EqualTo(4294967295L));

            Assert.That(
                changeEvent.IsGain,
                Is.True);

            Assert.That(
                changeEvent.IsDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithMaximumNegativeDelta_UsesLongWithoutOverflow()
        {
            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    CreateMetadata(),
                    CombatSide.Enemy,
                    new BattleHealth(int.MaxValue),
                    new BattleHealth(int.MinValue));

            Assert.That(
                changeEvent.Delta,
                Is.EqualTo(-4294967295L));

            Assert.That(
                changeEvent.ChangedAmount,
                Is.EqualTo(4294967295L));

            Assert.That(
                changeEvent.IsDamage,
                Is.True);

            Assert.That(
                changeEvent.IsGain,
                Is.False);
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleHealthChangedCombatEvent(
                        default(CombatEventMetadata),
                        CombatSide.Player,
                        new BattleHealth(20),
                        new BattleHealth(10)));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new BattleHealthChangedCombatEvent(
                        CreateMetadata(),
                        default(CombatSide),
                        new BattleHealth(20),
                        new BattleHealth(10)));
        }

        [Test]
        public void Constructor_WithoutActualChange_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleHealthChangedCombatEvent(
                        CreateMetadata(),
                        CombatSide.Player,
                        new BattleHealth(20),
                        new BattleHealth(20)));
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