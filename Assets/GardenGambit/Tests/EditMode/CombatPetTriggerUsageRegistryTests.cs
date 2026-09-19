using System;
using System.Collections.Generic;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerUsageRegistryTests
    {
        [Test]
        public void TryRegister_DeduplicatesAndPreservesSuccessfulRegistrationOrder()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var first = new InstanceId(1002);
            var second = new InstanceId(1001);

            Assert.That(registry.Contains(first), Is.False);
            Assert.That(registry.TryRegister(first), Is.True);
            Assert.That(registry.TryRegister(first), Is.False);
            Assert.That(registry.TryRegister(second), Is.True);
            Assert.That(registry.TryRegister(first), Is.False);

            Assert.That(registry.Contains(first), Is.True);
            Assert.That(registry.Contains(second), Is.True);
            Assert.That(registry.Contains(new InstanceId(9999)), Is.False);
            Assert.That(registry.Count, Is.EqualTo(2));
            Assert.That(registry.PetInstanceIds, Is.EqualTo(new[] { first, second }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InvalidInstanceId_ThrowsWithoutChangingExistingRegistrations(bool register)
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var validId = new InstanceId(1001);
            registry.TryRegister(validId);

            if (register)
            {
                Assert.Throws<ArgumentException>(
                    () => registry.TryRegister(default(InstanceId)));
            }
            else
            {
                Assert.Throws<ArgumentException>(
                    () => registry.Contains(default(InstanceId)));
            }

            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(registry.PetInstanceIds, Is.EqualTo(new[] { validId }));
        }

        [Test]
        public void PetInstanceIds_ExposesLiveReadOnlyRegistrationOrder()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var view = registry.PetInstanceIds;
            var petId = new InstanceId(1001);
            registry.TryRegister(petId);

            Assert.That(view, Is.EqualTo(new[] { petId }));
            Assert.Throws<NotSupportedException>(
                () => ((ICollection<InstanceId>)view).Clear());
            Assert.That(registry.Contains(petId), Is.True);
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void SeparateRegistries_DoNotShareUsageForSameInstanceId()
        {
            var first = new CombatPetTriggerUsageRegistry();
            var second = new CombatPetTriggerUsageRegistry();
            var petId = new InstanceId(1001);

            first.TryRegister(petId);
            Assert.That(second.Count, Is.Zero);
            Assert.That(second.Contains(petId), Is.False);
            Assert.That(second.TryRegister(petId), Is.True);
            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(second.Count, Is.EqualTo(1));
        }
    }
}
