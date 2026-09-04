using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetEventTriggerHandlerTests
    {
        [Test]
        public void
            Constructor_WithValidValues_SetsProperties()
        {
            var petInstanceId =
                new InstanceId(101);

            var handler =
                new TestPetTriggerHandler(
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
                    new TestPetTriggerHandler(
                        default(CombatSide),
                        new InstanceId(101)));
        }

        [Test]
        public void
            Constructor_WithInvalidPetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new TestPetTriggerHandler(
                        CombatSide.Player,
                        default(InstanceId)));
        }

        [Test]
        public void
            CanTrigger_WithMatchingEvent_ForwardsExactPet()
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
                new TestPetTriggerHandler(
                    CombatSide.Player,
                    pet.InstanceId)
                {
                    CanPetTriggerResult = true
                };

            var canTrigger =
                handler.CanTrigger(
                    state,
                    sourceEvent);

            Assert.That(
                canTrigger,
                Is.True);

            Assert.That(
                handler.CanPetTriggerCallCount,
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
            CanTrigger_WhenPetConditionFails_ReturnsFalse()
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
                new TestPetTriggerHandler(
                    CombatSide.Player,
                    pet.InstanceId)
                {
                    CanPetTriggerResult = false
                };

            var canTrigger =
                handler.CanTrigger(
                    state,
                    CreateSourceEvent());

            Assert.That(
                canTrigger,
                Is.False);

            Assert.That(
                handler.CanPetTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastCanTriggerPet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            CanTrigger_WithDifferentEventType_ReturnsFalseWithoutPetLookup()
        {
            var state =
                CreateState(
                    new CombatPetState[0],
                    new CombatPetState[0]);

            var handler =
                new TestPetTriggerHandler(
                    CombatSide.Player,
                    new InstanceId(999))
                {
                    CanPetTriggerResult = true
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
                handler.CanPetTriggerCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void
            Resolve_WithMatchingEvent_ForwardsExactPet()
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
                new TestPetTriggerHandler(
                    CombatSide.Enemy,
                    pet.InstanceId);

            handler.Resolve(
                state,
                sourceEvent);

            Assert.That(
                handler.ResolvePetTriggerCallCount,
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
                new TestPetTriggerHandler(
                    CombatSide.Player,
                    new InstanceId(101))
                {
                    CanPetTriggerResult = true
                };

            Assert.Throws<KeyNotFoundException>(
                () => handler.CanTrigger(
                    state,
                    CreateSourceEvent()));

            Assert.That(
                handler.CanPetTriggerCallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Resolve_WhenPetIsMissing_Throws()
        {
            var state =
                CreateState(
                    new CombatPetState[0],
                    new CombatPetState[0]);

            var handler =
                new TestPetTriggerHandler(
                    CombatSide.Enemy,
                    new InstanceId(201));

            Assert.Throws<KeyNotFoundException>(
                () => handler.Resolve(
                    state,
                    CreateSourceEvent()));

            Assert.That(
                handler.ResolvePetTriggerCallCount,
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

        private static CombatStartedCombatEvent
            CreateSourceEvent()
        {
            return new CombatStartedCombatEvent(
                CreateMetadata());
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private sealed class
            TestPetTriggerHandler :
            CombatPetEventTriggerHandler<
                CombatStartedCombatEvent>
        {
            public TestPetTriggerHandler(
                CombatSide side,
                InstanceId petInstanceId)
                : base(
                    side,
                    petInstanceId)
            {
            }

            public bool CanPetTriggerResult
            {
                get;
                set;
            }

            public int CanPetTriggerCallCount
            {
                get;
                private set;
            }

            public int ResolvePetTriggerCallCount
            {
                get;
                private set;
            }

            public CombatState LastCanTriggerState
            {
                get;
                private set;
            }

            public CombatStartedCombatEvent
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

            public CombatStartedCombatEvent
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
                CanPetTrigger(
                    CombatState state,
                    CombatStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                CanPetTriggerCallCount++;

                LastCanTriggerState =
                    state;

                LastCanTriggerEvent =
                    sourceEvent;

                LastCanTriggerPet =
                    pet;

                return CanPetTriggerResult;
            }

            protected override void
                ResolvePetTrigger(
                    CombatState state,
                    CombatStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                ResolvePetTriggerCallCount++;

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