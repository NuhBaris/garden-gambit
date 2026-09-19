using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class RainSparrowPetTriggerSourceFactoryTests
    {
        [Test]
        public void Constructor_WithInvalidDefinitionId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ = new RainSparrowPetTriggerSourceFactory(
                    default(DefinitionId),
                    CreateUsageCommitter(),
                    CreateArmorGainResolver()));
        }

        [Test]
        public void Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new RainSparrowPetTriggerSourceFactory(
                    CreateDefinitionId(),
                    null,
                    CreateArmorGainResolver()));
        }

        [Test]
        public void Constructor_WithNullArmorGainResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new RainSparrowPetTriggerSourceFactory(
                    CreateDefinitionId(),
                    CreateUsageCommitter(),
                    null));
        }

        [Test]
        public void Constructor_ExposesExactRegistrationAndDependencies()
        {
            var definitionId = CreateDefinitionId();
            var usageCommitter = CreateUsageCommitter();
            var armorGainResolver = CreateArmorGainResolver();

            var factory = new RainSparrowPetTriggerSourceFactory(
                definitionId,
                usageCommitter,
                armorGainResolver);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(definitionId));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(usageCommitter));

            Assert.That(
                factory.ArmorGainResolver,
                Is.SameAs(armorGainResolver));
        }

        [Test]
        public void CreateSources_WithInvalidSide_Throws()
        {
            var factory = CreateFactory();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => factory.CreateSources(
                    (CombatSide)999,
                    CreatePet(1001)));
        }

        [Test]
        public void CreateSources_WithNullPet_Throws()
        {
            var factory = CreateFactory();

            Assert.Throws<ArgumentNullException>(
                () => factory.CreateSources(
                    CombatSide.Player,
                    null));
        }

        [Test]
        public void CreateSources_WithMismatchedDefinitionId_Throws()
        {
            var factory = CreateFactory();

            var otherPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                new InstanceId(1001));

            Assert.Throws<ArgumentException>(
                () => factory.CreateSources(
                    CombatSide.Player,
                    otherPet));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void CreateSources_WithMatchingPet_ReturnsConfiguredSource(
            CombatSide side)
        {
            var factory = CreateFactory();
            var pet = CreatePet(1001);

            var source = GetSingleSource(
                factory,
                side,
                pet);

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(pet.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(factory.UsageCommitter));
            Assert.That(
                source.ArmorGainResolver,
                Is.SameAs(factory.ArmorGainResolver));

            Assert.That(source.Handler.Side, Is.EqualTo(side));
            Assert.That(
                source.Handler.PetInstanceId,
                Is.EqualTo(pet.InstanceId));

            Assert.That(
                source.Handler.UsageCommitter,
                Is.SameAs(factory.UsageCommitter));
            Assert.That(
                source.Handler.ArmorGainResolver,
                Is.SameAs(factory.ArmorGainResolver));

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(side));
            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(pet.InstanceId));
        }

        [Test]
        public void
            CreateSources_WithDifferentPetInstances_PreservesEachIdentityAndSharedDependencies()
        {
            var factory = CreateFactory();
            var firstPet = CreatePet(1001);
            var secondPet = CreatePet(1002);

            var firstSource = GetSingleSource(
                factory,
                CombatSide.Player,
                firstPet);

            var secondSource = GetSingleSource(
                factory,
                CombatSide.Player,
                secondPet);

            Assert.That(
                secondSource,
                Is.Not.SameAs(firstSource));
            Assert.That(
                secondSource.Handler,
                Is.Not.SameAs(firstSource.Handler));

            Assert.That(
                firstSource.PetInstanceId,
                Is.EqualTo(firstPet.InstanceId));
            Assert.That(
                secondSource.PetInstanceId,
                Is.EqualTo(secondPet.InstanceId));

            Assert.That(
                firstSource.Handler.PetInstanceId,
                Is.EqualTo(firstPet.InstanceId));
            Assert.That(
                secondSource.Handler.PetInstanceId,
                Is.EqualTo(secondPet.InstanceId));

            Assert.That(
                firstSource.UsageCommitter,
                Is.SameAs(factory.UsageCommitter));
            Assert.That(
                secondSource.UsageCommitter,
                Is.SameAs(factory.UsageCommitter));

            Assert.That(
                firstSource.ArmorGainResolver,
                Is.SameAs(factory.ArmorGainResolver));
            Assert.That(
                secondSource.ArmorGainResolver,
                Is.SameAs(factory.ArmorGainResolver));
        }

        private static RainSparrowPetTriggerSource GetSingleSource(
            RainSparrowPetTriggerSourceFactory factory,
            CombatSide side,
            CombatPetState pet)
        {
            var sources = new List<ICombatTriggerSource>(
                factory.CreateSources(side, pet));

            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(
                sources[0],
                Is.TypeOf<RainSparrowPetTriggerSource>());

            return (RainSparrowPetTriggerSource)sources[0];
        }

        private static RainSparrowPetTriggerSourceFactory CreateFactory()
        {
            return new RainSparrowPetTriggerSourceFactory(
                CreateDefinitionId(),
                CreateUsageCommitter(),
                CreateArmorGainResolver());
        }

        private static CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new CombatPetCardTriggerUsageCommitter(
                new CombatPetCardTriggerUsageRegistry());
        }

        private static CombatArmorGainResolver CreateArmorGainResolver()
        {
            return new CombatArmorGainResolver(
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator()),
                new CombatEventLog());
        }

        private static DefinitionId CreateDefinitionId()
        {
            return CombatPetDefinitionIds.RainSparrow;
        }

        private static CombatPetState CreatePet(long instanceId)
        {
            return new CombatPetState(
                CreateDefinitionId(),
                new InstanceId(instanceId));
        }
    }
}