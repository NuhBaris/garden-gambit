using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetDefinitionIdsMuskCatTests
    {
        [Test]
        public void
            MuskCatValue_UsesStableTechnicalIdentifier()
        {
            Assert.That(
                CombatPetDefinitionIds
                    .MuskCatValue,
                Is.EqualTo(
                    "pet.musk_cat"));
        }

        [Test]
        public void
            MuskCat_ReturnsValidDefinitionId()
        {
            var definitionId =
                CombatPetDefinitionIds
                    .MuskCat;

            Assert.That(
                definitionId.IsValid,
                Is.True);

            Assert.That(
                definitionId,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.musk_cat")));
        }

        [Test]
        public void
            MuskCat_RepeatedAccessReturnsEqualIdentity()
        {
            var first =
                CombatPetDefinitionIds
                    .MuskCat;

            var second =
                CombatPetDefinitionIds
                    .MuskCat;

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