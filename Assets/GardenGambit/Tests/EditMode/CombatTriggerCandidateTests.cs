using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatTriggerCandidateTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsSnapshot()
        {
            var orderKey =
                CreateValidOrderKey();

            var trigger =
                new TestTrigger();

            var candidate =
                new CombatTriggerCandidate<TestTrigger>(
                    orderKey,
                    trigger);

            Assert.That(
                candidate.OrderKey,
                Is.EqualTo(orderKey));

            Assert.That(
                candidate.Trigger,
                Is.SameAs(trigger));
        }

        [Test]
        public void Constructor_WithInvalidOrderKey_Throws()
        {
            var trigger =
                new TestTrigger();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatTriggerCandidate<TestTrigger>(
                        default(CombatTriggerOrderKey),
                        trigger));
        }

        [Test]
        public void Constructor_WithNullTrigger_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerCandidate<TestTrigger>(
                        CreateValidOrderKey(),
                        null));
        }

        private static CombatTriggerOrderKey
            CreateValidOrderKey()
        {
            return new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Slot,
                CombatSide.Player,
                0,
                0);
        }

        private sealed class TestTrigger
        {
        }
    }
}