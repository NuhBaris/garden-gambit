using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetLimitedTriggerUsageTests
    {
        [Test]
        public void RegistryStartsEmpty()
        {
            var registry =
                new CombatPetLimitedTriggerUsageRegistry();

            Assert.That(registry.Count, Is.Zero);
            Assert.That(registry.TotalUsageCount, Is.Zero);
            Assert.That(registry.PetInstanceIds, Is.Empty);
            Assert.That(
                registry.GetUsageCount(new InstanceId(1)),
                Is.Zero);
        }

        [Test]
        public void RegistryValidatesPetAndLimit()
        {
            var registry =
                new CombatPetLimitedTriggerUsageRegistry();

            Assert.Throws<ArgumentException>(() =>
                registry.GetUsageCount(
                    default(InstanceId)));
            Assert.Throws<ArgumentException>(() =>
                registry.TryRegisterUse(
                    default(InstanceId),
                    2));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                registry.HasReachedLimit(
                    new InstanceId(1),
                    0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                registry.TryRegisterUse(
                    new InstanceId(1),
                    -1));
        }

        [Test]
        public void RegistryRegistersUpToLimit()
        {
            var registry =
                new CombatPetLimitedTriggerUsageRegistry();
            var pet = new InstanceId(10);

            Assert.That(
                registry.TryRegisterUse(pet, 2),
                Is.True);
            Assert.That(
                registry.TryRegisterUse(pet, 2),
                Is.True);
            Assert.That(
                registry.TryRegisterUse(pet, 2),
                Is.False);
            Assert.That(
                registry.GetUsageCount(pet),
                Is.EqualTo(2));
            Assert.That(
                registry.HasReachedLimit(pet, 2),
                Is.True);
            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(
                registry.TotalUsageCount,
                Is.EqualTo(2));
            Assert.That(
                registry.PetInstanceIds[0],
                Is.EqualTo(pet));
        }

        [Test]
        public void RegistryTracksPetsIndependently()
        {
            var registry =
                new CombatPetLimitedTriggerUsageRegistry();
            var first = new InstanceId(10);
            var second = new InstanceId(20);

            registry.TryRegisterUse(first, 2);
            registry.TryRegisterUse(first, 2);
            registry.TryRegisterUse(second, 2);

            Assert.That(
                registry.HasReachedLimit(first, 2),
                Is.True);
            Assert.That(
                registry.HasReachedLimit(second, 2),
                Is.False);
            Assert.That(registry.Count, Is.EqualTo(2));
            Assert.That(
                registry.TotalUsageCount,
                Is.EqualTo(3));
        }

        [Test]
        public void CommitterValidatesDependencyAndAction()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CombatPetLimitedTriggerUsageCommitter(
                    null));

            var committer =
                new CombatPetLimitedTriggerUsageCommitter(
                    new
                        CombatPetLimitedTriggerUsageRegistry());

            Assert.Throws<ArgumentNullException>(() =>
                committer.TryCommit(
                    new InstanceId(1),
                    2,
                    null));
        }

        [Test]
        public void CommitterRunsOnlyAllowedUses()
        {
            var registry =
                new CombatPetLimitedTriggerUsageRegistry();
            var committer =
                new CombatPetLimitedTriggerUsageCommitter(
                    registry);
            var pet = new InstanceId(10);
            var resolutionCount = 0;

            Assert.That(
                committer.TryCommit(
                    pet,
                    2,
                    () => resolutionCount++),
                Is.True);
            Assert.That(
                committer.TryCommit(
                    pet,
                    2,
                    () => resolutionCount++),
                Is.True);
            Assert.That(
                committer.TryCommit(
                    pet,
                    2,
                    () => resolutionCount++),
                Is.False);

            Assert.That(resolutionCount, Is.EqualTo(2));
            Assert.That(
                committer.GetUsageCount(pet),
                Is.EqualTo(2));
            Assert.That(
                committer.HasReachedLimit(pet, 2),
                Is.True);
        }

        [Test]
        public void ThrowingResolutionDoesNotConsumeUse()
        {
            var registry =
                new CombatPetLimitedTriggerUsageRegistry();
            var committer =
                new CombatPetLimitedTriggerUsageCommitter(
                    registry);
            var pet = new InstanceId(10);

            Assert.Throws<InvalidOperationException>(() =>
                committer.TryCommit(
                    pet,
                    2,
                    () =>
                    {
                        throw new InvalidOperationException();
                    }));

            Assert.That(
                registry.GetUsageCount(pet),
                Is.Zero);
        }

        [Test]
        public void LegacyRequestRemainsCardScoped()
        {
            var request = new
                CombatNormalAttackTargetDamageReductionRequest(
                    new CombatEventId(1),
                    new InstanceId(10),
                    new InstanceId(20),
                    1);

            Assert.That(request.UsesCardScopedUsage, Is.True);
            Assert.That(request.UsesLimitedPetUsage, Is.False);
            Assert.That(
                request.MaximumPetUsageCount,
                Is.Zero);
            Assert.That(
                request.UsageKey,
                Is.EqualTo(
                    new CombatPetCardTriggerKey(
                        new InstanceId(10),
                        new InstanceId(20))));
        }

        [Test]
        public void LimitedRequestPreservesMaximumUsageCount()
        {
            var request = new
                CombatNormalAttackTargetDamageReductionRequest(
                    new CombatEventId(1),
                    new InstanceId(10),
                    new InstanceId(20),
                    1,
                    maximumPetUsageCount: 2);

            Assert.That(request.UsesLimitedPetUsage, Is.True);
            Assert.That(request.UsesCardScopedUsage, Is.False);
            Assert.That(
                request.MaximumPetUsageCount,
                Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void LimitedRequestRejectsInvalidMaximum(
            int maximumUsageCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        new CombatEventId(1),
                        new InstanceId(10),
                        new InstanceId(20),
                        1,
                        maximumUsageCount));
        }

        [Test]
        public void ResolverConstructorPreservesLimitedCommitter()
        {
            var environment = new Environment();

            Assert.That(
                environment.Resolver.LimitedUsageCommitter,
                Is.SameAs(environment.LimitedCommitter));

            Assert.Throws<ArgumentNullException>(() =>
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        environment.ReductionRegistry,
                        environment.CardCommitter,
                        null));
        }

        [Test]
        public void LegacyResolverConstructorCreatesIndependentLimitedUsage()
        {
            var reductionRegistry = new
                CombatNormalAttackTargetDamageReductionRegistry();
            var cardCommitter = new
                CombatPetCardTriggerUsageCommitter(
                    new CombatPetCardTriggerUsageRegistry());
            var resolver = new
                CombatNormalAttackTargetDamageReductionResolver(
                    reductionRegistry,
                    cardCommitter);

            Assert.That(
                resolver.LimitedUsageCommitter,
                Is.Not.Null);
            Assert.That(
                resolver.LimitedUsageCommitter.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void LimitedReductionAppliesOnlyFirstTwoEventsForSameTarget()
        {
            var environment = new Environment();
            var pet = new InstanceId(10);

            var first = environment.CreateAttack(1, 201);
            var second = environment.CreateAttack(2, 201);
            var third = environment.CreateAttack(3, 201);

            environment.RegisterLimited(first, pet, 2);
            environment.RegisterLimited(second, pet, 2);
            environment.RegisterLimited(third, pet, 2);

            Assert.That(
                environment.Resolver.ResolveDamage(first, 5),
                Is.EqualTo(4));
            Assert.That(
                environment.Resolver.ResolveDamage(second, 5),
                Is.EqualTo(4));
            Assert.That(
                environment.Resolver.ResolveDamage(third, 5),
                Is.EqualTo(5));
            Assert.That(
                environment.LimitedRegistry
                    .GetUsageCount(pet),
                Is.EqualTo(2));
            Assert.That(
                environment.CardRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void LimitedReductionIsSharedAcrossDifferentTargets()
        {
            var environment = new Environment();
            var pet = new InstanceId(10);

            var first = environment.CreateAttack(1, 201);
            var second = environment.CreateAttack(2, 202);
            var third = environment.CreateAttack(3, 203);

            environment.RegisterLimited(first, pet, 2);
            environment.RegisterLimited(second, pet, 2);
            environment.RegisterLimited(third, pet, 2);

            Assert.That(
                environment.Resolver.ResolveDamage(first, 3),
                Is.EqualTo(2));
            Assert.That(
                environment.Resolver.ResolveDamage(second, 3),
                Is.EqualTo(2));
            Assert.That(
                environment.Resolver.ResolveDamage(third, 3),
                Is.EqualTo(3));
            Assert.That(
                environment.LimitedRegistry.TotalUsageCount,
                Is.EqualTo(2));
        }

        [Test]
        public void LimitedReductionDoesNotConsumeOnZeroDamage()
        {
            var environment = new Environment();
            var pet = new InstanceId(10);
            var attack = environment.CreateAttack(1, 201);

            environment.RegisterLimited(attack, pet, 2);

            Assert.That(
                environment.Resolver.ResolveDamage(attack, 0),
                Is.Zero);
            Assert.That(
                environment.LimitedRegistry
                    .GetUsageCount(pet),
                Is.Zero);
            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void IndependentPetsKeepIndependentLimits()
        {
            var environment = new Environment();
            var attack = environment.CreateAttack(1, 201);
            var firstPet = new InstanceId(10);
            var secondPet = new InstanceId(11);

            environment.RegisterLimited(attack, firstPet, 2);
            environment.RegisterLimited(attack, secondPet, 2);

            Assert.That(
                environment.Resolver.ResolveDamage(attack, 5),
                Is.EqualTo(3));
            Assert.That(
                environment.LimitedRegistry
                    .GetUsageCount(firstPet),
                Is.EqualTo(1));
            Assert.That(
                environment.LimitedRegistry
                    .GetUsageCount(secondPet),
                Is.EqualTo(1));
        }

        [Test]
        public void CardScopedAndLimitedRequestsCoexist()
        {
            var environment = new Environment();
            var attack = environment.CreateAttack(1, 201);
            var limitedPet = new InstanceId(10);
            var cardScopedPet = new InstanceId(11);

            environment.RegisterLimited(
                attack,
                limitedPet,
                2);
            environment.RegisterCardScoped(
                attack,
                cardScopedPet);

            Assert.That(
                environment.Resolver.ResolveDamage(attack, 5),
                Is.EqualTo(3));
            Assert.That(
                environment.LimitedRegistry
                    .GetUsageCount(limitedPet),
                Is.EqualTo(1));
            Assert.That(
                environment.CardCommitter.HasTriggered(
                    cardScopedPet,
                    attack.TargetInstanceId),
                Is.True);
        }

        [Test]
        public void RuntimeExposesSharedLimitedUsageDependencies()
        {
            var runtime = new CombatPetTriggerRuntime();

            Assert.That(runtime.LimitedUsageRegistry, Is.Not.Null);
            Assert.That(runtime.LimitedUsageCommitter, Is.Not.Null);
            Assert.That(
                runtime.LimitedUsageCommitter.UsageRegistry,
                Is.SameAs(runtime.LimitedUsageRegistry));
            Assert.That(
                runtime.TargetDamageReductionResolver
                    .LimitedUsageCommitter,
                Is.SameAs(runtime.LimitedUsageCommitter));
        }

        [Test]
        public void RuntimePreservesInjectedLimitedRegistry()
        {
            var limitedRegistry =
                new CombatPetLimitedTriggerUsageRegistry();
            var runtime = new CombatPetTriggerRuntime(
                new CombatPetCardTriggerUsageRegistry(),
                new
                    CombatNormalAttackSourceDamageModifierRegistry(),
                new
                    CombatNormalAttackTargetDamageReductionRegistry(),
                new CombatFinalRankModifierRegistry(),
                new CombatPetTriggerUsageRegistry(),
                limitedRegistry);

            Assert.That(
                runtime.LimitedUsageRegistry,
                Is.SameAs(limitedRegistry));
            Assert.That(
                runtime.LimitedUsageCommitter.UsageRegistry,
                Is.SameAs(limitedRegistry));

            Assert.Throws<ArgumentNullException>(() =>
                new CombatPetTriggerRuntime(
                    new CombatPetCardTriggerUsageRegistry(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry(),
                    new
                        CombatNormalAttackTargetDamageReductionRegistry(),
                    new CombatFinalRankModifierRegistry(),
                    new CombatPetTriggerUsageRegistry(),
                    null));
        }

        [Test]
        public void FreshRuntimeHasIndependentLimitedUsage()
        {
            var first = new CombatPetTriggerRuntime();
            var second = new CombatPetTriggerRuntime();
            var pet = new InstanceId(10);

            first.LimitedUsageCommitter.TryCommit(
                pet,
                2,
                () =>
                {
                });

            Assert.That(
                first.LimitedUsageRegistry.TotalUsageCount,
                Is.EqualTo(1));
            Assert.That(
                second.LimitedUsageRegistry.TotalUsageCount,
                Is.Zero);
        }

        private sealed class Environment
        {
            public readonly
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry = new
                    CombatNormalAttackTargetDamageReductionRegistry();

            public readonly CombatPetCardTriggerUsageRegistry
                CardRegistry = new
                    CombatPetCardTriggerUsageRegistry();

            public readonly CombatPetCardTriggerUsageCommitter
                CardCommitter;

            public readonly CombatPetLimitedTriggerUsageRegistry
                LimitedRegistry = new
                    CombatPetLimitedTriggerUsageRegistry();

            public readonly CombatPetLimitedTriggerUsageCommitter
                LimitedCommitter;

            public readonly
                CombatNormalAttackTargetDamageReductionResolver
                Resolver;

            public Environment()
            {
                CardCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        CardRegistry);
                LimitedCommitter =
                    new CombatPetLimitedTriggerUsageCommitter(
                        LimitedRegistry);
                Resolver = new
                    CombatNormalAttackTargetDamageReductionResolver(
                        ReductionRegistry,
                        CardCommitter,
                        LimitedCommitter);
            }

            public NormalAttackCombatEvent CreateAttack(
                long eventId,
                long targetInstanceId)
            {
                var rootEventId = new CombatEventId(1000);
                var metadata = new CombatEventMetadata(
                    new CombatEventId(eventId),
                    new CombatSequenceNumber(eventId),
                    rootEventId,
                    rootEventId);

                return new NormalAttackCombatEvent(
                    metadata,
                    new InstanceId(100),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    new InstanceId(targetInstanceId),
                    new BoardPosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    5);
            }

            public void RegisterLimited(
                NormalAttackCombatEvent attack,
                InstanceId petInstanceId,
                int maximumUsageCount)
            {
                Assert.That(
                    ReductionRegistry.TryRegister(
                        new
                            CombatNormalAttackTargetDamageReductionRequest(
                                attack.Metadata.EventId,
                                petInstanceId,
                                attack.TargetInstanceId,
                                1,
                                maximumUsageCount)),
                    Is.True);
            }

            public void RegisterCardScoped(
                NormalAttackCombatEvent attack,
                InstanceId petInstanceId)
            {
                Assert.That(
                    ReductionRegistry.TryRegister(
                        new
                            CombatNormalAttackTargetDamageReductionRequest(
                                attack.Metadata.EventId,
                                petInstanceId,
                                attack.TargetInstanceId,
                                1)),
                    Is.True);
            }
        }
    }
}
