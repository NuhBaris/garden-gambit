using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetDefinitionIdsTests
    {
        [Test]
        public void
            SunBirdValue_UsesStableTechnicalIdentifier()
        {
            Assert.That(
                CombatPetDefinitionIds
                    .SunBirdValue,
                Is.EqualTo(
                    "pet.sun_bird"));
        }

        [Test]
        public void SunBird_ReturnsValidDefinitionId()
        {
            var definitionId =
                CombatPetDefinitionIds
                    .SunBird;

            Assert.That(
                definitionId.IsValid,
                Is.True);

            Assert.That(
                definitionId,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.sun_bird")));
        }

        [Test]
        public void
            SunBird_RepeatedAccessReturnsEqualIdentity()
        {
            var first =
                CombatPetDefinitionIds
                    .SunBird;

            var second =
                CombatPetDefinitionIds
                    .SunBird;

            Assert.That(
                first,
                Is.EqualTo(
                    second));

            Assert.That(
                first.GetHashCode(),
                Is.EqualTo(
                    second.GetHashCode()));
        }

        [Test]
        public void
            PolarFerretValue_UsesStableTechnicalIdentifier()
        {
            Assert.That(
                CombatPetDefinitionIds
                    .PolarFerretValue,
                Is.EqualTo(
                    "pet.polar_ferret"));
        }

        [Test]
        public void PolarFerret_ReturnsValidDefinitionId()
        {
            var definitionId =
                CombatPetDefinitionIds
                    .PolarFerret;

            Assert.That(
                definitionId.IsValid,
                Is.True);

            Assert.That(
                definitionId,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.polar_ferret")));
        }

        [Test]
        public void
            PolarFerret_RepeatedAccessReturnsEqualIdentity()
        {
            var first =
                CombatPetDefinitionIds
                    .PolarFerret;

            var second =
                CombatPetDefinitionIds
                    .PolarFerret;

            Assert.That(
                first,
                Is.EqualTo(
                    second));

            Assert.That(
                first.GetHashCode(),
                Is.EqualTo(
                    second.GetHashCode()));
        }
    }
}