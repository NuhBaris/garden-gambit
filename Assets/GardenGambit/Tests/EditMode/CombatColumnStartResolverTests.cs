using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnStartResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnStartResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnStartResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void StartColumn_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .StartColumn(
                        null,
                        environment.CombatStartedEvent,
                        new BoardColumn(1)));

            AssertHistoryContainsOnlyCombatStart(
                environment);
        }

        [Test]
        public void StartColumn_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        null,
                        new BoardColumn(1)));

            AssertHistoryContainsOnlyCombatStart(
                environment);
        }

        [Test]
        public void StartColumn_WithInvalidColumn_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        default(BoardColumn)));

            AssertHistoryContainsOnlyCombatStart(
                environment);
        }

        [Test]
        public void StartColumn_WithValidValues_AppendsChildEvent()
        {
            var environment =
                CreateEnvironment();

            var column =
                new BoardColumn(1);

            var columnEvent =
                environment.Resolver.StartColumn(
                    environment.State,
                    environment.CombatStartedEvent,
                    column);

            Assert.That(
                columnEvent,
                Is.Not.Null);

            Assert.That(
                columnEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.ColumnStarted));

            Assert.That(
                columnEvent.Column,
                Is.EqualTo(column));

            Assert.That(
                columnEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                columnEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                columnEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(columnEvent));

            Assert.That(
                environment.State.GetSide(
                        CombatSide.Player)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State.GetSide(
                        CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);
        }

        [Test]
        public void StartColumn_WithUnloggedCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            var unloggedEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        unloggedEvent,
                        new BoardColumn(1)));

            AssertHistoryContainsOnlyCombatStart(
                environment);
        }

        [Test]
        public void StartColumn_WithDifferentCombatStartedReference_Throws()
        {
            var environment =
                CreateEnvironment();

            var differentReference =
                new CombatStartedCombatEvent(
                    environment.CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        differentReference,
                        new BoardColumn(1)));

            AssertHistoryContainsOnlyCombatStart(
                environment);
        }

        [Test]
        public void StartColumn_WhenColumnAlreadyStarted_ThrowsWithoutDuplicate()
        {
            var environment =
                CreateEnvironment();

            var column =
                new BoardColumn(1);

            var firstColumnEvent =
                environment.Resolver.StartColumn(
                    environment.State,
                    environment.CombatStartedEvent,
                    column);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        column));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstColumnEvent));
        }

        [Test]
        public void StartColumn_WithDifferentColumns_AppendsBothUnderSameCombat()
        {
            var environment =
                CreateEnvironment();

            var firstColumnEvent =
                environment.Resolver.StartColumn(
                    environment.State,
                    environment.CombatStartedEvent,
                    new BoardColumn(1));

            var secondColumnEvent =
                environment.Resolver.StartColumn(
                    environment.State,
                    environment.CombatStartedEvent,
                    new BoardColumn(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstColumnEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(secondColumnEvent));

            Assert.That(
                firstColumnEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                secondColumnEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                firstColumnEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    secondColumnEvent.Metadata
                        .TriggerRootId));

            Assert.That(
                secondColumnEvent.Metadata.SequenceNo,
                Is.GreaterThan(
                    firstColumnEvent.Metadata.SequenceNo));
        }

        [Test]
        public void StartColumn_WhenFirstColumnIsNotMinimum_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        new BoardColumn(
                            BoardColumn.MinimumValue + 1)));

            AssertHistoryContainsOnlyCombatStart(
                environment);
        }

        [Test]
        public void StartColumn_WhenNextColumnIsSkipped_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            var firstColumnEvent =
                environment.Resolver.StartColumn(
                    environment.State,
                    environment.CombatStartedEvent,
                    new BoardColumn(
                        BoardColumn.MinimumValue));

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        new BoardColumn(
                            BoardColumn.MinimumValue + 2)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstColumnEvent));
        }

        [Test]
        public void StartColumn_WithEveryColumnInOrder_AppendsAllColumns()
        {
            var environment =
                CreateEnvironment();

            ColumnStartedCombatEvent
                lastColumnEvent = null;

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                     BoardColumn.MaximumValue;
                 columnValue++)
            {
                lastColumnEvent =
                    environment.Resolver.StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        new BoardColumn(
                            columnValue));
            }

            var columnCount =
                BoardColumn.MaximumValue -
                BoardColumn.MinimumValue +
                1;

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    1 + columnCount));

            Assert.That(
                lastColumnEvent,
                Is.Not.Null);

            Assert.That(
                lastColumnEvent.Column,
                Is.EqualTo(
                    new BoardColumn(
                        BoardColumn.MaximumValue)));

            Assert.That(
                environment.EventLog.Events[
                    environment.EventLog.Count - 1],
                Is.SameAs(lastColumnEvent));

            Assert.That(
                lastColumnEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));
        }

        [Test]
        public void StartColumn_WhenExistingHistoryIsOutOfOrder_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            var invalidColumnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment
                                .CombatStartedEvent
                                .Metadata),
                    new BoardColumn(
                        BoardColumn.MinimumValue + 1));

            environment.EventLog.Append(
                invalidColumnEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        new BoardColumn(
                            BoardColumn.MinimumValue)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    invalidColumnEvent));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            return new TestEnvironment
            {
                State =
                    CreateState(),
                MetadataFactory =
                    metadataFactory,
                EventLog =
                    eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Resolver =
                    new CombatColumnStartResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatState CreateState()
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

            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition),
                CreateEmptySide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition));
        }

        private static CombatSideState CreateEmptySide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
                            position)
                    }),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static void
            AssertHistoryContainsOnlyCombatStart(
                TestEnvironment environment)
        {
            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
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

            public CombatColumnStartResolver Resolver
            {
                get;
                set;
            }
        }
    }
}