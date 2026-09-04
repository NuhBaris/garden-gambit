using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackRunnerPolarFerretRetryTests
    {
        [Test]
        public void
            ResumeAfterTriggerBudgetExhaustion_DoesNotRepeatPolarFerretRequest()
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "card.player.summer",
                    new InstanceId(1),
                    CombatCardSeason.Summer,
                    attack: 3);

            var enemyCard =
                CreateCard(
                    "card.enemy.winter",
                    new InstanceId(101),
                    CombatCardSeason.Winter,
                    attack: 0);

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
                            Array.Empty<
                                CombatPetState>())),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new[]
                            {
                                polarFerret
                            })));

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
                    runtime
                        .SourceDamageModifierRegistry,
                    runtime
                        .TargetDamageReductionResolver);

            Assert.Throws<InvalidOperationException>(
                () =>
                    runner.StartAndResolve(
                        playerPosition,
                        enemyPosition,
                        maximumPassCount: 1,
                        maximumEventCountPerPass: 2,
                        maximumTriggerCountPerEvent: 10));

            Assert.That(
                runner.HasActiveExecution,
                Is.True);

            Assert.That(
                runner.ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                runtime
                    .TargetDamageReductionRegistry
                    .Count,
                Is.EqualTo(1));

            Assert.That(
                runtime.UsageCommitter
                    .HasTriggered(
                        polarFerret.InstanceId,
                        enemyCard.InstanceId),
                Is.False);

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(10));

            var application =
                runner.ResumeActiveExecution(
                    maximumPassCount: 10,
                    maximumEventCountPerPass: 10,
                    maximumTriggerCountPerEvent: 10);

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                runner.HasActiveExecution,
                Is.False);

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                playerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                runtime.UsageCommitter
                    .HasTriggered(
                        polarFerret.InstanceId,
                        enemyCard.InstanceId),
                Is.True);

            Assert.That(
                runtime
                    .TargetDamageReductionRegistry
                    .Count,
                Is.Zero);

            Assert.That(
                eventResolutionEngine.HasPendingWork,
                Is.False);
        }

        private static CombatCardState CreateCard(
            string definitionId,
            InstanceId instanceId,
            CombatCardSeason season,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                instanceId,
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
    }
}