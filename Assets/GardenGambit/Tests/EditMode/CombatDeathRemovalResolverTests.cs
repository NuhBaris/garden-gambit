using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDeathRemovalResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathRemovalResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathRemovalResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void TryApplyRemoval_WithDeadCard_RemovesCardAndLogsChildEvent()
        {
            var environment =
                CreateEnvironment();

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                removalEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

            Assert.That(
                removalEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                removalEvent.Position,
                Is.EqualTo(
                    environment.PlayerPosition));

            Assert.That(
                removalEvent.HpAtRemoval,
                Is.Zero);

            Assert.That(
                removalEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                removalEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.DeathEvent
                        .Metadata.EventId));

            Assert.That(
                removalEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.DeathEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(removalEvent));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Cards.Count,
                Is.Zero);

            Assert.Throws<KeyNotFoundException>(
                () => playerSide.Cards.GetCard(
                    environment.PlayerCard.InstanceId));
        }

        [Test]
        public void TryApplyRemoval_WithHpBelowZero_SnapshotsFinalHp()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: -3,
                    deathCurrentHp: -3);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                removalEvent.HpAtRemoval,
                Is.EqualTo(-3));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WhenCardIsAlive_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 1,
                    deathCurrentHp: 0);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryApplyRemoval_WhenDeathWasRescued_ReturnsNullWithoutRemovingCard()
        {
            var environment =
                CreateEnvironment();

            var rescueResolver =
                new CombatRescueResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var rescueEvent =
                rescueResolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                rescueEvent,
                Is.Not.Null);

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void TryApplyRemoval_WithUnloggedDeathEvent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var unloggedDeathEvent =
                new DeathCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    environment.PlayerCard.InstanceId,
                    environment.PlayerPosition,
                    3,
                    0);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryApplyRemoval(
                        environment.State,
                        unloggedDeathEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryApplyRemoval_WhenPositionContainsDifferentCard_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    deathInstanceId: 999);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryApplyRemoval(
                        environment.State,
                        environment.DeathEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerPosition)
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryApplyRemoval_WhenAlreadyResolved_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment();

            var firstRemovalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryApplyRemoval(
                        environment.State,
                        environment.DeathEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstRemovalEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WhenSameCardWasDirectDeleted_ReturnsNullWithoutSecondRemoval()
        {
            var environment =
                CreateEnvironment();

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var deleteEvent =
                directDeleteResolver.ApplyDirectDelete(
                    environment.State,
                    environment.DeathEvent,
                    environment.PlayerPosition);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                deleteEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(deleteEvent));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WhenDifferentCardWasDirectDeleted_StillRemovesDeathEventCard()
        {
            var environment =
                CreateEnvironment();

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var deleteEvent =
                directDeleteResolver.ApplyDirectDelete(
                    environment.State,
                    environment.DeathEvent,
                    enemyPosition);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                deleteEvent.InstanceId,
                Is.Not.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                removalEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                environment.EventLog.Events[2].Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WithDeadCard_AppendsMatchingTombstone()
        {
            var environment =
                CreateEnvironment();

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Not.Null);

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
                Is.Zero);

            Assert.That(
                tombstone.Armor,
                Is.Zero);

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
                    CombatCardRemovalReason.DeathRemoval));

            Assert.That(
                tombstone.RemovalMetadata.EventId,
                Is.EqualTo(
                    removalEvent.Metadata.EventId));

            Assert.That(
                tombstone.RemovalMetadata.SequenceNo,
                Is.EqualTo(
                    removalEvent.Metadata.SequenceNo));

            Assert.That(
                tombstone.RemovalMetadata.ParentEventId,
                Is.EqualTo(
                    removalEvent.Metadata.ParentEventId));

            Assert.That(
                tombstone.RemovalMetadata.TriggerRootId,
                Is.EqualTo(
                    removalEvent.Metadata.TriggerRootId));

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                environment.EventLog
                    .CardTombstones
                    .GetByRemovalEvent(
                        removalEvent.Metadata.EventId),
                Is.SameAs(tombstone));
        }

        [Test]
        public void TryApplyRemoval_WithHpBelowZero_SnapshotsHpInTombstone()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: -3,
                    deathCurrentHp: -3);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            var tombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.That(
                removalEvent.HpAtRemoval,
                Is.EqualTo(-3));

            Assert.That(
                tombstone.CurrentHp,
                Is.EqualTo(-3));

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryApplyRemoval_WhenCardIsAlive_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 1,
                    deathCurrentHp: 0);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WhenDeathWasRescued_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment();

            var rescueResolver =
                new CombatRescueResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            rescueResolver.ApplyRescue(
                environment.State,
                environment.DeathEvent);

            var removalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                removalEvent,
                Is.Null);

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WithUnloggedDeathEvent_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment();

            var unloggedDeathEvent =
                new DeathCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    environment.PlayerCard.InstanceId,
                    environment.PlayerPosition,
                    3,
                    0);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryApplyRemoval(
                        environment.State,
                        unloggedDeathEvent));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WhenPositionContainsDifferentCard_DoesNotAppendTombstone()
        {
            var environment =
                CreateEnvironment(
                    deathInstanceId: 999);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryApplyRemoval(
                        environment.State,
                        environment.DeathEvent));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void TryApplyRemoval_WhenAlreadyResolved_DoesNotAppendDuplicateTombstone()
        {
            var environment =
                CreateEnvironment();

            var firstRemovalEvent =
                environment.Resolver.TryApplyRemoval(
                    environment.State,
                    environment.DeathEvent);

            var firstTombstone =
                environment.EventLog
                    .CardTombstones.Get(
                        environment.PlayerCard.InstanceId);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryApplyRemoval(
                        environment.State,
                        environment.DeathEvent));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog
                    .CardTombstones
                    .Tombstones[0],
                Is.SameAs(firstTombstone));

            Assert.That(
                environment.EventLog
                    .CardTombstones
                    .GetByRemovalEvent(
                        firstRemovalEvent.Metadata.EventId),
                Is.SameAs(firstTombstone));
        }

        private static TestEnvironment
            CreateEnvironment(
                int playerCurrentHp = 0,
                int deathCurrentHp = 0,
                long deathInstanceId = 100)
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "card.player",
                    100,
                    playerCurrentHp);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    200,
                    5);

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition,
                    playerCard);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition,
                    enemyCard);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var deathEvent =
                new DeathCombatEvent(
                    metadataFactory.CreateRoot(),
                    new InstanceId(deathInstanceId),
                    playerPosition,
                    3,
                    deathCurrentHp);

            eventLog.Append(
                deathEvent);

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                PlayerPosition = playerPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                DeathEvent = deathEvent,
                Resolver =
                    new CombatDeathRemovalResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position,
            CombatCardState card)
        {
            var slot =
                new CombatSlotState(
                    slotId,
                    position,
                    card.InstanceId);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[] { slot }),
                new CombatCardRegistry(
                    new[] { card }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                0,
                3);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
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
            public CombatState State { get; set; }

            public CombatCardState PlayerCard { get; set; }

            public BoardPosition PlayerPosition { get; set; }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog { get; set; }

            public DeathCombatEvent DeathEvent { get; set; }

            public CombatDeathRemovalResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}