using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        NormalAttackExchangeResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new NormalAttackExchangeResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new NormalAttackExchangeResolver(
                        new CombatEventMetadataFactory(
                            new CombatEventIdAllocator(),
                            new CombatSequenceNumberAllocator()),
                        null));
        }

        [Test]
        public void Resolve_WithValidCards_AppliesBothDamagesAndLogsExchange()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 7,
                    playerArmor: 2,
                    playerAttack: 5,
                    enemyCurrentHp: 8,
                    enemyArmor: 3,
                    enemyAttack: 4);

            var exchangeEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

            Assert.That(
                exchangeEvent.PlayerInstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                exchangeEvent.PlayerAttack,
                Is.EqualTo(5));

            Assert.That(
                exchangeEvent.EnemyInstanceId,
                Is.EqualTo(
                    environment.EnemyCard.InstanceId));

            Assert.That(
                exchangeEvent.EnemyAttack,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyCard.Armor,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.PlayerCard.Armor,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(exchangeEvent));

            Assert.That(
                environment.EventLog.Events[1]
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    exchangeEvent.Metadata.EventId));

            Assert.That(
                environment.EventLog.Events[2]
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    exchangeEvent.Metadata.EventId));

            Assert.That(
                environment.EventLog.Events[1]
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    exchangeEvent.Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Events[2]
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    exchangeEvent.Metadata.TriggerRootId));
        }

        [Test]
        public void Resolve_WhenBothAttacksAreLethal_AllowsMutualDeath()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 3,
                    playerArmor: 0,
                    playerAttack: 4,
                    enemyCurrentHp: 4,
                    enemyArmor: 0,
                    enemyAttack: 3);

            environment.Resolver.Resolve(
                environment.State,
                environment.PlayerPosition,
                environment.EnemyPosition);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.IsAtDeathThreshold,
                Is.True);

            Assert.That(
                environment.EnemyCard.IsAtDeathThreshold,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            var playerDeathEvent =
                environment.EventLog.Events[3]
                    as DeathCombatEvent;

            var enemyDeathEvent =
                environment.EventLog.Events[4]
                    as DeathCombatEvent;

            Assert.That(
                playerDeathEvent,
                Is.Not.Null);

            Assert.That(
                enemyDeathEvent,
                Is.Not.Null);

            Assert.That(
                playerDeathEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                enemyDeathEvent.InstanceId,
                Is.EqualTo(
                    environment.EnemyCard.InstanceId));

            Assert.That(
                playerDeathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.EventLog.Events[2]
                        .Metadata.EventId));

            Assert.That(
                enemyDeathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.EventLog.Events[1]
                        .Metadata.EventId));
        }

        [Test]
        public void Resolve_WithZeroAttacks_LogsBothDamageEventsWithoutChangingCards()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 7,
                    playerArmor: 2,
                    playerAttack: 0,
                    enemyCurrentHp: 8,
                    enemyArmor: 3,
                    enemyAttack: 0);

            var exchangeEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

            Assert.That(
                exchangeEvent.PlayerAttack,
                Is.Zero);

            Assert.That(
                exchangeEvent.EnemyAttack,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerCard.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.EnemyCard.Armor,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void Resolve_WhenSecondDamageWouldOverflow_ThrowsBeforeChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: int.MinValue,
                    playerArmor: 0,
                    playerAttack: 3,
                    enemyCurrentHp: 8,
                    enemyArmor: 0,
                    enemyAttack: 1);

            Assert.Throws<OverflowException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.PlayerPosition,
                    environment.EnemyPosition));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(int.MinValue));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithInvalidPlayerPosition_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    default(BoardPosition),
                    environment.EnemyPosition));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithPlayerSideEnemyPosition_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.PlayerPosition,
                    environment.PlayerPosition));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);
        }

        private static TestEnvironment CreateEnvironment(
            int playerCurrentHp = 7,
            int playerArmor = 2,
            int playerAttack = 5,
            int enemyCurrentHp = 8,
            int enemyArmor = 3,
            int enemyAttack = 4)
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
                    "card.player",
                    100,
                    playerCurrentHp,
                    playerArmor,
                    playerAttack);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    200,
                    enemyCurrentHp,
                    enemyArmor,
                    enemyAttack);

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
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                EventLog = eventLog,
                Resolver =
                    new NormalAttackExchangeResolver(
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
            int currentHp,
            int armor,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                armor,
                attack);
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public CombatCardState PlayerCard { get; set; }

            public CombatCardState EnemyCard { get; set; }

            public BoardPosition PlayerPosition { get; set; }

            public BoardPosition EnemyPosition { get; set; }

            public CombatEventLog EventLog { get; set; }

            public NormalAttackExchangeResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}