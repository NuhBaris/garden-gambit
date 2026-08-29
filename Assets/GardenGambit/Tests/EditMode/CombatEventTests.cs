using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsProperties()
        {
            var metadata =
                CreateRootMetadata();

            var combatEvent =
                new TestCombatEvent(
                    metadata,
                    CombatEventKind.CombatStarted);

            Assert.That(
                combatEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                combatEvent.Metadata.SequenceNo,
                Is.EqualTo(metadata.SequenceNo));

            Assert.That(
                combatEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatStarted));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ = new TestCombatEvent(
                    default(CombatEventMetadata),
                    CombatEventKind.CombatStarted));
        }

        [Test]
        public void Constructor_WithUnspecifiedKind_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new TestCombatEvent(
                    CreateRootMetadata(),
                    CombatEventKind.Unspecified));
        }

        [Test]
        public void Constructor_WithUndefinedKind_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new TestCombatEvent(
                    CreateRootMetadata(),
                    (CombatEventKind)999));
        }

        private static CombatEventMetadata
            CreateRootMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata,
                CombatEventKind kind)
                : base(metadata, kind)
            {
            }
        }
    }
}