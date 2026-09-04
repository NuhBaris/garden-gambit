using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetNormalAttackTriggerHandlerContextFirstTests
    {
        [Test]
        public void
            ContextOnlyHandler_CanTriggerWithoutLegacyOverrides()
        {
            var handler =
                new ContextOnlyHandler(
                    CombatSide.Player,
                    canTrigger: true);

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent(
                    CombatSide.Player);

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
                handler.LastContext,
                Is.Not.Null);

            Assert.That(
                handler.LastContext.State,
                Is.SameAs(
                    state));

            Assert.That(
                handler.LastContext.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            ContextOnlyHandler_WhenRejecting_ReturnsFalse()
        {
            var handler =
                new ContextOnlyHandler(
                    CombatSide.Player,
                    canTrigger: false);

            var result =
                handler.InvokeCanPetTrigger(
                    CreateEmptyState(),
                    CreateNormalAttackEvent(
                        CombatSide.Player),
                    CreatePet());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.CanCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void
            ContextOnlyHandler_ResolveWorksWithoutLegacyOverrides()
        {
            var handler =
                new ContextOnlyHandler(
                    CombatSide.Enemy,
                    canTrigger: true);

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent(
                    CombatSide.Player);

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
                handler.LastContext.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                handler.LastContext.State,
                Is.SameAs(
                    state));

            Assert.That(
                handler.LastContext.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            HandlerWithoutOverrides_CanTriggerReturnsFalse()
        {
            var handler =
                new DefaultHandler();

            var result =
                handler.InvokeCanPetTrigger(
                    CreateEmptyState(),
                    CreateNormalAttackEvent(
                        CombatSide.Player),
                    CreatePet());

            Assert.That(
                result,
                Is.False);
        }

        [Test]
        public void
            HandlerWithoutResolveOverride_ResolveThrows()
        {
            var handler =
                new DefaultHandler();

            Assert.Throws<InvalidOperationException>(
                () => handler.InvokeResolvePetTrigger(
                    CreateEmptyState(),
                    CreateNormalAttackEvent(
                        CombatSide.Player),
                    CreatePet()));
        }

        private static NormalAttackCombatEvent
            CreateNormalAttackEvent(
                CombatSide attackerSide)
        {
            CombatSide targetSide;

            if (attackerSide ==
                CombatSide.Player)
            {
                targetSide =
                    CombatSide.Enemy;
            }
            else if (attackerSide ==
                     CombatSide.Enemy)
            {
                targetSide =
                    CombatSide.Player;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackerSide));
            }

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
                    attackerSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                new InstanceId(101),
                new BoardPosition(
                    targetSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                baseDamage: 5);
        }

        private static CombatPetState CreatePet()
        {
            return new CombatPetState(
                new DefinitionId(
                    "test-pet"),
                new InstanceId(1001));
        }

        private static CombatState CreateEmptyState()
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
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private sealed class ContextOnlyHandler :
            CombatPetNormalAttackTriggerHandler
        {
            private readonly bool
                _canTrigger;

            public ContextOnlyHandler(
                CombatSide side,
                bool canTrigger)
                : base(
                    side,
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

            public CombatPetNormalAttackContext
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
                    CombatPetNormalAttackContext context,
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
                ResolveOnNormalAttack(
                    CombatPetNormalAttackContext context,
                    CombatPetState pet)
            {
                ResolveCallCount++;

                LastContext =
                    context;

                LastPet =
                    pet;
            }
        }

        private sealed class DefaultHandler :
            CombatPetNormalAttackTriggerHandler
        {
            public DefaultHandler()
                : base(
                    CombatSide.Player,
                    new InstanceId(1001))
            {
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
        }
    }
}