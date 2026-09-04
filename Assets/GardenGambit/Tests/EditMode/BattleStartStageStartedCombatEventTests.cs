using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        BattleStartStageStartedCombatEventTests
    {
        [Test]
        public void Constructor_WithSlotStage_SetsValues()
        {
            var metadata =
                CreateDirectChildMetadata();

            var stageEvent =
                new BattleStartStageStartedCombatEvent(
                    metadata,
                    CombatBattleStartStage.Slot);

            Assert.That(
                stageEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .BattleStartStageStarted));

            Assert.That(
                stageEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                stageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                stageEvent.IsSlotStage,
                Is.True);

            Assert.That(
                stageEvent.IsPetStage,
                Is.False);

            Assert.That(
                stageEvent.IsCardStage,
                Is.False);
        }

        [Test]
        public void Constructor_WithPetStage_SetsValues()
        {
            var stageEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectChildMetadata(),
                    CombatBattleStartStage.Pet);

            Assert.That(
                stageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                stageEvent.IsSlotStage,
                Is.False);

            Assert.That(
                stageEvent.IsPetStage,
                Is.True);

            Assert.That(
                stageEvent.IsCardStage,
                Is.False);
        }

        [Test]
        public void Constructor_WithCardStage_SetsValues()
        {
            var stageEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectChildMetadata(),
                    CombatBattleStartStage.Card);

            Assert.That(
                stageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            Assert.That(
                stageEvent.IsSlotStage,
                Is.False);

            Assert.That(
                stageEvent.IsPetStage,
                Is.False);

            Assert.That(
                stageEvent.IsCardStage,
                Is.True);
        }

        [Test]
        public void
            Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new BattleStartStageStartedCombatEvent(
                        default(CombatEventMetadata),
                        CombatBattleStartStage.Pet));
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
                    new BattleStartStageStartedCombatEvent(
                        rootMetadata,
                        CombatBattleStartStage.Pet));
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
                    new BattleStartStageStartedCombatEvent(
                        metadata,
                        CombatBattleStartStage.Pet));
        }

        [Test]
        public void
            Constructor_WithUnspecifiedStage_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new BattleStartStageStartedCombatEvent(
                        CreateDirectChildMetadata(),
                        CombatBattleStartStage
                            .Unspecified));
        }

        [Test]
        public void
            Constructor_WithCompletedStage_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new BattleStartStageStartedCombatEvent(
                        CreateDirectChildMetadata(),
                        CombatBattleStartStage
                            .Completed));
        }

        [Test]
        public void
            Constructor_WithUndefinedStage_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new BattleStartStageStartedCombatEvent(
                        CreateDirectChildMetadata(),
                        (CombatBattleStartStage)999));
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
    }
}