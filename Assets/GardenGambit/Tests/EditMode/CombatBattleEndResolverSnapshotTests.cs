using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleEndResolverSnapshotTests
    {
        [Test]
        public void
            StartBattleEnd_WithCombatSnapshot_CopiesExactSnapshot()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: true);

            var expectedSnapshot =
                environment.CombatStartedEvent
                    .BattleStartSnapshot;

            var battleEndEvent =
                environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            Assert.That(
                expectedSnapshot,
                Is.Not.Null);

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.SameAs(
                    expectedSnapshot));

            Assert.That(
                battleEndEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                battleEndEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));
        }

        [Test]
        public void
            StartBattleEnd_WithoutCombatSnapshot_ReportsNoSnapshot()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: false);

            var battleEndEvent =
                environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            Assert.That(
                environment.CombatStartedEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.Null);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeSnapshot)
        {
            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            CombatStartedCombatEvent
                combatStartedEvent;

            if (includeSnapshot)
            {
                var combatStartResolver =
                    new CombatStartResolver(
                        metadataFactory,
                        eventLog);

                combatStartedEvent =
                    combatStartResolver.Start(
                        state);
            }
            else
            {
                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadataFactory.CreateRoot());

                eventLog.Append(
                    combatStartedEvent);
            }

            var columnStartResolver =
                new CombatColumnStartResolver(
                    metadataFactory,
                    eventLog);

            for (var columnValue = 1;
                 columnValue <= 5;
                 columnValue++)
            {
                columnStartResolver.StartColumn(
                    state,
                    combatStartedEvent,
                    new BoardColumn(
                        columnValue));
            }

            return new TestEnvironment
            {
                State =
                    state,

                CombatStartedEvent =
                    combatStartedEvent,

                Resolver =
                    new CombatBattleEndResolver(
                        metadataFactory,
                        eventLog)
            };
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

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public CombatBattleEndResolver Resolver
            {
                get;
                set;
            }
        }
    }
}