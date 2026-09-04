using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackEventResolverBatchTests
    {
        [Test]
        public void AppendExchangeAttacks_WithNullExchange_Throws()
        {
            var resolver =
                new CombatNormalAttackEventResolver(
                    CreateMetadataFactory(),
                    new CombatEventLog());

            Assert.Throws<ArgumentNullException>(
                () => resolver.AppendExchangeAttacks(
                    null));
        }

        [Test]
        public void AppendExchangeAttacks_WithUnloggedExchange_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var exchangeEvent =
                CreateExchangeEvent(
                    metadataFactory.CreateRoot());

            var resolver =
                new CombatNormalAttackEventResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<ArgumentException>(
                () => resolver.AppendExchangeAttacks(
                    exchangeEvent));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void AppendExchangeAttacks_WithCopiedExchange_Throws()
        {
            var environment =
                CreateEnvironment();

            var copiedExchange =
                CreateExchangeEvent(
                    environment.ExchangeEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendExchangeAttacks(
                        copiedExchange));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendExchangeAttacks_ReturnsValidatedBatch()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent);

            Assert.That(
                batch.ExchangeEvent,
                Is.SameAs(
                    environment.ExchangeEvent));

            Assert.That(
                batch.PlayerAttackEvent
                    .AttackerInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerInstanceId));

            Assert.That(
                batch.PlayerAttackEvent
                    .TargetInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyInstanceId));

            Assert.That(
                batch.PlayerAttackEvent
                    .BaseDamage,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerAttack));

            Assert.That(
                batch.EnemyAttackEvent
                    .AttackerInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyInstanceId));

            Assert.That(
                batch.EnemyAttackEvent
                    .TargetInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerInstanceId));

            Assert.That(
                batch.EnemyAttackEvent
                    .BaseDamage,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyAttack));
        }

        [Test]
        public void AppendExchangeAttacks_AppendsPlayerBeforeEnemy()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.ExchangeEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    batch.PlayerAttackEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    batch.EnemyAttackEvent));

            Assert.That(
                batch.PlayerAttackEvent
                    .IsPlayerAttack,
                Is.True);

            Assert.That(
                batch.EnemyAttackEvent
                    .IsEnemyAttack,
                Is.True);
        }

        [Test]
        public void AppendExchangeAttacks_CreatesOrderedChildMetadata()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent);

            Assert.That(
                batch.PlayerAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                batch.EnemyAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                batch.PlayerAttackEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                batch.EnemyAttackEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                batch.PlayerAttackEvent
                    .Metadata.SequenceNo,
                Is.LessThan(
                    batch.EnemyAttackEvent
                        .Metadata.SequenceNo));
        }

        [Test]
        public void AppendExchangeAttacks_WithExistingPlayerAttack_AddsNothing()
        {
            var environment =
                CreateEnvironment();

            var existingPlayerAttack =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Player);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    existingPlayerAttack));
        }

        [Test]
        public void AppendExchangeAttacks_WithExistingEnemyAttack_AddsNothing()
        {
            var environment =
                CreateEnvironment();

            var existingEnemyAttack =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Enemy);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    existingEnemyAttack));
        }

        [Test]
        public void AppendExchangeAttacks_CalledTwice_DoesNotRepeatEvents()
        {
            var environment =
                CreateEnvironment();

            var firstBatch =
                environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .AppendExchangeAttacks(
                        environment.ExchangeEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    firstBatch.PlayerAttackEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    firstBatch.EnemyAttackEvent));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var exchangeEvent =
                CreateExchangeEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                exchangeEvent);

            return new TestEnvironment
            {
                EventLog =
                    eventLog,
                ExchangeEvent =
                    exchangeEvent,
                Resolver =
                    new CombatNormalAttackEventResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static
            NormalAttackExchangeCombatEvent
            CreateExchangeEvent(
                CombatEventMetadata metadata)
        {
            return new
                NormalAttackExchangeCombatEvent(
                    metadata,
                    new InstanceId(1),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    playerAttack: 5,
                    new InstanceId(101),
                    new BoardPosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    enemyAttack: 7);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public NormalAttackExchangeCombatEvent
                ExchangeEvent
            {
                get;
                set;
            }

            public CombatNormalAttackEventResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}