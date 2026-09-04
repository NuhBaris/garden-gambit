using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatStartedCombatEventSnapshotTests
    {
        [Test]
        public void Constructor_WithSnapshot_SetsSnapshot()
        {
            var metadata =
                CreateRootMetadata();

            var snapshot =
                CreateSnapshot();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadata,
                    snapshot);

            Assert.That(
                combatStartedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatStarted));

            Assert.That(
                combatStartedEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                combatStartedEvent.BattleStartSnapshot,
                Is.SameAs(snapshot));

            Assert.That(
                combatStartedEvent
                    .HasBattleStartSnapshot,
                Is.True);
        }

        [Test]
        public void Constructor_WithoutSnapshot_PreservesLegacySupport()
        {
            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    CreateRootMetadata());

            Assert.That(
                combatStartedEvent
                    .BattleStartSnapshot,
                Is.Null);

            Assert.That(
                combatStartedEvent
                    .HasBattleStartSnapshot,
                Is.False);
        }

        [Test]
        public void Constructor_WithNullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatStartedCombatEvent(
                        CreateRootMetadata(),
                        null));
        }

        [Test]
        public void Constructor_WithNonRootMetadataAndSnapshot_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatStartedCombatEvent(
                        CreateChildMetadata(),
                        CreateSnapshot()));
        }

        private static CombatBattleStartSnapshot
            CreateSnapshot()
        {
            var player =
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    new CombatBattleStartCardSnapshot[0]);

            var enemy =
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    new CombatBattleStartCardSnapshot[0]);

            return new CombatBattleStartSnapshot(
                player,
                enemy);
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