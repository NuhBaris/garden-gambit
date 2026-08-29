using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardLookupTests
    {
        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardLookup(
                        null));
        }

        [Test]
        public void Get_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Lookup.Get(
                    null,
                    environment.PlayerCard.InstanceId));
        }

        [Test]
        public void Get_WithInvalidInstanceId_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Lookup.Get(
                    environment.State,
                    default(InstanceId)));
        }

        [Test]
        public void Get_WithActivePlayerCard_ReturnsActiveResult()
        {
            var environment =
                CreateEnvironment();

            var result =
                environment.Lookup.Get(
                    environment.State,
                    environment.PlayerCard.InstanceId);

            Assert.That(
                result.IsActive,
                Is.True);

            Assert.That(
                result.IsRemoved,
                Is.False);

            Assert.That(
                result.ActiveCard,
                Is.SameAs(
                    environment.PlayerCard));

            Assert.That(
                result.Position,
                Is.EqualTo(
                    environment.PlayerPosition));

            Assert.That(
                result.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                result.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.Unspecified));
        }

        [Test]
        public void Get_WithActiveEnemyCard_ReturnsActiveResult()
        {
            var environment =
                CreateEnvironment();

            var result =
                environment.Lookup.Get(
                    environment.State,
                    environment.EnemyCard.InstanceId);

            Assert.That(
                result.IsActive,
                Is.True);

            Assert.That(
                result.ActiveCard,
                Is.SameAs(
                    environment.EnemyCard));

            Assert.That(
                result.Position,
                Is.EqualTo(
                    environment.EnemyPosition));

            Assert.That(
                result.Position.Side,
                Is.EqualTo(
                    CombatSide.Enemy));
        }

        [Test]
        public void Get_AfterDirectDelete_ReturnsTombstoneResult()
        {
            var environment =
                CreateEnvironment();

            var parentEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                parentEvent);

            var resolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var deleteEvent =
                resolver.ApplyDirectDelete(
                    environment.State,
                    parentEvent,
                    environment.PlayerPosition);

            var result =
                environment.Lookup.Get(
                    environment.State,
                    environment.PlayerCard.InstanceId);

            Assert.That(
                result.IsActive,
                Is.False);

            Assert.That(
                result.IsRemoved,
                Is.True);

            Assert.That(
                result.ActiveCard,
                Is.Null);

            Assert.That(
                result.Tombstone,
                Is.SameAs(
                    environment.EventLog
                        .CardTombstones.Get(
                            environment.PlayerCard
                                .InstanceId)));

            Assert.That(
                result.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(
                    deleteEvent.HpAtDeletion));

            Assert.That(
                result.Position,
                Is.EqualTo(
                    environment.PlayerPosition));
        }

        [Test]
        public void Get_AfterDeathRemoval_ReturnsTombstoneResult()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 0);

            var deathEvent =
                new DeathCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    environment.PlayerCard.InstanceId,
                    environment.PlayerPosition,
                    3,
                    environment.PlayerCard.CurrentHp);

            environment.EventLog.Append(
                deathEvent);

            var resolver =
                new CombatDeathRemovalResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var removalEvent =
                resolver.TryApplyRemoval(
                    environment.State,
                    deathEvent);

            var result =
                environment.Lookup.Get(
                    environment.State,
                    environment.PlayerCard.InstanceId);

            Assert.That(
                removalEvent,
                Is.Not.Null);

            Assert.That(
                result.IsRemoved,
                Is.True);

            Assert.That(
                result.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DeathRemoval));

            Assert.That(
                result.CurrentHp,
                Is.Zero);

            Assert.That(
                result.Tombstone
                    .RemovalMetadata.EventId,
                Is.EqualTo(
                    removalEvent.Metadata.EventId));
        }

        [Test]
        public void Get_WithUnknownInstanceId_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<KeyNotFoundException>(
                () => environment.Lookup.Get(
                    environment.State,
                    new InstanceId(999)));
        }

        [Test]
        public void Get_WhenCardIsActiveAndHasTombstone_Throws()
        {
            var environment =
                CreateEnvironment();

            var rootMetadata =
                environment.MetadataFactory
                    .CreateRoot();

            var removalMetadata =
                environment.MetadataFactory
                    .CreateChild(
                        rootMetadata);

            var conflictingTombstone =
                new CombatCardTombstone(
                    environment.PlayerCard,
                    environment.PlayerPosition,
                    CombatCardRemovalReason.DirectDelete,
                    removalMetadata);

            environment.EventLog
                .CardTombstones.Append(
                    conflictingTombstone);

            Assert.Throws<InvalidOperationException>(
                () => environment.Lookup.Get(
                    environment.State,
                    environment.PlayerCard.InstanceId));
        }

        [Test]
        public void Get_WithRegisteredButUnplacedCard_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerOccupied: false);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);

            Assert.Throws<KeyNotFoundException>(
                () => environment.Lookup.Get(
                    environment.State,
                    environment.PlayerCard.InstanceId));
        }

        [Test]
        public void TryGet_WithActiveCard_ReturnsTrueAndActiveResult()
        {
            var environment =
                CreateEnvironment();

            CombatCardLookupResult result;

            var wasFound =
                environment.Lookup.TryGet(
                    environment.State,
                    environment.PlayerCard.InstanceId,
                    out result);

            Assert.That(
                wasFound,
                Is.True);

            Assert.That(
                result,
                Is.Not.Null);

            Assert.That(
                result.IsActive,
                Is.True);

            Assert.That(
                result.ActiveCard,
                Is.SameAs(
                    environment.PlayerCard));

            Assert.That(
                result.Position,
                Is.EqualTo(
                    environment.PlayerPosition));
        }

        [Test]
        public void TryGet_AfterDirectDelete_ReturnsTrueAndTombstoneResult()
        {
            var environment =
                CreateEnvironment();

            var parentEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                parentEvent);

            var resolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            resolver.ApplyDirectDelete(
                environment.State,
                parentEvent,
                environment.PlayerPosition);

            CombatCardLookupResult result;

            var wasFound =
                environment.Lookup.TryGet(
                    environment.State,
                    environment.PlayerCard.InstanceId,
                    out result);

            Assert.That(
                wasFound,
                Is.True);

            Assert.That(
                result,
                Is.Not.Null);

            Assert.That(
                result.IsRemoved,
                Is.True);

            Assert.That(
                result.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));

            Assert.That(
                result.Tombstone,
                Is.SameAs(
                    environment.EventLog
                        .CardTombstones.Get(
                            environment.PlayerCard
                                .InstanceId)));
        }

        [Test]
        public void TryGet_WithUnknownInstanceId_ReturnsFalseAndNullResult()
        {
            var environment =
                CreateEnvironment();

            CombatCardLookupResult result;

            var wasFound =
                environment.Lookup.TryGet(
                    environment.State,
                    new InstanceId(999),
                    out result);

            Assert.That(
                wasFound,
                Is.False);

            Assert.That(
                result,
                Is.Null);
        }

        [Test]
        public void TryGet_WithRegisteredButUnplacedCard_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    playerOccupied: false);

            CombatCardLookupResult result;

            var wasFound =
                environment.Lookup.TryGet(
                    environment.State,
                    environment.PlayerCard.InstanceId,
                    out result);

            Assert.That(
                wasFound,
                Is.False);

            Assert.That(
                result,
                Is.Null);
        }

        [Test]
        public void TryGet_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    CombatCardLookupResult ignored;

                    environment.Lookup.TryGet(
                        null,
                        environment.PlayerCard.InstanceId,
                        out ignored);
                });
        }

        [Test]
        public void TryGet_WithInvalidInstanceId_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () =>
                {
                    CombatCardLookupResult ignored;

                    environment.Lookup.TryGet(
                        environment.State,
                        default(InstanceId),
                        out ignored);
                });
        }

        [Test]
        public void TryGet_WhenCardIsActiveAndHasTombstone_Throws()
        {
            var environment =
                CreateEnvironment();

            var rootMetadata =
                environment.MetadataFactory
                    .CreateRoot();

            var removalMetadata =
                environment.MetadataFactory
                    .CreateChild(
                        rootMetadata);

            var conflictingTombstone =
                new CombatCardTombstone(
                    environment.PlayerCard,
                    environment.PlayerPosition,
                    CombatCardRemovalReason.DirectDelete,
                    removalMetadata);

            environment.EventLog
                .CardTombstones.Append(
                    conflictingTombstone);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    CombatCardLookupResult ignored;

                    environment.Lookup.TryGet(
                        environment.State,
                        environment.PlayerCard.InstanceId,
                        out ignored);
                });
        }

        private static TestEnvironment
            CreateEnvironment(
                int playerCurrentHp = 5,
                bool playerOccupied = true)
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
                CreateCard(
                    "player-card",
                    100,
                    playerCurrentHp);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    200,
                    5);

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition,
                    playerCard,
                    playerOccupied);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition,
                    enemyCard,
                    true);

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

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                Lookup =
                    new CombatCardLookup(
                        eventLog)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position,
            CombatCardState card,
            bool occupied)
        {
            CombatSlotState slot;

            if (occupied)
            {
                slot =
                    new CombatSlotState(
                        slotId,
                        position,
                        card.InstanceId);
            }
            else
            {
                slot =
                    new CombatSlotState(
                        slotId,
                        position);
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        slot
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
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
                7,
                currentHp,
                1,
                3);
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

            public CombatCardState EnemyCard
            {
                get;
                set;
            }

            public BoardPosition PlayerPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyPosition
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

            public CombatCardLookup Lookup
            {
                get;
                set;
            }
        }
    }
}