using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartStageResolverSnapshotTests
    {
        [Test]
        public void StartStage_WithSnapshot_CopiesToSlotStage()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: true);

            var stageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot);

            Assert.That(
                stageEvent.HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                stageEvent.BattleStartSnapshot,
                Is.SameAs(
                    environment.Snapshot));

            Assert.That(
                stageEvent.IsSlotStage,
                Is.True);
        }

        [Test]
        public void StartStage_WithSnapshot_CopiesToPetStage()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: true);

            environment.Resolver.StartStage(
                environment.CombatStartedEvent,
                CombatBattleStartStage.Slot);

            var petStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Pet);

            Assert.That(
                petStageEvent.HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                petStageEvent.BattleStartSnapshot,
                Is.SameAs(
                    environment.Snapshot));

            Assert.That(
                petStageEvent.IsPetStage,
                Is.True);
        }

        [Test]
        public void StartStage_WithSnapshot_CopiesSameSnapshotToEveryStage()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: true);

            var slotStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot);

            var petStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Pet);

            var cardStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Card);

            Assert.That(
                slotStageEvent.BattleStartSnapshot,
                Is.SameAs(
                    environment.Snapshot));

            Assert.That(
                petStageEvent.BattleStartSnapshot,
                Is.SameAs(
                    environment.Snapshot));

            Assert.That(
                cardStageEvent.BattleStartSnapshot,
                Is.SameAs(
                    environment.Snapshot));

            Assert.That(
                petStageEvent.BattleStartSnapshot,
                Is.SameAs(
                    slotStageEvent
                        .BattleStartSnapshot));

            Assert.That(
                cardStageEvent.BattleStartSnapshot,
                Is.SameAs(
                    petStageEvent
                        .BattleStartSnapshot));

            Assert.That(
                cardStageEvent.IsCardStage,
                Is.True);
        }

        [Test]
        public void StartStage_WithoutSnapshot_PreservesLegacyPath()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: false);

            var slotStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot);

            var petStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Pet);

            var cardStageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Card);

            Assert.That(
                slotStageEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                petStageEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                cardStageEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                slotStageEvent
                    .BattleStartSnapshot,
                Is.Null);

            Assert.That(
                petStageEvent
                    .BattleStartSnapshot,
                Is.Null);

            Assert.That(
                cardStageEvent
                    .BattleStartSnapshot,
                Is.Null);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeSnapshot)
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var snapshot =
                CreateSnapshot();

            var metadata =
                metadataFactory.CreateRoot();

            CombatStartedCombatEvent
                combatStartedEvent;

            if (includeSnapshot)
            {
                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadata,
                        snapshot);
            }
            else
            {
                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadata);
            }

            eventLog.Append(
                combatStartedEvent);

            return new TestEnvironment
            {
                Snapshot = snapshot,
                CombatStartedEvent =
                    combatStartedEvent,
                Resolver =
                    new CombatBattleStartStageResolver(
                        metadataFactory,
                        eventLog)
            };
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

        private sealed class TestEnvironment
        {
            public CombatBattleStartSnapshot Snapshot
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public CombatBattleStartStageResolver Resolver
            {
                get;
                set;
            }
        }
    }
}