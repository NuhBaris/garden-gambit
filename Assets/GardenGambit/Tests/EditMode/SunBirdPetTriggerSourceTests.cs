using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class SunBirdPetTriggerSourceTests
    {
        [Test]
        public void
            Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new SunBirdPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(1001),
                        null,
                        new
                            CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [Test]
        public void
            Constructor_WithNullModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new SunBirdPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(1001),
                        CreateUsageCommitter(),
                        null));
        }

        [Test]
        public void
            Constructor_ExposesExactDependenciesAndIdentity()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var source =
                new SunBirdPetTriggerSource(
                    CombatSide.Player,
                    new InstanceId(1001),
                    usageCommitter,
                    modifierRegistry);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    new InstanceId(1001)));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                source.SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));

            Assert.That(
                source.Handler,
                Is.Not.Null);

            Assert.That(
                source.Handler.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.Handler.PetInstanceId,
                Is.EqualTo(
                    new InstanceId(1001)));

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.OrderKeyProvider
                    .PetInstanceId,
                Is.EqualTo(
                    new InstanceId(1001)));
        }

        [Test]
        public void
            DiscoverTriggers_WithOwnedSummerAttack_ReturnsOneCandidate()
        {
            var environment =
                CreateEnvironment();

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateAttackEvent(
                        CombatSide.Player,
                        CombatCardSeason.Summer));

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            DiscoverTriggers_WithNonSummerOrOpposingAttack_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment();

            var nonSummerCandidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateAttackEvent(
                        CombatSide.Player,
                        CombatCardSeason.Autumn));

            var opposingCandidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateAttackEvent(
                        CombatSide.Enemy,
                        CombatCardSeason.Summer));

            Assert.That(
                nonSummerCandidates.Count,
                Is.Zero);

            Assert.That(
                opposingCandidates.Count,
                Is.Zero);
        }

        [Test]
        public void
            HandlerResolution_AddsModifierAndCommitsUsage()
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

            Assert.That(
                environment.Source
                    .DiscoverTriggers(
                        environment.State,
                        attackEvent),
                Is.Empty);
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "sun-bird"),
                    new InstanceId(1001));

            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var source =
                new SunBirdPetTriggerSource(
                    CombatSide.Player,
                    pet.InstanceId,
                    usageCommitter,
                    modifierRegistry);

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

                Source =
                    source
            };
        }

        private static CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new
                CombatPetCardTriggerUsageCommitter(
                    new
                        CombatPetCardTriggerUsageRegistry());
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                SunBirdPetTriggerSource source,
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

            public SunBirdPetTriggerSource Source
            {
                get;
                set;
            }
        }
    }
}