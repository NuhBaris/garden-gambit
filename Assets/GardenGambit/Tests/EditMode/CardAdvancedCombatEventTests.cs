using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CardAdvancedCombatEventTests
    {
        [Test]
        public void Constructor_WithBackToFrontMovement_SetsSnapshot()
        {
            var metadata =
                CreateMetadata();

            var sourcePosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    1);

            var destinationPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var advancedEvent =
                new CardAdvancedCombatEvent(
                    metadata,
                    new InstanceId(100),
                    sourcePosition,
                    destinationPosition);

            Assert.That(
                advancedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CardAdvanced));

            Assert.That(
                advancedEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                advancedEvent.InstanceId,
                Is.EqualTo(new InstanceId(100)));

            Assert.That(
                advancedEvent.SourcePosition,
                Is.EqualTo(sourcePosition));

            Assert.That(
                advancedEvent.DestinationPosition,
                Is.EqualTo(destinationPosition));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1)));
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1)));
        }

        [Test]
        public void Constructor_WithInvalidSourcePosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1)));
        }

        [Test]
        public void Constructor_WithInvalidDestinationPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        default(BoardPosition)));
        }

        [Test]
        public void Constructor_WithDifferentSides_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1)));
        }

        [Test]
        public void Constructor_WithDifferentColumns_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            2)));
        }

        [Test]
        public void Constructor_WithFrontRowSource_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1)));
        }

        [Test]
        public void Constructor_WithBackRowDestination_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CardAdvancedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            1)));
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private static BoardPosition CreatePosition(
            CombatSide side,
            BoardRow row,
            int column)
        {
            return new BoardPosition(
                side,
                row,
                new BoardColumn(column));
        }
    }
}