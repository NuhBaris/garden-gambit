using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        MuskCatPetTriggerSourceFactoryTests
    {
        [Test]
        public void
            Constructor_WithInvalidDefinitionId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        MuskCatPetTriggerSourceFactory(
                            default(DefinitionId),
                            CreateUsageCommitter(),
                            new
                                CombatFinalRankModifierRegistry()));
        }

        [Test]
        public void
            Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        MuskCatPetTriggerSourceFactory(
                            CombatPetDefinitionIds
                                .MuskCat,
                            null,
                            new
                                CombatFinalRankModifierRegistry()));
        }

        [Test]
        public void
            Constructor_WithNullFinalRankRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        MuskCatPetTriggerSourceFactory(
                            CombatPetDefinitionIds
                                .MuskCat,
                            CreateUsageCommitter(),
                            null));
        }

        [Test]
        public void
            Constructor_ExposesExactRegistrationAndDependencies()
        {
            var definitionId =
                CombatPetDefinitionIds
                    .MuskCat;

            var usageCommitter =
                CreateUsageCommitter();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var factory =
                new
                    MuskCatPetTriggerSourceFactory(
                        definitionId,
                        usageCommitter,
                        finalRankRegistry);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    definitionId));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                factory.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankRegistry));
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
                    CreateMuskCat(
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
            CreateSources_WithPlayerMuskCat_ReturnsConfiguredSource()
        {
            var factory =
                CreateFactory();

            var pet =
                CreateMuskCat(
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
                    as MuskCatPetTriggerSource;

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
                source.FinalRankModifierRegistry,
                Is.SameAs(
                    factory
                        .FinalRankModifierRegistry));
        }

        [Test]
        public void
            CreateSources_WithEnemyMuskCat_ReturnsEnemySource()
        {
            var factory =
                CreateFactory();

            var pet =
                CreateMuskCat(
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
                    as MuskCatPetTriggerSource;

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

            Assert.That(
                source.FinalRankModifierRegistry,
                Is.SameAs(
                    factory
                        .FinalRankModifierRegistry));
        }

        private static
            MuskCatPetTriggerSourceFactory
            CreateFactory()
        {
            return new
                MuskCatPetTriggerSourceFactory(
                    CombatPetDefinitionIds
                        .MuskCat,
                    CreateUsageCommitter(),
                    new
                        CombatFinalRankModifierRegistry());
        }

        private static CombatPetState
            CreateMuskCat(
                long instanceId)
        {
            return new CombatPetState(
                CombatPetDefinitionIds
                    .MuskCat,
                new InstanceId(
                    instanceId));
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