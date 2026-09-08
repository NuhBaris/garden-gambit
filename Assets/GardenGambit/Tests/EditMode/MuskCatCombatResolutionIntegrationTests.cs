using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        MuskCatCombatResolutionIntegrationTests
    {
        [Test]
        public void
            StagedCombat_WithExactlyOneLivingAffectedRowCard_AppliesFourFinalRank()
        {
            var muskCat =
                CreateMuskCat(
                    1001);

            var playerCard =
                CreateCard(
                    1);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        muskCat
                    },
                    new[]
                    {
                        playerCard
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            playerCard.InstanceId)
                    });

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                environment.Runtime
                    .FinalRankModifierRegistry
                    .GetTotalModifier(
                        playerCard.InstanceId),
                Is.EqualTo(4));

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        muskCat.InstanceId,
                        playerCard.InstanceId),
                Is.True);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void
            StagedCombat_WithTwoLivingAffectedRowCards_DoesNotApplyMuskCatBonus()
        {
            var muskCat =
                CreateMuskCat(
                    1001);

            var firstCard =
                CreateCard(
                    1);

            var secondCard =
                CreateCard(
                    2);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        muskCat
                    },
                    new[]
                    {
                        firstCard,
                        secondCard
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            firstCard.InstanceId),

                        CreateOccupiedSlot(
                            slotId: 2,
                            row: BoardRow.Front,
                            column: 2,
                            secondCard.InstanceId)
                    });

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(16)));

            Assert.That(
                environment.Runtime
                    .FinalRankModifierRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        muskCat.InstanceId,
                        firstCard.InstanceId),
                Is.False);

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        muskCat.InstanceId,
                        secondCard.InstanceId),
                Is.False);
        }

        [Test]
        public void
            ResumeStagedCombat_AfterMuskCatTriggerBudgetExhaustion_DoesNotRepeatBonusOrBattleEnd()
        {
            var upperMuskCat =
                CreateMuskCat(
                    1001);

            var lowerMuskCat =
                CreateMuskCat(
                    1002);

            var upperCard =
                CreateCard(
                    1);

            var lowerCard =
                CreateCard(
                    2);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        upperMuskCat,
                        lowerMuskCat
                    },
                    new[]
                    {
                        upperCard,
                        lowerCard
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            upperCard.InstanceId),

                        CreateOccupiedSlot(
                            slotId: 2,
                            row: BoardRow.Back,
                            column: 1,
                            lowerCard.InstanceId)
                    });

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runtime
                    .FinalRankModifierRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombatStaged(
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
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(8)));

            Assert.That(
                environment.Runtime
                    .FinalRankModifierRegistry
                    .GetTotalModifier(
                        upperCard.InstanceId),
                Is.EqualTo(4));

            Assert.That(
                environment.Runtime
                    .FinalRankModifierRegistry
                    .GetTotalModifier(
                        lowerCard.InstanceId),
                Is.EqualTo(4));

            Assert.That(
                environment.Runtime
                    .FinalRankModifierRegistry.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        upperMuskCat.InstanceId,
                        upperCard.InstanceId),
                Is.True);

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        lowerMuskCat.InstanceId,
                        lowerCard.InstanceId),
                Is.True);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatPetState[] playerPets,
                CombatCardState[] playerCards,
                CombatSlotState[] playerSlots)
        {
            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        playerCards,
                        playerSlots),
                    CreateSide(
                        CombatSide.Enemy,
                        Array.Empty<
                            CombatCardState>(),
                        Array.Empty<
                            CombatSlotState>()),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            playerPets)),
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
                    new
                        CombatSequenceNumberAllocator());

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
                        .TargetDamageReductionResolver,
                    runtime
                        .FinalRankModifierRegistry);

            return new TestEnvironment
            {
                State =
                    state,

                Runtime =
                    runtime,

                EventLog =
                    eventLog,

                Runner =
                    runner
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState[] cards,
            CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatPetState
            CreateMuskCat(
                long instanceId)
        {
            return new CombatPetState(
                CombatPetDefinitionIds
                    .MuskCat,
                new InstanceId(
                    instanceId));
        }

        private static CombatCardState CreateCard(
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"test.card-{instanceId}"),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                hpCapacity: 5,
                currentHp: 5,
                armor: 0,
                attack: 0);
        }

        private static CombatSlotState
            CreateOccupiedSlot(
                long slotId,
                BoardRow row,
                int column,
                InstanceId occupantInstanceId)
        {
            return new CombatSlotState(
                new SlotId(
                    slotId),
                new BoardPosition(
                    CombatSide.Player,
                    row,
                    new BoardColumn(
                        column)),
                occupantInstanceId);
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

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatPetTriggerRuntime Runtime
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