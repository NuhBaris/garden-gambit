using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatRescueTriggerHandlerTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new TestRescueTriggerHandler(
                        null,
                        environment.EventLog));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new TestRescueTriggerHandler(
                        environment.MetadataFactory,
                        null));
        }

        [Test]
        public void CanTrigger_WithEligibleDeath_ReturnsTrueAndForwardsArguments()
        {
            var environment =
                CreateEnvironment();

            var deathEvent =
                AppendDeathEvent(environment);

            var handler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog)
                {
                    CanRescueResult = true
                };

            var canTrigger =
                handler.CanTrigger(
                    environment.State,
                    deathEvent);

            Assert.That(
                canTrigger,
                Is.True);

            Assert.That(
                handler.CanRescueCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedState,
                Is.SameAs(environment.State));

            Assert.That(
                handler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));
        }

        [Test]
        public void CanTrigger_WithIneligibleDeath_ReturnsFalse()
        {
            var environment =
                CreateEnvironment();

            var deathEvent =
                AppendDeathEvent(environment);

            var handler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog)
                {
                    CanRescueResult = false
                };

            var canTrigger =
                handler.CanTrigger(
                    environment.State,
                    deathEvent);

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                handler.CanRescueCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedState,
                Is.SameAs(environment.State));

            Assert.That(
                handler.ReceivedDeathEvent,
                Is.SameAs(deathEvent));
        }

        [Test]
        public void Resolve_WithValidDeath_AppliesRescue()
        {
            var environment =
                CreateEnvironment();

            var deathEvent =
                AppendDeathEvent(environment);

            var handler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog)
                {
                    CanRescueResult = true
                };

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    deathEvent),
                Is.True);

            handler.Resolve(
                environment.State,
                deathEvent);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(deathEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<RescueCombatEvent>());
        }

        [Test]
        public void Resolve_WhenDirectDeleteFollowsDeath_SkipsRescue()
        {
            var environment =
                CreateEnvironment();

            var deathEvent =
                AppendDeathEvent(environment);

            var handler =
                new TestRescueTriggerHandler(
                    environment.MetadataFactory,
                    environment.EventLog)
                {
                    CanRescueResult = true
                };

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    deathEvent),
                Is.True);

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var directDeleteEvent =
                directDeleteResolver.ApplyDirectDelete(
                    environment.State,
                    deathEvent,
                    environment.PlayerPosition);

            handler.Resolve(
                environment.State,
                deathEvent);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

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
                Is.SameAs(deathEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(directDeleteEvent));

            Assert.That(
                environment.EventLog.Events,
                Has.None.TypeOf<RescueCombatEvent>());
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
                    0,
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

        private static DeathCombatEvent
            AppendDeathEvent(
                TestEnvironment environment)
        {
            var deathEvent =
                new DeathCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    environment.PlayerCard.InstanceId,
                    environment.PlayerPosition,
                    3,
                    environment.PlayerCard.CurrentHp);

            environment.EventLog.Append(
                deathEvent);

            return deathEvent;
        }

        private sealed class
            TestRescueTriggerHandler :
            CombatRescueTriggerHandler
        {
            public TestRescueTriggerHandler(
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog)
                : base(
                    metadataFactory,
                    eventLog)
            {
            }

            public bool CanRescueResult
            {
                get;
                set;
            }

            public int CanRescueCallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public DeathCombatEvent ReceivedDeathEvent
            {
                get;
                private set;
            }

            protected override bool CanRescue(
                CombatState state,
                DeathCombatEvent sourceEvent)
            {
                CanRescueCallCount++;
                ReceivedState = state;
                ReceivedDeathEvent = sourceEvent;

                return CanRescueResult;
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