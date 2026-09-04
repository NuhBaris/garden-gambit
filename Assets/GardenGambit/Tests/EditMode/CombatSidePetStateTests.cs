using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSidePetStateTests
    {
        [Test]
        public void Constructor_WithPlayerSide_SetsValues()
        {
            var registry =
                CreateRegistry();

            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    registry);

            Assert.That(
                sideState.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                sideState.Pets,
                Is.SameAs(registry));

            Assert.That(
                sideState.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Constructor_WithEnemySide_SetsValues()
        {
            var registry =
                CreateRegistry();

            var sideState =
                new CombatSidePetState(
                    CombatSide.Enemy,
                    registry);

            Assert.That(
                sideState.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                sideState.Pets,
                Is.SameAs(registry));

            Assert.That(
                sideState.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSidePetState(
                        default(CombatSide),
                        CreateRegistry()));
        }

        [Test]
        public void Constructor_WithNullRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSidePetState(
                        CombatSide.Player,
                        null));
        }

        [Test]
        public void Constructor_WithEmptyRegistry_AllowsEmptySide()
        {
            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        new CombatPetState[0]));

            Assert.That(
                sideState.Count,
                Is.Zero);

            Assert.That(
                sideState.Pets.Pets,
                Is.Empty);
        }

        [Test]
        public void GetPetAt_WithValidSourceOrder_ReturnsExactPet()
        {
            var registry =
                CreateRegistry();

            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    registry);

            Assert.That(
                sideState.GetPetAt(0),
                Is.SameAs(
                    registry.Pets[0]));

            Assert.That(
                sideState.GetPetAt(1),
                Is.SameAs(
                    registry.Pets[1]));
        }

        [Test]
        public void GetPetAt_WithNegativeSourceOrder_Throws()
        {
            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    CreateRegistry());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => sideState.GetPetAt(-1));
        }

        [Test]
        public void GetPetAt_WithSourceOrderEqualToCount_Throws()
        {
            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    CreateRegistry());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => sideState.GetPetAt(
                    sideState.Count));
        }

        [Test]
        public void GetSourceOrder_WithExistingPet_ReturnsRegistryIndex()
        {
            var registry =
                CreateRegistry();

            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    registry);

            Assert.That(
                sideState.GetSourceOrder(
                    registry.Pets[0].InstanceId),
                Is.Zero);

            Assert.That(
                sideState.GetSourceOrder(
                    registry.Pets[1].InstanceId),
                Is.EqualTo(1));
        }

        [Test]
        public void GetSourceOrder_WithInvalidInstanceId_Throws()
        {
            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    CreateRegistry());

            Assert.Throws<ArgumentException>(
                () => sideState.GetSourceOrder(
                    default(InstanceId)));
        }

        [Test]
        public void GetSourceOrder_WithPetNotOwnedBySide_Throws()
        {
            var sideState =
                new CombatSidePetState(
                    CombatSide.Player,
                    CreateRegistry());

            Assert.Throws<ArgumentException>(
                () => sideState.GetSourceOrder(
                    new InstanceId(999)));
        }

        private static CombatPetRegistry
            CreateRegistry()
        {
            return new CombatPetRegistry(
                new[]
                {
                    CreatePet(
                        "pet-first",
                        100),

                    CreatePet(
                        "pet-second",
                        200)
                });
        }

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId));
        }
    }
}