using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class AttackGainCombatEventTests
    {
        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Constructor_WithPositiveGain_SetsResolvedValues(CombatSide side)
        {
            var metadata = CreateChildMetadata();
            var position = CreatePosition(side);
            var gain = new AttackGainCombatEvent(
                metadata, new InstanceId(1), position, 2, 5);

            Assert.That(gain.Kind, Is.EqualTo(CombatEventKind.AttackGain));
            Assert.That(gain.Metadata, Is.EqualTo(metadata));
            Assert.That(gain.TargetInstanceId, Is.EqualTo(new InstanceId(1)));
            Assert.That(gain.TargetPosition, Is.EqualTo(position));
            Assert.That(gain.TargetSide, Is.EqualTo(side));
            Assert.That(gain.PreviousAttack, Is.EqualTo(2));
            Assert.That(gain.CurrentAttack, Is.EqualTo(5));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(3));
        }

        [TestCase(0, 1, 1)]
        [TestCase(0, int.MaxValue, int.MaxValue)]
        [TestCase(int.MaxValue - 1, int.MaxValue, 1)]
        public void Constructor_WithBoundaryValues_PreservesExactGain(
            int previous, int current, int expectedGain)
        {
            var gain = CreateEvent(previous, current);

            Assert.That(gain.PreviousAttack, Is.EqualTo(previous));
            Assert.That(gain.CurrentAttack, Is.EqualTo(current));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(expectedGain));
        }

        [Test]
        public void Constructor_WithTriggerRootMetadata_Throws()
        {
            var rootId = new CombatEventId(1);
            var metadata = new CombatEventMetadata(
                rootId, new CombatSequenceNumber(1), null, rootId);

            Assert.Throws<ArgumentException>(() => _ = new AttackGainCombatEvent(
                metadata, new InstanceId(1), CreatePosition(CombatSide.Player), 0, 1));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(() => _ = new AttackGainCombatEvent(
                default(CombatEventMetadata), new InstanceId(1),
                CreatePosition(CombatSide.Player), 0, 1));
        }

        [Test]
        public void Constructor_WithInvalidTargetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(() => _ = new AttackGainCombatEvent(
                CreateChildMetadata(), default(InstanceId),
                CreatePosition(CombatSide.Player), 0, 1));
        }

        [Test]
        public void Constructor_WithInvalidTargetPosition_Throws()
        {
            Assert.Throws<ArgumentException>(() => _ = new AttackGainCombatEvent(
                CreateChildMetadata(), new InstanceId(1),
                default(BoardPosition), 0, 1));
        }

        [TestCase(-1, 1)]
        [TestCase(0, -1)]
        public void Constructor_WithNegativeAttack_Throws(int previous, int current)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = CreateEvent(previous, current));
        }

        [TestCase(3, 3)]
        [TestCase(3, 2)]
        public void Constructor_WithoutPositiveIncrease_Throws(int previous, int current)
        {
            Assert.Throws<ArgumentException>(() => _ = CreateEvent(previous, current));
        }

        private static AttackGainCombatEvent CreateEvent(int previous, int current)
        {
            return new AttackGainCombatEvent(
                CreateChildMetadata(), new InstanceId(1),
                CreatePosition(CombatSide.Player), previous, current);
        }

        private static BoardPosition CreatePosition(CombatSide side)
        {
            return new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
        }

        private static CombatEventMetadata CreateChildMetadata()
        {
            var rootId = new CombatEventId(1);
            return new CombatEventMetadata(
                new CombatEventId(2), new CombatSequenceNumber(2), rootId, rootId);
        }
    }
}
