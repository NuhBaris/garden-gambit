using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetStateTests
    {
        [Test]
        public void Constructor_WithValidIdentities_SetsValues()
        {
            var definitionId =
                new DefinitionId(
                    "pet-sunflower");

            var instanceId =
                new InstanceId(100);

            var pet =
                new CombatPetState(
                    definitionId,
                    instanceId);

            Assert.That(
                pet.DefinitionId,
                Is.EqualTo(definitionId));

            Assert.That(
                pet.InstanceId,
                Is.EqualTo(instanceId));
        }

        [Test]
        public void Constructor_WithInvalidDefinitionId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetState(
                        default(DefinitionId),
                        new InstanceId(100)));
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetState(
                        new DefinitionId(
                            "pet-sunflower"),
                        default(InstanceId)));
        }
    }
}