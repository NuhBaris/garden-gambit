using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDirectDeleteTombstoneTests
    {
        [Test]
        public void ApplyDirectDelete_WithAliveCard_AppendsMatchingTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var deleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.PlayerPosition);

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            var tombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                tombstone.DefinitionId,
                Is.EqualTo(
                    environment.PlayerCard.DefinitionId));

            Assert.That(
                tombstone.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                tombstone.Rank,
                Is.EqualTo(
                    environment.PlayerCard.Rank));

            Assert.That(
                tombstone.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                tombstone.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                tombstone.Armor,
                Is.EqualTo(2));

            Assert.That(
                tombstone.Attack,
                Is.EqualTo(3));

            Assert.That(
                tombstone.LastPosition,
                Is.EqualTo(
                    environment.PlayerPosition));

            Assert.That(
                tombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));

            Assert.That(
                tombstone.RemovalMetadata.EventId,
                Is.EqualTo(
                    deleteEvent.Metadata.EventId));

            Assert.That(
                tombstone.RemovalMetadata.SequenceNo,
                Is.EqualTo(
                    deleteEvent.Metadata.SequenceNo));

            Assert.That(
                tombstone.RemovalMetadata.ParentEventId,
                Is.EqualTo(
                    deleteEvent.Metadata.ParentEventId));

            Assert.That(
                tombstone.RemovalMetadata.TriggerRootId,
                Is.EqualTo(
                    deleteEvent.Metadata.TriggerRootId));

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.False);

            Assert.That(
                environment.EventLog
                    .CardTombstones
                    .GetByRemovalEvent(
                        deleteEvent.Metadata.EventId),
                Is.SameAs(tombstone));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);
        }

        [Test]
        public void ApplyDirectDelete_WithZeroHp_SnapshotsDeathThreshold()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 0);

            var deleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.PlayerPosition);

            var tombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.Zero);

            Assert.That(
                tombstone.CurrentHp,
                Is.Zero);

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                tombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));
        }

        [Test]
        public void ApplyDirectDelete_WithNegativeHp_SnapshotsFinalHp()
        {
            var environment =
                CreateEnvironment(
                    currentHp: -3);

            var deleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.PlayerPosition);

            var tombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.EqualTo(-3));

            Assert.That(
                tombstone.CurrentHp,
                Is.EqualTo(-3));

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void ApplyDirectDelete_WithInvalidPosition_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        environment.ParentEvent,
                        default(BoardPosition)));

            AssertStateAndHistoryUnchanged(
                environment);
        }

        [Test]
        public void ApplyDirectDelete_WithUnloggedParent_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var unloggedParent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        unloggedParent,
                        environment.PlayerPosition));

            AssertStateAndHistoryUnchanged(
                environment);
        }

        [Test]
        public void ApplyDirectDelete_WithDifferentParentReference_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var differentParentReference =
                new TestCombatEvent(
                    environment.ParentEvent.Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        differentParentReference,
                        environment.PlayerPosition));

            AssertStateAndHistoryUnchanged(
                environment);
        }

        [Test]
        public void ApplyDirectDelete_WhenAlreadyDeleted_DoesNotAppendDuplicateTombstone()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var firstDeleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.PlayerPosition);

            var firstTombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        environment.ParentEvent,
                        environment.PlayerPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstDeleteEvent));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog
                    .CardTombstones
                    .Tombstones[0],
                Is.SameAs(firstTombstone));
        }

        [Test]
        public void ApplyDirectDelete_WhenTombstoneConflicts_ThrowsBeforeChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    currentHp: 5);

            var conflictingMetadata =
                environment.MetadataFactory
                    .CreateChild(
                        environment.ParentEvent.Metadata);

            var conflictingTombstone =
                new CombatCardTombstone(
                    environment.PlayerCard,
                    environment.PlayerPosition,
                    CombatCardRemovalReason.DirectDelete,
                    conflictingMetadata);

            environment.EventLog
                .CardTombstones.Append(
                    conflictingTombstone);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        environment.ParentEvent,
                        environment.PlayerPosition));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.PlayerSide.GetCardAt(
                    environment.PlayerPosition),
                Is.SameAs(
                    environment.PlayerCard));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog
                    .CardTombstones
                    .Tombstones[0],
                Is.SameAs(
                    conflictingTombstone));
        }

        private static TestEnvironment
            CreateEnvironment(
                int currentHp)
        {
            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerCard =
                new CombatCardState(
                    new DefinitionId("player-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    10,
                    currentHp,
                    2,
                    3);

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                playerPosition,
                                playerCard.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            playerCard
                        }),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var enemySide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Enemy,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(2),
                                enemyPosition)
                        }),
                    new CombatCardRegistry(
                        new CombatCardState[0]),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

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
                State = state,
                PlayerSide = playerSide,
                PlayerCard = playerCard,
                PlayerPosition = playerPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                ParentEvent = parentEvent,
                Resolver =
                    new CombatDirectDeleteResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static void
            AssertStateAndHistoryUnchanged(
                TestEnvironment environment)
        {
            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.PlayerSide.GetCardAt(
                    environment.PlayerPosition),
                Is.SameAs(
                    environment.PlayerCard));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
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
            public CombatState State
            {
                get;
                set;
            }

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatCardState PlayerCard
            {
                get;
                set;
            }

            public BoardPosition PlayerPosition
            {
                get;
                set;
            }

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

            public CombatDirectDeleteResolver Resolver
            {
                get;
                set;
            }
        }
    }
}