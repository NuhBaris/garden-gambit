using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        ArmorGainCombatEventTests
    {
        [Test]
        public void
            Constructor_WithPositiveGain_SetsResolvedValues()
        {
            var metadata =
                CreateChildMetadata();

            var armorGainEvent =
                new ArmorGainCombatEvent(
                    metadata,
                    new InstanceId(1),
                    CreatePosition(),
                    previousArmor: 2,
                    currentArmor: 5);

            Assert.That(
                armorGainEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.ArmorGain));

            Assert.That(
                armorGainEvent.Metadata,
                Is.EqualTo(
                    metadata));

            Assert.That(
                armorGainEvent.TargetInstanceId,
                Is.EqualTo(
                    new InstanceId(1)));

            Assert.That(
                armorGainEvent.TargetPosition,
                Is.EqualTo(
                    CreatePosition()));

            Assert.That(
                armorGainEvent.TargetSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                armorGainEvent.PreviousArmor,
                Is.EqualTo(2));

            Assert.That(
                armorGainEvent.CurrentArmor,
                Is.EqualTo(5));

            Assert.That(
                armorGainEvent.ActualGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                armorGainEvent.WasUnarmored,
                Is.False);

            Assert.That(
                armorGainEvent.IsArmored,
                Is.True);
        }

        [Test]
        public void
            Constructor_FromZeroArmor_SetsArmorFlags()
        {
            var armorGainEvent =
                CreateEvent(
                    previousArmor: 0,
                    currentArmor: 1);

            Assert.That(
                armorGainEvent.WasUnarmored,
                Is.True);

            Assert.That(
                armorGainEvent.IsArmored,
                Is.True);

            Assert.That(
                armorGainEvent.ActualGainedAmount,
                Is.EqualTo(1));
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
                    new ArmorGainCombatEvent(
                        rootMetadata,
                        new InstanceId(1),
                        CreatePosition(),
                        previousArmor: 0,
                        currentArmor: 1));
        }

        [Test]
        public void
            Constructor_WithInvalidTargetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new ArmorGainCombatEvent(
                        CreateChildMetadata(),
                        default(InstanceId),
                        CreatePosition(),
                        previousArmor: 0,
                        currentArmor: 1));
        }

        [Test]
        public void
            Constructor_WithInvalidTargetPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new ArmorGainCombatEvent(
                        CreateChildMetadata(),
                        new InstanceId(1),
                        default(BoardPosition),
                        previousArmor: 0,
                        currentArmor: 1));
        }

        [Test]
        public void
            Constructor_WithNegativePreviousArmor_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        previousArmor: -1,
                        currentArmor: 1));
        }

        [Test]
        public void
            Constructor_WithNegativeCurrentArmor_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        previousArmor: 0,
                        currentArmor: -1));
        }

        [Test]
        public void
            Constructor_WithoutActualArmorIncrease_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    CreateEvent(
                        previousArmor: 3,
                        currentArmor: 3));
        }

        [Test]
        public void
            Constructor_WithArmorDecrease_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    CreateEvent(
                        previousArmor: 3,
                        currentArmor: 2));
        }

        private static ArmorGainCombatEvent
            CreateEvent(
                int previousArmor,
                int currentArmor)
        {
            return new ArmorGainCombatEvent(
                CreateChildMetadata(),
                new InstanceId(1),
                CreatePosition(),
                previousArmor,
                currentArmor);
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
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                triggerRootId,
                triggerRootId);
        }
    }
}