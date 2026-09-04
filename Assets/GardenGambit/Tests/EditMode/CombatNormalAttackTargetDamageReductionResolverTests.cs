using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackTargetDamageReductionResolverTests
    {
        [Test]
        public void ResolveDamage_WithoutRequest_ReturnsIncomingDamage()
        {
            var environment =
                CreateEnvironment();

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 5);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(5));

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void ResolveDamage_WithRequest_ReducesDamageAndCommitsUsage()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                petId: 101,
                reductionAmount: 1);

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 5);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(4));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(101),
                        new InstanceId(201)),
                Is.True);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void ResolveDamage_WithZeroDamage_DoesNotCommitUsage()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                petId: 101,
                reductionAmount: 1);

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 0);

            Assert.That(
                resolvedDamage,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(101),
                        new InstanceId(201)),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void ResolveDamage_ReductionCannotMakeDamageNegative()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                petId: 101,
                reductionAmount: 10);

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 3);

            Assert.That(
                resolvedDamage,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(101),
                        new InstanceId(201)),
                Is.True);
        }

        [Test]
        public void ResolveDamage_WithTwoRequests_AppliesSequentially()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                petId: 101,
                reductionAmount: 1);

            RegisterRequest(
                environment,
                petId: 102,
                reductionAmount: 2);

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 5);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(2));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(101),
                        new InstanceId(201)),
                Is.True);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(102),
                        new InstanceId(201)),
                Is.True);
        }

        [Test]
        public void ResolveDamage_WhenDamageReachesZero_DoesNotConsumeLaterRequest()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                petId: 101,
                reductionAmount: 1);

            RegisterRequest(
                environment,
                petId: 102,
                reductionAmount: 1);

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 1);

            Assert.That(
                resolvedDamage,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(101),
                        new InstanceId(201)),
                Is.True);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(102),
                        new InstanceId(201)),
                Is.False);
        }

        [Test]
        public void ResolveDamage_WithPreviouslyUsedKey_SkipsRequest()
        {
            var environment =
                CreateEnvironment();

            var usageKey =
                new CombatPetCardTriggerKey(
                    new InstanceId(101),
                    new InstanceId(201));

            environment.UsageCommitter.TryCommit(
                usageKey,
                () =>
                {
                });

            RegisterRequest(
                environment,
                petId: 101,
                reductionAmount: 1);

            var resolvedDamage =
                environment.Resolver.ResolveDamage(
                    environment.AttackEvent,
                    incomingDamage: 5);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(5));

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void ResolveDamage_WithWrongTargetRequest_Throws()
        {
            var environment =
                CreateEnvironment();

            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        environment.AttackEvent
                            .Metadata.EventId,
                        new InstanceId(101),
                        new InstanceId(202),
                        reductionAmount: 1);

            environment.ReductionRegistry
                .TryRegister(
                    request);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Resolver.ResolveDamage(
                        environment.AttackEvent,
                        incomingDamage: 5));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(101),
                        new InstanceId(202)),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ResolveDamage_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () =>
                    environment.Resolver.ResolveDamage(
                        null,
                        incomingDamage: 5));
        }

        [Test]
        public void ResolveDamage_WithNegativeDamage_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                    () =>
                        environment.Resolver
                            .ResolveDamage(
                                environment.AttackEvent,
                                incomingDamage: -1));
        }

        [Test]
        public void Constructor_WithNullRegistry_Throws()
        {
            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        usageRegistry);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackTargetDamageReductionResolver(
                            null,
                            usageCommitter));
        }

        [Test]
        public void Constructor_WithNullCommitter_Throws()
        {
            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackTargetDamageReductionResolver(
                            reductionRegistry,
                            null));
        }

        private static void RegisterRequest(
            TestEnvironment environment,
            long petId,
            int reductionAmount)
        {
            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        environment.AttackEvent
                            .Metadata.EventId,
                        new InstanceId(
                            petId),
                        environment.AttackEvent
                            .TargetInstanceId,
                        reductionAmount);

            var wasRegistered =
                environment.ReductionRegistry
                    .TryRegister(
                        request);

            Assert.That(
                wasRegistered,
                Is.True);
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        usageRegistry);

            var resolver =
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        reductionRegistry,
                        usageCommitter);

            return new TestEnvironment
            {
                AttackEvent =
                    CreateAttackEvent(),

                ReductionRegistry =
                    reductionRegistry,

                UsageCommitter =
                    usageCommitter,

                Resolver =
                    resolver
            };
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent()
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            return new NormalAttackCombatEvent(
                metadata,
                new InstanceId(1),
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardSeason.Summer,
                new InstanceId(201),
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardSeason.Winter,
                baseDamage: 5);
        }

        private sealed class TestEnvironment
        {
            public NormalAttackCombatEvent
                AttackEvent
            {
                get;
                set;
            }

            public
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry
            {
                get;
                set;
            }

            public CombatPetCardTriggerUsageCommitter
                UsageCommitter
            {
                get;
                set;
            }

            public
                CombatNormalAttackTargetDamageReductionResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}