using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerStagedNormalAttackTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        CreateEmptyState(),
                        CreateMetadataFactory(),
                        eventLog,
                        new CombatEventQueue(
                            eventLog),
                        CreateEmptySourceRegistry(),
                        null));
        }

        [Test]
        public void
            Constructor_WithSourceDamageModifierRegistry_ExposesExactRegistry()
        {
            var eventLog =
                new CombatEventLog();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var runner =
                new CombatResolutionRunner(
                    CreateEmptyState(),
                    CreateMetadataFactory(),
                    eventLog,
                    new CombatEventQueue(
                        eventLog),
                    CreateEmptySourceRegistry(),
                    modifierRegistry);

            Assert.That(
                runner.SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));
        }

        [Test]
        public void
            StartAndResolveCombatStaged_WithEmptyBoards_CompletesDraw()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: false,
                    enemyHasCard: false);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatCompleted),
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartAndResolveCombatStaged_AppendsSemanticNormalAttackEventsBeforeDamage()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 3,
                    enemyHp: 3,
                    playerAttack: 3,
                    enemyAttack: 3);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));

            var exchangeIndex =
                FindFirstEventIndex(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange);

            var normalAttackIndex =
                FindFirstEventIndex(
                    environment.EventLog,
                    CombatEventKind.NormalAttack);

            var damageIndex =
                FindFirstEventIndex(
                    environment.EventLog,
                    CombatEventKind.DamageApplied);

            Assert.That(
                exchangeIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                normalAttackIndex,
                Is.GreaterThan(
                    exchangeIndex));

            Assert.That(
                damageIndex,
                Is.GreaterThan(
                    normalAttackIndex));
        }

        [Test]
        public void
            StartAndResolveCombatStaged_WithSourceModifiers_AppliesModifiersBeforeDamage()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new SourceDamageModifierHandler(
                    modifierRegistry,
                    1);

            var source =
                new NormalAttackTriggerSource(
                    handler,
                    1);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        source
                    });

            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 3,
                    enemyHp: 3,
                    playerAttack: 2,
                    enemyAttack: 2,
                    sourceRegistry:
                        sourceRegistry,
                    modifierRegistry:
                        modifierRegistry);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));

            Assert.That(
                environment.State.Player.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void
            StartAndResolveCombat_LegacyPath_DoesNotAppendSemanticNormalAttackEvents()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 3,
                    enemyHp: 3,
                    playerAttack: 3,
                    enemyAttack: 3);

            environment.Runner
                .StartAndResolveCombat(
                    10,
                    100,
                    100,
                    100);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));
        }

        [Test]
        public void
            StartAndResolveCombatStaged_WhenExchangeBudgetExhausts_PreservesStagedCombat()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 7,
                    enemyHp: 7,
                    playerAttack: 3,
                    enemyAttack: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombatStaged(
                        1,
                        100,
                        100,
                        100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));
        }

        [Test]
        public void
            ResumeActiveCombatStaged_AfterExchangeBudgetExhaustion_CompletesWithoutRepeatingExchange()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 7,
                    enemyHp: 7,
                    playerAttack: 3,
                    enemyAttack: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombatStaged(
                        1,
                        100,
                        100,
                        100));

            var combatStartedEvent =
                environment.Runner
                    .ActiveCombatStartedEvent;

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(3));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(3));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(6));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(6));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    combatStartedEvent));

            Assert.That(
                environment.EventLog.Events[
                    environment.EventLog.Count - 1],
                Is.SameAs(
                    completedEvent));
        }

        [Test]
        public void
            ResumeActiveCombat_ForStagedCombat_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 7,
                    enemyHp: 7,
                    playerAttack: 3,
                    enemyAttack: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombatStaged(
                        1,
                        100,
                        100,
                        100));

            var eventCount =
                environment.EventLog.Count;

            var playerHp =
                environment.PlayerCard.CurrentHp;

            var enemyHp =
                environment.EnemyCard.CurrentHp;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCombat(
                        10,
                        100,
                        100,
                        100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCount));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    playerHp));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    enemyHp));
        }

        [Test]
        public void
            ResumeActiveCombatStaged_ForLegacyCombat_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 7,
                    enemyHp: 7,
                    playerAttack: 3,
                    enemyAttack: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        1,
                        100,
                        100,
                        100));

            var eventCount =
                environment.EventLog.Count;

            var playerHp =
                environment.PlayerCard.CurrentHp;

            var enemyHp =
                environment.EnemyCard.CurrentHp;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        100,
                        100,
                        100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCount));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    playerHp));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    enemyHp));
        }

        [Test]
        public void
            ResumeActiveCombatStaged_AfterTriggerBudgetExhaustion_DoesNotRepeatPreparedEventsOrDamage()
        {
            var handler =
                new CountingTriggerHandler();

            var source =
                new NormalAttackTriggerSource(
                    handler,
                    2);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        source
                    });

            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerHp: 3,
                    enemyHp: 3,
                    playerAttack: 3,
                    enemyAttack: 3,
                    sourceRegistry:
                        sourceRegistry);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombatStaged(
                        10,
                        100,
                        100,
                        1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.Zero);

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        100,
                        100,
                        10);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(4));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatCompleted),
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerHasCard,
                bool enemyHasCard,
                int playerHp = 7,
                int enemyHp = 7,
                int playerAttack = 3,
                int enemyAttack = 3,
                CombatTriggerSourceRegistry
                    sourceRegistry = null,
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry = null)
        {
            CombatCardState playerCard;
            CombatCardState enemyCard;

            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerHasCard,
                    playerHp,
                    playerAttack,
                    new SlotId(1),
                    new SlotId(2),
                    new InstanceId(100),
                    "player-card",
                    out playerCard);

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    enemyHasCard,
                    enemyHp,
                    enemyAttack,
                    new SlotId(3),
                    new SlotId(4),
                    new InstanceId(200),
                    "enemy-card",
                    out enemyCard);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            if (sourceRegistry == null)
            {
                sourceRegistry =
                    CreateEmptySourceRegistry();
            }

            if (modifierRegistry == null)
            {
                modifierRegistry =
                    new
                        CombatNormalAttackSourceDamageModifierRegistry();
            }

            return new TestEnvironment
            {
                State =
                    state,

                PlayerCard =
                    playerCard,

                EnemyCard =
                    enemyCard,

                EventLog =
                    eventLog,

                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry,
                        modifierRegistry)
            };
        }

        private static CombatState CreateEmptyState()
        {
            return new CombatState(
                CreateEmptySideState(
                    CombatSide.Player),
                CreateEmptySideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState
            CreateEmptySideState(
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

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                bool hasCard,
                int hp,
                int attack,
                SlotId frontSlotId,
                SlotId backSlotId,
                InstanceId instanceId,
                string definitionId,
                out CombatCardState card)
        {
            if (!hasCard)
            {
                card = null;

                return CreateEmptySideState(
                    side);
            }

            var frontPosition =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));

            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    new BoardColumn(1));

            card =
                new CombatCardState(
                    new DefinitionId(
                        definitionId),
                    instanceId,
                    new CardRank(2),
                    hp,
                    hp,
                    0,
                    attack);

            var frontSlot =
                new CombatSlotState(
                    frontSlotId,
                    frontPosition,
                    card.InstanceId);

            var backSlot =
                new CombatSlotState(
                    backSlotId,
                    backPosition);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        frontSlot,
                        backSlot
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

        private static CombatTriggerSourceRegistry
            CreateEmptySourceRegistry()
        {
            return new CombatTriggerSourceRegistry(
                Array.Empty<ICombatTriggerSource>());
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private static int CountEvents(
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

        private static int FindFirstEventIndex(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    return index;
                }
            }

            return -1;
        }

        private static
            CombatTriggerCandidate<
                ICombatTriggerHandler>
            CreateCandidate(
                ICombatTriggerHandler handler,
                int localOrder)
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        0,
                        localOrder),
                    handler);
        }

        private sealed class
            NormalAttackTriggerSource :
            ICombatTriggerSource
        {
            private readonly ICombatTriggerHandler
                _handler;

            private readonly int
                _candidateCount;

            public NormalAttackTriggerSource(
                ICombatTriggerHandler handler,
                int candidateCount)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException(
                        nameof(handler));
                }

                if (candidateCount <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(candidateCount));
                }

                _handler =
                    handler;

                _candidateCount =
                    candidateCount;
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                DiscoverTriggers(
                    CombatState state,
                    CombatEvent sourceEvent)
            {
                if (sourceEvent.Kind !=
                    CombatEventKind.NormalAttack)
                {
                    return Array.Empty<
                        CombatTriggerCandidate<
                            ICombatTriggerHandler>>();
                }

                var candidates =
                    new List<
                        CombatTriggerCandidate<
                            ICombatTriggerHandler>>();

                for (var index = 0;
                     index < _candidateCount;
                     index++)
                {
                    candidates.Add(
                        CreateCandidate(
                            _handler,
                            index));
                }

                return candidates;
            }
        }

        private sealed class
            CountingTriggerHandler :
            ICombatTriggerHandler
        {
            public int ResolveCallCount
            {
                get;
                private set;
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                return sourceEvent.Kind ==
                       CombatEventKind.NormalAttack;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;
            }
        }

        private sealed class
            SourceDamageModifierHandler :
            ICombatTriggerHandler
        {
            private readonly
                CombatNormalAttackSourceDamageModifierRegistry
                _modifierRegistry;

            private readonly int
                _modifier;

            public SourceDamageModifierHandler(
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry,
                int modifier)
            {
                if (modifierRegistry == null)
                {
                    throw new ArgumentNullException(
                        nameof(modifierRegistry));
                }

                _modifierRegistry =
                    modifierRegistry;

                _modifier =
                    modifier;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                return sourceEvent.Kind ==
                       CombatEventKind.NormalAttack;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;

                _modifierRegistry.AddModifier(
                    sourceEvent.Metadata.EventId,
                    _modifier);
            }
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

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatResolutionRunner Runner
            {
                get;
                set;
            }
        }
    }
}