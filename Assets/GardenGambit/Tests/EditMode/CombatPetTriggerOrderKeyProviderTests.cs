using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerOrderKeyProviderTests
    {
        [Test]
        public void
            Constructor_WithValidValues_SetsProperties()
        {
            var petInstanceId =
                new InstanceId(101);

            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Player,
                    petInstanceId);

            Assert.That(
                provider.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                provider.PetInstanceId,
                Is.EqualTo(
                    petInstanceId));
        }

        [Test]
        public void
            Constructor_WithEnemySide_AllowsEnemy()
        {
            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Enemy,
                    new InstanceId(101));

            Assert.That(
                provider.Side,
                Is.EqualTo(
                    CombatSide.Enemy));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatPetTriggerOrderKeyProvider(
                        default(CombatSide),
                        new InstanceId(101)));
        }

        [Test]
        public void
            Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetTriggerOrderKeyProvider(
                        CombatSide.Player,
                        default(InstanceId)));
        }

        [Test]
        public void GetOrderKey_WithNullState_Throws()
        {
            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Player,
                    new InstanceId(101));

            Assert.Throws<ArgumentNullException>(
                () => provider.GetOrderKey(
                    null,
                    CreateSourceEvent()));
        }

        [Test]
        public void
            GetOrderKey_WithNullSourceEvent_Throws()
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

            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Player,
                    pet.InstanceId);

            Assert.Throws<ArgumentNullException>(
                () => provider.GetOrderKey(
                    state,
                    null));
        }

        [Test]
        public void
            GetOrderKey_WithFirstPlayerPet_ReturnsFirstPetOrder()
        {
            var pet =
                CreatePet(
                    "pet.player.first",
                    101);

            var state =
                CreateState(
                    new[]
                    {
                        pet
                    },
                    new CombatPetState[0]);

            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Player,
                    pet.InstanceId);

            var orderKey =
                provider.GetOrderKey(
                    state,
                    CreateSourceEvent());

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
                Is.EqualTo(0));

            Assert.That(
                orderKey.VerticalOrder,
                Is.EqualTo(0));

            Assert.That(
                orderKey.IsValid,
                Is.True);
        }

        [Test]
        public void
            GetOrderKey_WithSecondPlayerPet_UsesRegistryOrder()
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

            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Player,
                    secondPet.InstanceId);

            var orderKey =
                provider.GetOrderKey(
                    state,
                    CreateSourceEvent());

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
            GetOrderKey_WithEnemyPet_UsesEnemyPetRegistry()
        {
            var playerPet =
                CreatePet(
                    "pet.player",
                    101);

            var firstEnemyPet =
                CreatePet(
                    "pet.enemy.first",
                    201);

            var secondEnemyPet =
                CreatePet(
                    "pet.enemy.second",
                    202);

            var state =
                CreateState(
                    new[]
                    {
                        playerPet
                    },
                    new[]
                    {
                        firstEnemyPet,
                        secondEnemyPet
                    });

            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Enemy,
                    secondEnemyPet.InstanceId);

            var orderKey =
                provider.GetOrderKey(
                    state,
                    CreateSourceEvent());

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
                Is.EqualTo(1));

            Assert.That(
                orderKey.VerticalOrder,
                Is.EqualTo(0));
        }

        [Test]
        public void
            GetOrderKey_WhenPetDoesNotBelongToSide_Throws()
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

            var provider =
                new CombatPetTriggerOrderKeyProvider(
                    CombatSide.Player,
                    enemyPet.InstanceId);

            Assert.Throws<ArgumentException>(
                () => provider.GetOrderKey(
                    state,
                    CreateSourceEvent()));
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
    }
}