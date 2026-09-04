using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        SunBirdCombatResolutionIntegrationTests
    {
        [Test]
        public void
            StagedCombat_WithPlayerSunBird_AddsOneToSummerCardsFirstAttack()
        {
            var environment =
                CreateEnvironment(
                    playerSeason:
                        CombatCardSeason.Summer,
                    enemySeason:
                        CombatCardSeason.Winter,
                    playerHp: 10,
                    enemyHp: 3,
                    playerAttack: 2,
                    enemyAttack: 0,
                    playerSunBirdCount: 1,
                    enemySunBirdCount: 0);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        10,
                        100,
                        100,
                        100);

            var playerAttacks =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Player);

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
                playerAttacks.Count,
                Is.EqualTo(1));

            Assert.That(
                playerAttacks[0].IsSummerAttack,
                Is.True);

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        playerAttacks[0]
                            .Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.ModifierRegistry
                    .ResolveDamage(
                        playerAttacks[0]),
                Is.EqualTo(3));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.PlayerPets[0]
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);
        }

        [Test]
        public void
            StagedCombat_WithNonSummerCard_DoesNotApplySunBirdBonus()
        {
            var environment =
                CreateEnvironment(
                    playerSeason:
                        CombatCardSeason.Winter,
                    enemySeason:
                        CombatCardSeason.Winter,
                    playerHp: 10,
                    enemyHp: 3,
                    playerAttack: 2,
                    enemyAttack: 0,
                    playerSunBirdCount: 1,
                    enemySunBirdCount: 0);

            environment.Runner
                .StartAndResolveCombatStaged(
                    10,
                    100,
                    100,
                    100);

            var playerAttacks =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Player);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(2));

            Assert.That(
                playerAttacks.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.ModifierRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.PlayerPets[0]
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.False);

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void
            StagedCombat_WithRepeatedSummerAttacks_AppliesBonusOnlyToFirstAttack()
        {
            var environment =
                CreateEnvironment(
                    playerSeason:
                        CombatCardSeason.Summer,
                    enemySeason:
                        CombatCardSeason.Winter,
                    playerHp: 10,
                    enemyHp: 5,
                    playerAttack: 1,
                    enemyAttack: 0,
                    playerSunBirdCount: 1,
                    enemySunBirdCount: 0);

            environment.Runner
                .StartAndResolveCombatStaged(
                    10,
                    100,
                    100,
                    100);

            var playerAttacks =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Player);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(4));

            Assert.That(
                playerAttacks.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        playerAttacks[0]
                            .Metadata.EventId),
                Is.EqualTo(1));

            for (var index = 1;
                 index < playerAttacks.Count;
                 index++)
            {
                Assert.That(
                    environment.ModifierRegistry
                        .GetTotalModifier(
                            playerAttacks[index]
                                .Metadata.EventId),
                    Is.Zero);
            }

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.PlayerPets[0]
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void
            StagedCombat_WithTwoSunBirds_StacksOneBonusFromEachPet()
        {
            var environment =
                CreateEnvironment(
                    playerSeason:
                        CombatCardSeason.Summer,
                    enemySeason:
                        CombatCardSeason.Winter,
                    playerHp: 10,
                    enemyHp: 3,
                    playerAttack: 1,
                    enemyAttack: 0,
                    playerSunBirdCount: 2,
                    enemySunBirdCount: 0);

            environment.Runner
                .StartAndResolveCombatStaged(
                    10,
                    100,
                    100,
                    100);

            var playerAttacks =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Player);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                playerAttacks.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        playerAttacks[0]
                            .Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                environment.ModifierRegistry
                    .ResolveDamage(
                        playerAttacks[0]),
                Is.EqualTo(3));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.PlayerPets[0]
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.PlayerPets[1]
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId),
                Is.True);
        }

        [Test]
        public void
            ResumeStagedCombat_AfterSunBirdTriggerBudgetExhaustion_DoesNotRepeatAttackOrModifier()
        {
            var environment =
                CreateEnvironment(
                    playerSeason:
                        CombatCardSeason.Summer,
                    enemySeason:
                        CombatCardSeason.Winter,
                    playerHp: 10,
                    enemyHp: 3,
                    playerAttack: 1,
                    enemyAttack: 0,
                    playerSunBirdCount: 2,
                    enemySunBirdCount: 0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombatStaged(
                        10,
                        100,
                        100,
                        1));

            var playerAttacksBeforeResume =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Player);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                playerAttacksBeforeResume.Count,
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

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        playerAttacksBeforeResume[0]
                            .Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                CountTriggeredPets(
                    environment),
                Is.EqualTo(1));

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        100,
                        100,
                        10);

            var playerAttacksAfterResume =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Player);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                playerAttacksAfterResume.Count,
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
                environment.ModifierRegistry
                    .GetTotalModifier(
                        playerAttacksAfterResume[0]
                            .Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                CountTriggeredPets(
                    environment),
                Is.EqualTo(2));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void
            StagedCombat_WithEnemySunBird_AppliesBonusToEnemySummerAttack()
        {
            var environment =
                CreateEnvironment(
                    playerSeason:
                        CombatCardSeason.Winter,
                    enemySeason:
                        CombatCardSeason.Summer,
                    playerHp: 3,
                    enemyHp: 10,
                    playerAttack: 0,
                    enemyAttack: 2,
                    playerSunBirdCount: 0,
                    enemySunBirdCount: 1);

            environment.Runner
                .StartAndResolveCombatStaged(
                    10,
                    100,
                    100,
                    100);

            var enemyAttacks =
                GetAttackEvents(
                    environment.EventLog,
                    CombatSide.Enemy);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                enemyAttacks.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        enemyAttacks[0]
                            .Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.ModifierRegistry
                    .ResolveDamage(
                        enemyAttacks[0]),
                Is.EqualTo(3));

            Assert.That(
                environment.State.Player.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.EnemyPets[0]
                            .InstanceId,
                        environment.EnemyCard
                            .InstanceId),
                Is.True);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatCardSeason playerSeason,
                CombatCardSeason enemySeason,
                int playerHp,
                int enemyHp,
                int playerAttack,
                int enemyAttack,
                int playerSunBirdCount,
                int enemySunBirdCount)
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
                    playerSeason,
                    playerHp,
                    playerAttack);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    new InstanceId(2),
                    enemySeason,
                    enemyHp,
                    enemyAttack);

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        new
                            CombatPetCardTriggerUsageRegistry());

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var playerPets =
                new List<CombatPetState>();

            var enemyPets =
                new List<CombatPetState>();

            var triggerSources =
                new List<ICombatTriggerSource>();

            for (var index = 0;
                 index < playerSunBirdCount;
                 index++)
            {
                var pet =
                    CreatePet(
                        1001 + index,
                        $"player-sun-bird-{index}");

                playerPets.Add(
                    pet);

                triggerSources.Add(
                    CreateSunBirdSource(
                        CombatSide.Player,
                        pet,
                        usageCommitter,
                        modifierRegistry));
            }

            for (var index = 0;
                 index < enemySunBirdCount;
                 index++)
            {
                var pet =
                    CreatePet(
                        2001 + index,
                        $"enemy-sun-bird-{index}");

                enemyPets.Add(
                    pet);

                triggerSources.Add(
                    CreateSunBirdSource(
                        CombatSide.Enemy,
                        pet,
                        usageCommitter,
                        modifierRegistry));
            }

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
                            playerPets.ToArray())),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            enemyPets.ToArray())));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    triggerSources);

            return new TestEnvironment
            {
                State =
                    state,

                PlayerCard =
                    playerCard,

                EnemyCard =
                    enemyCard,

                PlayerPets =
                    playerPets.ToArray(),

                EnemyPets =
                    enemyPets.ToArray(),

                UsageCommitter =
                    usageCommitter,

                ModifierRegistry =
                    modifierRegistry,

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

        private static ICombatTriggerSource
            CreateSunBirdSource(
                CombatSide side,
                CombatPetState pet,
                CombatPetCardTriggerUsageCommitter
                    usageCommitter,
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry)
        {
            var handler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        side,
                        pet.InstanceId,
                        usageCommitter,
                        modifierRegistry);

            return new
                CombatPetNormalAttackTriggerSource(
                    handler);
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
                    BattleHealth.NormalBaselineValue),
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

        private static CombatPetState CreatePet(
            long instanceId,
            string definitionId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static List<
            NormalAttackCombatEvent>
            GetAttackEvents(
                CombatEventLog eventLog,
                CombatSide attackerSide)
        {
            var result =
                new List<
                    NormalAttackCombatEvent>();

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var attackEvent =
                    eventLog.Events[index]
                        as NormalAttackCombatEvent;

                if (attackEvent == null ||
                    attackEvent.AttackerSide !=
                    attackerSide)
                {
                    continue;
                }

                result.Add(
                    attackEvent);
            }

            return result;
        }

        private static int CountEvents(
            CombatEventLog eventLog,
            CombatEventKind eventKind)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    eventKind)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountTriggeredPets(
            TestEnvironment environment)
        {
            var count = 0;

            for (var index = 0;
                 index < environment
                     .PlayerPets.Length;
                 index++)
            {
                if (environment.UsageCommitter
                    .HasTriggered(
                        environment.PlayerPets[index]
                            .InstanceId,
                        environment.PlayerCard
                            .InstanceId))
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

            public CombatPetState[] PlayerPets
            {
                get;
                set;
            }

            public CombatPetState[] EnemyPets
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
                CombatNormalAttackSourceDamageModifierRegistry
                ModifierRegistry
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