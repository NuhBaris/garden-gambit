using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetNormalAttackTriggerHandlerContextTests
    {
        [Test]
        public void
            CanPetTrigger_WithContextOverride_ReceivesExactContext()
        {
            var handler =
                new ContextTestHandler(
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
                handler.ContextCanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LegacyCanCallCount,
                Is.Zero);

            Assert.That(
                handler.LastContext,
                Is.Not.Null);

            Assert.That(
                handler.LastContext.State,
                Is.SameAs(
                    state));

            Assert.That(
                handler.LastContext.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                handler.LastContext.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastContext.SideState,
                Is.SameAs(
                    state.Player));

            Assert.That(
                handler.LastContext
                    .OpposingSideState,
                Is.SameAs(
                    state.Enemy));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            CanPetTrigger_WhenContextOverrideRejects_ReturnsFalse()
        {
            var handler =
                new ContextTestHandler(
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
                handler.ContextCanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LegacyCanCallCount,
                Is.Zero);
        }

        [Test]
        public void
            ResolvePetTrigger_WithContextOverride_ReceivesExactContext()
        {
            var handler =
                new ContextTestHandler(
                    CombatSide.Player,
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
                handler.ContextResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LegacyResolveCallCount,
                Is.Zero);

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
            Context_WithEnemyPetAndPlayerAttack_KeepsPetOwnerSideIndependent()
        {
            var handler =
                new ContextTestHandler(
                    CombatSide.Enemy,
                    canTrigger: true);

            var state =
                CreateEmptyState();

            var playerAttackEvent =
                CreateNormalAttackEvent(
                    CombatSide.Player);

            var result =
                handler.InvokeCanPetTrigger(
                    state,
                    playerAttackEvent,
                    CreatePet());

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.LastContext.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                handler.LastContext
                    .SourceEvent.AttackerSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                handler.LastContext.SideState,
                Is.SameAs(
                    state.Enemy));

            Assert.That(
                handler.LastContext
                    .OpposingSideState,
                Is.SameAs(
                    state.Player));

            Assert.That(
                handler.LegacyCanCallCount,
                Is.Zero);
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

        private sealed class
            ContextTestHandler :
            CombatPetNormalAttackTriggerHandler
        {
            private readonly bool
                _canTrigger;

            public ContextTestHandler(
                CombatSide side,
                bool canTrigger)
                : base(
                    side,
                    new InstanceId(1001))
            {
                _canTrigger =
                    canTrigger;
            }

            public int ContextCanCallCount
            {
                get;
                private set;
            }

            public int ContextResolveCallCount
            {
                get;
                private set;
            }

            public int LegacyCanCallCount
            {
                get;
                private set;
            }

            public int LegacyResolveCallCount
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
                    CombatPetNormalAttackContext
                        context,
                    CombatPetState pet)
            {
                ContextCanCallCount++;

                LastContext =
                    context;

                LastPet =
                    pet;

                return _canTrigger;
            }

            protected override void
                ResolveOnNormalAttack(
                    CombatPetNormalAttackContext
                        context,
                    CombatPetState pet)
            {
                ContextResolveCallCount++;

                LastContext =
                    context;

                LastPet =
                    pet;
            }

            protected override bool
                CanTriggerOnNormalAttack(
                    CombatState state,
                    NormalAttackCombatEvent sourceEvent,
                    CombatPetState pet)
            {
                LegacyCanCallCount++;

                return false;
            }

            protected override void
                ResolveOnNormalAttack(
                    CombatState state,
                    NormalAttackCombatEvent sourceEvent,
                    CombatPetState pet)
            {
                LegacyResolveCallCount++;
            }
        }
    }
}