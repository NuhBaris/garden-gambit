using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        BattleStartStageStartedCombatEventSnapshotTests
    {
        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Pet)]
        [TestCase(CombatBattleStartStage.Card)]
        public void Constructor_WithSnapshot_SetsState(
            CombatBattleStartStage stage)
        {
            var snapshot =
                CreateSnapshot();

            var stageEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectRootChildMetadata(),
                    stage,
                    snapshot);

            Assert.That(
                stageEvent.Stage,
                Is.EqualTo(stage));

            Assert.That(
                stageEvent.BattleStartSnapshot,
                Is.SameAs(snapshot));

            Assert.That(
                stageEvent.HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                stageEvent.IsSlotStage,
                Is.EqualTo(
                    stage ==
                    CombatBattleStartStage.Slot));

            Assert.That(
                stageEvent.IsPetStage,
                Is.EqualTo(
                    stage ==
                    CombatBattleStartStage.Pet));

            Assert.That(
                stageEvent.IsCardStage,
                Is.EqualTo(
                    stage ==
                    CombatBattleStartStage.Card));
        }

        [Test]
        public void Constructor_WithoutSnapshot_PreservesLegacySupport()
        {
            var stageEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectRootChildMetadata(),
                    CombatBattleStartStage.Pet);

            Assert.That(
                stageEvent.BattleStartSnapshot,
                Is.Null);

            Assert.That(
                stageEvent.HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                stageEvent.IsPetStage,
                Is.True);
        }

        [Test]
        public void Constructor_WithNullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new BattleStartStageStartedCombatEvent(
                        CreateDirectRootChildMetadata(),
                        CombatBattleStartStage.Pet,
                        null));
        }

        [Test]
        public void Constructor_WithNonDirectRootChildAndSnapshot_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleStartStageStartedCombatEvent(
                        CreateNonDirectRootChildMetadata(),
                        CombatBattleStartStage.Pet,
                        CreateSnapshot()));
        }

        private static CombatBattleStartSnapshot
            CreateSnapshot()
        {
            return new CombatBattleStartSnapshot(
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    new CombatBattleStartCardSnapshot[0]),
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    new CombatBattleStartCardSnapshot[0]));
        }

        private static CombatEventMetadata
            CreateDirectRootChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }

        private static CombatEventMetadata
            CreateNonDirectRootChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(3),
                new CombatSequenceNumber(3),
                new CombatEventId(2),
                rootEventId);
        }
    }
}