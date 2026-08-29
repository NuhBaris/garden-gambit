using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardRemovalCommitterTests
    {
        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardRemovalCommitter(
                        null));
        }

        [Test]
        public void EnsureCanCommit_WithValidDeathRemoval_DoesNotChangeHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 0);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var removalEvent =
                new DeathRemovalCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DeathRemoval,
                    metadata);

            Assert.DoesNotThrow(
                () => environment.Committer
                    .EnsureCanCommit(
                        removalEvent,
                        tombstone));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.CardTombstones.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.ContainsEvent(
                    removalEvent.Metadata.EventId),
                Is.False);
        }

        [Test]
        public void Commit_WithDeathRemoval_AppendsEventAndTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 0);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var removalEvent =
                new DeathRemovalCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DeathRemoval,
                    metadata);

            environment.Committer.Commit(
                removalEvent,
                tombstone);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(removalEvent));

            Assert.That(
                environment.EventLog.CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.CardTombstones.Get(
                    environment.Card.InstanceId),
                Is.SameAs(tombstone));

            Assert.That(
                environment.EventLog.CardTombstones
                    .GetByRemovalEvent(
                        removalEvent.Metadata.EventId),
                Is.SameAs(tombstone));
        }

        [Test]
        public void Commit_WithDirectDelete_AppendsEventAndTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            environment.Committer.Commit(
                deleteEvent,
                tombstone);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(deleteEvent));

            Assert.That(
                environment.EventLog.CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.CardTombstones.Get(
                    environment.Card.InstanceId),
                Is.SameAs(tombstone));
        }

        [Test]
        public void EnsureCanCommit_WithNullEvent_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            Assert.Throws<ArgumentNullException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        null,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithNullTombstone_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            Assert.Throws<ArgumentNullException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        deleteEvent,
                        null));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithDifferentMetadata_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var eventMetadata =
                CreateRemovalMetadata(
                    environment);

            var tombstoneMetadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    eventMetadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    tombstoneMetadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        deleteEvent,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithDifferentRemovalReason_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 0);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DeathRemoval,
                    metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        deleteEvent,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithDifferentInstanceId_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var differentCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 5);

            var tombstone =
                new CombatCardTombstone(
                    differentCard,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        deleteEvent,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithDifferentPosition_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var differentPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    differentPosition,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        deleteEvent,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithDifferentHp_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    4);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        deleteEvent,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void EnsureCanCommit_WithUnsupportedEvent_ThrowsWithoutChangingHistory()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var unsupportedEvent =
                new TestCombatEvent(
                    metadata);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer
                    .EnsureCanCommit(
                        unsupportedEvent,
                        tombstone));

            AssertHistoryUnchanged(environment);
        }

        [Test]
        public void Commit_WhenEventLogRejectsEvent_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var metadata =
                CreateRemovalMetadata(
                    environment);

            var existingEvent =
                new TestCombatEvent(
                    metadata);

            environment.EventLog.Append(
                existingEvent);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer.Commit(
                    deleteEvent,
                    tombstone));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(existingEvent));

            Assert.That(
                environment.EventLog.CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void Commit_WhenTombstoneRegistryRejects_DoesNotAppendSecondEvent()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var firstMetadata =
                CreateRemovalMetadata(
                    environment);

            var firstDeleteEvent =
                new DirectDeleteCombatEvent(
                    firstMetadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var firstTombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    firstMetadata);

            environment.Committer.Commit(
                firstDeleteEvent,
                firstTombstone);

            var secondRoot =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                secondRoot);

            var secondMetadata =
                environment.MetadataFactory
                    .CreateChild(
                        secondRoot.Metadata);

            var secondDeleteEvent =
                new DirectDeleteCombatEvent(
                    secondMetadata,
                    environment.Card.InstanceId,
                    environment.Position,
                    environment.Card.CurrentHp);

            var secondTombstone =
                new CombatCardTombstone(
                    environment.Card,
                    environment.Position,
                    CombatCardRemovalReason.DirectDelete,
                    secondMetadata);

            Assert.Throws<ArgumentException>(
                () => environment.Committer.Commit(
                    secondDeleteEvent,
                    secondTombstone));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(environment.ParentEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstDeleteEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(secondRoot));

            Assert.That(
                environment.EventLog.ContainsEvent(
                    secondDeleteEvent.Metadata.EventId),
                Is.False);

            Assert.That(
                environment.EventLog.CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.CardTombstones
                    .Tombstones[0],
                Is.SameAs(firstTombstone));
        }

        private static TestEnvironment CreateEnvironment(
            int currentHp)
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var parentEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                parentEvent);

            return new TestEnvironment
            {
                MetadataFactory =
                    metadataFactory,
                EventLog =
                    eventLog,
                ParentEvent =
                    parentEvent,
                Card =
                    CreateCard(
                        instanceId: 100,
                        currentHp: currentHp),
                Position =
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                Committer =
                    new CombatCardRemovalCommitter(
                        eventLog)
            };
        }

        private static CombatEventMetadata
            CreateRemovalMetadata(
                TestEnvironment environment)
        {
            return environment.MetadataFactory
                .CreateChild(
                    environment.ParentEvent.Metadata);
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                currentHp,
                1,
                3);
        }

        private static void AssertHistoryUnchanged(
            TestEnvironment environment)
        {
            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(environment.ParentEvent));

            Assert.That(
                environment.EventLog.CardTombstones.Count,
                Is.Zero);
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
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

            public TestCombatEvent ParentEvent
            {
                get;
                set;
            }

            public CombatCardState Card
            {
                get;
                set;
            }

            public BoardPosition Position
            {
                get;
                set;
            }

            public CombatCardRemovalCommitter Committer
            {
                get;
                set;
            }
        }
    }
}