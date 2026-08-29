using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultCalculationResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultCalculationResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultCalculationResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void Resolve_WithNullState_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new CombatCardState[0],
                    1,
                    true);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Resolve(
                    null,
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithNullCombatStartedEvent_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new CombatCardState[0],
                    1,
                    true);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    null));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithValidState_LogsChildResultSnapshot()
        {
            var firstPlayerCard =
                CreateCard(
                    100,
                    2,
                    5);

            var secondPlayerCard =
                CreateCard(
                    101,
                    10,
                    7);

            var enemyCard =
                CreateCard(
                    200,
                    7,
                    4);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        firstPlayerCard,
                        secondPlayerCard
                    },
                    2,
                    new[]
                    {
                        enemyCard
                    },
                    3,
                    true);

            var previousPlayerBattleHealth =
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth;

            var previousEnemyBattleHealth =
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth;

            var resultEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.That(
                resultEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .CombatResultCalculated));

            Assert.That(
                resultEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                resultEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                resultEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                resultEvent.PlayerContribution
                    .SurvivorCount,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(12));

            Assert.That(
                resultEvent.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(24));

            Assert.That(
                resultEvent.EnemyContribution
                    .SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                resultEvent.EnemyContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                resultEvent.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(21));

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(21));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(24));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToPlayer,
                Is.EqualTo(21));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToEnemy,
                Is.EqualTo(24));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(resultEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    previousPlayerBattleHealth));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    previousEnemyBattleHealth));
        }

        [Test]
        public void Resolve_WithEmptyBoards_LogsValidZeroDamageResult()
        {
            var environment =
                CreateEnvironment(
                    new CombatCardState[0],
                    2,
                    new CombatCardState[0],
                    3,
                    true);

            var resultEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.That(
                resultEvent.Calculation.IsValid,
                Is.True);

            Assert.That(
                resultEvent.PlayerContribution
                    .SurvivorCount,
                Is.Zero);

            Assert.That(
                resultEvent.EnemyContribution
                    .SurvivorCount,
                Is.Zero);

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.ResolvedIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.ResolvedIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WithUnloggedCombatStartedEvent_ThrowsWithoutChangingTargetLog()
        {
            var externalEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new CombatCardState[0],
                    1,
                    true);

            var targetEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new CombatCardState[0],
                    1,
                    false);

            Assert.That(
                targetEnvironment.EventLog.Count,
                Is.Zero);

            Assert.Throws<ArgumentException>(
                () => targetEnvironment
                    .Resolver.Resolve(
                        targetEnvironment.State,
                        externalEnvironment
                            .CombatStartedEvent));

            Assert.That(
                targetEnvironment.EventLog.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithDifferentCombatStartedReferenceUsingSameEventId_Throws()
        {
            var targetEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new CombatCardState[0],
                    1,
                    true);

            var foreignEnvironment =
                CreateEnvironment(
                    new CombatCardState[0],
                    1,
                    new CombatCardState[0],
                    1,
                    true);

            Assert.That(
                foreignEnvironment
                    .CombatStartedEvent
                    .Metadata.EventId,
                Is.EqualTo(
                    targetEnvironment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                foreignEnvironment
                    .CombatStartedEvent,
                Is.Not.SameAs(
                    targetEnvironment
                        .CombatStartedEvent));

            Assert.Throws<ArgumentException>(
                () => targetEnvironment
                    .Resolver.Resolve(
                        targetEnvironment.State,
                        foreignEnvironment
                            .CombatStartedEvent));

            Assert.That(
                targetEnvironment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WhenResultAlreadyLogged_ThrowsWithoutAddingDuplicate()
        {
            var playerCard =
                CreateCard(
                    100,
                    8,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    6,
                    5);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    2,
                    new[] { enemyCard },
                    3,
                    true);

            var firstResultEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            var previousPlayerBattleHealth =
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth;

            var previousEnemyBattleHealth =
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstResultEvent));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .CombatResultCalculated),
                Is.EqualTo(1));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    previousPlayerBattleHealth));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    previousEnemyBattleHealth));
        }

        [Test]
        public void Resolve_WhenContributionOverflows_ThrowsWithoutChangingStateOrLog()
        {
            var playerCard =
                CreateCard(
                    100,
                    2,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    14,
                    10);

            var environment =
                CreateEnvironment(
                    new[] { playerCard },
                    1,
                    new[] { enemyCard },
                    int.MaxValue,
                    true);

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            var enemySide =
                environment.State.GetSide(
                    CombatSide.Enemy);

            var previousPlayerBattleHealth =
                playerSide.BattleHealth;

            var previousEnemyBattleHealth =
                enemySide.BattleHealth;

            Assert.Throws<OverflowException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .CombatResultCalculated),
                Is.Zero);

            Assert.That(
                playerCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                playerSide.BattleHealth,
                Is.EqualTo(
                    previousPlayerBattleHealth));

            Assert.That(
                enemySide.BattleHealth,
                Is.EqualTo(
                    previousEnemyBattleHealth));
        }

        private static TestEnvironment
            CreateEnvironment(
                IReadOnlyList<CombatCardState>
                    playerCards,
                int playerAttackMultiplier,
                IReadOnlyList<CombatCardState>
                    enemyCards,
                int enemyAttackMultiplier,
                bool startCombat)
        {
            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    playerCards,
                    1,
                    playerAttackMultiplier);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    enemyCards,
                    11,
                    enemyAttackMultiplier);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            CombatStartedCombatEvent
                combatStartedEvent = null;

            if (startCombat)
            {
                var startResolver =
                    new CombatStartResolver(
                        metadataFactory,
                        eventLog);

                combatStartedEvent =
                    startResolver.Start(
                        state);
            }

            return new TestEnvironment
            {
                State = state,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Resolver =
                    new CombatResultCalculationResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            IReadOnlyList<CombatCardState> cards,
            int firstSlotId,
            int attackMultiplier)
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
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
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
                    "result-event-card-" +
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

        private static int CountEventsOfKind(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
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

            public CombatResultCalculationResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}