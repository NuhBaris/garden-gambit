using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatRescueResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatRescueResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatRescueResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void ApplyRescue_WithValidDeathEvent_UpdatesCardAndLogsChildEvent()
        {
            var environment =
                CreateEnvironment();

            var rescueEvent =
                environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.IsAtDeathThreshold,
                Is.False);

            Assert.That(
                rescueEvent.Kind,
                Is.EqualTo(CombatEventKind.Rescue));

            Assert.That(
                rescueEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                rescueEvent.Position,
                Is.EqualTo(
                    environment.PlayerPosition));

            Assert.That(
                rescueEvent.PreviousHp,
                Is.Zero);

            Assert.That(
                rescueEvent.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                rescueEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                rescueEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.DeathEvent
                        .Metadata.EventId));

            Assert.That(
                rescueEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.DeathEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(rescueEvent));
        }

        [Test]
        public void ApplyRescue_WhenCardHpFellFurtherBelowZero_SnapshotsCurrentHp()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: -3,
                    deathCurrentHp: -3);

            var rescueEvent =
                environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                rescueEvent.PreviousHp,
                Is.EqualTo(-3));

            Assert.That(
                rescueEvent.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyRescue_WithNullState_ThrowsWithoutChangingCardOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.ApplyRescue(
                    null,
                    environment.DeathEvent));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyRescue_WithNullDeathEvent_ThrowsWithoutChangingCardOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.ApplyRescue(
                    environment.State,
                    null));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyRescue_WithUnloggedDeathEvent_ThrowsWithoutChangingCardOrLog()
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
                () => environment.Resolver.ApplyRescue(
                    environment.State,
                    unloggedDeathEvent));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyRescue_WhenPositionContainsDifferentCard_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    deathInstanceId: 999);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyRescue_WhenCardIsAlreadyAlive_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 1,
                    deathCurrentHp: 0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyRescue_WhenAlreadyResolved_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment();

            var firstRescueEvent =
                environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstRescueEvent));
        }

        [Test]
        public void ApplyRescue_WhenSameCardWasDirectDeleted_ThrowsWithoutLoggingRescue()
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

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent));

            Assert.That(
                deleteEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);
        }

        [Test]
        public void ApplyRescue_WhenDifferentCardWasDirectDeleted_StillRescuesDeathEventCard()
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

            var rescueEvent =
                environment.Resolver.ApplyRescue(
                    environment.State,
                    environment.DeathEvent);

            Assert.That(
                deleteEvent.InstanceId,
                Is.Not.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                rescueEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

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
                    CombatEventKind.Rescue));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);
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
                    new CombatRescueResolver(
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

            public CombatRescueResolver Resolver { get; set; }
        }
    }
}