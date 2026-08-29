using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatStartResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatStartResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatStartResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void Start_WithNullState_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var resolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<ArgumentNullException>(
                () => resolver.Start(null));

            Assert.That(
                eventLog.Count,
                Is.Zero);

            Assert.That(
                eventLog.CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void Start_WithEmptyHistory_AppendsCombatStartedRoot()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var state =
                CreateState();

            var resolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            var startedEvent =
                resolver.Start(state);

            Assert.That(
                startedEvent,
                Is.Not.Null);

            Assert.That(
                startedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatStarted));

            Assert.That(
                startedEvent.Metadata.IsTriggerRoot,
                Is.True);

            Assert.That(
                startedEvent.Metadata.HasParent,
                Is.False);

            Assert.That(
                startedEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    startedEvent.Metadata.EventId));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(startedEvent));

            Assert.That(
                eventLog.CardTombstones.Count,
                Is.Zero);

            Assert.That(
                state.GetSide(
                        CombatSide.Player)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                state.GetSide(
                        CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);
        }

        [Test]
        public void Start_WhenEventLogIsNotEmpty_ThrowsWithoutAppending()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var existingEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                existingEvent);

            var resolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<InvalidOperationException>(
                () => resolver.Start(
                    CreateState()));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(existingEvent));

            Assert.That(
                eventLog.CardTombstones.Count,
                Is.Zero);
        }

        [Test]
        public void Start_WhenTombstoneRegistryIsNotEmpty_ThrowsWithoutAppending()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var rootMetadata =
                metadataFactory.CreateRoot();

            var removalMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            var card =
                new CombatCardState(
                    new DefinitionId("removed-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    7,
                    5,
                    0,
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

            var resolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            Assert.Throws<InvalidOperationException>(
                () => resolver.Start(
                    CreateState()));

            Assert.That(
                eventLog.Count,
                Is.Zero);

            Assert.That(
                eventLog.CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.CardTombstones
                    .Tombstones[0],
                Is.SameAs(tombstone));
        }

        private static CombatState CreateState()
        {
            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerSide =
                CreateEmptySide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition);

            var enemySide =
                CreateEmptySide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition);

            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState CreateEmptySide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
                            position)
                    }),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }
    }
}