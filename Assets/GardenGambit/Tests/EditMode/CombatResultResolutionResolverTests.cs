using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultResolutionResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultResolutionResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultResolutionResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 3,
                    playerMultiplier: 2,
                    enemyRank: 2,
                    enemyMultiplier: 1);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Resolve(
                    null,
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 3,
                    playerMultiplier: 2,
                    enemyRank: 2,
                    enemyMultiplier: 1);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    null));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithUnloggedCombatStartedEvent_ThrowsWithoutChangingState()
        {
            var state =
                CreateState(
                    playerRank: 3,
                    playerMultiplier: 2,
                    enemyRank: 2,
                    enemyMultiplier: 1);

            var sourceMetadataFactory =
                CreateMetadataFactory();

            var sourceEventLog =
                new CombatEventLog();

            var sourceStartResolver =
                new CombatStartResolver(
                    sourceMetadataFactory,
                    sourceEventLog);

            var combatStartedEvent =
                sourceStartResolver.Start(
                    state);

            var targetMetadataFactory =
                CreateMetadataFactory();

            var targetEventLog =
                new CombatEventLog();

            var resolver =
                new CombatResultResolutionResolver(
                    targetMetadataFactory,
                    targetEventLog);

            Assert.Throws<ArgumentException>(
                () => resolver.Resolve(
                    state,
                    combatStartedEvent));

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                targetEventLog.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithPlayerAdvantage_CompletesPlayerVictoryPipeline()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 3,
                    playerMultiplier: 2,
                    enemyRank: 2,
                    enemyMultiplier: 1);

            var completedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(4L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(4L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.True);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    CombatResultCalculatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<
                    BattleHealthChangedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[3],
                Is.TypeOf<
                    BattleHealthChangedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[4],
                Is.SameAs(completedEvent));

            var resultEvent =
                (CombatResultCalculatedCombatEvent)
                    environment.EventLog.Events[1];

            Assert.That(
                resultEvent
                    .BaseIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent
                    .BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resultEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                completedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    resultEvent.Metadata.EventId));

            Assert.That(
                completedEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.TriggerRootId));
        }

        [Test]
        public void Resolve_WithEnemyAdvantage_CompletesEnemyVictoryPipeline()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 2,
                    playerMultiplier: 1,
                    enemyRank: 3,
                    enemyMultiplier: 2);

            var completedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.EnemyVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(-4L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(4L));

            Assert.That(
                completedEvent.IsEnemyVictory,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void Resolve_WithEqualContributions_CompletesDrawPipeline()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 4,
                    playerMultiplier: 1,
                    enemyRank: 4,
                    enemyMultiplier: 1);

            var completedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(16)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(16)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.Zero);

            Assert.That(
                completedEvent.WinningMargin,
                Is.Zero);

            Assert.That(
                completedEvent.IsDraw,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void Resolve_WithNoSurvivors_CompletesWithoutHealthChangeEvents()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 0,
                    playerMultiplier: 1,
                    enemyRank: 0,
                    enemyMultiplier: 1);

            var completedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.TypeOf<
                    CombatStartedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    CombatResultCalculatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(completedEvent));

            Assert.That(
                environment.EventLog.Events,
                Has.None.TypeOf<
                    BattleHealthChangedCombatEvent>());
        }

        [Test]
        public void Resolve_WhenCalledAgain_ThrowsWithoutApplyingDamageAgain()
        {
            var environment =
                CreateEnvironment(
                    playerRank: 3,
                    playerMultiplier: 2,
                    enemyRank: 2,
                    enemyMultiplier: 1);

            var firstCompletedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.CombatStartedEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Events[4],
                Is.SameAs(firstCompletedEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                int playerRank,
                int playerMultiplier,
                int enemyRank,
                int enemyMultiplier)
        {
            var state =
                CreateState(
                    playerRank,
                    playerMultiplier,
                    enemyRank,
                    enemyMultiplier);

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

            return new TestEnvironment
            {
                State = state,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Resolver =
                    new CombatResultResolutionResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatState CreateState(
            int playerRank,
            int playerMultiplier,
            int enemyRank,
            int enemyMultiplier)
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerRank,
                    playerMultiplier,
                    new SlotId(1),
                    new InstanceId(100),
                    "player-card");

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    enemyRank,
                    enemyMultiplier,
                    new SlotId(2),
                    new InstanceId(200),
                    "enemy-card");

            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState CreateSideState(
            CombatSide side,
            int rank,
            int attackMultiplier,
            SlotId slotId,
            InstanceId instanceId,
            string definitionId)
        {
            if (rank == 0)
            {
                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        Array.Empty<CombatSlotState>()),
                    new CombatCardRegistry(
                        Array.Empty<CombatCardState>()),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        attackMultiplier));
            }

            var position =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));

            var card =
                new CombatCardState(
                    new DefinitionId(definitionId),
                    instanceId,
                    new CardRank(rank),
                    7,
                    7,
                    0,
                    3);

            var slot =
                new CombatSlotState(
                    slotId,
                    position,
                    card.InstanceId);

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
                    attackMultiplier));
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

            public CombatResultResolutionResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}