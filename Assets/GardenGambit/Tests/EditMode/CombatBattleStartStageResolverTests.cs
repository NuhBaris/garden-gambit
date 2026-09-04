using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartStageResolverTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartStageResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartStageResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void StartStage_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.StartStage(
                    null,
                    CombatBattleStartStage.Slot));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartStage_WithUnspecifiedStage_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage
                        .Unspecified));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartStage_WithCompletedStage_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage
                        .Completed));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartStage_WithUnloggedCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            var unloggedEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.StartStage(
                    unloggedEvent,
                    CombatBattleStartStage.Slot));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartStage_WithImpostorCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            var impostorEvent =
                new CombatStartedCombatEvent(
                    environment
                        .CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.StartStage(
                    impostorEvent,
                    CombatBattleStartStage.Slot));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartStage_WithSlotStage_AppendsChildEvent()
        {
            var environment =
                CreateEnvironment();

            var stageEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    stageEvent));

            Assert.That(
                stageEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .BattleStartStageStarted));

            Assert.That(
                stageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                stageEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                stageEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                stageEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                stageEvent.Metadata.SequenceNo >
                environment
                    .CombatStartedEvent
                    .Metadata.SequenceNo,
                Is.True);
        }

        [Test]
        public void
            StartStage_WithPetBeforeSlot_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Pet));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartStage_WithCardBeforePet_Throws()
        {
            var environment =
                CreateEnvironment();

            environment.Resolver.StartStage(
                environment.CombatStartedEvent,
                CombatBattleStartStage.Slot);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Card));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void
            StartStage_WithAllStages_AppendsDeterministicOrder()
        {
            var environment =
                CreateEnvironment();

            var slotEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot);

            var petEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Pet);

            var cardEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Card);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    slotEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    petEvent));

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(
                    cardEvent));

            Assert.That(
                slotEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                petEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                cardEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            Assert.That(
                slotEvent.Metadata.SequenceNo <
                petEvent.Metadata.SequenceNo,
                Is.True);

            Assert.That(
                petEvent.Metadata.SequenceNo <
                cardEvent.Metadata.SequenceNo,
                Is.True);
        }

        [Test]
        public void
            StartStage_WhenSlotAlreadyStarted_ThrowsWithoutDuplicate()
        {
            var environment =
                CreateEnvironment();

            var slotEvent =
                environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Slot));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    slotEvent));
        }

        [Test]
        public void
            StartStage_AfterCardStage_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            environment.Resolver.StartStage(
                environment.CombatStartedEvent,
                CombatBattleStartStage.Slot);

            environment.Resolver.StartStage(
                environment.CombatStartedEvent,
                CombatBattleStartStage.Pet);

            environment.Resolver.StartStage(
                environment.CombatStartedEvent,
                CombatBattleStartStage.Card);

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Resolver.StartStage(
                    environment.CombatStartedEvent,
                    CombatBattleStartStage.Card));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            return new TestEnvironment
            {
                MetadataFactory =
                    metadataFactory,

                EventLog =
                    eventLog,

                CombatStartedEvent =
                    combatStartedEvent,

                Resolver =
                    new CombatBattleStartStageResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
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

            public CombatBattleStartStageResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}