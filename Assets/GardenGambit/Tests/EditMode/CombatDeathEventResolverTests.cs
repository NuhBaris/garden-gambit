using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDeathEventResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathEventResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathEventResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void AppendFromDamage_WithLethalDamage_AppendsDeathChild()
        {
            var environment =
                CreateEnvironment(
                    targetCurrentHp: 3,
                    targetArmor: 0,
                    incomingDamage: 3);

            var deathEvent =
                environment.Resolver.AppendFromDamage(
                    environment.DamageEvent);

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.Kind,
                Is.EqualTo(CombatEventKind.Death));

            Assert.That(
                deathEvent.InstanceId,
                Is.EqualTo(
                    environment.DamageEvent
                        .TargetInstanceId));

            Assert.That(
                deathEvent.Position,
                Is.EqualTo(
                    environment.DamageEvent
                        .TargetPosition));

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(3));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);

            Assert.That(
                deathEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                deathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.DamageEvent
                        .Metadata.EventId));

            Assert.That(
                deathEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.DamageEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(deathEvent));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.Zero);
        }

        [Test]
        public void AppendFromDamage_WithNonlethalDamage_ReturnsNullWithoutChangingLog()
        {
            var environment =
                CreateEnvironment(
                    targetCurrentHp: 5,
                    targetArmor: 0,
                    incomingDamage: 2);

            var deathEvent =
                environment.Resolver.AppendFromDamage(
                    environment.DamageEvent);

            Assert.That(
                deathEvent,
                Is.Null);

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendFromDamage_WhenTargetWasAlreadyAtDeathThreshold_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    targetCurrentHp: 0,
                    targetArmor: 0,
                    incomingDamage: 1);

            var deathEvent =
                environment.Resolver.AppendFromDamage(
                    environment.DamageEvent);

            Assert.That(
                environment.DamageEvent.Result
                    .EnteredDeathThreshold,
                Is.False);

            Assert.That(
                deathEvent,
                Is.Null);

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(-1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendFromDamage_WithNullDamageEvent_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .AppendFromDamage(null));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendFromDamage_WithUnloggedDamageEvent_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment();

            var targetCard =
                CreateCard(
                    currentHp: 3,
                    armor: 0);

            var result =
                targetCard.ApplyIncomingDamage(3);

            var unloggedDamageEvent =
                CreateDamageEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    result);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendFromDamage(
                        unloggedDamageEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendFromDamage_WithDifferentEventObjectUsingLoggedId_Throws()
        {
            var environment =
                CreateEnvironment();

            var differentDamageEvent =
                CreateDamageEvent(
                    environment.DamageEvent.Metadata,
                    environment.DamageEvent.Result);

            Assert.That(
                differentDamageEvent,
                Is.Not.SameAs(
                    environment.DamageEvent));

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendFromDamage(
                        differentDamageEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void AppendFromDamage_WhenDeathAlreadyLogged_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment();

            var firstDeathEvent =
                environment.Resolver.AppendFromDamage(
                    environment.DamageEvent);

            Assert.That(
                firstDeathEvent,
                Is.Not.Null);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .AppendFromDamage(
                        environment.DamageEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(firstDeathEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                int targetCurrentHp = 3,
                int targetArmor = 0,
                int incomingDamage = 3)
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var targetCard =
                CreateCard(
                    targetCurrentHp,
                    targetArmor);

            var result =
                targetCard.ApplyIncomingDamage(
                    incomingDamage);

            var damageEvent =
                CreateDamageEvent(
                    metadataFactory.CreateRoot(),
                    result);

            eventLog.Append(damageEvent);

            return new TestEnvironment
            {
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                TargetCard = targetCard,
                DamageEvent = damageEvent,
                Resolver =
                    new CombatDeathEventResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static DamageAppliedCombatEvent
            CreateDamageEvent(
                CombatEventMetadata metadata,
                DamageApplicationResult result)
        {
            return new DamageAppliedCombatEvent(
                metadata,
                new InstanceId(100),
                CreatePosition(
                    CombatSide.Player),
                new InstanceId(200),
                CreatePosition(
                    CombatSide.Enemy),
                result);
        }

        private static CombatCardState CreateCard(
            int currentHp,
            int armor)
        {
            return new CombatCardState(
                new DefinitionId("card.target"),
                new InstanceId(200),
                new CardRank(2),
                10,
                currentHp,
                armor,
                3);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
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
            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog { get; set; }

            public CombatCardState TargetCard { get; set; }

            public DamageAppliedCombatEvent
                DamageEvent
            {
                get;
                set;
            }

            public CombatDeathEventResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}