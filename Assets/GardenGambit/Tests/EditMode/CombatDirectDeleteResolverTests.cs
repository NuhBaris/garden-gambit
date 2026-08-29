using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDirectDeleteResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDirectDeleteResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDirectDeleteResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void ApplyDirectDelete_WithPositiveHp_RemovesCardAndLogsChildEvent()
        {
            var environment =
                CreateEnvironment(
                    firstCardHp: 5);

            var deleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.FirstPosition);

            Assert.That(
                deleteEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                deleteEvent.InstanceId,
                Is.EqualTo(
                    environment.FirstCard.InstanceId));

            Assert.That(
                deleteEvent.Position,
                Is.EqualTo(
                    environment.FirstPosition));

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.EqualTo(5));

            Assert.That(
                deleteEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                deleteEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                deleteEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(deleteEvent));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.FirstPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                playerSide.Cards.Cards[0],
                Is.SameAs(environment.SecondCard));

            Assert.Throws<KeyNotFoundException>(
                () => playerSide.Cards.GetCard(
                    environment.FirstCard.InstanceId));
        }

        [Test]
        public void ApplyDirectDelete_WithHpAtZero_RemovesWithoutDeathEvent()
        {
            var environment =
                CreateEnvironment(
                    firstCardHp: 0);

            var deleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.FirstPosition);

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0].Kind,
                Is.EqualTo(
                    CombatEventKind.NormalAttack));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyDirectDelete_WithHpBelowZero_RemovesWithoutDeathEvent()
        {
            var environment =
                CreateEnvironment(
                    firstCardHp: -3);

            var deleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.FirstPosition);

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.EqualTo(-3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));
        }

        [Test]
        public void ApplyDirectDelete_WithInvalidPosition_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        environment.ParentEvent,
                        default(BoardPosition)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyDirectDelete_WithUnloggedParent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var unloggedParent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        unloggedParent,
                        environment.FirstPosition));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyDirectDelete_WithDifferentParentObjectUsingLoggedId_Throws()
        {
            var environment =
                CreateEnvironment();

            var differentParent =
                new TestCombatEvent(
                    environment.ParentEvent.Metadata);

            Assert.That(
                differentParent,
                Is.Not.SameAs(
                    environment.ParentEvent));

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        differentParent,
                        environment.FirstPosition));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyDirectDelete_WhenSameTargetAlreadyDeleted_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment();

            var firstDeleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.FirstPosition);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ApplyDirectDelete(
                        environment.State,
                        environment.ParentEvent,
                        environment.FirstPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstDeleteEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyDirectDelete_WithSameParent_AllowsDifferentTargetPositions()
        {
            var environment =
                CreateEnvironment();

            var firstDeleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.FirstPosition);

            var secondDeleteEvent =
                environment.Resolver.ApplyDirectDelete(
                    environment.State,
                    environment.ParentEvent,
                    environment.SecondPosition);

            Assert.That(
                firstDeleteEvent.Position,
                Is.EqualTo(
                    environment.FirstPosition));

            Assert.That(
                secondDeleteEvent.Position,
                Is.EqualTo(
                    environment.SecondPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.FirstPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.SecondPosition)
                    .IsOccupied,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                int firstCardHp = 5)
        {
            var firstPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var secondPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var firstCard =
                CreateCard(
                    "card.first",
                    100,
                    firstCardHp);

            var secondCard =
                CreateCard(
                    "card.second",
                    200,
                    5);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    300,
                    5);

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            firstPosition,
                            firstCard.InstanceId),
                        new CombatSlotState(
                            new SlotId(2),
                            secondPosition,
                            secondCard.InstanceId)
                    },
                    new[]
                    {
                        firstCard,
                        secondCard
                    });

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(3),
                            enemyPosition,
                            enemyCard.InstanceId)
                    },
                    new[] { enemyCard });

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

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
                FirstCard = firstCard,
                SecondCard = secondCard,
                FirstPosition = firstPosition,
                SecondPosition = secondPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                ParentEvent = parentEvent,
                Resolver =
                    new CombatDirectDeleteResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatSlotState[] slots,
            CombatCardState[] cards)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
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
            public CombatState State { get; set; }

            public CombatCardState FirstCard { get; set; }

            public CombatCardState SecondCard { get; set; }

            public BoardPosition FirstPosition { get; set; }

            public BoardPosition SecondPosition { get; set; }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog { get; set; }

            public CombatEvent ParentEvent { get; set; }

            public CombatDirectDeleteResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}