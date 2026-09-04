using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartTriggerSourceTests
    {
        [Test]
        public void
            Constructor_WithValidHandler_ExposesHandlerIdentity()
        {
            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    new InstanceId(101),
                    true);

            var source =
                new CombatPetBattleStartTriggerSource(
                    handler);

            Assert.That(
                source.Handler,
                Is.SameAs(
                    handler));

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    new InstanceId(101)));

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
                    new InstanceId(101)));
        }

        [Test]
        public void Constructor_WithNullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleStartTriggerSource(
                        null));
        }

        [Test]
        public void
            DiscoverTriggers_WithCombatStartedEvent_ReturnsCandidate()
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
                    pet.InstanceId,
                    true);

            var source =
                new CombatPetBattleStartTriggerSource(
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
                candidates[0].OrderKey.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Pet));

            Assert.That(
                candidates[0].OrderKey.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                handler.LastPet,
                Is.SameAs(
                    pet));
        }

        [Test]
        public void
            DiscoverTriggers_WhenConditionFails_ReturnsEmpty()
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
                    pet.InstanceId,
                    false);

            var source =
                new CombatPetBattleStartTriggerSource(
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

        [Test]
        public void
            DiscoverTriggers_WithDifferentEventType_ReturnsEmpty()
        {
            var state =
                CreateState(
                    new CombatPetState[0],
                    new CombatPetState[0]);

            var handler =
                new TestBattleStartHandler(
                    CombatSide.Player,
                    new InstanceId(999),
                    true);

            var source =
                new CombatPetBattleStartTriggerSource(
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    new TestCombatEvent(
                        CreateMetadata()));

            Assert.That(
                candidates,
                Is.Empty);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(0));
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
                new TestBattleStartHandler(
                    CombatSide.Player,
                    secondPet.InstanceId,
                    true);

            var source =
                new CombatPetBattleStartTriggerSource(
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    CreateSourceEvent());

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].OrderKey.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Pet));

            Assert.That(
                candidates[0].OrderKey.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                candidates[0].OrderKey
                    .HorizontalOrder,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].OrderKey
                    .VerticalOrder,
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
                new TestBattleStartHandler(
                    CombatSide.Enemy,
                    enemyPet.InstanceId,
                    true);

            var source =
                new CombatPetBattleStartTriggerSource(
                    handler);

            var candidates =
                GetCandidates(
                    source,
                    state,
                    CreateSourceEvent());

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].OrderKey.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Pet));

            Assert.That(
                candidates[0].OrderKey.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                candidates[0].OrderKey
                    .HorizontalOrder,
                Is.EqualTo(0));

            Assert.That(
                candidates[0].OrderKey
                    .VerticalOrder,
                Is.EqualTo(0));
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
            private readonly bool
                _canTrigger;

            public TestBattleStartHandler(
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

            public int CanTriggerCallCount
            {
                get;
                private set;
            }

            public CombatEvent LastSourceEvent
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
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                CanTriggerCallCount++;

                LastSourceEvent =
                    sourceEvent;

                LastPet =
                    pet;

                return _canTrigger;
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