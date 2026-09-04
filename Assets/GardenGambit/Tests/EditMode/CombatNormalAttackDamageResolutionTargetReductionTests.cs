using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackDamageResolutionTargetReductionTests
    {
        [Test]
        public void Resolve_AppliesTargetReductionAfterSourceResolution()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                environment.Batch.PlayerAttackEvent,
                petId: 1001,
                reductionAmount: 1);

            RegisterRequest(
                environment,
                environment.Batch.EnemyAttackEvent,
                petId: 1002,
                reductionAmount: 2);

            var resolution =
                environment.DamageResolver.Resolve(
                    environment.Batch,
                    attackEvent =>
                        checked(
                            attackEvent.BaseDamage + 3),
                    environment.TargetResolver);

            Assert.That(
                resolution.ResolvedDamageToEnemy,
                Is.EqualTo(7));

            Assert.That(
                resolution.ResolvedDamageToPlayer,
                Is.EqualTo(8));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1001),
                        new InstanceId(201)),
                Is.True);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1002),
                        new InstanceId(1)),
                Is.True);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_RequestForOneAttack_DoesNotAffectOtherAttack()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                environment.Batch.EnemyAttackEvent,
                petId: 1002,
                reductionAmount: 1);

            var resolution =
                environment.DamageResolver.Resolve(
                    environment.Batch,
                    attackEvent =>
                        attackEvent.BaseDamage,
                    environment.TargetResolver);

            Assert.That(
                resolution.ResolvedDamageToEnemy,
                Is.EqualTo(5));

            Assert.That(
                resolution.ResolvedDamageToPlayer,
                Is.EqualTo(6));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1002),
                        new InstanceId(1)),
                Is.True);
        }

        [Test]
        public void Resolve_WithZeroSourceDamage_DoesNotConsumeUsage()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                environment.Batch.PlayerAttackEvent,
                petId: 1001,
                reductionAmount: 1);

            RegisterRequest(
                environment,
                environment.Batch.EnemyAttackEvent,
                petId: 1002,
                reductionAmount: 1);

            var resolution =
                environment.DamageResolver.Resolve(
                    environment.Batch,
                    attackEvent => 0,
                    environment.TargetResolver);

            Assert.That(
                resolution.ResolvedDamageToEnemy,
                Is.Zero);

            Assert.That(
                resolution.ResolvedDamageToPlayer,
                Is.Zero);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1001),
                        new InstanceId(201)),
                Is.False);

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1002),
                        new InstanceId(1)),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WhenSecondSourceIsNegative_DoesNotConsumeFirstUsage()
        {
            var environment =
                CreateEnvironment();

            RegisterRequest(
                environment,
                environment.Batch.PlayerAttackEvent,
                petId: 1001,
                reductionAmount: 1);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.DamageResolver.Resolve(
                        environment.Batch,
                        attackEvent =>
                        {
                            if (ReferenceEquals(
                                    attackEvent,
                                    environment.Batch
                                        .PlayerAttackEvent))
                            {
                                return 5;
                            }

                            return -1;
                        },
                        environment.TargetResolver));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1001),
                        new InstanceId(201)),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithNullTargetResolver_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () =>
                    environment.DamageResolver.Resolve(
                        environment.Batch,
                        attackEvent =>
                            attackEvent.BaseDamage,
                        null));
        }

        [Test]
        public void Resolve_WithPreviouslyUsedKey_SkipsReduction()
        {
            var environment =
                CreateEnvironment();

            var usageKey =
                new CombatPetCardTriggerKey(
                    new InstanceId(1001),
                    new InstanceId(201));

            environment.UsageCommitter.TryCommit(
                usageKey,
                () =>
                {
                });

            RegisterRequest(
                environment,
                environment.Batch.PlayerAttackEvent,
                petId: 1001,
                reductionAmount: 1);

            var resolution =
                environment.DamageResolver.Resolve(
                    environment.Batch,
                    attackEvent =>
                        attackEvent.BaseDamage,
                    environment.TargetResolver);

            Assert.That(
                resolution.ResolvedDamageToEnemy,
                Is.EqualTo(5));

            Assert.That(
                resolution.ResolvedDamageToPlayer,
                Is.EqualTo(7));

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        private static void RegisterRequest(
            TestEnvironment environment,
            NormalAttackCombatEvent attackEvent,
            long petId,
            int reductionAmount)
        {
            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        attackEvent.Metadata.EventId,
                        new InstanceId(
                            petId),
                        attackEvent.TargetInstanceId,
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
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var exchangeMetadata =
                metadataFactory.CreateRoot();

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    exchangeMetadata,
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    playerAttack: 5,
                    new InstanceId(201),
                    CreatePosition(
                        CombatSide.Enemy),
                    enemyAttack: 7);

            var playerAttackEvent =
                new NormalAttackCombatEvent(
                    metadataFactory.CreateChild(
                        exchangeMetadata),
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    CombatCardSeason.Summer,
                    new InstanceId(201),
                    CreatePosition(
                        CombatSide.Enemy),
                    CombatCardSeason.Winter,
                    baseDamage: 5);

            var enemyAttackEvent =
                new NormalAttackCombatEvent(
                    metadataFactory.CreateChild(
                        exchangeMetadata),
                    new InstanceId(201),
                    CreatePosition(
                        CombatSide.Enemy),
                    CombatCardSeason.Winter,
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    CombatCardSeason.Summer,
                    baseDamage: 7);

            var batch =
                new CombatNormalAttackEventBatch(
                    exchangeEvent,
                    playerAttackEvent,
                    enemyAttackEvent);

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

            var targetResolver =
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        reductionRegistry,
                        usageCommitter);

            return new TestEnvironment
            {
                Batch =
                    batch,

                ReductionRegistry =
                    reductionRegistry,

                UsageCommitter =
                    usageCommitter,

                TargetResolver =
                    targetResolver,

                DamageResolver =
                    new
                        CombatNormalAttackDamageResolutionResolver()
            };
        }

        private static BoardPosition
            CreatePosition(
                CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private sealed class TestEnvironment
        {
            public CombatNormalAttackEventBatch Batch
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
                TargetResolver
            {
                get;
                set;
            }

            public CombatNormalAttackDamageResolutionResolver
                DamageResolver
            {
                get;
                set;
            }
        }
    }
}