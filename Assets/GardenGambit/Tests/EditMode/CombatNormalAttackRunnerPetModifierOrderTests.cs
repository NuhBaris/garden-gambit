using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackRunnerPetModifierOrderTests
    {
        [Test]
        public void
            StartAndResolve_WithSunBirdAndPolarFerret_AppliesSourceBonusBeforeTargetReduction()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var sunBird =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .SunBird,
                    new InstanceId(1001));

            var polarFerret =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .PolarFerret,
                    new InstanceId(2001));

            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "player-summer-card",
                    new InstanceId(1),
                    CombatCardSeason.Summer,
                    hp: 10,
                    attack: 0);

            var enemyCard =
                CreateCard(
                    "enemy-winter-card",
                    new InstanceId(201),
                    CombatCardSeason.Winter,
                    hp: 10,
                    attack: 0);

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
                                sunBird
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new[]
                            {
                                polarFerret
                            })));

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

            var application =
                runner.StartAndResolve(
                    playerPosition,
                    enemyPosition,
                    maximumPassCount: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            var playerAttackEvent =
                GetFirstAttackEvent(
                    eventLog,
                    CombatSide.Player);

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                runner.HasActiveExecution,
                Is.False);

            Assert.That(
                runtime
                    .SourceDamageModifierRegistry
                    .GetTotalModifier(
                        playerAttackEvent
                            .Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                runtime.UsageCommitter
                    .HasTriggered(
                        sunBird.InstanceId,
                        playerCard.InstanceId),
                Is.True);

            Assert.That(
                runtime.UsageCommitter
                    .HasTriggered(
                        polarFerret.InstanceId,
                        enemyCard.InstanceId),
                Is.True);

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                playerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                runtime
                    .TargetDamageReductionRegistry
                    .Count,
                Is.Zero);

            Assert.That(
                eventResolutionEngine
                    .HasPendingWork,
                Is.False);

            Assert.That(
                eventQueue.HasPending,
                Is.False);
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
    }
}