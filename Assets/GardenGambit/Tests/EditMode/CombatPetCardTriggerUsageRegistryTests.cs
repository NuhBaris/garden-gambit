using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetCardTriggerUsageRegistryTests
    {
        [Test]
        public void Constructor_CreatesEmptyRegistry()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(0));

            Assert.That(
                registry.Keys,
                Is.Empty);
        }

        [Test]
        public void TryRegister_WithNewKey_ReturnsTrue()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var result =
                registry.TryRegister(
                    key);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Keys[0],
                Is.EqualTo(key));
        }

        [Test]
        public void TryRegister_WithExistingKey_ReturnsFalse()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            Assert.That(
                registry.TryRegister(key),
                Is.True);

            Assert.That(
                registry.TryRegister(key),
                Is.False);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Keys[0],
                Is.EqualTo(key));
        }

        [Test]
        public void TryRegister_SamePetDifferentCards_RegistersBoth()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var firstKey =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var secondKey =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 102);

            Assert.That(
                registry.TryRegister(firstKey),
                Is.True);

            Assert.That(
                registry.TryRegister(secondKey),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void TryRegister_DifferentPetsSameCard_RegistersBoth()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var firstKey =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var secondKey =
                CreateKey(
                    petInstanceId: 2,
                    cardInstanceId: 101);

            Assert.That(
                registry.TryRegister(firstKey),
                Is.True);

            Assert.That(
                registry.TryRegister(secondKey),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Contains_WithKey_ReturnsRegistrationState()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            Assert.That(
                registry.Contains(key),
                Is.False);

            registry.TryRegister(
                key);

            Assert.That(
                registry.Contains(key),
                Is.True);
        }

        [Test]
        public void Contains_WithIds_ReturnsRegistrationState()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var petInstanceId =
                new InstanceId(1);

            var cardInstanceId =
                new InstanceId(101);

            Assert.That(
                registry.Contains(
                    petInstanceId,
                    cardInstanceId),
                Is.False);

            registry.TryRegister(
                petInstanceId,
                cardInstanceId);

            Assert.That(
                registry.Contains(
                    petInstanceId,
                    cardInstanceId),
                Is.True);
        }

        [Test]
        public void TryRegister_WithIds_AddsKey()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var petInstanceId =
                new InstanceId(1);

            var cardInstanceId =
                new InstanceId(101);

            var result =
                registry.TryRegister(
                    petInstanceId,
                    cardInstanceId);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Keys[0],
                Is.EqualTo(
                    new CombatPetCardTriggerKey(
                        petInstanceId,
                        cardInstanceId)));
        }

        [Test]
        public void Keys_PreserveSuccessfulRegistrationOrder()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var first =
                CreateKey(
                    petInstanceId: 2,
                    cardInstanceId: 103);

            var second =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var third =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 102);

            registry.TryRegister(
                first);

            registry.TryRegister(
                second);

            registry.TryRegister(
                first);

            registry.TryRegister(
                third);

            Assert.That(
                registry.Count,
                Is.EqualTo(3));

            Assert.That(
                registry.Keys[0],
                Is.EqualTo(first));

            Assert.That(
                registry.Keys[1],
                Is.EqualTo(second));

            Assert.That(
                registry.Keys[2],
                Is.EqualTo(third));
        }

        [Test]
        public void Contains_WithInvalidKey_Throws()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.Contains(
                    default(
                        CombatPetCardTriggerKey)));

            Assert.That(
                registry.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void TryRegister_WithInvalidKey_ThrowsWithoutMutation()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.TryRegister(
                    default(
                        CombatPetCardTriggerKey)));

            Assert.That(
                registry.Count,
                Is.EqualTo(0));

            Assert.That(
                registry.Keys,
                Is.Empty);
        }

        [Test]
        public void IdOverloads_WithInvalidIds_ThrowWithoutMutation()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var validPetInstanceId =
                new InstanceId(1);

            var validCardInstanceId =
                new InstanceId(101);

            Assert.Throws<ArgumentException>(
                () => registry.Contains(
                    default(InstanceId),
                    validCardInstanceId));

            Assert.Throws<ArgumentException>(
                () => registry.Contains(
                    validPetInstanceId,
                    default(InstanceId)));

            Assert.Throws<ArgumentException>(
                () => registry.TryRegister(
                    default(InstanceId),
                    validCardInstanceId));

            Assert.Throws<ArgumentException>(
                () => registry.TryRegister(
                    validPetInstanceId,
                    default(InstanceId)));

            Assert.That(
                registry.Count,
                Is.EqualTo(0));
        }

        private static CombatPetCardTriggerKey
            CreateKey(
                long petInstanceId,
                long cardInstanceId)
        {
            return new CombatPetCardTriggerKey(
                new InstanceId(
                    petInstanceId),
                new InstanceId(
                    cardInstanceId));
        }
    }
}