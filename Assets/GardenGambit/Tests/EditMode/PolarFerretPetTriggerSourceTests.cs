using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        PolarFerretPetTriggerSourceTests
    {
        [Test]
        public void Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new PolarFerretPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(1001),
                        null,
                        new
                            CombatNormalAttackTargetDamageReductionRegistry()));
        }

        [Test]
        public void Constructor_WithNullReductionRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new PolarFerretPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(1001),
                        CreateUsageCommitter(),
                        null));
        }

        [Test]
        public void Constructor_ExposesExactDependenciesAndIdentity()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var source =
                new PolarFerretPetTriggerSource(
                    CombatSide.Player,
                    new InstanceId(1001),
                    usageCommitter,
                    reductionRegistry);

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
                source.TargetDamageReductionRegistry,
                Is.SameAs(
                    reductionRegistry));

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
        public void DiscoverTriggers_WithOwnedWinterTarget_ReturnsOneCandidate()
        {
            var environment =
                CreateEnvironment();

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateAttackEvent(
                        CombatSide.Player,
                        CombatCardSeason.Winter));

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            DiscoverTriggers_WithNonWinterOrOpposingTarget_ReturnsNoCandidates()
        {
            var environment =
                CreateEnvironment();

            var nonWinterCandidates =
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
                        CombatCardSeason.Winter));

            Assert.That(
                nonWinterCandidates.Count,
                Is.Zero);

            Assert.That(
                opposingCandidates.Count,
                Is.Zero);
        }

        [Test]
        public void
            HandlerResolution_RegistersThenConsumesReductionAtDamageStage()
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
                Is.EqualTo(1));

            environment.Source.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        attackEvent.TargetInstanceId),
                Is.False);

            Assert.That(
                environment.Source
                    .DiscoverTriggers(
                        environment.State,
                        attackEvent),
                Is.Not.Empty);

            var targetResolver =
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        environment.ReductionRegistry,
                        environment.UsageCommitter);

            var resolvedDamage =
                targetResolver.ResolveDamage(
                    attackEvent,
                    incomingDamage: 5);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(4));

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        attackEvent.TargetInstanceId),
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
                        "polar-ferret"),
                    new InstanceId(1001));

            var usageCommitter =
                CreateUsageCommitter();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var source =
                new PolarFerretPetTriggerSource(
                    CombatSide.Player,
                    pet.InstanceId,
                    usageCommitter,
                    reductionRegistry);

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

                ReductionRegistry =
                    reductionRegistry,

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
                PolarFerretPetTriggerSource source,
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
                    Array.Empty<
                        CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<
                        CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                CombatSide targetSide,
                CombatCardSeason targetSeason)
        {
            var attackerSide =
                targetSide ==
                CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            var attackerInstanceId =
                attackerSide ==
                CombatSide.Player
                    ? new InstanceId(1)
                    : new InstanceId(101);

            var targetInstanceId =
                targetSide ==
                CombatSide.Player
                    ? new InstanceId(1)
                    : new InstanceId(101);

            return new NormalAttackCombatEvent(
                CreateChildMetadata(),
                attackerInstanceId,
                new BoardPosition(
                    attackerSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardSeason.Summer,
                targetInstanceId,
                new BoardPosition(
                    targetSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                targetSeason,
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
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry
            {
                get;
                set;
            }

            public PolarFerretPetTriggerSource Source
            {
                get;
                set;
            }
        }
    }
}