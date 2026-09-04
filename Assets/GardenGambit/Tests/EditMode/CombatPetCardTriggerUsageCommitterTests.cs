using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetCardTriggerUsageCommitterTests
    {
        [Test]
        public void Constructor_WithNullRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetCardTriggerUsageCommitter(
                        null));
        }

        [Test]
        public void Constructor_WithRegistry_ExposesExactRegistry()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            Assert.That(
                committer.UsageRegistry,
                Is.SameAs(registry));
        }

        [Test]
        public void HasTriggered_ReturnsRegistryState()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            Assert.That(
                committer.HasTriggered(key),
                Is.False);

            registry.TryRegister(
                key);

            Assert.That(
                committer.HasTriggered(key),
                Is.True);
        }

        [Test]
        public void TryCommit_WithUnusedKey_ResolvesAndRegisters()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var resolveCallCount = 0;

            var result =
                committer.TryCommit(
                    key,
                    () => resolveCallCount++);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                resolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                registry.Contains(key),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryCommit_WithUsedKey_SkipsCallback()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var resolveCallCount = 0;

            Assert.That(
                committer.TryCommit(
                    key,
                    () => resolveCallCount++),
                Is.True);

            Assert.That(
                committer.TryCommit(
                    key,
                    () => resolveCallCount++),
                Is.False);

            Assert.That(
                resolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryCommit_WhenCallbackThrows_DoesNotRegister()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            Assert.Throws<TestResolutionException>(
                () => committer.TryCommit(
                    key,
                    () => throw
                        new TestResolutionException()));

            Assert.That(
                registry.Contains(key),
                Is.False);

            Assert.That(
                registry.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void TryCommit_AfterCallbackFailure_CanRetry()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var resolveCallCount = 0;

            Assert.Throws<TestResolutionException>(
                () => committer.TryCommit(
                    key,
                    () =>
                    {
                        resolveCallCount++;

                        throw new
                            TestResolutionException();
                    }));

            var retryResult =
                committer.TryCommit(
                    key,
                    () => resolveCallCount++);

            Assert.That(
                retryResult,
                Is.True);

            Assert.That(
                resolveCallCount,
                Is.EqualTo(2));

            Assert.That(
                registry.Contains(key),
                Is.True);
        }

        [Test]
        public void TryCommit_WithIds_ResolvesAndRegisters()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var petInstanceId =
                new InstanceId(1);

            var cardInstanceId =
                new InstanceId(101);

            var resolveCallCount = 0;

            var result =
                committer.TryCommit(
                    petInstanceId,
                    cardInstanceId,
                    () => resolveCallCount++);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                resolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                committer.HasTriggered(
                    petInstanceId,
                    cardInstanceId),
                Is.True);
        }

        [Test]
        public void TryCommit_WithNullCallback_ThrowsWithoutMutation()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var key =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            Assert.Throws<ArgumentNullException>(
                () => committer.TryCommit(
                    key,
                    null));

            Assert.That(
                registry.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void TryCommit_WithInvalidKey_ThrowsWithoutCallback()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var resolveCallCount = 0;

            Assert.Throws<ArgumentException>(
                () => committer.TryCommit(
                    default(
                        CombatPetCardTriggerKey),
                    () => resolveCallCount++));

            Assert.That(
                resolveCallCount,
                Is.EqualTo(0));

            Assert.That(
                registry.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void TryCommit_WithDifferentPairs_ResolvesIndependently()
        {
            var registry =
                new CombatPetCardTriggerUsageRegistry();

            var committer =
                new CombatPetCardTriggerUsageCommitter(
                    registry);

            var firstKey =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 101);

            var secondKey =
                CreateKey(
                    petInstanceId: 1,
                    cardInstanceId: 102);

            var thirdKey =
                CreateKey(
                    petInstanceId: 2,
                    cardInstanceId: 101);

            var resolveCallCount = 0;

            Assert.That(
                committer.TryCommit(
                    firstKey,
                    () => resolveCallCount++),
                Is.True);

            Assert.That(
                committer.TryCommit(
                    secondKey,
                    () => resolveCallCount++),
                Is.True);

            Assert.That(
                committer.TryCommit(
                    thirdKey,
                    () => resolveCallCount++),
                Is.True);

            Assert.That(
                resolveCallCount,
                Is.EqualTo(3));

            Assert.That(
                registry.Count,
                Is.EqualTo(3));
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

        private sealed class
            TestResolutionException :
            Exception
        {
        }
    }
}