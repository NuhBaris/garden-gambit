using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerBattleEndSnapshotTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void
            StartAndResolveCombat_BattleEndReceivesExactCombatSnapshot(
                bool useStagedPipeline)
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(100),
                    new CardRank(3),
                    hpCapacity: 7,
                    currentHp: 7,
                    armor: 0,
                    attack: 3);

            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                playerPosition,
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
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    new CombatEventQueue(
                        eventLog),
                    new CombatTriggerSourceRegistry(
                        Array.Empty<
                            ICombatTriggerSource>()));

            if (useStagedPipeline)
            {
                runner.StartAndResolveCombatStaged(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }
            else
            {
                runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }

            var combatStartedEvent =
                GetSingleCombatStartedEvent(
                    eventLog);

            var battleEndEvent =
                GetSingleBattleEndEvent(
                    eventLog);

            Assert.That(
                combatStartedEvent
                    .HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.SameAs(
                    combatStartedEvent
                        .BattleStartSnapshot));

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot
                    .TotalCardCount,
                Is.EqualTo(1));

            var cardSnapshot =
                battleEndEvent
                    .BattleStartSnapshot
                    .Player
                    .GetCard(
                        card.InstanceId);

            Assert.That(
                cardSnapshot.InstanceId,
                Is.EqualTo(
                    card.InstanceId));

            Assert.That(
                cardSnapshot.Position,
                Is.EqualTo(
                    playerPosition));

            Assert.That(
                cardSnapshot.Rank,
                Is.EqualTo(
                    new CardRank(3)));
        }

        private static
            CombatStartedCombatEvent
            GetSingleCombatStartedEvent(
                CombatEventLog eventLog)
        {
            CombatStartedCombatEvent
                combatStartedEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as CombatStartedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (combatStartedEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Combat Started events " +
                        "were found.");
                }

                combatStartedEvent =
                    candidate;
            }

            if (combatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "Combat Started event was not found.");
            }

            return combatStartedEvent;
        }

        private static BattleEndStartedCombatEvent
            GetSingleBattleEndEvent(
                CombatEventLog eventLog)
        {
            BattleEndStartedCombatEvent
                battleEndEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as BattleEndStartedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (battleEndEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Battle End Started " +
                        "events were found.");
                }

                battleEndEvent =
                    candidate;
            }

            if (battleEndEvent == null)
            {
                throw new InvalidOperationException(
                    "Battle End Started event was not found.");
            }

            return battleEndEvent;
        }

        private static CombatSideState
            CreateEmptySide(
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
    }
}