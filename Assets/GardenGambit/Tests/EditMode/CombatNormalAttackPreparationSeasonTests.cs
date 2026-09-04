using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackPreparationSeasonTests
    {
        [Test]
        public void
            Prepare_CopiesPlayerAndEnemySeasonsToAttackEvents()
        {
            var environment =
                CreateEnvironment(
                    CombatCardSeason.Summer,
                    CombatCardSeason.Winter);

            environment.Resolver.Prepare(
                environment.State,
                environment.PlayerPosition,
                environment.EnemyPosition);

            var playerAttack =
                GetAttackEvent(
                    environment.EventLog,
                    CombatSide.Player);

            var enemyAttack =
                GetAttackEvent(
                    environment.EventLog,
                    CombatSide.Enemy);

            Assert.That(
                playerAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                playerAttack.IsSummerAttack,
                Is.True);

            Assert.That(
                enemyAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                enemyAttack.IsWinterAttack,
                Is.True);
        }

        [Test]
        public void
            Prepare_WithLegacyCards_UsesUnspecifiedSeason()
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateLegacyCard(
                    "player-card",
                    new InstanceId(1));

            var enemyCard =
                CreateLegacyCard(
                    "enemy-card",
                    new InstanceId(2));

            var state =
                CreateState(
                    playerCard,
                    playerPosition,
                    enemyCard,
                    enemyPosition);

            var eventLog =
                new CombatEventLog();

            var resolver =
                new
                    CombatNormalAttackPreparationResolver(
                        CreateMetadataFactory(),
                        eventLog);

            resolver.Prepare(
                state,
                playerPosition,
                enemyPosition);

            var playerAttack =
                GetAttackEvent(
                    eventLog,
                    CombatSide.Player);

            var enemyAttack =
                GetAttackEvent(
                    eventLog,
                    CombatSide.Enemy);

            Assert.That(
                playerAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                enemyAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                playerAttack
                    .HasSpecifiedAttackerSeason,
                Is.False);

            Assert.That(
                enemyAttack
                    .HasSpecifiedAttackerSeason,
                Is.False);
        }

        [Test]
        public void
            Prepare_AppendsExchangeThenPlayerThenEnemyAttack()
        {
            var environment =
                CreateEnvironment(
                    CombatCardSeason.Spring,
                    CombatCardSeason.Autumn);

            environment.Resolver.Prepare(
                environment.State,
                environment.PlayerPosition,
                environment.EnemyPosition);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0].Kind,
                Is.EqualTo(
                    CombatEventKind
                        .NormalAttackExchange));

            var playerAttack =
                environment.EventLog.Events[1]
                    as NormalAttackCombatEvent;

            var enemyAttack =
                environment.EventLog.Events[2]
                    as NormalAttackCombatEvent;

            Assert.That(
                playerAttack,
                Is.Not.Null);

            Assert.That(
                enemyAttack,
                Is.Not.Null);

            Assert.That(
                playerAttack.AttackerSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                playerAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Spring));

            Assert.That(
                enemyAttack.AttackerSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                enemyAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Autumn));
        }

        [Test]
        public void
            AppendExchangeAttacks_WithExplicitSeasons_CopiesBothSeasons()
        {
            CombatEventLog eventLog;

            CombatEventMetadataFactory
                metadataFactory;

            var exchangeEvent =
                CreateLoggedExchange(
                    out eventLog,
                    out metadataFactory);

            var resolver =
                new CombatNormalAttackEventResolver(
                    metadataFactory,
                    eventLog);

            resolver.AppendExchangeAttacks(
                exchangeEvent,
                CombatCardSeason.Summer,
                CombatCardSeason.Winter);

            var playerAttack =
                GetAttackEvent(
                    eventLog,
                    CombatSide.Player);

            var enemyAttack =
                GetAttackEvent(
                    eventLog,
                    CombatSide.Enemy);

            Assert.That(
                playerAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                enemyAttack.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));
        }

        [Test]
        public void
            AppendExchangeAttacks_WithInvalidPlayerSeason_ThrowsWithoutAppendingAttacks()
        {
            CombatEventLog eventLog;

            CombatEventMetadataFactory
                metadataFactory;

            var exchangeEvent =
                CreateLoggedExchange(
                    out eventLog,
                    out metadataFactory);

            var resolver =
                new CombatNormalAttackEventResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => resolver
                    .AppendExchangeAttacks(
                        exchangeEvent,
                        (CombatCardSeason)(-1),
                        CombatCardSeason.Winter));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(
                    exchangeEvent));
        }

        [Test]
        public void
            AppendExchangeAttacks_WithInvalidEnemySeason_ThrowsWithoutAppendingAttacks()
        {
            CombatEventLog eventLog;

            CombatEventMetadataFactory
                metadataFactory;

            var exchangeEvent =
                CreateLoggedExchange(
                    out eventLog,
                    out metadataFactory);

            var resolver =
                new CombatNormalAttackEventResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => resolver
                    .AppendExchangeAttacks(
                        exchangeEvent,
                        CombatCardSeason.Summer,
                        (CombatCardSeason)5));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(
                    exchangeEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatCardSeason playerSeason,
                CombatCardSeason enemySeason)
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "player-card",
                    new InstanceId(1),
                    playerSeason);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    new InstanceId(2),
                    enemySeason);

            var state =
                CreateState(
                    playerCard,
                    playerPosition,
                    enemyCard,
                    enemyPosition);

            var eventLog =
                new CombatEventLog();

            return new TestEnvironment
            {
                State =
                    state,

                EventLog =
                    eventLog,

                PlayerPosition =
                    playerPosition,

                EnemyPosition =
                    enemyPosition,

                Resolver =
                    new
                        CombatNormalAttackPreparationResolver(
                            CreateMetadataFactory(),
                            eventLog)
            };
        }

        private static CombatState CreateState(
            CombatCardState playerCard,
            BoardPosition playerPosition,
            CombatCardState enemyCard,
            BoardPosition enemyPosition)
        {
            var player =
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
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var enemy =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Enemy,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(2),
                                enemyPosition,
                                enemyCard.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            enemyCard
                        }),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            return new CombatState(
                player,
                enemy);
        }

        private static CombatCardState CreateCard(
            string definitionId,
            InstanceId instanceId,
            CombatCardSeason season)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                instanceId,
                new CardRank(5),
                season,
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 4);
        }

        private static CombatCardState
            CreateLegacyCard(
                string definitionId,
                InstanceId instanceId)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                instanceId,
                new CardRank(5),
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 4);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static
            NormalAttackExchangeCombatEvent
            CreateLoggedExchange(
                out CombatEventLog eventLog,
                out CombatEventMetadataFactory
                    metadataFactory)
        {
            metadataFactory =
                CreateMetadataFactory();

            eventLog =
                new CombatEventLog();

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    metadataFactory.CreateRoot(),
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    playerAttack: 4,
                    new InstanceId(2),
                    CreatePosition(
                        CombatSide.Enemy),
                    enemyAttack: 5);

            eventLog.EnsureCanAppend(
                exchangeEvent);

            eventLog.Append(
                exchangeEvent);

            return exchangeEvent;
        }

        private static NormalAttackCombatEvent
            GetAttackEvent(
                CombatEventLog eventLog,
                CombatSide attackerSide)
        {
            NormalAttackCombatEvent
                result = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as NormalAttackCombatEvent;

                if (candidate == null ||
                    candidate.AttackerSide !=
                    attackerSide)
                {
                    continue;
                }

                if (result != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Normal Attack events " +
                        "were found for the same side.");
                }

                result =
                    candidate;
            }

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Normal Attack event was not found " +
                    $"for {attackerSide}.");
            }

            return result;
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

            public CombatEventLog EventLog
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

            public CombatNormalAttackPreparationResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}