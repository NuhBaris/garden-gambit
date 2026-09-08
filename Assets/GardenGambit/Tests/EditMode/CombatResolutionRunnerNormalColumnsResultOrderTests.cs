using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerNormalColumnsResultOrderTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void
            StartAndResolveCombat_LogsResultOnlyAfterAllFiveColumns(
                bool useStagedPipeline)
        {
            var state =
                CreateEmptyState();

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

            CombatCompletedCombatEvent
                completedEvent;

            if (useStagedPipeline)
            {
                completedEvent =
                    runner.StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);
            }
            else
            {
                completedEvent =
                    runner.StartAndResolveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);
            }

            Assert.That(
                completedEvent,
                Is.Not.Null);

            var combatStartedEvent =
                eventLog.Events[0]
                    as CombatStartedCombatEvent;

            Assert.That(
                combatStartedEvent,
                Is.Not.Null);

            var columnCount = 0;

            var expectedColumnValue =
                BoardColumn.MinimumValue;

            var resultEventIndex = -1;

            CombatResultCalculatedCombatEvent
                resultEvent = null;

            ColumnStartedCombatEvent
                lastColumnEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var combatEvent =
                    eventLog.Events[index];

                var columnEvent =
                    combatEvent
                        as ColumnStartedCombatEvent;

                if (columnEvent != null)
                {
                    Assert.That(
                        resultEventIndex,
                        Is.EqualTo(-1),
                        "A Column Started event cannot " +
                        "be logged after result calculation.");

                    Assert.That(
                        columnEvent.Column.Value,
                        Is.EqualTo(
                            expectedColumnValue));

                    columnCount++;

                    expectedColumnValue =
                        checked(
                            expectedColumnValue + 1);

                    lastColumnEvent =
                        columnEvent;

                    continue;
                }

                var calculatedEvent =
                    combatEvent
                        as
                            CombatResultCalculatedCombatEvent;

                if (calculatedEvent == null)
                {
                    continue;
                }

                Assert.That(
                    resultEventIndex,
                    Is.EqualTo(-1),
                    "Only one result event may be logged.");

                resultEventIndex =
                    index;

                resultEvent =
                    calculatedEvent;
            }

            Assert.That(
                columnCount,
                Is.EqualTo(5));

            Assert.That(
                expectedColumnValue,
                Is.EqualTo(
                    BoardColumn.MaximumValue + 1));

            Assert.That(
                lastColumnEvent,
                Is.Not.Null);

            Assert.That(
                lastColumnEvent.Column.Value,
                Is.EqualTo(
                    BoardColumn.MaximumValue));

            Assert.That(
                resultEventIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                resultEvent,
                Is.Not.Null);

            Assert.That(
                lastColumnEvent.Metadata.SequenceNo,
                Is.LessThan(
                    resultEvent.Metadata.SequenceNo));

            Assert.That(
                resultEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent.Metadata.EventId));

            Assert.That(
                resultEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent.Metadata.EventId));

            Assert.That(
                completedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    resultEvent.Metadata.EventId));

            Assert.That(
                completedEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent.Metadata.EventId));

            Assert.That(
                eventLog.Events[
                    eventLog.Count - 1],
                Is.SameAs(
                    completedEvent));

            Assert.That(
                runner.ActiveCompletedEvent,
                Is.Null);

            Assert.That(
                runner.HasActiveCombat,
                Is.False);

            Assert.That(
                runner.HasActiveColumn,
                Is.False);

            Assert.That(
                runner.HasPendingColumnResolution,
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
    }
}