using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatAltarRecipientTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsSnapshot()
        {
            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(3));

            var card =
                CreateCard();

            var recipient =
                new CombatAltarRecipient(
                    position,
                    card);

            Assert.That(
                recipient.Position,
                Is.EqualTo(
                    position));

            Assert.That(
                recipient.Card,
                Is.SameAs(
                    card));

            Assert.That(
                recipient.InstanceId,
                Is.EqualTo(
                    card.InstanceId));

            Assert.That(
                recipient.InstanceId,
                Is.EqualTo(
                    new InstanceId(100)));
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            var card =
                CreateCard();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatAltarRecipient(
                        default(BoardPosition),
                        card));
        }

        [Test]
        public void Constructor_WithNullCard_Throws()
        {
            var position =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(5));

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarRecipient(
                        position,
                        null));
        }

        private static CombatCardState CreateCard()
        {
            return new CombatCardState(
                new DefinitionId("recipient-card"),
                new InstanceId(100),
                new CardRank(2),
                7,
                7,
                0,
                3);
        }
    }
}