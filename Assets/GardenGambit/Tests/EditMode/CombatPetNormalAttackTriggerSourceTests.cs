using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetNormalAttackTriggerSourceTests
    {
        [Test]
        public void Constructor_WithNullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatPetNormalAttackTriggerSource(
                            null));
        }

        [Test]
        public void
            Constructor_ExposesHandlerPetAndOrderKeyProvider()
        {
            var environment =
                CreateEnvironment();

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
            DiscoverTriggers_WithOwnedSummerAttack_ReturnsOneCandidate()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                CreateAttackEvent(
                    CombatSide.Player,
                    CombatCardSeason.Summer);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    attackEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            DiscoverTriggers_WithOwnedNonSummerAttack_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                CreateAttackEvent(
                    CombatSide.Player,
                    CombatCardSeason.Winter);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    attackEvent);

            Assert.That(
                candidates.Count,
                Is.Zero);
        }

        [Test]
        public void
            DiscoverTriggers_WithOpposingSummerAttack_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                CreateAttackEvent(
                    CombatSide.Enemy,
                    CombatCardSeason.Summer);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    attackEvent);

            Assert.That(
                candidates.Count,
                Is.Zero);
        }

        [Test]
        public void
            DiscoverTriggers_WithNonNormalAttackEvent_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    CreateRootMetadata());

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    combatStartedEvent);

            Assert.That(
                candidates.Count,
                Is.Zero);
        }

        [Test]
        public void
            DiscoveredHandler_WhenResolved_AppliesSunBirdModifier()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                CreateAttackEvent(
                    CombatSide.Player,
                    CombatCardSeason.Summer);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    attackEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            environment.Source.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        attackEvent.Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        attackEvent
                            .AttackerInstanceId),
                Is.True);
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "sun-bird"),
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

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        new
                            CombatPetCardTriggerUsageRegistry());

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        pet.InstanceId,
                        usageCommitter,
                        modifierRegistry);

            return new TestEnvironment
            {
                State =
                    state,

                Pet =
                    pet,

                UsageCommitter =
                    usageCommitter,

                ModifierRegistry =
                    modifierRegistry,

                Handler =
                    handler,

                Source =
                    new
                        CombatPetNormalAttackTriggerSource(
                            handler)
            };
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                CombatPetNormalAttackTriggerSource source,
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
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                CombatSide attackerSide,
                CombatCardSeason season)
        {
            var targetSide =
                attackerSide ==
                CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            return new NormalAttackCombatEvent(
                CreateChildMetadata(),
                new InstanceId(1),
                new BoardPosition(
                    attackerSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                season,
                new InstanceId(101),
                new BoardPosition(
                    targetSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                baseDamage: 5);
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

        private static CombatEventMetadata
            CreateChildMetadata()
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                triggerRootId,
                triggerRootId);
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

            public CombatPetCardTriggerUsageCommitter
                UsageCommitter
            {
                get;
                set;
            }

            public
                CombatNormalAttackSourceDamageModifierRegistry
                ModifierRegistry
            {
                get;
                set;
            }

            public
                SunBirdPetNormalAttackTriggerHandler
                Handler
            {
                get;
                set;
            }

            public CombatPetNormalAttackTriggerSource
                Source
            {
                get;
                set;
            }
        }
    }
}