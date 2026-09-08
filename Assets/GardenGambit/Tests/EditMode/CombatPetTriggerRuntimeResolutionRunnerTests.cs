using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeResolutionRunnerTests
    {
        [Test]
        public void
            CreateResolutionRunner_WithNullState_Throws()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => runtime.CreateResolutionRunner(
                    null,
                    CreateMetadataFactory(),
                    eventLog,
                    new CombatEventQueue(
                        eventLog)));
        }

        [Test]
        public void
            CreateResolutionRunner_WithNullMetadataFactory_Throws()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => runtime.CreateResolutionRunner(
                    CreateEmptyState(),
                    null,
                    eventLog,
                    new CombatEventQueue(
                        eventLog)));
        }

        [Test]
        public void
            CreateResolutionRunner_WithNullEventLog_Throws()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var queueEventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => runtime.CreateResolutionRunner(
                    CreateEmptyState(),
                    CreateMetadataFactory(),
                    null,
                    new CombatEventQueue(
                        queueEventLog)));
        }

        [Test]
        public void
            CreateResolutionRunner_WithNullEventQueue_Throws()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            Assert.Throws<ArgumentNullException>(
                () => runtime.CreateResolutionRunner(
                    CreateEmptyState(),
                    CreateMetadataFactory(),
                    new CombatEventLog(),
                    null));
        }

        [Test]
        public void
            CreateResolutionRunner_UsesExactRuntimeDependencies()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var eventLog =
                new CombatEventLog();

            var runner =
                runtime.CreateResolutionRunner(
                    CreateEmptyState(),
                    CreateMetadataFactory(),
                    eventLog,
                    new CombatEventQueue(
                        eventLog));

            Assert.That(
                runner,
                Is.Not.Null);

            Assert.That(
                runner.UsesStagedNormalAttackByDefault,
                Is.True);

            Assert.That(
                runner.SourceDamageModifierRegistry,
                Is.SameAs(
                    runtime
                        .SourceDamageModifierRegistry));

            Assert.That(
                runner.TargetDamageReductionResolver,
                Is.SameAs(
                    runtime
                        .TargetDamageReductionResolver));

            Assert.That(
                runner.FinalRankModifierRegistry,
                Is.SameAs(
                    runtime
                        .FinalRankModifierRegistry));
        }

        [Test]
        public void
            CreateResolutionRunner_WithSunBird_BuildsAndUsesPetSource()
        {
            CombatCardState playerCard;
            CombatCardState enemyCard;
            CombatPetState sunBird;

            var state =
                CreateSunBirdCombatState(
                    out playerCard,
                    out enemyCard,
                    out sunBird);

            var runtime =
                new CombatPetTriggerRuntime();

            var eventLog =
                new CombatEventLog();

            var runner =
                runtime.CreateResolutionRunner(
                    state,
                    CreateMetadataFactory(),
                    eventLog,
                    new CombatEventQueue(
                        eventLog));

            var completedEvent =
                runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                runner.ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                state.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                runtime.UsageCommitter.HasTriggered(
                    sunBird.InstanceId,
                    playerCard.InstanceId),
                Is.True);

            Assert.That(
                runtime
                    .SourceDamageModifierRegistry
                    .Count,
                Is.EqualTo(1));

            Assert.That(
                runner.HasActiveCombat,
                Is.False);
        }

        private static CombatState
            CreateEmptyState()
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy));
        }

        private static CombatState
            CreateSunBirdCombatState(
                out CombatCardState playerCard,
                out CombatCardState enemyCard,
                out CombatPetState sunBird)
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

            playerCard =
                new CombatCardState(
                    new DefinitionId(
                        "test.player-summer"),
                    new InstanceId(1),
                    new CardRank(2),
                    CombatCardSeason.Summer,
                    hpCapacity: 5,
                    currentHp: 5,
                    armor: 0,
                    attack: 2);

            enemyCard =
                new CombatCardState(
                    new DefinitionId(
                        "test.enemy-winter"),
                    new InstanceId(101),
                    new CardRank(2),
                    CombatCardSeason.Winter,
                    hpCapacity: 3,
                    currentHp: 3,
                    armor: 0,
                    attack: 0);

            sunBird =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .SunBird,
                    new InstanceId(1001));

            return new CombatState(
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
                            sunBird
                        })),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        Array.Empty<
                            CombatPetState>())));
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

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<
                        CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<
                        CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new
                    CombatSequenceNumberAllocator());
        }
    }
}