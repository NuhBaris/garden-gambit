using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCompletionResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCompletionResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCompletionResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment(
                    applyBattleHealthChanges: false);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Resolve(
                    null,
                    environment.ResultEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithNullResultEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    applyBattleHealthChanges: false);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    null));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithUnloggedResultEvent_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var resultEvent =
                CreateResultEvent(
                    metadataFactory,
                    4,
                    6);

            var resolver =
                new CombatCompletionResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<ArgumentException>(
                () => resolver.Resolve(
                    CreateState(),
                    resultEvent));

            Assert.That(
                eventLog.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithDifferentLoggedInstance_Throws()
        {
            var environment =
                CreateEnvironment(
                    applyBattleHealthChanges: false);

            var differentInstance =
                new CombatResultCalculatedCombatEvent(
                    environment.ResultEvent.Metadata,
                    environment.ResultEvent.Calculation,
                    environment.ResultEvent
                        .ResolvedIncomingDamageToPlayer,
                    environment.ResultEvent
                        .ResolvedIncomingDamageToEnemy);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    differentInstance));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithAppliedResultDamage_AppendsCompletionSnapshot()
        {
            var environment =
                CreateEnvironment();

            var playerHealthBeforeCompletion =
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth;

            var enemyHealthBeforeCompletion =
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth;

            var completedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent);

            Assert.That(
                completedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatCompleted));

            Assert.That(
                completedEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                completedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ResultEvent
                        .Metadata.EventId));

            Assert.That(
                completedEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ResultEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(16)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(2L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(2L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(completedEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    playerHealthBeforeCompletion));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    enemyHealthBeforeCompletion));
        }

        [Test]
        public void Resolve_WithZeroDamage_CompletesWithoutHealthChangeEvents()
        {
            var environment =
                CreateEnvironment(
                    damageToPlayer: 0,
                    damageToEnemy: 0,
                    applyBattleHealthChanges: false);

            var completedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent);

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
                completedEvent.IsDraw,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(completedEvent));
        }

        [Test]
        public void Resolve_BeforeResultDamageIsApplied_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment(
                    applyBattleHealthChanges: false);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithMismatchedDamageChange_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment(
                    applyBattleHealthChanges: false);

            AppendBattleHealthChange(
                environment,
                CombatSide.Player,
                3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(17)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WhenChangeEventDoesNotMatchState_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment(
                    applyBattleHealthChanges: false);

            var changeMetadata =
                environment.MetadataFactory
                    .CreateChild(
                        environment.ResultEvent.Metadata);

            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    changeMetadata,
                    CombatSide.Player,
                    new BattleHealth(20),
                    new BattleHealth(16));

            environment.EventLog.Append(
                changeEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WhenCompletionAlreadyLogged_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            var firstCompletedEvent =
                environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Resolve(
                    environment.State,
                    environment.ResultEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(firstCompletedEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                int damageToPlayer = 4,
                int damageToEnemy = 6,
                bool applyBattleHealthChanges = true)
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var state =
                CreateState();

            var resultEvent =
                CreateResultEvent(
                    metadataFactory,
                    damageToPlayer,
                    damageToEnemy);

            eventLog.Append(
                resultEvent);

            var environment =
                new TestEnvironment
                {
                    State = state,
                    MetadataFactory = metadataFactory,
                    EventLog = eventLog,
                    ResultEvent = resultEvent,
                    Resolver =
                        new CombatCompletionResolver(
                            metadataFactory,
                            eventLog)
                };

            if (applyBattleHealthChanges)
            {
                if (damageToPlayer > 0)
                {
                    AppendBattleHealthChange(
                        environment,
                        CombatSide.Player,
                        damageToPlayer);
                }

                if (damageToEnemy > 0)
                {
                    AppendBattleHealthChange(
                        environment,
                        CombatSide.Enemy,
                        damageToEnemy);
                }
            }

            return environment;
        }

        private static void AppendBattleHealthChange(
            TestEnvironment environment,
            CombatSide side,
            int damage)
        {
            var sideState =
                environment.State.GetSide(side);

            var previousBattleHealth =
                sideState.BattleHealth;

            var currentBattleHealth =
                sideState.ApplyBattleHealthDamage(
                    damage);

            var metadata =
                environment.MetadataFactory.CreateChild(
                    environment.ResultEvent.Metadata);

            var changeEvent =
                new BattleHealthChangedCombatEvent(
                    metadata,
                    side,
                    previousBattleHealth,
                    currentBattleHealth);

            environment.EventLog.Append(
                changeEvent);
        }

        private static CombatResultCalculatedCombatEvent
            CreateResultEvent(
                CombatEventMetadataFactory metadataFactory,
                int damageToPlayer,
                int damageToEnemy)
        {
            var playerContribution =
                CreateContribution(
                    CombatSide.Player,
                    damageToEnemy);

            var enemyContribution =
                CreateContribution(
                    CombatSide.Enemy,
                    damageToPlayer);

            var calculation =
                new CombatResultDamageCalculation(
                    playerContribution,
                    enemyContribution);

            return new CombatResultCalculatedCombatEvent(
                metadataFactory.CreateRoot(),
                calculation,
                damageToPlayer,
                damageToEnemy);
        }

        private static CombatSideResultContribution
            CreateContribution(
                CombatSide side,
                int contribution)
        {
            var survivorCount =
                contribution > 0
                    ? 1
                    : 0;

            return new CombatSideResultContribution(
                side,
                survivorCount,
                contribution,
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player),
                CreateSideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState CreateSideState(
            CombatSide side)
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

            public CombatCompletionResolver Resolver
            {
                get;
                set;
            }
        }
    }
}