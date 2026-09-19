using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetDeathTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, CombatSide.Player)]
        [TestCase(CombatSide.Player, CombatSide.Enemy)]
        [TestCase(CombatSide.Enemy, CombatSide.Player)]
        [TestCase(CombatSide.Enemy, CombatSide.Enemy)]
        public void CanTriggerAndResolve_DelegateExactContextAndPet(
            CombatSide petSide,
            CombatSide deathSide)
        {
            var state = CreateState();
            var pet = state.GetPets(petSide).GetPetAt(1);
            var sourceEvent = CreateDeathEvent(deathSide);

            var handler = new TestHandler(
                petSide,
                pet.InstanceId,
                canTrigger: true);

            var result = handler.CanTrigger(state, sourceEvent);

            Assert.That(result, Is.True);
            Assert.That(handler.CanCallCount, Is.EqualTo(1));
            Assert.That(handler.ResolveCallCount, Is.Zero);

            AssertContext(
                handler,
                state,
                petSide,
                sourceEvent,
                pet);

            handler.Resolve(state, sourceEvent);

            Assert.That(handler.CanCallCount, Is.EqualTo(1));
            Assert.That(handler.ResolveCallCount, Is.EqualTo(1));

            AssertContext(
                handler,
                state,
                petSide,
                sourceEvent,
                pet);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void CanTrigger_WhenHandlerRejects_ReturnsFalse(
            CombatSide side)
        {
            var state = CreateState();
            var pet = state.GetPets(side).GetPetAt(1);
            var sourceEvent = CreateDeathEvent(side);

            var handler = new TestHandler(
                side,
                pet.InstanceId,
                canTrigger: false);

            var result = handler.CanTrigger(state, sourceEvent);

            Assert.That(result, Is.False);
            Assert.That(handler.CanCallCount, Is.EqualTo(1));
            Assert.That(handler.ResolveCallCount, Is.Zero);

            AssertContext(
                handler,
                state,
                side,
                sourceEvent,
                pet);
        }

        [Test]
        public void NonDeathEvent_IsRejectedWithoutCallingDeathHooks()
        {
            var state = CreateState();
            var pet = state.PlayerPets.GetPetAt(1);

            var handler = new TestHandler(
                CombatSide.Player,
                pet.InstanceId,
                canTrigger: true);

            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());

            var sourceEvent = new CombatStartedCombatEvent(
                metadataFactory.CreateRoot());

            Assert.That(
                handler.CanTrigger(state, sourceEvent),
                Is.False);

            Assert.Throws<ArgumentException>(
                () => handler.Resolve(state, sourceEvent));

            Assert.That(handler.CanCallCount, Is.Zero);
            Assert.That(handler.ResolveCallCount, Is.Zero);
            Assert.That(handler.LastContext, Is.Null);
            Assert.That(handler.LastPet, Is.Null);
        }

        [Test]
        public void Resolve_CalledTwice_DelegatesTwice()
        {
            var state = CreateState();
            var pet = state.PlayerPets.GetPetAt(1);
            var sourceEvent = CreateDeathEvent(CombatSide.Player);

            var handler = new TestHandler(
                CombatSide.Player,
                pet.InstanceId,
                canTrigger: true);

            handler.Resolve(state, sourceEvent);
            handler.Resolve(state, sourceEvent);

            Assert.That(handler.CanCallCount, Is.Zero);
            Assert.That(handler.ResolveCallCount, Is.EqualTo(2));

            AssertContext(
                handler,
                state,
                CombatSide.Player,
                sourceEvent,
                pet);
        }

        private static void AssertContext(
            TestHandler handler,
            CombatState state,
            CombatSide side,
            DeathCombatEvent sourceEvent,
            CombatPetState pet)
        {
            var context = handler.LastContext;

            Assert.That(context, Is.Not.Null);
            Assert.That(context.State, Is.SameAs(state));
            Assert.That(context.Side, Is.EqualTo(side));
            Assert.That(context.SourceEvent, Is.SameAs(sourceEvent));
            Assert.That(handler.LastPet, Is.SameAs(pet));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    side == CombatSide.Player
                        ? state.Player
                        : state.Enemy));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    side == CombatSide.Player
                        ? state.Enemy
                        : state.Player));

            Assert.That(
                context.SidePetState,
                Is.SameAs(
                    side == CombatSide.Player
                        ? state.PlayerPets
                        : state.EnemyPets));

            Assert.That(
                context.GetAffectedRow(pet),
                Is.EqualTo(BoardRow.Back));
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateEmptySide(CombatSide.Player),
                CreateEmptySide(CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        new[]
                        {
                            CreatePet(1001),
                            CreatePet(1002)
                        })),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        new[]
                        {
                            CreatePet(2001),
                            CreatePet(2002)
                        })));
        }

        private static CombatSideState CreateEmptySide(
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

        private static CombatPetState CreatePet(long instanceId)
        {
            return new CombatPetState(
                new DefinitionId("test.death_handler_pet"),
                new InstanceId(instanceId));
        }

        private static DeathCombatEvent CreateDeathEvent(
            CombatSide side)
        {
            var rootId = new CombatEventId(1);

            return new DeathCombatEvent(
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootId,
                    rootId),
                new InstanceId(1),
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1)),
                previousHp: 3,
                currentHp: 0);
        }

        private sealed class TestHandler :
            CombatPetDeathTriggerHandler
        {
            private readonly bool _canTrigger;

            public TestHandler(
                CombatSide side,
                InstanceId petInstanceId,
                bool canTrigger)
                : base(side, petInstanceId)
            {
                _canTrigger = canTrigger;
            }

            public int CanCallCount { get; private set; }

            public int ResolveCallCount { get; private set; }

            public CombatPetDeathContext LastContext { get; private set; }

            public CombatPetState LastPet { get; private set; }

            protected override bool CanTriggerOnDeath(
                CombatPetDeathContext context,
                CombatPetState pet)
            {
                CanCallCount++;
                LastContext = context;
                LastPet = pet;

                return _canTrigger;
            }

            protected override void ResolveOnDeath(
                CombatPetDeathContext context,
                CombatPetState pet)
            {
                ResolveCallCount++;
                LastContext = context;
                LastPet = pet;
            }
        }
    }
}