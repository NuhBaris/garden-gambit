using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardEventTriggerHandlerTests
    {
        [Test]
        public void
            Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new TestHandler(
                        default(CombatSide),
                        new InstanceId(1001),
                        canTrigger: true));
        }

        [Test]
        public void
            Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new TestHandler(
                        CombatSide.Player,
                        default(InstanceId),
                        canTrigger: true));
        }

        [Test]
        public void
            Constructor_ExposesExactIdentity()
        {
            var instanceId =
                new InstanceId(1001);

            var handler =
                new TestHandler(
                    CombatSide.Enemy,
                    instanceId,
                    canTrigger: true);

            Assert.That(
                handler.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                handler.CardInstanceId,
                Is.EqualTo(
                    instanceId));
        }

        [Test]
        public void
            CanTrigger_WhenCardExists_DelegatesExactArguments()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    environment.FirstCard.InstanceId,
                    canTrigger: true);

            var result =
                handler.CanTrigger(
                    environment.State,
                    environment.SourceEvent);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.CanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastState,
                Is.SameAs(
                    environment.State));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(
                    environment.SourceEvent));

            Assert.That(
                handler.LastCard,
                Is.SameAs(
                    environment.FirstCard));
        }

        [Test]
        public void
            CanTrigger_WhenSourceCardIsMissing_ReturnsFalseWithoutDelegating()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    new InstanceId(9999),
                    canTrigger: true);

            var result =
                handler.CanTrigger(
                    environment.State,
                    environment.SourceEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.CanCallCount,
                Is.Zero);

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void
            Resolve_WhenCardExists_DelegatesExactArguments()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    environment.SecondCard.InstanceId,
                    canTrigger: true);

            handler.Resolve(
                environment.State,
                environment.SourceEvent);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastState,
                Is.SameAs(
                    environment.State));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(
                    environment.SourceEvent));

            Assert.That(
                handler.LastCard,
                Is.SameAs(
                    environment.SecondCard));
        }

        [Test]
        public void
            Resolve_AfterSourceCardWasDirectDeleted_CancelsOnlyDeletedCardTrigger()
        {
            var environment =
                CreateEnvironment();

            var deletedCardHandler =
                new TestHandler(
                    CombatSide.Player,
                    environment.FirstCard.InstanceId,
                    canTrigger: true);

            var remainingCardHandler =
                new TestHandler(
                    CombatSide.Player,
                    environment.SecondCard.InstanceId,
                    canTrigger: true);

            Assert.That(
                deletedCardHandler.CanTrigger(
                    environment.State,
                    environment.SourceEvent),
                Is.True);

            Assert.That(
                remainingCardHandler.CanTrigger(
                    environment.State,
                    environment.SourceEvent),
                Is.True);

            var directDeleteResolver =
                new CombatDirectDeleteResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var directDeleteEvent =
                directDeleteResolver
                    .ApplyDirectDelete(
                        environment.State,
                        environment.SourceEvent,
                        environment.FirstPosition);

            deletedCardHandler.Resolve(
                environment.State,
                environment.SourceEvent);

            remainingCardHandler.Resolve(
                environment.State,
                environment.SourceEvent);

            Assert.That(
                directDeleteEvent.InstanceId,
                Is.EqualTo(
                    environment.FirstCard.InstanceId));

            Assert.That(
                deletedCardHandler.ResolveCallCount,
                Is.Zero);

            Assert.That(
                remainingCardHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                remainingCardHandler.LastCard,
                Is.SameAs(
                    environment.SecondCard));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Cards[0],
                Is.SameAs(
                    environment.SecondCard));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    directDeleteEvent));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new
                        CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var sourceEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                sourceEvent);

            var firstPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var secondPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            var firstCard =
                CreateCard(
                    "first-card",
                    1001);

            var secondCard =
                CreateCard(
                    "second-card",
                    1002);

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                firstPosition,
                                firstCard.InstanceId),
                            new CombatSlotState(
                                new SlotId(2),
                                secondPosition,
                                secondCard.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            firstCard,
                            secondCard
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

            return new TestEnvironment
            {
                State =
                    state,
                PlayerSide =
                    playerSide,
                FirstCard =
                    firstCard,
                SecondCard =
                    secondCard,
                FirstPosition =
                    firstPosition,
                MetadataFactory =
                    metadataFactory,
                EventLog =
                    eventLog,
                SourceEvent =
                    sourceEvent
            };
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

        private sealed class TestHandler :
            CombatCardEventTriggerHandler<
                TestCombatEvent>
        {
            private readonly bool
                _canTrigger;

            public TestHandler(
                CombatSide side,
                InstanceId cardInstanceId,
                bool canTrigger)
                : base(
                    side,
                    cardInstanceId)
            {
                _canTrigger =
                    canTrigger;
            }

            public int CanCallCount
            {
                get;
                private set;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public CombatState LastState
            {
                get;
                private set;
            }

            public TestCombatEvent
                LastSourceEvent
            {
                get;
                private set;
            }

            public CombatCardState LastCard
            {
                get;
                private set;
            }

            protected override bool
                CanCardTrigger(
                    CombatState state,
                    TestCombatEvent sourceEvent,
                    CombatCardState card)
            {
                CanCallCount++;

                Capture(
                    state,
                    sourceEvent,
                    card);

                return _canTrigger;
            }

            protected override void
                ResolveCardTrigger(
                    CombatState state,
                    TestCombatEvent sourceEvent,
                    CombatCardState card)
            {
                ResolveCallCount++;

                Capture(
                    state,
                    sourceEvent,
                    card);
            }

            private void Capture(
                CombatState state,
                TestCombatEvent sourceEvent,
                CombatCardState card)
            {
                LastState =
                    state;

                LastSourceEvent =
                    sourceEvent;

                LastCard =
                    card;
            }
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

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatCardState FirstCard
            {
                get;
                set;
            }

            public CombatCardState SecondCard
            {
                get;
                set;
            }

            public BoardPosition FirstPosition
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

            public TestCombatEvent SourceEvent
            {
                get;
                set;
            }
        }
    }
}