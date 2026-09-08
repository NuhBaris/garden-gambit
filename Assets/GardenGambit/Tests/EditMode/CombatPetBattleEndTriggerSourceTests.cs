using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleEndTriggerSourceTests
    {
        [Test]
        public void Constructor_WithNullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleEndTriggerSource(
                        null));
        }

        [Test]
        public void
            Constructor_ExposesHandlerPetAndOrderKeyProvider()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true);

            Assert.That(
                environment.Source.Handler,
                Is.SameAs(
                    environment.Handler));

            Assert.That(
                environment.Source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                environment.Source.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            Assert.That(
                environment.Source
                    .OrderKeyProvider.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                environment.Source
                    .OrderKeyProvider.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));
        }

        [Test]
        public void
            DiscoverTriggers_WithAllowedBattleEnd_ReturnsOneCandidate()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(
                    environment.Handler));
        }

        [Test]
        public void
            DiscoverTriggers_WithRejectedBattleEnd_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: false);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                candidates.Count,
                Is.Zero);

            Assert.That(
                environment.Handler.CanCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void
            DiscoverTriggers_WithNonBattleEndEvent_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    new CombatStartedCombatEvent(
                        CreateRootMetadata()));

            Assert.That(
                candidates.Count,
                Is.Zero);

            Assert.That(
                environment.Handler.CanCallCount,
                Is.Zero);
        }

        [Test]
        public void
            DiscoverTriggers_UsesExactPetOrderKey()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true);

            var sourceEvent =
                CreateBattleEndEvent();

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    sourceEvent);

            var expectedOrderKey =
                environment.Source
                    .OrderKeyProvider
                    .GetOrderKey(
                        environment.State,
                        sourceEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].OrderKey,
                Is.EqualTo(
                    expectedOrderKey));

            Assert.That(
                candidates[0].OrderKey.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Pet));

            Assert.That(
                candidates[0].OrderKey.Side,
                Is.EqualTo(
                    CombatSide.Player));
        }

        [Test]
        public void
            DiscoveredHandler_WhenResolved_ReceivesExactBattleEndContext()
        {
            var environment =
                CreateEnvironment(
                    canTrigger: true);

            var sourceEvent =
                CreateBattleEndEvent();

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    sourceEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            candidates[0].Trigger.Resolve(
                environment.State,
                sourceEvent);

            Assert.That(
                environment.Handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Handler.LastContext,
                Is.Not.Null);

            Assert.That(
                environment.Handler.LastContext.State,
                Is.SameAs(
                    environment.State));

            Assert.That(
                environment.Handler
                    .LastContext.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                environment.Handler.LastPet,
                Is.SameAs(
                    environment.Pet));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool canTrigger)
        {
            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "test-battle-end-pet"),
                    new InstanceId(1001));

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
                            Array.Empty<
                                CombatPetState>())));

            var handler =
                new TestHandler(
                    CombatSide.Player,
                    pet.InstanceId,
                    canTrigger);

            return new TestEnvironment
            {
                State =
                    state,

                Pet =
                    pet,

                Handler =
                    handler,

                Source =
                    new
                        CombatPetBattleEndTriggerSource(
                            handler)
            };
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                CombatPetBattleEndTriggerSource source,
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

        private static
            BattleEndStartedCombatEvent
            CreateBattleEndEvent()
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            return new BattleEndStartedCombatEvent(
                metadata,
                new CombatBattleStartSnapshot(
                    new
                        CombatBattleStartSideSnapshot(
                            CombatSide.Player,
                            Array.Empty<
                                CombatBattleStartCardSnapshot>()),
                    new
                        CombatBattleStartSideSnapshot(
                            CombatSide.Enemy,
                            Array.Empty<
                                CombatBattleStartCardSnapshot>())));
        }

        private static CombatEventMetadata
            CreateRootMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
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
            CombatPetBattleEndTriggerHandler
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

            public CombatPetBattleEndContext
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
                CanTriggerAtBattleEnd(
                    CombatPetBattleEndContext
                        context,
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
                ResolveAtBattleEnd(
                    CombatPetBattleEndContext
                        context,
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

            public CombatPetState Pet
            {
                get;
                set;
            }

            public TestHandler Handler
            {
                get;
                set;
            }

            public CombatPetBattleEndTriggerSource
                Source
            {
                get;
                set;
            }
        }
    }
}