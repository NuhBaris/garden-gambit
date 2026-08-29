using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatEventLogTombstoneTests
    {
        [Test]
        public void Constructor_CreatesEmptyTombstoneRegistry()
        {
            var eventLog =
                new CombatEventLog();

            Assert.That(
                eventLog.CardTombstones,
                Is.Not.Null);

            Assert.That(
                eventLog.CardTombstones.Count,
                Is.Zero);

            Assert.That(
                eventLog.CardTombstones,
                Is.SameAs(
                    eventLog.CardTombstones));
        }

        [Test]
        public void SeparateEventLogs_OwnSeparateTombstoneRegistries()
        {
            var firstEventLog =
                new CombatEventLog();

            var secondEventLog =
                new CombatEventLog();

            Assert.That(
                firstEventLog.CardTombstones,
                Is.Not.SameAs(
                    secondEventLog.CardTombstones));

            Assert.That(
                firstEventLog.CardTombstones.Count,
                Is.Zero);

            Assert.That(
                secondEventLog.CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void CardTombstones_StoresRemovalHistoryForEventLog()
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var rootEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot(),
                    CombatEventKind.NormalAttack);

            eventLog.Append(rootEvent);

            var removalMetadata =
                metadataFactory.CreateChild(
                    rootEvent.Metadata);

            var removalEvent =
                new TestCombatEvent(
                    removalMetadata,
                    CombatEventKind.DirectDelete);

            eventLog.Append(removalEvent);

            var card =
                new CombatCardState(
                    new DefinitionId("test-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    7,
                    5,
                    1,
                    3);

            var tombstone =
                new CombatCardTombstone(
                    card,
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    CombatCardRemovalReason.DirectDelete,
                    removalMetadata);

            eventLog.CardTombstones.Append(
                tombstone);

            Assert.That(
                eventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                eventLog.CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.CardTombstones.Get(
                    card.InstanceId),
                Is.SameAs(tombstone));

            Assert.That(
                eventLog.CardTombstones
                    .GetByRemovalEvent(
                        removalEvent.Metadata.EventId),
                Is.SameAs(tombstone));
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata,
                CombatEventKind kind)
                : base(
                    metadata,
                    kind)
            {
            }
        }
    }
}