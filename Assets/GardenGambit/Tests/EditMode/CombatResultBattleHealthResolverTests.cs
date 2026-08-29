using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultBattleHealthResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultBattleHealthResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultBattleHealthResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void Apply_WithNullState_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20));

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Apply(
                    null,
                    environment.ResultEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Apply_WithNullResultEvent_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20));

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Apply(
                    environment.State,
                    null));

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Apply_WithMutualDamage_UpdatesBothSidesAndLogsPlayerThenEnemy()
        {
            var playerCard =
                CreateCard(
                    100,
                    8,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    5,
                    5);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    2,
                    new BattleHealth(20),
                    new[] { enemyCard },
                    3,
                    new BattleHealth(20));

            var events =
                environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent);

            Assert.That(
                events.Count,
                Is.EqualTo(2));

            var playerChangeEvent =
                events[0];

            var enemyChangeEvent =
                events[1];

            Assert.That(
                playerChangeEvent.Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(
                playerChangeEvent
                    .PreviousBattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                playerChangeEvent
                    .CurrentBattleHealth.Value,
                Is.EqualTo(5));

            Assert.That(
                playerChangeEvent.Delta,
                Is.EqualTo(-15L));

            Assert.That(
                enemyChangeEvent.Side,
                Is.EqualTo(CombatSide.Enemy));

            Assert.That(
                enemyChangeEvent
                    .PreviousBattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                enemyChangeEvent
                    .CurrentBattleHealth.Value,
                Is.EqualTo(4));

            Assert.That(
                enemyChangeEvent.Delta,
                Is.EqualTo(-16L));

            Assert.That(
                playerChangeEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ResultEvent
                        .Metadata.EventId));

            Assert.That(
                enemyChangeEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ResultEvent
                        .Metadata.EventId));

            Assert.That(
                playerChangeEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment.ResultEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                enemyChangeEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment.ResultEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(5));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(playerChangeEvent));

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(enemyChangeEvent));
        }

        [Test]
        public void Apply_WithOnlyEnemySurvivor_ChangesOnlyPlayerBattleHealth()
        {
            var enemyCard =
                CreateCard(
                    200,
                    5,
                    5);

            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20),
                    new[] { enemyCard },
                    2,
                    new BattleHealth(20));

            var events =
                environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent);

            Assert.That(
                events.Count,
                Is.EqualTo(1));

            Assert.That(
                events[0].Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(
                events[0].ChangedAmount,
                Is.EqualTo(10L));

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void Apply_WithOnlyPlayerSurvivor_ChangesOnlyEnemyBattleHealth()
        {
            var playerCard =
                CreateCard(
                    100,
                    6,
                    5);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    3,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20));

            var events =
                environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent);

            Assert.That(
                events.Count,
                Is.EqualTo(1));

            Assert.That(
                events[0].Side,
                Is.EqualTo(CombatSide.Enemy));

            Assert.That(
                events[0].ChangedAmount,
                Is.EqualTo(18L));

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void Apply_WithZeroDamage_ReturnsEmptyListWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    2,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    3,
                    new BattleHealth(20));

            var firstEvents =
                environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent);

            var secondEvents =
                environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent);

            Assert.That(
                firstEvents.Count,
                Is.Zero);

            Assert.That(
                secondEvents.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                CountBattleHealthChangeEvents(
                    environment.EventLog),
                Is.Zero);
        }

        [Test]
        public void Apply_WithUnloggedResultEvent_ThrowsWithoutChangingTargetLog()
        {
            var externalEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20));

            var targetEventLog =
                new CombatEventLog();

            var targetResolver =
                new CombatResultBattleHealthResolver(
                    CreateMetadataFactory(),
                    targetEventLog);

            Assert.Throws<ArgumentException>(
                () => targetResolver.Apply(
                    externalEnvironment.State,
                    externalEnvironment.ResultEvent));

            Assert.That(
                targetEventLog.Count,
                Is.Zero);

            Assert.That(
                externalEnvironment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                externalEnvironment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(20));
        }

        [Test]
        public void Apply_WithDifferentResultReferenceUsingSameEventId_Throws()
        {
            var targetEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20));

            var foreignEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20),
                    new CombatCardState[0],
                    1,
                    new BattleHealth(20));

            Assert.That(
                foreignEnvironment.ResultEvent
                    .Metadata.EventId,
                Is.EqualTo(
                    targetEnvironment.ResultEvent
                        .Metadata.EventId));

            Assert.That(
                foreignEnvironment.ResultEvent,
                Is.Not.SameAs(
                    targetEnvironment.ResultEvent));

            Assert.Throws<ArgumentException>(
                () => targetEnvironment
                    .Resolver.Apply(
                        targetEnvironment.State,
                        foreignEnvironment.ResultEvent));

            Assert.That(
                targetEnvironment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                targetEnvironment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                targetEnvironment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(20));
        }

        [Test]
        public void Apply_WhenDamageAlreadyApplied_ThrowsWithoutApplyingTwice()
        {
            var playerCard =
                CreateCard(
                    100,
                    8,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    5,
                    5);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    2,
                    new BattleHealth(20),
                    new[] { enemyCard },
                    3,
                    new BattleHealth(20));

            var firstEvents =
                environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent);

            Assert.That(
                firstEvents.Count,
                Is.EqualTo(2));

            var playerBattleHealthAfterFirstApply =
                environment.PlayerSide
                    .BattleHealth;

            var enemyBattleHealthAfterFirstApply =
                environment.EnemySide
                    .BattleHealth;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.PlayerSide.BattleHealth,
                Is.EqualTo(
                    playerBattleHealthAfterFirstApply));

            Assert.That(
                environment.EnemySide.BattleHealth,
                Is.EqualTo(
                    enemyBattleHealthAfterFirstApply));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                CountBattleHealthChangeEvents(
                    environment.EventLog),
                Is.EqualTo(2));
        }

        [Test]
        public void Apply_WhenEnemyBattleHealthWouldUnderflow_ThrowsBeforeChangingEitherSide()
        {
            var playerCard =
                CreateCard(
                    100,
                    2,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    2,
                    5);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    1,
                    new BattleHealth(20),
                    new[] { enemyCard },
                    1,
                    new BattleHealth(int.MinValue));

            Assert.Throws<OverflowException>(
                () => environment.Resolver.Apply(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(int.MinValue));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                CountBattleHealthChangeEvents(
                    environment.EventLog),
                Is.Zero);
        }

        [Test]
        public void Apply_WithDuplicateAllocatedEventId_ThrowsBeforeChangingEitherSide()
        {
            var playerCard =
                CreateCard(
                    100,
                    8,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    5,
                    5);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    2,
                    new BattleHealth(20),
                    new[] { enemyCard },
                    3,
                    new BattleHealth(20));

            var resolverWithFreshAllocators =
                new CombatResultBattleHealthResolver(
                    CreateMetadataFactory(),
                    environment.EventLog);

            Assert.Throws<InvalidOperationException>(
                () => resolverWithFreshAllocators.Apply(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.PlayerSide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EnemySide
                    .BattleHealth.Value,
                Is.EqualTo(20));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                CountBattleHealthChangeEvents(
                    environment.EventLog),
                Is.Zero);
        }

        private static TestEnvironment
            CreateEnvironment(
                IReadOnlyList<CombatCardState>
                    playerCards,
                int playerAttackMultiplier,
                BattleHealth playerBattleHealth,
                IReadOnlyList<CombatCardState>
                    enemyCards,
                int enemyAttackMultiplier,
                BattleHealth enemyBattleHealth)
        {
            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    playerCards,
                    1,
                    playerAttackMultiplier,
                    playerBattleHealth);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    enemyCards,
                    11,
                    enemyAttackMultiplier,
                    enemyBattleHealth);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var startResolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            var combatStartedEvent =
                startResolver.Start(
                    state);

            var resultCalculationResolver =
                new CombatResultCalculationResolver(
                    metadataFactory,
                    eventLog);

            var resultEvent =
                resultCalculationResolver.Resolve(
                    state,
                    combatStartedEvent);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                EnemySide = enemySide,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                ResultEvent = resultEvent,
                Resolver =
                    new CombatResultBattleHealthResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            IReadOnlyList<CombatCardState> cards,
            int firstSlotId,
            int attackMultiplier,
            BattleHealth battleHealth)
        {
            var slots =
                new List<CombatSlotState>();

            var nextSlotId =
                firstSlotId;

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                 BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

                var frontPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        column);

                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        column);

                var cardIndex =
                    columnValue -
                    BoardColumn.MinimumValue;

                if (cardIndex < cards.Count)
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition,
                            cards[cardIndex]
                                .InstanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition));
                }

                nextSlotId++;

                slots.Add(
                    new CombatSlotState(
                        new SlotId(
                            nextSlotId),
                        backPosition));

                nextSlotId++;
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                battleHealth,
                new AttackMultiplier(
                    attackMultiplier));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    "battle-health-card-" +
                    instanceId),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                10,
                currentHp,
                0,
                3);
        }

        private static
            CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private static int
            CountBattleHealthChangeEvents(
                CombatEventLog eventLog)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    CombatEventKind
                        .BattleHealthChanged)
                {
                    count++;
                }
            }

            return count;
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

            public CombatSideState EnemySide
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

            public CombatResultCalculatedCombatEvent
                ResultEvent
            {
                get;
                set;
            }

            public CombatResultBattleHealthResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}