using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackEventResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackEventResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackEventResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void AppendAttack_WithNullExchangeEvent_Throws()
        {
            var resolver =
                new CombatNormalAttackEventResolver(
                    CreateMetadataFactory(),
                    new CombatEventLog());

            Assert.Throws<ArgumentNullException>(
                () => resolver.AppendAttack(
                    null,
                    CombatSide.Player));
        }

        [Test]
        public void AppendAttack_WithInvalidSide_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .AppendAttack(
                        environment.ExchangeEvent,
                        default(CombatSide)));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendAttack_WithUnloggedExchange_Throws()
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
                () => resolver.AppendAttack(
                    exchangeEvent,
                    CombatSide.Player));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void AppendAttack_WithCopiedExchangeObject_Throws()
        {
            var environment =
                CreateEnvironment();

            var copiedExchange =
                CreateExchangeEvent(
                    environment.ExchangeEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendAttack(
                        copiedExchange,
                        CombatSide.Player));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendAttack_WithPlayer_CreatesPlayerAttack()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Player);

            Assert.That(
                attackEvent.AttackerInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerInstanceId));

            Assert.That(
                attackEvent.AttackerPosition,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerPosition));

            Assert.That(
                attackEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyInstanceId));

            Assert.That(
                attackEvent.TargetPosition,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyPosition));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerAttack));

            Assert.That(
                attackEvent.IsPlayerAttack,
                Is.True);

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(attackEvent));
        }

        [Test]
        public void AppendAttack_WithEnemy_CreatesEnemyAttack()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Enemy);

            Assert.That(
                attackEvent.AttackerInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyInstanceId));

            Assert.That(
                attackEvent.AttackerPosition,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyPosition));

            Assert.That(
                attackEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerInstanceId));

            Assert.That(
                attackEvent.TargetPosition,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .PlayerPosition));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .EnemyAttack));

            Assert.That(
                attackEvent.IsEnemyAttack,
                Is.True);

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(attackEvent));
        }

        [Test]
        public void AppendAttack_WithBothSides_AllowsBoth()
        {
            var environment =
                CreateEnvironment();

            var playerAttackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Player);

            var enemyAttackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Enemy);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    playerAttackEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    enemyAttackEvent));

            Assert.That(
                playerAttackEvent.IsPlayerAttack,
                Is.True);

            Assert.That(
                enemyAttackEvent.IsEnemyAttack,
                Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void AppendAttack_WithDuplicateSide_Throws(
            CombatSide attackerSide)
        {
            var environment =
                CreateEnvironment();

            var firstAttack =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    attackerSide);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .AppendAttack(
                        environment.ExchangeEvent,
                        attackerSide));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstAttack));
        }

        [Test]
        public void AppendAttack_CreatesChildOfExchange()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Player);

            Assert.That(
                attackEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                attackEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                attackEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ExchangeEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                attackEvent.Metadata.IsTriggerRoot,
                Is.False);
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