using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackPreparationResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackPreparationResolver(
                            null,
                            new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackPreparationResolver(
                            CreateMetadataFactory(),
                            null));
        }

        [Test]
        public void Prepare_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Prepare(
                    null,
                    environment.PlayerPosition,
                    environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Prepare_WithInvalidPlayerPosition_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.Prepare(
                    environment.State,
                    default(BoardPosition),
                    environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Prepare_WithInvalidEnemyPosition_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.Prepare(
                    environment.State,
                    environment.PlayerPosition,
                    default(BoardPosition)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Prepare_WithEmptyPlayerSlot_ThrowsWithoutLog()
        {
            var environment =
                CreateEnvironment(
                    includePlayerCard: false,
                    includeEnemyCard: true);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Prepare(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Prepare_WithEmptyEnemySlot_ThrowsWithoutLog()
        {
            var environment =
                CreateEnvironment(
                    includePlayerCard: true,
                    includeEnemyCard: false);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Prepare(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Prepare_WithValidState_ReturnsBatch()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver.Prepare(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

            Assert.That(
                batch,
                Is.Not.Null);

            Assert.That(
                batch.ExchangeEvent,
                Is.Not.Null);

            Assert.That(
                batch.PlayerAttackEvent,
                Is.Not.Null);

            Assert.That(
                batch.EnemyAttackEvent,
                Is.Not.Null);

            Assert.That(
                batch.ExchangeEvent.PlayerInstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                batch.ExchangeEvent.EnemyInstanceId,
                Is.EqualTo(
                    environment.EnemyCard.InstanceId));
        }

        [Test]
        public void Prepare_AppendsDeterministicEventOrder()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver.Prepare(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    batch.ExchangeEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    batch.PlayerAttackEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    batch.EnemyAttackEvent));

            Assert.That(
                batch.ExchangeEvent
                    .Metadata.IsTriggerRoot,
                Is.True);

            Assert.That(
                batch.PlayerAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    batch.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                batch.EnemyAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    batch.ExchangeEvent
                        .Metadata.EventId));
        }

        [Test]
        public void Prepare_SnapshotsAttackWithoutApplyingDamage()
        {
            var environment =
                CreateEnvironment();

            var previousPlayerHp =
                environment.PlayerCard.CurrentHp;

            var previousEnemyHp =
                environment.EnemyCard.CurrentHp;

            var batch =
                environment.Resolver.Prepare(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

            Assert.That(
                batch.ExchangeEvent.PlayerAttack,
                Is.EqualTo(
                    environment.PlayerCard.Attack));

            Assert.That(
                batch.ExchangeEvent.EnemyAttack,
                Is.EqualTo(
                    environment.EnemyCard.Attack));

            Assert.That(
                batch.PlayerAttackEvent.BaseDamage,
                Is.EqualTo(
                    environment.PlayerCard.Attack));

            Assert.That(
                batch.EnemyAttackEvent.BaseDamage,
                Is.EqualTo(
                    environment.EnemyCard.Attack));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    previousPlayerHp));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    previousEnemyHp));

            Assert.That(
                environment.PlayerCard
                    .IsAtDeathThreshold,
                Is.False);

            Assert.That(
                environment.EnemyCard
                    .IsAtDeathThreshold,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includePlayerCard = true,
                bool includeEnemyCard = true)
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
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(1),
                    new CardRank(4),
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: 0,
                    attack: 5);

            var enemyCard =
                new CombatCardState(
                    new DefinitionId(
                        "enemy-card"),
                    new InstanceId(101),
                    new CardRank(6),
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: 0,
                    attack: 7);

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition,
                    playerCard,
                    includePlayerCard);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    new SlotId(101),
                    enemyPosition,
                    enemyCard,
                    includeEnemyCard);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            return new TestEnvironment
            {
                State =
                    new CombatState(
                        playerSide,
                        enemySide),
                PlayerCard =
                    playerCard,
                EnemyCard =
                    enemyCard,
                PlayerPosition =
                    playerPosition,
                EnemyPosition =
                    enemyPosition,
                EventLog =
                    eventLog,
                Resolver =
                    new
                        CombatNormalAttackPreparationResolver(
                            metadataFactory,
                            eventLog)
            };
        }

        private static CombatSideState
            CreateSide(
                CombatSide side,
                SlotId slotId,
                BoardPosition position,
                CombatCardState card,
                bool includeCard)
        {
            var cards =
                includeCard
                    ? new[]
                    {
                        card
                    }
                    : new CombatCardState[0];

            InstanceId? occupantInstanceId =
                includeCard
                    ? card.InstanceId
                    : (InstanceId?)null;

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
                            position,
                            occupantInstanceId)
                    }),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
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
            public CombatState State
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

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatNormalAttackPreparationResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}