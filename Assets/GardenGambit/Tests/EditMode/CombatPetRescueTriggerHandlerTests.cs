using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetRescueTriggerHandlerTests
    {
        [Test]
        public void
            CanTrigger_WhenHandlerAllows_CreatesContextAndReturnsTrue()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    environment.PlayerPet.InstanceId,
                    canTrigger: true);

            var sourceEvent =
                CreateSourceEvent();

            var result =
                handler.CanTrigger(
                    environment.State,
                    sourceEvent);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.CanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastContext,
                Is.Not.Null);

            Assert.That(
                handler.LastContext.State,
                Is.SameAs(
                    environment.State));

            Assert.That(
                handler.LastContext.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                handler.LastContext.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    environment.PlayerPet));
        }

        [Test]
        public void
            CanTrigger_WhenHandlerRejects_ReturnsFalse()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    environment.PlayerPet.InstanceId,
                    canTrigger: false);

            var result =
                handler.CanTrigger(
                    environment.State,
                    CreateSourceEvent());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.CanCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void
            Resolve_CreatesContextAndDelegatesExactPet()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    environment.PlayerPet.InstanceId,
                    canTrigger: true);

            var sourceEvent =
                CreateSourceEvent();

            handler.Resolve(
                environment.State,
                sourceEvent);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastContext,
                Is.Not.Null);

            Assert.That(
                handler.LastContext.State,
                Is.SameAs(
                    environment.State));

            Assert.That(
                handler.LastContext.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastContext.SideState,
                Is.SameAs(
                    environment.State.Player));

            Assert.That(
                handler.LastContext
                    .OpposingSideState,
                Is.SameAs(
                    environment.State.Enemy));

            Assert.That(
                handler.LastContext
                    .SidePetState,
                Is.SameAs(
                    environment.State.PlayerPets));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    environment.PlayerPet));
        }

        [Test]
        public void
            Resolve_CalledTwice_DelegatesTwice()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    environment.PlayerPet.InstanceId,
                    canTrigger: true);

            var sourceEvent =
                CreateSourceEvent();

            handler.Resolve(
                environment.State,
                sourceEvent);

            handler.Resolve(
                environment.State,
                sourceEvent);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(2));
        }

        [Test]
        public void
            CanTrigger_WithEnemyPet_UsesEnemyContextAndPlacement()
        {
            var environment =
                CreateEnvironment();

            var handler =
                new TestHandler(
                    CombatSide.Enemy,
                    environment.EnemyPet.InstanceId,
                    canTrigger: true);

            var result =
                handler.CanTrigger(
                    environment.State,
                    CreateSourceEvent());

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.LastContext.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                handler.LastContext.SideState,
                Is.SameAs(
                    environment.State.Enemy));

            Assert.That(
                handler.LastContext
                    .OpposingSideState,
                Is.SameAs(
                    environment.State.Player));

            Assert.That(
                handler.LastContext
                    .SidePetState,
                Is.SameAs(
                    environment.State.EnemyPets));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    environment.EnemyPet));

            var expectedRow =
                environment.State.EnemyPets
                    .GetAffectedRow(
                        environment.EnemyPet
                            .InstanceId);

            Assert.That(
                handler.LastContext.GetAffectedRow(
                    handler.LastPet),
                Is.EqualTo(
                    expectedRow));
        }

        [TestCase(-1)]
        [TestCase(99)]
        public void Constructor_WithInvalidSide_Throws(int side)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TestHandler((CombatSide)side, new InstanceId(1001), true));
        }

        [Test]
        public void Constructor_WithInvalidPetId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new TestHandler(CombatSide.Player, default(InstanceId), true));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NonRescueEvent_IsRejectedWithoutCallingPetHooks(bool hpGain)
        {
            var e = CreateEnvironment();
            var handler = new TestHandler(CombatSide.Player, e.PlayerPet.InstanceId, true);
            var sample = CreateSourceEvent();
            CombatEvent wrongEvent;
            if (hpGain)
            {
                wrongEvent = new HpGainCombatEvent(sample.Metadata, sample.InstanceId, sample.Position,
                    previousHpCapacity: 10, currentHpCapacity: 10, previousHp: 0, currentHp: 1);
            }
            else
            {
                wrongEvent = new DeathCombatEvent(sample.Metadata, sample.InstanceId, sample.Position, 2, 0);
            }
            Assert.That(handler.CanTrigger(e.State, wrongEvent), Is.False);
            Assert.Throws<ArgumentException>(() => handler.Resolve(e.State, wrongEvent));
            Assert.That(handler.CanCallCount, Is.Zero);
            Assert.That(handler.ResolveCallCount, Is.Zero);
            Assert.That(handler.LastContext, Is.Null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NullStateOrEvent_IsRejectedBeforePetHooks(bool nullState)
        {
            var e = CreateEnvironment();
            var handler = new TestHandler(CombatSide.Player, e.PlayerPet.InstanceId, true);
            var state = nullState ? null : e.State;
            var sourceEvent = nullState ? CreateSourceEvent() : null;
            Assert.Throws<ArgumentNullException>(() => handler.CanTrigger(state, sourceEvent));
            Assert.Throws<ArgumentNullException>(() => handler.Resolve(state, sourceEvent));
            Assert.That(handler.CanCallCount, Is.Zero);
            Assert.That(handler.ResolveCallCount, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrOpposingPetIdentity_IsRejectedBeforePetHooks(bool opposingPet)
        {
            var e = CreateEnvironment();
            var id = opposingPet ? e.EnemyPet.InstanceId : new InstanceId(9001);
            var handler = new TestHandler(CombatSide.Player, id, true);
            var rescue = CreateSourceEvent();
            Assert.Throws<KeyNotFoundException>(() => handler.CanTrigger(e.State, rescue));
            Assert.Throws<KeyNotFoundException>(() => handler.Resolve(e.State, rescue));
            Assert.That(handler.CanCallCount, Is.Zero);
            Assert.That(handler.ResolveCallCount, Is.Zero);
        }

        [Test]
        public void Resolve_DoesNotImplicitlyCallCanTrigger()
        {
            var e = CreateEnvironment();
            var handler = new TestHandler(CombatSide.Player, e.PlayerPet.InstanceId, false);
            handler.Resolve(e.State, CreateSourceEvent());
            Assert.That(handler.CanCallCount, Is.Zero);
            Assert.That(handler.ResolveCallCount, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void LowerPet_ReceivesExactRegisteredInstanceAndBackRow(CombatSide side)
        {
            var upper = new CombatPetState(new DefinitionId("test.rescue_pet"), new InstanceId(2002));
            var lower = new CombatPetState(new DefinitionId("test.rescue_pet"), new InstanceId(2001));
            var ownPets = new[] { upper, lower };
            var noPets = Array.Empty<CombatPetState>();
            var state = new CombatState(CreateEmptySide(CombatSide.Player), CreateEmptySide(CombatSide.Enemy),
                new CombatSidePetState(CombatSide.Player,
                    new CombatPetRegistry(side == CombatSide.Player ? ownPets : noPets)),
                new CombatSidePetState(CombatSide.Enemy,
                    new CombatPetRegistry(side == CombatSide.Enemy ? ownPets : noPets)));
            var handler = new TestHandler(side, lower.InstanceId, true);
            var rescue = CreateSourceEvent();
            Assert.That(handler.CanTrigger(state, rescue), Is.True);
            handler.Resolve(state, rescue);
            Assert.That(handler.LastPet, Is.SameAs(lower));
            Assert.That(handler.LastContext.SideState, Is.SameAs(state.GetSide(side)));
            Assert.That(handler.LastContext.GetAffectedRow(handler.LastPet), Is.EqualTo(BoardRow.Back));
            Assert.That(handler.LastContext.SourceEvent, Is.SameAs(rescue));
        }

        [Test]
        public void ExistingRescueResolverEvent_IsDeliveredWithoutRepeatingRescue()
        {
            var e = CreateEnvironment();
            var position = new BoardPosition(CombatSide.Player, BoardRow.Front, new BoardColumn(1));
            var card = new CombatCardState(new DefinitionId("test.rescued_card"), new InstanceId(1),
                new CardRank(2), hpCapacity: 5, currentHp: 2, armor: 0, attack: 3);
            var player = new CombatSideState(new CombatBoardState(CombatSide.Player,
                new[] { new CombatSlotState(new SlotId(1), position, card.InstanceId) }),
                new CombatCardRegistry(new[] { card }), new BattleHealth(20), new AttackMultiplier(1));
            var state = new CombatState(player, e.State.Enemy, e.State.PlayerPets, e.State.EnemyPets);
            var metadata = new CombatEventMetadataFactory(new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            var log = new CombatEventLog();
            var root = new CombatStartedCombatEvent(metadata.CreateRoot());
            log.Append(root);
            card.SetCurrentHpToZero();
            var death = new DeathCombatEvent(metadata.CreateChild(root.Metadata), card.InstanceId, position, 2, 0);
            log.Append(death);
            var rescue = new CombatRescueResolver(metadata, log).ApplyRescue(state, death);
            var handler = new TestHandler(CombatSide.Player, e.PlayerPet.InstanceId, true);

            Assert.That(handler.CanTrigger(state, rescue), Is.True);
            handler.Resolve(state, rescue);

            Assert.That(handler.LastContext.SourceEvent, Is.SameAs(rescue));
            Assert.That(handler.LastPet, Is.SameAs(e.PlayerPet));
            Assert.That(card.CurrentHp, Is.EqualTo(1));
            Assert.That(card.Attack, Is.EqualTo(3));
            Assert.That(log.Count, Is.EqualTo(3));
            Assert.That(handler.ResolveCallCount, Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var playerPet =
                new CombatPetState(
                    new DefinitionId(
                        "player-pet"),
                    new InstanceId(1001));

            var enemyPet =
                new CombatPetState(
                    new DefinitionId(
                        "enemy-pet"),
                    new InstanceId(2001));

            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                playerPet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new[]
                            {
                                enemyPet
                            })));

            return new TestEnvironment
            {
                State =
                    state,

                PlayerPet =
                    playerPet,

                EnemyPet =
                    enemyPet
            };
        }

        private static RescueCombatEvent
            CreateSourceEvent()
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            return new RescueCombatEvent(
                metadata,
                new InstanceId(1),
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                previousHp: 0,
                currentHp: 1);
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

        private sealed class TestHandler :
            CombatPetRescueTriggerHandler
        {
            private readonly bool
                _canTrigger;

            public TestHandler(
                CombatSide side,
                InstanceId petInstanceId,
                bool canTrigger)
                : base(
                    side,
                    petInstanceId)
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

            public CombatPetRescueContext
                LastContext
            {
                get;
                private set;
            }

            public CombatPetState LastPet
            {
                get;
                private set;
            }

            protected override bool
                CanTriggerOnRescue(
                    CombatPetRescueContext context,
                    CombatPetState pet)
            {
                CanCallCount++;

                LastContext =
                    context;

                LastPet =
                    pet;

                return _canTrigger;
            }

            protected override void
                ResolveOnRescue(
                    CombatPetRescueContext context,
                    CombatPetState pet)
            {
                ResolveCallCount++;

                LastContext =
                    context;

                LastPet =
                    pet;
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatPetState PlayerPet
            {
                get;
                set;
            }

            public CombatPetState EnemyPet
            {
                get;
                set;
            }
        }
    }
}
