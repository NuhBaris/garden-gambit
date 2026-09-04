using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerBattleStartSnapshotTests
    {
        [Test]
        public void StartAndResolveCombat_AllStagesShareRootSnapshot()
        {
            var state =
                CreateEmptyState();

            CombatEventLog eventLog;

            var runner =
                CreateRunner(
                    state,
                    out eventLog);

            runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 10,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            var combatStartedEvent =
                eventLog.Events[0]
                    as CombatStartedCombatEvent;

            Assert.That(
                combatStartedEvent,
                Is.Not.Null);

            Assert.That(
                combatStartedEvent
                    .HasBattleStartSnapshot,
                Is.True);

            var stageEvents =
                GetStageEvents(
                    eventLog);

            Assert.That(
                stageEvents.Count,
                Is.EqualTo(3));

            Assert.That(
                stageEvents[0].Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                stageEvents[1].Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                stageEvents[2].Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            for (var index = 0;
                 index < stageEvents.Count;
                 index++)
            {
                Assert.That(
                    stageEvents[index]
                        .HasBattleStartSnapshot,
                    Is.True);

                Assert.That(
                    stageEvents[index]
                        .BattleStartSnapshot,
                    Is.SameAs(
                        combatStartedEvent
                            .BattleStartSnapshot));
            }
        }

        [Test]
        public void StartAndResolveCombat_SnapshotContainsInitialBoardCard()
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(1),
                    new CardRank(4),
                    hpCapacity: 12,
                    currentHp: 7,
                    armor: 2,
                    attack: 5);

            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            var player =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                position,
                                card.InstanceId)
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

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            CombatEventLog eventLog;

            var runner =
                CreateRunner(
                    state,
                    out eventLog);

            runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 10,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            var combatStartedEvent =
                (CombatStartedCombatEvent)
                    eventLog.Events[0];

            var cardSnapshot =
                combatStartedEvent
                    .BattleStartSnapshot
                    .Player
                    .GetCard(
                        card.InstanceId);

            Assert.That(
                combatStartedEvent
                    .BattleStartSnapshot
                    .TotalCardCount,
                Is.EqualTo(1));

            Assert.That(
                cardSnapshot.Position,
                Is.EqualTo(position));

            Assert.That(
                cardSnapshot.Rank,
                Is.EqualTo(
                    new CardRank(4)));

            Assert.That(
                cardSnapshot.HpCapacity,
                Is.EqualTo(12));

            Assert.That(
                cardSnapshot.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                cardSnapshot.Armor,
                Is.EqualTo(2));

            Assert.That(
                cardSnapshot.Attack,
                Is.EqualTo(5));

            var stageEvents =
                GetStageEvents(
                    eventLog);

            Assert.That(
                stageEvents[1].IsPetStage,
                Is.True);

            Assert.That(
                stageEvents[1]
                    .BattleStartSnapshot
                    .Player
                    .GetCard(
                        card.InstanceId),
                Is.SameAs(
                    cardSnapshot));
        }

        private static CombatResolutionRunner
            CreateRunner(
                CombatState state,
                out CombatEventLog eventLog)
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    Array.Empty<
                        ICombatTriggerSource>());

            return new CombatResolutionRunner(
                state,
                metadataFactory,
                eventLog,
                eventQueue,
                sourceRegistry);
        }

        private static List<
            BattleStartStageStartedCombatEvent>
            GetStageEvents(
                CombatEventLog eventLog)
        {
            var stageEvents =
                new List<
                    BattleStartStageStartedCombatEvent>();

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var stageEvent =
                    eventLog.Events[index]
                        as
                        BattleStartStageStartedCombatEvent;

                if (stageEvent != null)
                {
                    stageEvents.Add(
                        stageEvent);
                }
            }

            return stageEvents;
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

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}