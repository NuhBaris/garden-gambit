using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HpGainCombatEventTests
    {
        [Test]
        public void
            Constructor_WithHeal_SetsResolvedValues()
        {
            var hpGainEvent =
                CreateEvent(
                    previousHpCapacity: 10,
                    currentHpCapacity: 10,
                    previousHp: 4,
                    currentHp: 7);

            Assert.That(
                hpGainEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.HpGain));

            Assert.That(
                hpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    new InstanceId(1)));

            Assert.That(
                hpGainEvent.TargetPosition,
                Is.EqualTo(
                    CreatePosition()));

            Assert.That(
                hpGainEvent.TargetSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                hpGainEvent.PreviousHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                hpGainEvent.CurrentHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                hpGainEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                hpGainEvent.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                hpGainEvent.CapacityGainedAmount,
                Is.Zero);

            Assert.That(
                hpGainEvent.IsHeal,
                Is.True);

            Assert.That(
                hpGainEvent.IsHpStatGain,
                Is.False);
        }

        [Test]
        public void
            Constructor_WithHpStatGain_SetsResolvedValues()
        {
            var hpGainEvent =
                CreateEvent(
                    previousHpCapacity: 10,
                    currentHpCapacity: 13,
                    previousHp: 4,
                    currentHp: 7);

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                hpGainEvent.CapacityGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                hpGainEvent.IsHpStatGain,
                Is.True);

            Assert.That(
                hpGainEvent.IsHeal,
                Is.False);
        }

        [Test]
        public void
            Constructor_WhenGainRestoresPositiveHp_SetsRestorationFlags()
        {
            var hpGainEvent =
                CreateEvent(
                    previousHpCapacity: 10,
                    currentHpCapacity: 10,
                    previousHp: 0,
                    currentHp: 2);

            Assert.That(
                hpGainEvent.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                hpGainEvent.IsAtDeathThreshold,
                Is.False);

            Assert.That(
                hpGainEvent
                    .RestoredAboveDeathThreshold,
                Is.True);
        }

        [Test]
        public void
            Constructor_WhenGainRemainsAtThreshold_DoesNotSetRestoredFlag()
        {
            var hpGainEvent =
                CreateEvent(
                    previousHpCapacity: 10,
                    currentHpCapacity: 10,
                    previousHp: -3,
                    currentHp: -1);

            Assert.That(
                hpGainEvent.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                hpGainEvent.IsAtDeathThreshold,
                Is.True);

            Assert.That(
                hpGainEvent
                    .RestoredAboveDeathThreshold,
                Is.False);
        }

        [Test]
        public void
            Constructor_WithTriggerRootMetadata_Throws()
        {
            var eventId =
                new CombatEventId(1);

            var rootMetadata =
                new CombatEventMetadata(
                    eventId,
                    new CombatSequenceNumber(1),
                    null,
                    eventId);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new HpGainCombatEvent(
                        rootMetadata,
                        new InstanceId(1),
                        CreatePosition(),
                        10,
                        10,
                        4,
                        5));
        }

        [Test]
        public void
            Constructor_WithInvalidTargetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new HpGainCombatEvent(
                        CreateChildMetadata(),
                        default(InstanceId),
                        CreatePosition(),
                        10,
                        10,
                        4,
                        5));
        }

        [Test]
        public void
            Constructor_WithInvalidTargetPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new HpGainCombatEvent(
                        CreateChildMetadata(),
                        new InstanceId(1),
                        default(BoardPosition),
                        10,
                        10,
                        4,
                        5));
        }

        [Test]
        public void
            Constructor_WithNonPositivePreviousCapacity_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 0,
                        currentHpCapacity: 10,
                        previousHp: 0,
                        currentHp: 1));
        }

        [Test]
        public void
            Constructor_WithNonPositiveCurrentCapacity_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 0,
                        previousHp: -2,
                        currentHp: -1));
        }

        [Test]
        public void
            Constructor_WithPreviousHpAboveCapacity_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 12,
                        previousHp: 11,
                        currentHp: 12));
        }

        [Test]
        public void
            Constructor_WithCurrentHpAboveCapacity_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 10,
                        previousHp: 9,
                        currentHp: 11));
        }

        [Test]
        public void
            Constructor_WithoutActualHpIncrease_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 10,
                        previousHp: 5,
                        currentHp: 5));
        }

        [Test]
        public void
            Constructor_WithHpDecrease_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 10,
                        previousHp: 5,
                        currentHp: 4));
        }

        [Test]
        public void
            Constructor_WithCapacityDecrease_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 9,
                        previousHp: 4,
                        currentHp: 5));
        }

        [Test]
        public void
            Constructor_WithMismatchedStatGainAmounts_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 10,
                        currentHpCapacity: 12,
                        previousHp: 4,
                        currentHp: 5));
        }

        [Test]
        public void
            Constructor_WhenActualGainExceedsIntMaximum_Throws()
        {
            Assert.Throws<OverflowException>(
                () => _ =
                    CreateEvent(
                        previousHpCapacity: 1,
                        currentHpCapacity:
                            int.MaxValue,
                        previousHp:
                            int.MinValue,
                        currentHp:
                            int.MaxValue));
        }

        private static HpGainCombatEvent CreateEvent(
            int previousHpCapacity,
            int currentHpCapacity,
            int previousHp,
            int currentHp)
        {
            return new HpGainCombatEvent(
                CreateChildMetadata(),
                new InstanceId(1),
                CreatePosition(),
                previousHpCapacity,
                currentHpCapacity,
                previousHp,
                currentHp);
        }

        private static BoardPosition CreatePosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static CombatEventMetadata
            CreateChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }
    }
}