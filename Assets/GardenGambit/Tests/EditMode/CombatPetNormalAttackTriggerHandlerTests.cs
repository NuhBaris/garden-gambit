using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetNormalAttackTriggerHandlerTests
    {
        [Test]
        public void CanPetTrigger_WhenHandlerAllows_ReturnsTrue()
        {
            var handler =
                new TestHandler(
                    canTrigger: true);

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent();

            var pet =
                CreatePet();

            var result =
                handler.InvokeCanPetTrigger(
                    state,
                    sourceEvent,
                    pet);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.CanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastState,
                Is.SameAs(state));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                handler.LastPet,
                Is.SameAs(pet));
        }

        [Test]
        public void CanPetTrigger_WhenHandlerRejects_ReturnsFalse()
        {
            var handler =
                new TestHandler(
                    canTrigger: false);

            var result =
                handler.InvokeCanPetTrigger(
                    CreateEmptyState(),
                    CreateNormalAttackEvent(),
                    CreatePet());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.CanCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void ResolvePetTrigger_DelegatesExactArguments()
        {
            var handler =
                new TestHandler(
                    canTrigger: true);

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent();

            var pet =
                CreatePet();

            handler.InvokeResolvePetTrigger(
                state,
                sourceEvent,
                pet);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastState,
                Is.SameAs(state));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                handler.LastPet,
                Is.SameAs(pet));
        }

        [Test]
        public void ResolvePetTrigger_CalledTwice_DelegatesTwice()
        {
            var handler =
                new TestHandler(
                    canTrigger: true);

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent();

            var pet =
                CreatePet();

            handler.InvokeResolvePetTrigger(
                state,
                sourceEvent,
                pet);

            handler.InvokeResolvePetTrigger(
                state,
                sourceEvent,
                pet);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(2));
        }

        private static NormalAttackCombatEvent
            CreateNormalAttackEvent()
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            return new NormalAttackCombatEvent(
                metadata,
                new InstanceId(1),
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                new InstanceId(101),
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1)),
                baseDamage: 5);
        }

        private static CombatPetState
            CreatePet()
        {
            return new CombatPetState(
                new DefinitionId(
                    "test-pet"),
                new InstanceId(1001));
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
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private sealed class TestHandler :
            CombatPetNormalAttackTriggerHandler
        {
            private readonly bool
                _canTrigger;

            public TestHandler(
                bool canTrigger)
                : base(
                    CombatSide.Player,
                    new InstanceId(1001))
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

            public NormalAttackCombatEvent
                LastSourceEvent
            {
                get;
                private set;
            }

            public CombatPetState LastPet
            {
                get;
                private set;
            }

            public bool InvokeCanPetTrigger(
                CombatState state,
                NormalAttackCombatEvent sourceEvent,
                CombatPetState pet)
            {
                return CanPetTrigger(
                    state,
                    sourceEvent,
                    pet);
            }

            public void InvokeResolvePetTrigger(
                CombatState state,
                NormalAttackCombatEvent sourceEvent,
                CombatPetState pet)
            {
                ResolvePetTrigger(
                    state,
                    sourceEvent,
                    pet);
            }

            protected override bool
                CanTriggerOnNormalAttack(
                    CombatState state,
                    NormalAttackCombatEvent sourceEvent,
                    CombatPetState pet)
            {
                CanCallCount++;

                LastState =
                    state;

                LastSourceEvent =
                    sourceEvent;

                LastPet =
                    pet;

                return _canTrigger;
            }

            protected override void
                ResolveOnNormalAttack(
                    CombatState state,
                    NormalAttackCombatEvent sourceEvent,
                    CombatPetState pet)
            {
                ResolveCallCount++;

                LastState =
                    state;

                LastSourceEvent =
                    sourceEvent;

                LastPet =
                    pet;
            }
        }
    }
}