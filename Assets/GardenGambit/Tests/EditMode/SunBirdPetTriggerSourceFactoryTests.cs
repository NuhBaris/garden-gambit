using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        SunBirdPetTriggerSourceFactoryTests
    {
        [Test]
        public void
            Constructor_WithInvalidDefinitionId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        SunBirdPetTriggerSourceFactory(
                            default(DefinitionId),
                            CreateUsageCommitter(),
                            new
                                CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [Test]
        public void
            Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        SunBirdPetTriggerSourceFactory(
                            CreateSunBirdDefinitionId(),
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
                    new
                        SunBirdPetTriggerSourceFactory(
                            CreateSunBirdDefinitionId(),
                            CreateUsageCommitter(),
                            null));
        }

        [Test]
        public void
            Constructor_ExposesExactRegistrationAndDependencies()
        {
            var definitionId =
                CreateSunBirdDefinitionId();

            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var factory =
                new
                    SunBirdPetTriggerSourceFactory(
                        definitionId,
                        usageCommitter,
                        modifierRegistry);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    definitionId));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                factory.SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));
        }

        [Test]
        public void
            CreateSources_WithInvalidSide_Throws()
        {
            var factory =
                CreateFactory();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => factory.CreateSources(
                    default(CombatSide),
                    CreateSunBirdPet(
                        1001)));
        }

        [Test]
        public void
            CreateSources_WithNullPet_Throws()
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
            CreateSources_WithPlayerSunBird_ReturnsConfiguredSource()
        {
            var factory =
                CreateFactory();

            var pet =
                CreateSunBirdPet(
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
                    as SunBirdPetTriggerSource;

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
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    factory
                        .SourceDamageModifierRegistry));
        }

        [Test]
        public void
            CreateSources_WithEnemySunBird_ReturnsEnemySource()
        {
            var factory =
                CreateFactory();

            var pet =
                CreateSunBirdPet(
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
                    as SunBirdPetTriggerSource;

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
            SunBirdPetTriggerSourceFactory
            CreateFactory()
        {
            return new
                SunBirdPetTriggerSourceFactory(
                    CreateSunBirdDefinitionId(),
                    CreateUsageCommitter(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry());
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
            CreateSunBirdDefinitionId()
        {
            return new DefinitionId(
                "pet.sun_bird");
        }

        private static CombatPetState
            CreateSunBirdPet(
                long instanceId)
        {
            return new CombatPetState(
                CreateSunBirdDefinitionId(),
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