using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerPolarFerretIntegrationTests
    {
        [Test]
        public void Constructor_WithNullTargetResolver_Throws()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 3);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry,
                        environment.Runtime
                            .SourceDamageModifierRegistry,
                        null));
        }

        [Test]
        public void Constructor_ExposesRuntimeTargetResolver()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 3);

            Assert.That(
                environment.Runner
                    .TargetDamageReductionResolver,
                Is.SameAs(
                    environment.Runtime
                        .TargetDamageReductionResolver));

            Assert.That(
                environment.Runner
                    .TargetDamageReductionResolver
                    .ReductionRegistry,
                Is.SameAs(
                    environment.Runtime
                        .TargetDamageReductionRegistry));
        }

        [Test]
        public void
            StartAndResolveCombatStaged_WithPolarFerret_ReducesWinterCardsFirstDamage()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 3);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            var enemyAttackEvent =
                GetFirstAttackEvent(
                    environment.EventLog,
                    CombatSide.Enemy);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                enemyAttackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        environment.PolarFerret
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);

            Assert.That(
                environment.Runtime
                    .TargetDamageReductionRegistry
                    .Count,
                Is.Zero);
        }

        [Test]
        public void
            StartAndResolveCombatStaged_WithZeroDamage_DoesNotConsumePolarFerret()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 0);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        environment.PolarFerret
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.False);

            Assert.That(
                environment.Runtime
                    .TargetDamageReductionRegistry
                    .Count,
                Is.Zero);
        }

        private static TestEnvironment
            CreateEnvironment(
                int enemyAttack)
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "card.player.winter",
                    new InstanceId(1),
                    CombatCardSeason.Winter,
                    hp: 10,
                    attack: 1);

            var enemyCard =
                CreateCard(
                    "card.enemy.summer",
                    new InstanceId(101),
                    CombatCardSeason.Summer,
                    hp: 1,
                    attack: enemyAttack);

            var polarFerret =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .PolarFerret,
                    new InstanceId(1001));

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        playerCard,
                        playerPosition,
                        new SlotId(1),
                        new SlotId(2)),
                    CreateSide(
                        CombatSide.Enemy,
                        enemyCard,
                        enemyPosition,
                        new SlotId(3),
                        new SlotId(4)),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                polarFerret
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            Array.Empty<
                                CombatPetState>())));

            var runtime =
                new CombatPetTriggerRuntime();

            var sourceRegistry =
                runtime.BuildSourceRegistry(
                    state);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry,
                    runtime
                        .SourceDamageModifierRegistry,
                    runtime
                        .TargetDamageReductionResolver);

            return new TestEnvironment
            {
                State =
                    state,

                PlayerCard =
                    playerCard,

                PolarFerret =
                    polarFerret,

                Runtime =
                    runtime,

                MetadataFactory =
                    metadataFactory,

                EventLog =
                    eventLog,

                EventQueue =
                    eventQueue,

                SourceRegistry =
                    sourceRegistry,

                Runner =
                    runner
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState card,
            BoardPosition frontPosition,
            SlotId frontSlotId,
            SlotId backSlotId)
        {
            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    frontPosition.Column);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition,
                            card.InstanceId),

                        new CombatSlotState(
                            backSlotId,
                            backPosition)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            InstanceId instanceId,
            CombatCardSeason season,
            int hp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                instanceId,
                new CardRank(2),
                season,
                hpCapacity: hp,
                currentHp: hp,
                armor: 0,
                attack: attack);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static NormalAttackCombatEvent
            GetFirstAttackEvent(
                CombatEventLog eventLog,
                CombatSide attackerSide)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var attackEvent =
                    eventLog.Events[index]
                        as NormalAttackCombatEvent;

                if (attackEvent != null &&
                    attackEvent.AttackerSide ==
                    attackerSide)
                {
                    return attackEvent;
                }
            }

            throw new InvalidOperationException(
                $"Normal Attack event was not found " +
                $"for {attackerSide}.");
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

            public CombatPetState PolarFerret
            {
                get;
                set;
            }

            public CombatPetTriggerRuntime Runtime
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

            public CombatEventQueue EventQueue
            {
                get;
                set;
            }

            public CombatTriggerSourceRegistry
                SourceRegistry
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