using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        BattleEndStartedCombatEventTests
    {
        [Test]
        public void
            Constructor_WithDirectChildMetadata_SetsValues()
        {
            var metadata =
                CreateDirectChildMetadata();

            var battleEndEvent =
                new BattleEndStartedCombatEvent(
                    metadata);

            Assert.That(
                battleEndEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .BattleEndStarted));

            Assert.That(
                battleEndEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                battleEndEvent.Metadata.SequenceNo,
                Is.EqualTo(
                    metadata.SequenceNo));

            Assert.That(
                battleEndEvent.Metadata
                    .ParentEventId,
                Is.EqualTo(
                    metadata.ParentEventId));

            Assert.That(
                battleEndEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    metadata.TriggerRootId));

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.Null);
        }

        [Test]
        public void
            Constructor_WithSnapshot_ExposesExactSnapshot()
        {
            var snapshot =
                CreateEmptySnapshot();

            var battleEndEvent =
                new BattleEndStartedCombatEvent(
                    CreateDirectChildMetadata(),
                    snapshot);

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.SameAs(
                    snapshot));
        }

        [Test]
        public void
            Constructor_WithNullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new BattleEndStartedCombatEvent(
                        CreateDirectChildMetadata(),
                        null));
        }

        [Test]
        public void
            Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleEndStartedCombatEvent(
                        default(
                            CombatEventMetadata)));
        }

        [Test]
        public void
            Constructor_WithRootMetadata_Throws()
        {
            var rootEventId =
                new CombatEventId(1);

            var rootMetadata =
                new CombatEventMetadata(
                    rootEventId,
                    new CombatSequenceNumber(1),
                    null,
                    rootEventId);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleEndStartedCombatEvent(
                        rootMetadata));
        }

        [Test]
        public void
            Constructor_WithNonDirectRootChild_Throws()
        {
            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(3),
                    new CombatSequenceNumber(3),
                    new CombatEventId(2),
                    new CombatEventId(1));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleEndStartedCombatEvent(
                        metadata));
        }

        private static CombatEventMetadata
            CreateDirectChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }

        private static CombatBattleStartSnapshot
            CreateEmptySnapshot()
        {
            return new CombatBattleStartSnapshot(
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>()),
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>()));
        }
    }
}