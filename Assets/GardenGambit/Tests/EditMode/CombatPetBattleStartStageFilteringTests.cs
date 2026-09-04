using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartStageFilteringTests
    {
        [Test]
        public void
            CanTrigger_WithSlotStage_ReturnsFalse()
        {
            var environment =
                CreateEnvironment();

            var canTrigger =
                environment.Handler.CanTrigger(
                    environment.State,
                    CreateStageEvent(
                        CombatBattleStartStage.Slot));

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                environment.Handler
                    .CanTriggerAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            CanTrigger_WithCardStage_ReturnsFalse()
        {
            var environment =
                CreateEnvironment();

            var canTrigger =
                environment.Handler.CanTrigger(
                    environment.State,
                    CreateStageEvent(
                        CombatBattleStartStage.Card));

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                environment.Handler
                    .CanTriggerAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            Resolve_WithSlotStage_ThrowsWithoutResolving()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                InvalidOperationException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    CreateStageEvent(
                        CombatBattleStartStage.Slot)));

            Assert.That(
                environment.Handler
                    .ResolveAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            Source_WithSlotStage_ReturnsNoCandidate()
        {
            var environment =
                CreateEnvironment();

            var candidates =
                GetCandidates(
                    environment.Source,
                    environment.State,
                    CreateStageEvent(
                        CombatBattleStartStage.Slot));

            Assert.That(
                candidates,
                Is.Empty);

            Assert.That(
                environment.Handler
                    .CanTriggerAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            Source_WithCardStage_ReturnsNoCandidate()
        {
            var environment =
                CreateEnvironment();

            var candidates =
                GetCandidates(
                    environment.Source,
                    environment.State,
                    CreateStageEvent(
                        CombatBattleStartStage.Card));

            Assert.That(
                candidates,
                Is.Empty);

            Assert.That(
                environment.Handler
                    .CanTriggerAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "pet.player"),
                    new InstanceId(101));

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
                                pet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new CombatPetState[0])));

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    pet.InstanceId);

            return new TestEnvironment
            {
                State = state,
                Handler = handler,
                Source =
                    new CombatPetBattleStartTriggerSource(
                        handler)
            };
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            GetCandidates(
                CombatPetBattleStartTriggerSource
                    source,
                CombatState state,
                CombatEvent sourceEvent)
        {
            return new List<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>(
                source.DiscoverTriggers(
                    state,
                    sourceEvent));
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
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static
            BattleStartStageStartedCombatEvent
            CreateStageEvent(
                CombatBattleStartStage stage)
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            return new
                BattleStartStageStartedCombatEvent(
                    metadata,
                    stage);
        }

        private sealed class
            TestBattleStartHandler :
            CombatPetBattleStartTriggerHandler
        {
            public TestBattleStartHandler(
                CombatSide side,
                InstanceId petInstanceId)
                : base(
                    side,
                    petInstanceId)
            {
            }

            public int
                CanTriggerAtBattleStartCallCount
            {
                get;
                private set;
            }

            public int
                ResolveAtBattleStartCallCount
            {
                get;
                private set;
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                CanTriggerAtBattleStartCallCount++;

                return true;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                ResolveAtBattleStartCallCount++;
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public TestBattleStartHandler Handler
            {
                get;
                set;
            }

            public CombatPetBattleStartTriggerSource
                Source
            {
                get;
                set;
            }
        }
    }
}