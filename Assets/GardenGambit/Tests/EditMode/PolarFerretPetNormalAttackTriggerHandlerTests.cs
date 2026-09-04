using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        PolarFerretPetNormalAttackTriggerHandlerTests
    {
        [Test]
        public void Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        PolarFerretPetNormalAttackTriggerHandler(
                            CombatSide.Player,
                            new InstanceId(1001),
                            null,
                            new
                                CombatNormalAttackTargetDamageReductionRegistry()));
        }

        [Test]
        public void Constructor_WithNullReductionRegistry_Throws()
        {
            var usageCommitter =
                CreateUsageCommitter();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        PolarFerretPetNormalAttackTriggerHandler(
                            CombatSide.Player,
                            new InstanceId(1001),
                            usageCommitter,
                            null));
        }

        [Test]
        public void CanTrigger_WithOwnedWinterTarget_ReturnsTrue()
        {
            var pet =
                CreatePet(
                    1001,
                    "polar-ferret");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Winter);

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        attackEvent.TargetInstanceId),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void CanTrigger_WithOpposingWinterTarget_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001,
                        "polar-ferret"));

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    targetInstanceId: 101,
                    targetSeason:
                        CombatCardSeason.Winter);

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void CanTrigger_WithOwnedNonWinterTarget_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001,
                        "polar-ferret"));

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Summer);

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void CanTrigger_WithUsedPetCardKey_ReturnsFalse()
        {
            var pet =
                CreatePet(
                    1001,
                    "polar-ferret");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Winter);

            environment.UsageCommitter.TryCommit(
                pet.InstanceId,
                attackEvent.TargetInstanceId,
                () =>
                {
                });

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithOwnedWinterTarget_RegistersRequestWithoutUsage()
        {
            var pet =
                CreatePet(
                    1001,
                    "polar-ferret");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Winter);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            var requests =
                environment.ReductionRegistry
                    .GetRequests(
                        attackEvent.Metadata.EventId);

            Assert.That(
                requests.Count,
                Is.EqualTo(1));

            Assert.That(
                requests[0].PetInstanceId,
                Is.EqualTo(
                    pet.InstanceId));

            Assert.That(
                requests[0].TargetCardInstanceId,
                Is.EqualTo(
                    attackEvent.TargetInstanceId));

            Assert.That(
                requests[0].ReductionAmount,
                Is.EqualTo(
                    PolarFerretPetNormalAttackTriggerHandler
                        .DamageReduction));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        attackEvent.TargetInstanceId),
                Is.False);
        }

        [Test]
        public void Resolve_SameEventTwice_DoesNotDuplicateRequest()
        {
            var pet =
                CreatePet(
                    1001,
                    "polar-ferret");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Winter);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.ReductionRegistry
                    .GetRequests(
                        attackEvent.Metadata.EventId)
                    .Count,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        attackEvent.TargetInstanceId),
                Is.False);
        }

        [Test]
        public void Resolve_WithOwnedNonWinterTarget_DoesNotRegister()
        {
            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001,
                        "polar-ferret"));

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Autumn);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithOpposingWinterTarget_DoesNotRegister()
        {
            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001,
                        "polar-ferret"));

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    targetInstanceId: 101,
                    targetSeason:
                        CombatCardSeason.Winter);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithUsedPetCardKey_DoesNotRegister()
        {
            var pet =
                CreatePet(
                    1001,
                    "polar-ferret");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Winter);

            environment.UsageCommitter.TryCommit(
                pet.InstanceId,
                attackEvent.TargetInstanceId,
                () =>
                {
                });

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithTwoPolarFerrets_RegistersOneRequestPerPet()
        {
            var firstPet =
                CreatePet(
                    1001,
                    "polar-ferret-one");

            var secondPet =
                CreatePet(
                    1002,
                    "polar-ferret-two");

            var state =
                CreateState(
                    new[]
                    {
                        firstPet,
                        secondPet
                    });

            var usageCommitter =
                CreateUsageCommitter();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var firstHandler =
                new
                    PolarFerretPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        firstPet.InstanceId,
                        usageCommitter,
                        reductionRegistry);

            var secondHandler =
                new
                    PolarFerretPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        secondPet.InstanceId,
                        usageCommitter,
                        reductionRegistry);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    targetInstanceId: 1,
                    targetSeason:
                        CombatCardSeason.Winter);

            Assert.That(
                firstHandler.CanTrigger(
                    state,
                    attackEvent),
                Is.True);

            Assert.That(
                secondHandler.CanTrigger(
                    state,
                    attackEvent),
                Is.True);

            firstHandler.Resolve(
                state,
                attackEvent);

            secondHandler.Resolve(
                state,
                attackEvent);

            var requests =
                reductionRegistry.GetRequests(
                    attackEvent.Metadata.EventId);

            Assert.That(
                requests.Count,
                Is.EqualTo(2));

            Assert.That(
                requests[0].PetInstanceId,
                Is.EqualTo(
                    firstPet.InstanceId));

            Assert.That(
                requests[1].PetInstanceId,
                Is.EqualTo(
                    secondPet.InstanceId));

            Assert.That(
                usageCommitter.HasTriggered(
                    firstPet.InstanceId,
                    attackEvent.TargetInstanceId),
                Is.False);

            Assert.That(
                usageCommitter.HasTriggered(
                    secondPet.InstanceId,
                    attackEvent.TargetInstanceId),
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatPetState pet)
        {
            var state =
                CreateState(
                    new[]
                    {
                        pet
                    });

            var usageCommitter =
                CreateUsageCommitter();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            return new TestEnvironment
            {
                State =
                    state,

                UsageCommitter =
                    usageCommitter,

                ReductionRegistry =
                    reductionRegistry,

                Handler =
                    new
                        PolarFerretPetNormalAttackTriggerHandler(
                            CombatSide.Player,
                            pet.InstanceId,
                            usageCommitter,
                            reductionRegistry)
            };
        }

        private static
            CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new
                CombatPetCardTriggerUsageCommitter(
                    new
                        CombatPetCardTriggerUsageRegistry());
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets)
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
                        Array.Empty<
                            CombatPetState>())));
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

        private static CombatPetState CreatePet(
            long instanceId,
            string definitionId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                long eventId,
                long attackerInstanceId,
                CombatSide attackerSide,
                long targetInstanceId,
                CombatCardSeason targetSeason)
        {
            var targetSide =
                attackerSide ==
                CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            return new NormalAttackCombatEvent(
                CreateMetadata(
                    eventId),
                new InstanceId(
                    attackerInstanceId),
                new BoardPosition(
                    attackerSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardSeason.Summer,
                new InstanceId(
                    targetInstanceId),
                new BoardPosition(
                    targetSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                targetSeason,
                baseDamage: 5);
        }

        private static CombatEventMetadata
            CreateMetadata(
                long eventId)
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(
                    eventId),
                new CombatSequenceNumber(
                    eventId),
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

            public
                PolarFerretPetNormalAttackTriggerHandler
                Handler
            {
                get;
                set;
            }
        }
    }
}