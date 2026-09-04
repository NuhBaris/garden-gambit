using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        SunBirdPetNormalAttackTriggerHandlerTests
    {
        [Test]
        public void
            Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        SunBirdPetNormalAttackTriggerHandler(
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
            var usageCommitter =
                CreateUsageCommitter();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        SunBirdPetNormalAttackTriggerHandler(
                            CombatSide.Player,
                            new InstanceId(1001),
                            usageCommitter,
                            null));
        }

        [Test]
        public void
            CanTrigger_WithOwnedSummerAttack_ReturnsTrue()
        {
            var pet =
                CreatePet(
                    1001,
                    "sun-bird");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

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
                        attackEvent
                            .AttackerInstanceId),
                Is.False);

            Assert.That(
                environment.ModifierRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            CanTrigger_WithOpposingSummerAttack_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001,
                        "sun-bird"));

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 101,
                    attackerSide:
                        CombatSide.Enemy,
                    season:
                        CombatCardSeason.Summer);

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.ModifierRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            CanTrigger_WithOwnedNonSummerAttack_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001,
                        "sun-bird"));

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Winter);

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent);

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.ModifierRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            Resolve_WithFirstOwnedSummerAttack_AddsOneDamageAndCommitsUsage()
        {
            var pet =
                CreatePet(
                    1001,
                    "sun-bird");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        attackEvent.Metadata.EventId),
                Is.EqualTo(
                    SunBirdPetNormalAttackTriggerHandler
                        .DamageBonus));

            Assert.That(
                environment.ModifierRegistry
                    .ResolveDamage(
                        attackEvent),
                Is.EqualTo(
                    attackEvent.BaseDamage + 1));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        attackEvent
                            .AttackerInstanceId),
                Is.True);
        }

        [Test]
        public void
            Resolve_WithSameCardsSecondAttack_DoesNotAddSecondBonus()
        {
            var pet =
                CreatePet(
                    1001,
                    "sun-bird");

            var environment =
                CreateEnvironment(
                    pet);

            var firstAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

            var secondAttack =
                CreateAttackEvent(
                    eventId: 3,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

            environment.Handler.Resolve(
                environment.State,
                firstAttack);

            var canTriggerAgain =
                environment.Handler.CanTrigger(
                    environment.State,
                    secondAttack);

            environment.Handler.Resolve(
                environment.State,
                secondAttack);

            Assert.That(
                canTriggerAgain,
                Is.False);

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        firstAttack.Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        secondAttack.Metadata.EventId),
                Is.Zero);

            Assert.That(
                environment.ModifierRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            Resolve_WithDifferentSummerCards_AllowsEachCardsFirstAttack()
        {
            var pet =
                CreatePet(
                    1001,
                    "sun-bird");

            var environment =
                CreateEnvironment(
                    pet);

            var firstCardAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

            var secondCardAttack =
                CreateAttackEvent(
                    eventId: 3,
                    attackerInstanceId: 2,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

            environment.Handler.Resolve(
                environment.State,
                firstCardAttack);

            environment.Handler.Resolve(
                environment.State,
                secondCardAttack);

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        firstCardAttack.Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.ModifierRegistry
                    .GetTotalModifier(
                        secondCardAttack.Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        firstCardAttack
                            .AttackerInstanceId),
                Is.True);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        secondCardAttack
                            .AttackerInstanceId),
                Is.True);
        }

        [Test]
        public void
            Resolve_WithTwoSunBirds_AllowsOneBonusPerPetForSameCard()
        {
            var firstPet =
                CreatePet(
                    1001,
                    "sun-bird-one");

            var secondPet =
                CreatePet(
                    1002,
                    "sun-bird-two");

            var state =
                CreateState(
                    new[]
                    {
                        firstPet,
                        secondPet
                    });

            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var firstHandler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        firstPet.InstanceId,
                        usageCommitter,
                        modifierRegistry);

            var secondHandler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        secondPet.InstanceId,
                        usageCommitter,
                        modifierRegistry);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Summer);

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

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                usageCommitter.HasTriggered(
                    firstPet.InstanceId,
                    attackEvent.AttackerInstanceId),
                Is.True);

            Assert.That(
                usageCommitter.HasTriggered(
                    secondPet.InstanceId,
                    attackEvent.AttackerInstanceId),
                Is.True);
        }

        [Test]
        public void
            Resolve_WithNonSummerAttack_DoesNotCommitOrModify()
        {
            var pet =
                CreatePet(
                    1001,
                    "sun-bird");

            var environment =
                CreateEnvironment(
                    pet);

            var attackEvent =
                CreateAttackEvent(
                    eventId: 2,
                    attackerInstanceId: 1,
                    attackerSide:
                        CombatSide.Player,
                    season:
                        CombatCardSeason.Autumn);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ModifierRegistry
                    .HasModifier(
                        attackEvent.Metadata.EventId),
                Is.False);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        attackEvent
                            .AttackerInstanceId),
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

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            return new TestEnvironment
            {
                State =
                    state,

                UsageCommitter =
                    usageCommitter,

                ModifierRegistry =
                    modifierRegistry,

                Handler =
                    new
                        SunBirdPetNormalAttackTriggerHandler(
                            CombatSide.Player,
                            pet.InstanceId,
                            usageCommitter,
                            modifierRegistry)
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
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
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
                CombatCardSeason season)
        {
            var targetSide =
                attackerSide ==
                CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            var targetInstanceId =
                attackerInstanceId + 100;

            return new NormalAttackCombatEvent(
                CreateMetadata(
                    eventId),
                new InstanceId(
                    attackerInstanceId),
                new BoardPosition(
                    attackerSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                season,
                new InstanceId(
                    targetInstanceId),
                new BoardPosition(
                    targetSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
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
        }
    }
}