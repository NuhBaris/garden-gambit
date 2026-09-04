using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceFactoryRegistryTests
    {
        [Test]
        public void Constructor_WithNullFactories_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryRegistry(
                            null));
        }

        [Test]
        public void Constructor_WithNullFactory_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryRegistry(
                            new
                                ICombatPetTriggerSourceFactory[]
                            {
                                CreateFactory(
                                    "pet.first"),
                                null
                            }));
        }

        [Test]
        public void
            Constructor_WithInvalidFactoryDefinitionId_Throws()
        {
            var invalidFactory =
                new TestFactory(
                    default(DefinitionId));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryRegistry(
                            new[]
                            {
                                invalidFactory
                            }));
        }

        [Test]
        public void
            Constructor_WithDuplicateDefinitionId_Throws()
        {
            var firstFactory =
                CreateFactory(
                    "pet.sun_bird");

            var duplicateFactory =
                CreateFactory(
                    "pet.sun_bird");

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryRegistry(
                            new[]
                            {
                                firstFactory,
                                duplicateFactory
                            }));
        }

        [Test]
        public void
            Constructor_WithEmptyFactories_CreatesEmptyRegistry()
        {
            var registry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        Array.Empty<
                            ICombatPetTriggerSourceFactory>());

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.Factories,
                Is.Empty);
        }

        [Test]
        public void
            Constructor_PreservesFactoryRegistrationOrder()
        {
            var firstFactory =
                CreateFactory(
                    "pet.first");

            var secondFactory =
                CreateFactory(
                    "pet.second");

            var thirdFactory =
                CreateFactory(
                    "pet.third");

            var registry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        new[]
                        {
                            firstFactory,
                            secondFactory,
                            thirdFactory
                        });

            Assert.That(
                registry.Count,
                Is.EqualTo(3));

            Assert.That(
                registry.Factories[0],
                Is.SameAs(
                    firstFactory));

            Assert.That(
                registry.Factories[1],
                Is.SameAs(
                    secondFactory));

            Assert.That(
                registry.Factories[2],
                Is.SameAs(
                    thirdFactory));
        }

        [Test]
        public void
            ContainsAndGetFactory_WithRegisteredId_ReturnExactFactory()
        {
            var factory =
                CreateFactory(
                    "pet.sun_bird");

            var registry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        new[]
                        {
                            factory
                        });

            var definitionId =
                new DefinitionId(
                    "pet.sun_bird");

            Assert.That(
                registry.Contains(
                    definitionId),
                Is.True);

            Assert.That(
                registry.GetFactory(
                    definitionId),
                Is.SameAs(
                    factory));
        }

        [Test]
        public void
            GetFactory_WithUnregisteredId_Throws()
        {
            var registry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        new[]
                        {
                            CreateFactory(
                                "pet.sun_bird")
                        });

            Assert.Throws<KeyNotFoundException>(
                () => registry.GetFactory(
                    new DefinitionId(
                        "pet.other")));
        }

        [Test]
        public void
            TryGetFactory_WithRegisteredId_ReturnsTrueAndFactory()
        {
            var factory =
                CreateFactory(
                    "pet.sun_bird");

            var registry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        new[]
                        {
                            factory
                        });

            ICombatPetTriggerSourceFactory
                result;

            var wasFound =
                registry.TryGetFactory(
                    new DefinitionId(
                        "pet.sun_bird"),
                    out result);

            Assert.That(
                wasFound,
                Is.True);

            Assert.That(
                result,
                Is.SameAs(
                    factory));
        }

        [Test]
        public void
            TryGetFactory_WithUnregisteredId_ReturnsFalseAndNull()
        {
            var registry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        new[]
                        {
                            CreateFactory(
                                "pet.sun_bird")
                        });

            ICombatPetTriggerSourceFactory
                result;

            var wasFound =
                registry.TryGetFactory(
                    new DefinitionId(
                        "pet.other"),
                    out result);

            Assert.That(
                wasFound,
                Is.False);

            Assert.That(
                result,
                Is.Null);
        }

        private static TestFactory CreateFactory(
            string definitionId)
        {
            return new TestFactory(
                new DefinitionId(
                    definitionId));
        }

        private sealed class TestFactory :
            ICombatPetTriggerSourceFactory
        {
            public TestFactory(
                DefinitionId petDefinitionId)
            {
                PetDefinitionId =
                    petDefinitionId;
            }

            public DefinitionId PetDefinitionId
            {
                get;
            }

            public IEnumerable<ICombatTriggerSource>
                CreateSources(
                    CombatSide side,
                    CombatPetState pet)
            {
                return Array.Empty<
                    ICombatTriggerSource>();
            }
        }
    }
}