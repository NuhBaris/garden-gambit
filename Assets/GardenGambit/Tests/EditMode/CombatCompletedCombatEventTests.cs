using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCompletedCombatEventTests
    {
        [Test]
        public void Constructor_WithPlayerVictory_SetsCompleteSnapshot()
        {
            var metadata =
                CreateMetadata();

            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(13),
                    new BattleHealth(8));

            var completedEvent =
                new CombatCompletedCombatEvent(
                    metadata,
                    calculation);

            Assert.That(
                completedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatCompleted));

            Assert.That(
                completedEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                completedEvent.Calculation.IsValid,
                Is.True);

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(13)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(8)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(5L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(5L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.True);

            Assert.That(
                completedEvent.IsEnemyVictory,
                Is.False);

            Assert.That(
                completedEvent.IsDraw,
                Is.False);
        }

        [Test]
        public void Constructor_WithEnemyVictory_SetsFlagsAndDifference()
        {
            var completedEvent =
                new CombatCompletedCombatEvent(
                    CreateMetadata(),
                    new CombatOutcomeCalculation(
                        new BattleHealth(4),
                        new BattleHealth(11)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.EnemyVictory));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(-7L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(7L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.False);

            Assert.That(
                completedEvent.IsEnemyVictory,
                Is.True);

            Assert.That(
                completedEvent.IsDraw,
                Is.False);
        }

        [Test]
        public void Constructor_WithEqualHealth_SetsDraw()
        {
            var completedEvent =
                new CombatCompletedCombatEvent(
                    CreateMetadata(),
                    new CombatOutcomeCalculation(
                        new BattleHealth(10),
                        new BattleHealth(10)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.Zero);

            Assert.That(
                completedEvent.WinningMargin,
                Is.Zero);

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.False);

            Assert.That(
                completedEvent.IsEnemyVictory,
                Is.False);

            Assert.That(
                completedEvent.IsDraw,
                Is.True);
        }

        [Test]
        public void Constructor_WithNegativeHealthValues_PreservesSnapshot()
        {
            var completedEvent =
                new CombatCompletedCombatEvent(
                    CreateMetadata(),
                    new CombatOutcomeCalculation(
                        new BattleHealth(-3),
                        new BattleHealth(-7)));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(-3)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(-7)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(4L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(4L));
        }

        [Test]
        public void Constructor_WithMaximumDifference_UsesLongWithoutOverflow()
        {
            var completedEvent =
                new CombatCompletedCombatEvent(
                    CreateMetadata(),
                    new CombatOutcomeCalculation(
                        new BattleHealth(int.MaxValue),
                        new BattleHealth(int.MinValue)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(4294967295L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(4294967295L));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(13),
                    new BattleHealth(8));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatCompletedCombatEvent(
                        default(CombatEventMetadata),
                        calculation));
        }

        [Test]
        public void Constructor_WithInvalidCalculation_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatCompletedCombatEvent(
                        CreateMetadata(),
                        default(CombatOutcomeCalculation)));
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