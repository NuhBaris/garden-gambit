using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalColumnsCompletionValidatorTests
    {
        [Test]
        public void
            Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalColumnsCompletionValidator(
                            null));
        }

        [Test]
        public void
            Validate_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Validator.Validate(
                    null,
                    environment.CombatStartedEvent));
        }

        [Test]
        public void
            Validate_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Validator.Validate(
                    environment.State,
                    null));
        }

        [Test]
        public void
            Validate_WithDifferentCombatStartedReference_Throws()
        {
            var environment =
                CreateEnvironment();

            var differentReference =
                new CombatStartedCombatEvent(
                    environment.CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Validator.Validate(
                    environment.State,
                    differentReference));
        }

        [Test]
        public void
            Validate_WithNoStartedColumns_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment.State,
                    environment.CombatStartedEvent));
        }

        [Test]
        public void
            Validate_WithOnlyFirstFourColumns_Throws()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 4);

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment.State,
                    environment.CombatStartedEvent));
        }

        [Test]
        public void
            Validate_WithAllFiveResolvedColumns_DoesNotThrow()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 5);

            Assert.DoesNotThrow(
                () => environment.Validator.Validate(
                    environment.State,
                    environment.CombatStartedEvent));
        }

        [Test]
        public void
            Validate_WithOutOfOrderColumns_Throws()
        {
            var environment =
                CreateEnvironment();

            AppendColumn(
                environment,
                parentMetadata:
                    environment.CombatStartedEvent
                        .Metadata,
                columnValue: 1);

            AppendColumn(
                environment,
                parentMetadata:
                    environment.CombatStartedEvent
                        .Metadata,
                columnValue: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment.State,
                    environment.CombatStartedEvent));
        }

        [Test]
        public void
            Validate_WithNonDirectColumnParent_Throws()
        {
            var environment =
                CreateEnvironment();

            var intermediateEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment
                                .CombatStartedEvent
                                .Metadata));

            environment.EventLog.Append(
                intermediateEvent);

            AppendColumn(
                environment,
                intermediateEvent.Metadata,
                columnValue: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment.State,
                    environment.CombatStartedEvent));
        }

        [Test]
        public void
            Validate_WhenColumnStillHasOpposingLivingFrontCards_Throws()
        {
            var environment =
                CreateEnvironment(
                    includeOpposingFrontCards: true);

            AppendColumns(
                environment,
                columnCount: 5);

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment.State,
                    environment.CombatStartedEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeOpposingFrontCards = false)
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new
                        CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            CombatState state;

            if (includeOpposingFrontCards)
            {
                state =
                    CreateStateWithOpposingFrontCards();
            }
            else
            {
                state =
                    new CombatState(
                        CreateEmptySide(
                            CombatSide.Player),
                        CreateEmptySide(
                            CombatSide.Enemy));
            }

            return new TestEnvironment
            {
                State =
                    state,
                MetadataFactory =
                    metadataFactory,
                EventLog =
                    eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Validator =
                    new
                        CombatNormalColumnsCompletionValidator(
                            eventLog)
            };
        }

        private static void AppendColumns(
            TestEnvironment environment,
            int columnCount)
        {
            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <
                     BoardColumn.MinimumValue +
                     columnCount;
                 columnValue++)
            {
                AppendColumn(
                    environment,
                    environment.CombatStartedEvent
                        .Metadata,
                    columnValue);
            }
        }

        private static void AppendColumn(
            TestEnvironment environment,
            CombatEventMetadata parentMetadata,
            int columnValue)
        {
            var columnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            parentMetadata),
                    new BoardColumn(
                        columnValue));

            environment.EventLog.Append(
                columnEvent);
        }

        private static CombatState
            CreateStateWithOpposingFrontCards()
        {
            var column =
                new BoardColumn(1);

            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column);

            var playerCard =
                CreateCard(
                    "player-card",
                    1001);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    2001);

            var playerSide =
                CreateOccupiedSide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition,
                    playerCard);

            var enemySide =
                CreateOccupiedSide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition,
                    enemyCard);

            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState
            CreateOccupiedSide(
                CombatSide side,
                SlotId slotId,
                BoardPosition position,
                CombatCardState card)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
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

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 2);
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.HpGain)
            {
            }
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

            public CombatNormalColumnsCompletionValidator
                Validator
            {
                get;
                set;
            }
        }
    }
}