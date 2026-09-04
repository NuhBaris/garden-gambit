using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerSlotEnhanceIntegrationTests
    {
        [Test]
        public void StartAndResolveCombat_AppliesLivingBattleEndSlotEnhances()
        {
            var state =
                CreateState();

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
                    Array.Empty<
                        ICombatTriggerSource>());

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var completedEvent =
                runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(
                    BoardColumn.MaximumValue));

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.Zero);

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind
                        .CombatResultCalculated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    eventLog,
                    CombatEventKind.CombatCompleted),
                Is.EqualTo(1));

            var resultEvent =
                GetSingleResultEvent(
                    eventLog);

            Assert.That(
                resultEvent
                    .PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(20));

            Assert.That(
                resultEvent
                    .EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(19));

            Assert.That(
                resultEvent.PreventedDamageForEnemy,
                Is.EqualTo(1L));

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(1)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(17L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(17L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.True);

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(1)));

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(1)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(1)));

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                runner.HasActiveCombat,
                Is.False);

            Assert.That(
                runner.HasActiveColumn,
                Is.False);

            Assert.That(
                runner.HasPendingColumnResolution,
                Is.False);

            Assert.That(
                runner.ResolvedExchangeCount,
                Is.Zero);
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player,
                    slotIdBase: 0,
                    occupantColumn: 1,
                    instanceId: 100,
                    definitionId: "player-card",
                    rank: 10,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .WarBanner),
                CreateSideState(
                    CombatSide.Enemy,
                    slotIdBase: 10,
                    occupantColumn: 2,
                    instanceId: 200,
                    definitionId: "enemy-card",
                    rank: 2,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .ProtectiveSeal));
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                int slotIdBase,
                int occupantColumn,
                long instanceId,
                string definitionId,
                int rank,
                CombatSlotEnhanceKind enhanceKind)
        {
            var card =
                new CombatCardState(
                    new DefinitionId(definitionId),
                    new InstanceId(instanceId),
                    new CardRank(rank),
                    7,
                    7,
                    0,
                    3);

            var slots =
                new CombatSlotState[
                    CombatBoardState.MaximumSlotCount];

            var slotIndex = 0;

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                     BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

                var frontPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        column);

                var frontOccupant =
                    columnValue == occupantColumn
                        ? card.InstanceId
                        : (InstanceId?)null;

                var frontEnhanceKind =
                    columnValue == occupantColumn
                        ? enhanceKind
                        : CombatSlotEnhanceKind.None;

                slots[slotIndex] =
                    new CombatSlotState(
                        new SlotId(
                            slotIdBase +
                            slotIndex + 1),
                        frontPosition,
                        frontOccupant,
                        frontEnhanceKind);

                slotIndex++;

                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        column);

                slots[slotIndex] =
                    new CombatSlotState(
                        new SlotId(
                            slotIdBase +
                            slotIndex + 1),
                        backPosition);

                slotIndex++;
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(20),
                new AttackMultiplier(1));
        }

        private static
            CombatResultCalculatedCombatEvent
            GetSingleResultEvent(
                CombatEventLog eventLog)
        {
            CombatResultCalculatedCombatEvent
                resultEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as
                        CombatResultCalculatedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (resultEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple result events were found.");
                }

                resultEvent =
                    candidate;
            }

            if (resultEvent == null)
            {
                throw new InvalidOperationException(
                    "Combat result event was not found.");
            }

            return resultEvent;
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
    }
}