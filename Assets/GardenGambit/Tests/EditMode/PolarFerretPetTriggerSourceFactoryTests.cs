using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        PolarFerretPetTriggerSourceFactoryTests
    {
        [Test]
        public void Constructor_WithInvalidDefinitionId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        PolarFerretPetTriggerSourceFactory(
                            default(DefinitionId),
                            CreateUsageCommitter(),
                            new
                                CombatNormalAttackTargetDamageReductionRegistry()));
        }

        [Test]
        public void Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        PolarFerretPetTriggerSourceFactory(
                            CreatePolarFerretDefinitionId(),
                            null,
                            new
                                CombatNormalAttackTargetDamageReductionRegistry()));
        }

        [Test]
        public void Constructor_WithNullReductionRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        PolarFerretPetTriggerSourceFactory(
                            CreatePolarFerretDefinitionId(),
                            CreateUsageCommitter(),
                            null));
        }

        [Test]
        public void
            Constructor_ExposesExactRegistrationAndDependencies()
        {
            var definitionId =
                CreatePolarFerretDefinitionId();

            var usageCommitter =
                CreateUsageCommitter();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var factory =
                new
                    PolarFerretPetTriggerSourceFactory(
                        definitionId,
                        usageCommitter,
                        reductionRegistry);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    definitionId));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                factory.TargetDamageReductionRegistry,
                Is.SameAs(
                    reductionRegistry));
        }

        [Test]
        public void CreateSources_WithInvalidSide_Throws()
        {
            var factory =
                CreateFactory();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                    () => factory.CreateSources(
                        default(CombatSide),
                        CreatePolarFerretPet(
                            1001)));
        }

        [Test]
        public void CreateSources_WithNullPet_Throws()
        {
            var factory =
                CreateFactory();

            Assert.Throws<ArgumentNullException>(
                () => factory.CreateSources(
                    CombatSide.Player,
                    null));
        }

        [Test]
        public void
            CreateSources_WithMismatchedDefinitionId_Throws()
        {
            var factory =
                CreateFactory();

            var otherPet =
                new CombatPetState(
                    new DefinitionId(
                        "pet.other"),
                    new InstanceId(1001));

            Assert.Throws<ArgumentException>(
                () => factory.CreateSources(
                    CombatSide.Player,
                    otherPet));
        }

        [Test]
        public void
            CreateSources_WithPlayerPolarFerret_ReturnsConfiguredSource()
        {
            var factory =
                CreateFactory();

            var pet =
                CreatePolarFerretPet(
                    1001);

            var sources =
                ToList(
                    factory.CreateSources(
                        CombatSide.Player,
                        pet));

            Assert.That(
                sources.Count,
                Is.EqualTo(1));

            var source =
                sources[0]
                    as PolarFerretPetTriggerSource;

            Assert.That(
                source,
                Is.Not.Null);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    pet.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    factory.UsageCommitter));

            Assert.That(
                source
                    .TargetDamageReductionRegistry,
                Is.SameAs(
                    factory
                        .TargetDamageReductionRegistry));
        }

        [Test]
        public void
            CreateSources_WithEnemyPolarFerret_ReturnsEnemySource()
        {
            var factory =
                CreateFactory();

            var pet =
                CreatePolarFerretPet(
                    2001);

            var sources =
                ToList(
                    factory.CreateSources(
                        CombatSide.Enemy,
                        pet));

            Assert.That(
                sources.Count,
                Is.EqualTo(1));

            var source =
                sources[0]
                    as PolarFerretPetTriggerSource;

            Assert.That(
                source,
                Is.Not.Null);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    pet.InstanceId));

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(
                    CombatSide.Enemy));
        }

        private static
            PolarFerretPetTriggerSourceFactory
            CreateFactory()
        {
            return new
                PolarFerretPetTriggerSourceFactory(
                    CreatePolarFerretDefinitionId(),
                    CreateUsageCommitter(),
                    new
                        CombatNormalAttackTargetDamageReductionRegistry());
        }

        private static CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new
                CombatPetCardTriggerUsageCommitter(
                    new
                        CombatPetCardTriggerUsageRegistry());
        }

        private static DefinitionId
            CreatePolarFerretDefinitionId()
        {
            return new DefinitionId(
                "pet.polar_ferret");
        }

        private static CombatPetState
            CreatePolarFerretPet(
                long instanceId)
        {
            return new CombatPetState(
                CreatePolarFerretDefinitionId(),
                new InstanceId(
                    instanceId));
        }

        private static List<ICombatTriggerSource>
            ToList(
                IEnumerable<ICombatTriggerSource>
                    sources)
        {
            return new List<ICombatTriggerSource>(
                sources);
        }
    }
}