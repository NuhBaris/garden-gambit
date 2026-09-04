using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartContextRoutingTests
    {
        [Test]
        public void CanTrigger_WithSnapshot_UsesContextPath()
        {
            var handler =
                new ContextAwareTestHandler();

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    includeSnapshot: true);

            var result =
                handler.InvokeCanPetTrigger(
                    state,
                    sourceEvent);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.ContextCanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LegacyCanCallCount,
                Is.EqualTo(0));

            Assert.That(
                handler.LastContext,
                Is.Not.Null);

            Assert.That(
                handler.LastContext.State,
                Is.SameAs(state));

            Assert.That(
                handler.LastContext.SourceEvent,
                Is.SameAs(sourceEvent));
        }

        [Test]
        public void Resolve_WithSnapshot_UsesContextPath()
        {
            var handler =
                new ContextAwareTestHandler();

            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    includeSnapshot: true);

            handler.InvokeResolvePetTrigger(
                state,
                sourceEvent);

            Assert.That(
                handler.ContextResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LegacyResolveCallCount,
                Is.EqualTo(0));

            Assert.That(
                handler.LastContext,
                Is.Not.Null);

            Assert.That(
                handler.LastContext
                    .BattleStartSnapshot,
                Is.SameAs(
                    sourceEvent
                        .BattleStartSnapshot));
        }

        [Test]
        public void CanTrigger_WithoutSnapshot_UsesLegacyPath()
        {
            var handler =
                new ContextAwareTestHandler();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    includeSnapshot: false);

            var result =
                handler.InvokeCanPetTrigger(
                    CreateEmptyState(),
                    sourceEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.LegacyCanCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ContextCanCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Resolve_WithoutSnapshot_UsesLegacyPath()
        {
            var handler =
                new ContextAwareTestHandler();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    includeSnapshot: false);

            handler.InvokeResolvePetTrigger(
                CreateEmptyState(),
                sourceEvent);

            Assert.That(
                handler.LegacyResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ContextResolveCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void ContextDefault_WithSnapshot_ForwardsToLegacyOverride()
        {
            var handler =
                new LegacyOnlyTestHandler();

            var result =
                handler.InvokeCanPetTrigger(
                    CreateEmptyState(),
                    CreateStageEvent(
                        CombatBattleStartStage.Pet,
                        includeSnapshot: true));

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.LegacyCanCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void NonPetStage_DoesNotUseEitherPath()
        {
            var handler =
                new ContextAwareTestHandler();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Slot,
                    includeSnapshot: true);

            var result =
                handler.InvokeCanPetTrigger(
                    CreateEmptyState(),
                    sourceEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.ContextCanCallCount,
                Is.EqualTo(0));

            Assert.That(
                handler.LegacyCanCallCount,
                Is.EqualTo(0));

            Assert.Throws<InvalidOperationException>(
                () => handler.InvokeResolvePetTrigger(
                    CreateEmptyState(),
                    sourceEvent));

            Assert.That(
                handler.ContextResolveCallCount,
                Is.EqualTo(0));

            Assert.That(
                handler.LegacyResolveCallCount,
                Is.EqualTo(0));
        }

        private static
            BattleStartStageStartedCombatEvent
            CreateStageEvent(
                CombatBattleStartStage stage,
                bool includeSnapshot)
        {
            var metadata =
                CreateDirectRootChildMetadata();

            if (includeSnapshot)
            {
                return new
                    BattleStartStageStartedCombatEvent(
                        metadata,
                        stage,
                        CreateEmptySnapshot());
            }

            return new
                BattleStartStageStartedCombatEvent(
                    metadata,
                    stage);
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
            return new CombatBattleStartSnapshot(
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    new CombatBattleStartCardSnapshot[0]),
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    new CombatBattleStartCardSnapshot[0]));
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

        private sealed class
            ContextAwareTestHandler :
            CombatPetBattleStartTriggerHandler
        {
            public ContextAwareTestHandler()
                : base(
                    CombatSide.Player,
                    new InstanceId(1))
            {
            }

            public int ContextCanCallCount
            {
                get;
                private set;
            }

            public int LegacyCanCallCount
            {
                get;
                private set;
            }

            public int ContextResolveCallCount
            {
                get;
                private set;
            }

            public int LegacyResolveCallCount
            {
                get;
                private set;
            }

            public CombatPetBattleStartContext
                LastContext
            {
                get;
                private set;
            }

            public bool InvokeCanPetTrigger(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent)
            {
                return CanPetTrigger(
                    state,
                    sourceEvent,
                    null);
            }

            public void InvokeResolvePetTrigger(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent)
            {
                ResolvePetTrigger(
                    state,
                    sourceEvent,
                    null);
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatPetBattleStartContext context,
                    CombatPetState pet)
            {
                ContextCanCallCount++;

                LastContext =
                    context;

                return true;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatPetBattleStartContext context,
                    CombatPetState pet)
            {
                ContextResolveCallCount++;

                LastContext =
                    context;
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                LegacyCanCallCount++;

                return false;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                LegacyResolveCallCount++;
            }
        }

        private sealed class
            LegacyOnlyTestHandler :
            CombatPetBattleStartTriggerHandler
        {
            public LegacyOnlyTestHandler()
                : base(
                    CombatSide.Player,
                    new InstanceId(2))
            {
            }

            public int LegacyCanCallCount
            {
                get;
                private set;
            }

            public bool InvokeCanPetTrigger(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent)
            {
                return CanPetTrigger(
                    state,
                    sourceEvent,
                    null);
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                LegacyCanCallCount++;

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