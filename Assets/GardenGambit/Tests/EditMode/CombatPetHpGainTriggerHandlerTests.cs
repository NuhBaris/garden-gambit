using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetHpGainTriggerHandlerTests
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

        private static HpGainCombatEvent
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

            return new HpGainCombatEvent(
                metadata,
                new InstanceId(1),
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                previousHpCapacity: 10,
                currentHpCapacity: 10,
                previousHp: 5,
                currentHp: 6);
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
            CombatPetHpGainTriggerHandler
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

            public CombatPetHpGainContext
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
                CanTriggerOnHpGain(
                    CombatPetHpGainContext context,
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
                ResolveOnHpGain(
                    CombatPetHpGainContext context,
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