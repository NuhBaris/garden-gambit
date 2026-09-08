using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        BattleEndStartedCombatEventSnapshotTests
    {
        [Test]
        public void
            Constructor_WithoutSnapshot_ReportsNoSnapshot()
        {
            var battleEndEvent =
                new BattleEndStartedCombatEvent(
                    CreateMetadata());

            Assert.That(
                battleEndEvent.HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                battleEndEvent.BattleStartSnapshot,
                Is.Null);
        }

        [Test]
        public void
            Constructor_WithSnapshot_ExposesExactSnapshot()
        {
            var snapshot =
                CreateSnapshot();

            var battleEndEvent =
                new BattleEndStartedCombatEvent(
                    CreateMetadata(),
                    snapshot);

            Assert.That(
                battleEndEvent.HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                battleEndEvent.BattleStartSnapshot,
                Is.SameAs(
                    snapshot));

            Assert.That(
                battleEndEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.BattleEndStarted));
        }

        [Test]
        public void
            Constructor_WithNullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new BattleEndStartedCombatEvent(
                        CreateMetadata(),
                        null));
        }

        private static CombatEventMetadata
            CreateMetadata()
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
            CreateSnapshot()
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