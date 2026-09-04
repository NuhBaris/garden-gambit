using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartTriggerHandlerTests
    {
        [Test]
        public void
            Constructor_WithValidValues_SetsProperties()
        {
            var petInstanceId =
                new InstanceId(101);

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    petInstanceId);

            Assert.That(
                handler.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                handler.PetInstanceId,
                Is.EqualTo(
                    petInstanceId));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new TestBattleStartHandler(
                        default(CombatSide),
                        new InstanceId(101)));
        }

        [Test]
        public void
            Constructor_WithInvalidPetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new TestBattleStartHandler(
                        CombatSide.Player,
                        default(InstanceId)));
        }

        [Test]
        public void
            CanTrigger_WithCombatStartedEvent_ForwardsExactValues()
        {
            var pet =
                CreatePet(
                    "pet.player",
                    101);

            var state =
                CreateState(
                    new[]
                    {
                        pet
                    },
                    new CombatPetState[0]);

            var sourceEvent =
                CreateSourceEvent();

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    pet.InstanceId)
                {
                    CanTriggerAtBattleStartResult =
                        true
                };

            var canTrigger =
                handler.CanTrigger(
                    state,
                    sourceEvent);

            Assert.That(
                canTrigger,
                Is.True);

            Assert.That(
                handler.CanTriggerAtBattleStartCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastCanTriggerState,
                Is.SameAs(
                    state));

            Assert.That(
                handler.LastCanTriggerEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastCanTriggerPet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            CanTrigger_WhenBattleStartConditionFails_ReturnsFalse()
        {
            var pet =
                CreatePet(
                    "pet.player",
                    101);

            var state =
                CreateState(
                    new[]
                    {
                        pet
                    },
                    new CombatPetState[0]);

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    pet.InstanceId)
                {
                    CanTriggerAtBattleStartResult =
                        false
                };

            var canTrigger =
                handler.CanTrigger(
                    state,
                    CreateSourceEvent());

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                handler.CanTriggerAtBattleStartCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void
            CanTrigger_WithDifferentEventType_ReturnsFalse()
        {
            var state =
                CreateState(
                    new CombatPetState[0],
                    new CombatPetState[0]);

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    new InstanceId(999))
                {
                    CanTriggerAtBattleStartResult =
                        true
                };

            var canTrigger =
                handler.CanTrigger(
                    state,
                    new TestCombatEvent(
                        CreateMetadata()));

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                handler.CanTriggerAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            Resolve_WithCombatStartedEvent_ForwardsExactValues()
        {
            var pet =
                CreatePet(
                    "pet.enemy",
                    201);

            var state =
                CreateState(
                    new CombatPetState[0],
                    new[]
                    {
                        pet
                    });

            var sourceEvent =
                CreateSourceEvent();

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Enemy,
                    pet.InstanceId);

            handler.Resolve(
                state,
                sourceEvent);

            Assert.That(
                handler.ResolveAtBattleStartCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastResolveState,
                Is.SameAs(
                    state));

            Assert.That(
                handler.LastResolveEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastResolvePet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            CanTrigger_WhenPetIsMissing_Throws()
        {
            var state =
                CreateState(
                    new CombatPetState[0],
                    new CombatPetState[0]);

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    new InstanceId(101))
                {
                    CanTriggerAtBattleStartResult =
                        true
                };

            Assert.Throws<KeyNotFoundException>(
                () => handler.CanTrigger(
                    state,
                    CreateSourceEvent()));

            Assert.That(
                handler.CanTriggerAtBattleStartCallCount,
                Is.EqualTo(0));
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets,
            CombatPetState[] enemyPets)
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        enemyPets)));
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

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
        }

        private static
            BattleStartStageStartedCombatEvent
            CreateSourceEvent()
        {
            return new BattleStartStageStartedCombatEvent(
                CreateMetadata(),
                CombatBattleStartStage.Pet);
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
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

            public bool
                CanTriggerAtBattleStartResult
            {
                get;
                set;
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

            public CombatState LastCanTriggerState
            {
                get;
                private set;
            }

            public BattleStartStageStartedCombatEvent
                LastCanTriggerEvent
            {
                get;
                private set;
            }

            public CombatPetState LastCanTriggerPet
            {
                get;
                private set;
            }

            public CombatState LastResolveState
            {
                get;
                private set;
            }

            public BattleStartStageStartedCombatEvent
                LastResolveEvent
            {
                get;
                private set;
            }

            public CombatPetState LastResolvePet
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

                LastCanTriggerState =
                    state;

                LastCanTriggerEvent =
                    sourceEvent;

                LastCanTriggerPet =
                    pet;

                return
                    CanTriggerAtBattleStartResult;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                ResolveAtBattleStartCallCount++;

                LastResolveState =
                    state;

                LastResolveEvent =
                    sourceEvent;

                LastResolvePet =
                    pet;
            }
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.CombatStarted)
            {
            }
        }
    }
}