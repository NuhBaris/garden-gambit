using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDeathEventResolverAltarTests
    {
        [Test]
        public void AppendFromAltar_WithSacrificialAltar_AppendsDonorDeathEvent()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: false);

            var deathEvent =
                environment.Resolver.AppendFromAltar(
                    environment.AltarEvent);

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.Death));

            Assert.That(
                deathEvent.InstanceId,
                Is.EqualTo(
                    environment.DonorInstanceId));

            Assert.That(
                deathEvent.Position,
                Is.EqualTo(
                    environment.DonorPosition));

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);

            Assert.That(
                deathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.AltarEvent
                        .Metadata.EventId));

            Assert.That(
                deathEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(deathEvent));
        }

        [Test]
        public void AppendFromAltar_WithWarAltar_AppendsDonorDeathEvent()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: true,
                    transferredAttack: 6,
                    donorPreviousHp: 7);

            var deathEvent =
                environment.Resolver.AppendFromAltar(
                    environment.AltarEvent);

            Assert.That(
                deathEvent.InstanceId,
                Is.EqualTo(
                    environment.DonorInstanceId));

            Assert.That(
                deathEvent.Position,
                Is.EqualTo(
                    environment.DonorPosition));

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(7));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);

            Assert.That(
                deathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.AltarEvent
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(deathEvent));
        }

        [Test]
        public void AppendFromAltar_WithZeroAttackWarAltar_StillAppendsDeathEvent()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: true,
                    transferredAttack: 0,
                    donorPreviousHp: 4);

            var deathEvent =
                environment.Resolver.AppendFromAltar(
                    environment.AltarEvent);

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void AppendFromAltar_WithNullEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: false);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .AppendFromAltar(null));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void AppendFromAltar_WithNonAltarEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: false);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendFromAltar(
                        environment
                            .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void AppendFromAltar_WithUnloggedAltarEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: false);

            var metadata =
                environment.MetadataFactory.CreateChild(
                    environment.CombatStartedEvent
                        .Metadata);

            var unloggedEvent =
                new SacrificialAltarActivatedCombatEvent(
                    metadata,
                    new InstanceId(300),
                    environment.DonorPosition,
                    new InstanceId(400),
                    environment.RecipientPosition,
                    transferredHp: 3);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendFromAltar(
                        unloggedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void AppendFromAltar_WithDifferentEventInstance_Throws()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: false);

            var loggedEvent =
                (SacrificialAltarActivatedCombatEvent)
                    environment.AltarEvent;

            var differentInstance =
                new SacrificialAltarActivatedCombatEvent(
                    loggedEvent.Metadata,
                    loggedEvent.DonorInstanceId,
                    loggedEvent.DonorPosition,
                    loggedEvent.RecipientInstanceId,
                    loggedEvent.RecipientPosition,
                    loggedEvent.TransferredHp);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .AppendFromAltar(
                        differentInstance));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void AppendFromAltar_WhenDeathAlreadyLogged_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment(
                    isWarAltar: false);

            var firstDeathEvent =
                environment.Resolver.AppendFromAltar(
                    environment.AltarEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .AppendFromAltar(
                        environment.AltarEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(firstDeathEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool isWarAltar,
                int transferredAttack = 6,
                int donorPreviousHp = 4)
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            var donorInstanceId =
                new InstanceId(100);

            var recipientInstanceId =
                new InstanceId(200);

            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(2));

            var altarMetadata =
                metadataFactory.CreateChild(
                    combatStartedEvent.Metadata);

            CombatEvent altarEvent;

            if (isWarAltar)
            {
                altarEvent =
                    new WarAltarActivatedCombatEvent(
                        altarMetadata,
                        donorInstanceId,
                        donorPosition,
                        recipientInstanceId,
                        recipientPosition,
                        transferredAttack,
                        donorPreviousHp);
            }
            else
            {
                altarEvent =
                    new SacrificialAltarActivatedCombatEvent(
                        altarMetadata,
                        donorInstanceId,
                        donorPosition,
                        recipientInstanceId,
                        recipientPosition,
                        donorPreviousHp);
            }

            eventLog.Append(
                altarEvent);

            return new TestEnvironment
            {
                MetadataFactory =
                    metadataFactory,
                EventLog =
                    eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                AltarEvent =
                    altarEvent,
                Resolver =
                    new CombatDeathEventResolver(
                        metadataFactory,
                        eventLog),
                DonorInstanceId =
                    donorInstanceId,
                DonorPosition =
                    donorPosition,
                RecipientPosition =
                    recipientPosition
            };
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

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public CombatEvent AltarEvent
            {
                get;
                set;
            }

            public CombatDeathEventResolver Resolver
            {
                get;
                set;
            }

            public InstanceId DonorInstanceId
            {
                get;
                set;
            }

            public BoardPosition DonorPosition
            {
                get;
                set;
            }

            public BoardPosition RecipientPosition
            {
                get;
                set;
            }
        }
    }
}