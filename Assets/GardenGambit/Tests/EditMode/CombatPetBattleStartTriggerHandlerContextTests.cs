using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartTriggerHandlerContextTests
    {
        [Test]
        public void CreateContext_WithPlayerHandler_UsesPlayerSide()
        {
            var state =
                CreateEmptyState();

            var snapshot =
                CreateEmptySnapshot();

            var sourceEvent =
                CreatePetStageEvent(
                    snapshot);

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    new InstanceId(1));

            var context =
                handler.CreateContext(
                    state,
                    sourceEvent);

            Assert.That(
                handler.ExposedPetSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                context.SideSnapshot,
                Is.SameAs(
                    snapshot.Player));

            Assert.That(
                context.OpposingSideSnapshot,
                Is.SameAs(
                    snapshot.Enemy));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    state.Player));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    state.Enemy));
        }

        [Test]
        public void CreateContext_WithEnemyHandler_UsesEnemySide()
        {
            var state =
                CreateEmptyState();

            var snapshot =
                CreateEmptySnapshot();

            var sourceEvent =
                CreatePetStageEvent(
                    snapshot);

            var handler =
                new TestHandler(
                    CombatSide.Enemy,
                    new InstanceId(101));

            var context =
                handler.CreateContext(
                    state,
                    sourceEvent);

            Assert.That(
                handler.ExposedPetSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                context.SideSnapshot,
                Is.SameAs(
                    snapshot.Enemy));

            Assert.That(
                context.OpposingSideSnapshot,
                Is.SameAs(
                    snapshot.Player));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    state.Enemy));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    state.Player));
        }

        [Test]
        public void CreateContext_UsesExactStageSnapshot()
        {
            var state =
                CreateEmptyState();

            var snapshot =
                CreateEmptySnapshot();

            var sourceEvent =
                CreatePetStageEvent(
                    snapshot);

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    new InstanceId(1));

            var context =
                handler.CreateContext(
                    state,
                    sourceEvent);

            Assert.That(
                context.State,
                Is.SameAs(state));

            Assert.That(
                context.SourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                context.BattleStartSnapshot,
                Is.SameAs(snapshot));
        }

        [Test]
        public void CreateContext_WithNonPetStage_Throws()
        {
            var sourceEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectRootChildMetadata(),
                    CombatBattleStartStage.Slot,
                    CreateEmptySnapshot());

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    new InstanceId(1));

            Assert.Throws<ArgumentException>(
                () => handler.CreateContext(
                    CreateEmptyState(),
                    sourceEvent));
        }

        [Test]
        public void CreateContext_WithoutSnapshot_Throws()
        {
            var sourceEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectRootChildMetadata(),
                    CombatBattleStartStage.Pet);

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    new InstanceId(1));

            Assert.Throws<InvalidOperationException>(
                () => handler.CreateContext(
                    CreateEmptyState(),
                    sourceEvent));
        }

        private static
            BattleStartStageStartedCombatEvent
            CreatePetStageEvent(
                CombatBattleStartSnapshot snapshot)
        {
            return new BattleStartStageStartedCombatEvent(
                CreateDirectRootChildMetadata(),
                CombatBattleStartStage.Pet,
                snapshot);
        }

        private static CombatEventMetadata
            CreateDirectRootChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }

        private static CombatBattleStartSnapshot
            CreateEmptySnapshot()
        {
            var player =
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    new CombatBattleStartCardSnapshot[0]);

            var enemy =
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    new CombatBattleStartCardSnapshot[0]);

            return new CombatBattleStartSnapshot(
                player,
                enemy);
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
            CombatPetBattleStartTriggerHandler
        {
            public TestHandler(
                CombatSide side,
                InstanceId petInstanceId)
                : base(
                    side,
                    petInstanceId)
            {
            }

            public CombatSide ExposedPetSide =>
                PetSide;

            public CombatPetBattleStartContext
                CreateContext(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent)
            {
                return CreateBattleStartContext(
                    state,
                    sourceEvent);
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                return true;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
            }
        }
    }
}