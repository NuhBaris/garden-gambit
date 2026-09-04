using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackRunnerTargetReductionTests
    {
        [Test]
        public void Constructor_WithNullTargetResolver_Throws()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 3);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventResolutionEngine,
                        environment.SourceModifierRegistry,
                        null));
        }

        [Test]
        public void Constructor_ExposesExactTargetResolver()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 3);

            Assert.That(
                environment.Runner
                    .TargetDamageReductionResolver,
                Is.SameAs(
                    environment.TargetResolver));
        }

        [Test]
        public void
            StartAndResolve_WithPolarFerret_ReducesFirstWinterTargetDamage()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 3);

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    maximumPassCount: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);
        }

        [Test]
        public void
            StartAndResolve_ZeroDamageDoesNotConsumeThenNextPositiveDamageReduces()
        {
            var environment =
                CreateEnvironment(
                    enemyAttack: 0);

            environment.Runner.StartAndResolve(
                environment.PlayerPosition,
                environment.EnemyPosition,
                maximumPassCount: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);

            environment.EnemyCard.ApplyAttackGain(
                amount: 2);

            environment.Runner.StartAndResolve(
                environment.PlayerPosition,
                environment.EnemyPosition,
                maximumPassCount: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);

            Assert.That(
                environment.ReductionRegistry.Count,
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
                    instanceId: 1,
                    CombatCardSeason.Winter,
                    attack: 0);

            var enemyCard =
                CreateCard(
                    "card.enemy.summer",
                    instanceId: 101,
                    CombatCardSeason.Summer,
                    attack: enemyAttack);

            var pet =
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
                                pet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            Array.Empty<
                                CombatPetState>())));

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        new
                            CombatPetCardTriggerUsageRegistry());

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var targetResolver =
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        reductionRegistry,
                        usageCommitter);

            var sourceModifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var polarFerretSource =
                new PolarFerretPetTriggerSource(
                    CombatSide.Player,
                    pet.InstanceId,
                    usageCommitter,
                    reductionRegistry);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        polarFerretSource
                    });

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var eventResolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var runner =
                new CombatNormalAttackRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    sourceModifierRegistry,
                    targetResolver);

            return new TestEnvironment
            {
                State =
                    state,

                PlayerCard =
                    playerCard,

                EnemyCard =
                    enemyCard,

                Pet =
                    pet,

                PlayerPosition =
                    playerPosition,

                EnemyPosition =
                    enemyPosition,

                MetadataFactory =
                    metadataFactory,

                EventLog =
                    eventLog,

                EventResolutionEngine =
                    eventResolutionEngine,

                SourceModifierRegistry =
                    sourceModifierRegistry,

                UsageCommitter =
                    usageCommitter,

                ReductionRegistry =
                    reductionRegistry,

                TargetResolver =
                    targetResolver,

                Runner =
                    runner
            };
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            CombatCardSeason season,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                season,
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: attack);
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

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
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

            public CombatPetState Pet
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

            public CombatEventResolutionEngine
                EventResolutionEngine
            {
                get;
                set;
            }

            public
                CombatNormalAttackSourceDamageModifierRegistry
                SourceModifierRegistry
            {
                get;
                set;
            }

            public CombatPetCardTriggerUsageCommitter
                UsageCommitter
            {
                get;
                set;
            }

            public
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry
            {
                get;
                set;
            }

            public
                CombatNormalAttackTargetDamageReductionResolver
                TargetResolver
            {
                get;
                set;
            }

            public CombatNormalAttackRunner Runner
            {
                get;
                set;
            }
        }
    }
}