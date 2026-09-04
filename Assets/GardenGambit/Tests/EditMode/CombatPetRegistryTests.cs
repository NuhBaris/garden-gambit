using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetRegistryTests
    {
        [Test]
        public void Constructor_WithValidPets_PreservesOrder()
        {
            var firstPet =
                CreatePet(
                    "pet-first",
                    100);

            var secondPet =
                CreatePet(
                    "pet-second",
                    200);

            var registry =
                new CombatPetRegistry(
                    new[]
                    {
                        firstPet,
                        secondPet
                    });

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Pets[0],
                Is.SameAs(firstPet));

            Assert.That(
                registry.Pets[1],
                Is.SameAs(secondPet));
        }

        [Test]
        public void Constructor_WithEmptyCollection_CreatesEmptyRegistry()
        {
            var registry =
                new CombatPetRegistry(
                    new CombatPetState[0]);

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.Pets,
                Is.Empty);
        }

        [Test]
        public void Constructor_WithNullCollection_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetRegistry(null));
        }

        [Test]
        public void Constructor_WithNullPet_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetRegistry(
                        new CombatPetState[]
                        {
                            CreatePet(
                                "pet-first",
                                100),
                            null
                        }));
        }

        [Test]
        public void Constructor_WithDuplicateInstanceId_Throws()
        {
            var firstPet =
                CreatePet(
                    "pet-first",
                    100);

            var secondPet =
                CreatePet(
                    "pet-second",
                    100);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetRegistry(
                        new[]
                        {
                            firstPet,
                            secondPet
                        }));
        }

        [Test]
        public void Constructor_WithDuplicateDefinitionId_AllowsDistinctInstances()
        {
            var firstPet =
                CreatePet(
                    "pet-shared",
                    100);

            var secondPet =
                CreatePet(
                    "pet-shared",
                    200);

            var registry =
                new CombatPetRegistry(
                    new[]
                    {
                        firstPet,
                        secondPet
                    });

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Pets[0].DefinitionId,
                Is.EqualTo(
                    registry.Pets[1].DefinitionId));

            Assert.That(
                registry.Pets[0].InstanceId,
                Is.Not.EqualTo(
                    registry.Pets[1].InstanceId));
        }

        [Test]
        public void GetPet_WithExistingInstanceId_ReturnsExactPet()
        {
            var firstPet =
                CreatePet(
                    "pet-first",
                    100);

            var secondPet =
                CreatePet(
                    "pet-second",
                    200);

            var registry =
                new CombatPetRegistry(
                    new[]
                    {
                        firstPet,
                        secondPet
                    });

            var result =
                registry.GetPet(
                    secondPet.InstanceId);

            Assert.That(
                result,
                Is.SameAs(secondPet));
        }

        [Test]
        public void GetPet_WithInvalidInstanceId_Throws()
        {
            var registry =
                new CombatPetRegistry(
                    new[]
                    {
                        CreatePet(
                            "pet-first",
                            100)
                    });

            Assert.Throws<ArgumentException>(
                () => registry.GetPet(
                    default(InstanceId)));
        }

        [Test]
        public void GetPet_WithMissingInstanceId_Throws()
        {
            var registry =
                new CombatPetRegistry(
                    new[]
                    {
                        CreatePet(
                            "pet-first",
                            100)
                    });

            Assert.Throws<KeyNotFoundException>(
                () => registry.GetPet(
                    new InstanceId(999)));
        }

        [Test]
        public void Pets_ReturnsReadOnlyCollection()
        {
            var registry =
                new CombatPetRegistry(
                    new[]
                    {
                        CreatePet(
                            "pet-first",
                            100)
                    });

            var mutablePets =
                registry.Pets as
                    IList<CombatPetState>;

            Assert.That(
                mutablePets,
                Is.Not.Null);

            Assert.Throws<NotSupportedException>(
                () => mutablePets.Add(
                    CreatePet(
                        "pet-second",
                        200)));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));
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