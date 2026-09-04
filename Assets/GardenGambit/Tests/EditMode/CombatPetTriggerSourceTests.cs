using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerSourceTests
    {
        [Test]
        public void
            Constructor_WithValidValues_SetsProperties()
        {
            var handler =
                new TestTriggerHandler(
                    true);

            var petInstanceId =
                new InstanceId(101);

            var source =
                new CombatPetTriggerSource(
                    CombatSide.Player,
                    petInstanceId,
                    handler);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    petInstanceId));

            Assert.That(
                source.Handler,
                Is.SameAs(
                    handler));

            Assert.That(
                source.OrderKeyProvider,
                Is.Not.Null);

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.OrderKeyProvider
                    .PetInstanceId,
                Is.EqualTo(
                    petInstanceId));
        }

        [Test]
        public void Constructor_WithNullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(101),
                        null));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatPetTriggerSource(
                        default(CombatSide),
                        new InstanceId(101),
                        new TestTriggerHandler(
                            true)));
        }

        [Test]
        public void
            Constructor_WithInvalidPetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetTriggerSource(
                        CombatSide.Player,
                        default(InstanceId),
                        new TestTriggerHandler(
                            true)));
        }

        [Test]
        public void
            DiscoverTriggers_WhenHandlerCanTrigger_ReturnsCandidate()
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
                new TestTriggerHandler(
                    true);

            var source =
                new CombatPetTriggerSource(
                    CombatSide.Player,
                    pet.InstanceId,
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    sourceEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(
                    handler));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastState,
                Is.SameAs(
                    state));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(
                    sourceEvent));
        }

        [Test]
        public void
            DiscoverTriggers_WithSecondPet_UsesRegistryOrder()
        {
            var firstPet =
                CreatePet(
                    "pet.player.first",
                    101);

            var secondPet =
                CreatePet(
                    "pet.player.second",
                    102);

            var state =
                CreateState(
                    new[]
                    {
                        firstPet,
                        secondPet
                    },
                    new CombatPetState[0]);

            var handler =
                new TestTriggerHandler(
                    true);

            var source =
                new CombatPetTriggerSource(
                    CombatSide.Player,
                    secondPet.InstanceId,
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    CreateSourceEvent());

            var orderKey =
                candidates[0].OrderKey;

            Assert.That(
                orderKey.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Pet));

            Assert.That(
                orderKey.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                orderKey.HorizontalOrder,
                Is.EqualTo(1));

            Assert.That(
                orderKey.VerticalOrder,
                Is.EqualTo(0));
        }

        [Test]
        public void
            DiscoverTriggers_WithEnemyPet_UsesEnemySide()
        {
            var enemyPet =
                CreatePet(
                    "pet.enemy",
                    201);

            var state =
                CreateState(
                    new CombatPetState[0],
                    new[]
                    {
                        enemyPet
                    });

            var handler =
                new TestTriggerHandler(
                    true);

            var source =
                new CombatPetTriggerSource(
                    CombatSide.Enemy,
                    enemyPet.InstanceId,
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    CreateSourceEvent());

            var orderKey =
                candidates[0].OrderKey;

            Assert.That(
                orderKey.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Pet));

            Assert.That(
                orderKey.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                orderKey.HorizontalOrder,
                Is.EqualTo(0));

            Assert.That(
                orderKey.VerticalOrder,
                Is.EqualTo(0));
        }

        [Test]
        public void
            DiscoverTriggers_WhenHandlerCannotTrigger_ReturnsEmpty()
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
                new TestTriggerHandler(
                    false);

            var source =
                new CombatPetTriggerSource(
                    CombatSide.Player,
                    pet.InstanceId,
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    CreateSourceEvent());

            Assert.That(
                candidates,
                Is.Empty);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            GetCandidates(
                CombatPetTriggerSource source,
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
            var eventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    eventId,
                    new CombatSequenceNumber(1),
                    null,
                    eventId);

            return new CombatStartedCombatEvent(
                metadata);
        }

        private sealed class TestTriggerHandler :
            ICombatTriggerHandler
        {
            private readonly bool
                _canTrigger;

            public TestTriggerHandler(
                bool canTrigger)
            {
                _canTrigger =
                    canTrigger;
            }

            public int CanTriggerCallCount
            {
                get;
                private set;
            }

            public CombatState LastState
            {
                get;
                private set;
            }

            public CombatEvent LastSourceEvent
            {
                get;
                private set;
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                CanTriggerCallCount++;

                LastState = state;
                LastSourceEvent = sourceEvent;

                return _canTrigger;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
            }
        }
    }
}