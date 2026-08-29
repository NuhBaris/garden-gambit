using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDirectDeleteTriggerHandlerTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new TestDirectDeleteTriggerHandler(
                        null,
                        environment.EventLog,
                        environment.PlayerPosition));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new TestDirectDeleteTriggerHandler(
                        environment.MetadataFactory,
                        null,
                        environment.PlayerPosition));
        }

        [Test]
        public void Constructor_WithInvalidTargetPosition_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new TestDirectDeleteTriggerHandler(
                        environment.MetadataFactory,
                        environment.EventLog,
                        default(BoardPosition)));
        }

        [Test]
        public void CanTrigger_WithEligibleEvent_ReturnsTrueAndForwardsArguments()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendTestEvent(environment);

            var handler =
                new TestDirectDeleteTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog,
                    environment.PlayerPosition)
                {
                    CanDirectDeleteResult = true
                };

            var canTrigger =
                handler.CanTrigger(
                    environment.State,
                    sourceEvent);

            Assert.That(
                canTrigger,
                Is.True);

            Assert.That(
                handler.CanDirectDeleteCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedState,
                Is.SameAs(environment.State));

            Assert.That(
                handler.ReceivedSourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                handler.ExposedTargetPosition,
                Is.EqualTo(
                    environment.PlayerPosition));
        }

        [Test]
        public void CanTrigger_WithIneligibleEvent_ReturnsFalse()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendTestEvent(environment);

            var handler =
                new TestDirectDeleteTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog,
                    environment.PlayerPosition)
                {
                    CanDirectDeleteResult = false
                };

            var canTrigger =
                handler.CanTrigger(
                    environment.State,
                    sourceEvent);

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                handler.CanDirectDeleteCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithOccupiedTarget_AppliesDirectDelete()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendTestEvent(environment);

            var handler =
                new TestDirectDeleteTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog,
                    environment.PlayerPosition)
                {
                    CanDirectDeleteResult = true
                };

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    sourceEvent),
                Is.True);

            handler.Resolve(
                environment.State,
                sourceEvent);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<DirectDeleteCombatEvent>());

            var directDeleteEvent =
                (DirectDeleteCombatEvent)
                environment.EventLog.Events[1];

            Assert.That(
                directDeleteEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                directDeleteEvent.Position,
                Is.EqualTo(
                    environment.PlayerPosition));

            Assert.That(
                directDeleteEvent.HpAtDeletion,
                Is.EqualTo(
                    environment.PlayerCard.CurrentHp));

            Assert.That(
                directDeleteEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    sourceEvent.Metadata.EventId));
        }

        [Test]
        public void Resolve_WhenTargetWasAlreadyDeleted_SkipsSecondDelete()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendTestEvent(environment);

            var handler =
                new TestDirectDeleteTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog,
                    environment.PlayerPosition)
                {
                    CanDirectDeleteResult = true
                };

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    sourceEvent),
                Is.True);

            var priorResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var priorDeleteEvent =
                priorResolver.ApplyDirectDelete(
                    environment.State,
                    sourceEvent,
                    environment.PlayerPosition);

            Assert.DoesNotThrow(
                () => handler.Resolve(
                    environment.State,
                    sourceEvent));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(priorDeleteEvent));

            Assert.That(
                environment.EventLog.Events,
                Has.Exactly(1)
                    .TypeOf<DirectDeleteCombatEvent>());
        }

        private static TestEnvironment
            CreateEnvironment()
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

            var playerCard =
                new CombatCardState(
                    new DefinitionId("player-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    7,
                    7,
                    0,
                    3);

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                playerPosition,
                                playerCard.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            playerCard
                        }),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var enemySide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Enemy,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(2),
                                enemyPosition)
                        }),
                    new CombatCardRegistry(
                        new CombatCardState[0]),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            return new TestEnvironment
            {
                State =
                    new CombatState(
                        playerSide,
                        enemySide),
                PlayerSide = playerSide,
                PlayerCard = playerCard,
                PlayerPosition = playerPosition,
                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator()),
                EventLog =
                    new CombatEventLog()
            };
        }

        private static TestCombatEvent
            AppendTestEvent(
                TestEnvironment environment)
        {
            var sourceEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                sourceEvent);

            return sourceEvent;
        }

        private sealed class
            TestDirectDeleteTriggerHandler :
            CombatDirectDeleteTriggerHandler<
                TestCombatEvent>
        {
            public TestDirectDeleteTriggerHandler(
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog,
                BoardPosition targetPosition)
                : base(
                    metadataFactory,
                    eventLog,
                    targetPosition)
            {
            }

            public bool CanDirectDeleteResult
            {
                get;
                set;
            }

            public int CanDirectDeleteCallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public TestCombatEvent ReceivedSourceEvent
            {
                get;
                private set;
            }

            public BoardPosition
                ExposedTargetPosition =>
                    TargetPosition;

            protected override bool CanDirectDelete(
                CombatState state,
                TestCombatEvent sourceEvent)
            {
                CanDirectDeleteCallCount++;
                ReceivedState = state;
                ReceivedSourceEvent = sourceEvent;

                return CanDirectDeleteResult;
            }
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
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

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatCardState PlayerCard
            {
                get;
                set;
            }

            public BoardPosition PlayerPosition
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
        }
    }
}