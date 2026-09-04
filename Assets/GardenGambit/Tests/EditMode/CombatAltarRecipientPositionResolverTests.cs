using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarRecipientPositionResolverTests
    {
        [Test]
        public void Resolve_PlayerFront_ReturnsPlayerBackInSameColumn()
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var resolver =
                new CombatAltarRecipientPositionResolver();

            var recipientPosition =
                resolver.Resolve(
                    donorPosition);

            Assert.That(
                recipientPosition,
                Is.EqualTo(
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        new BoardColumn(1))));
        }

        [Test]
        public void Resolve_PlayerBack_ReturnsPlayerFrontInSameColumn()
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(5));

            var resolver =
                new CombatAltarRecipientPositionResolver();

            var recipientPosition =
                resolver.Resolve(
                    donorPosition);

            Assert.That(
                recipientPosition,
                Is.EqualTo(
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(5))));
        }

        [Test]
        public void Resolve_EnemyFront_ReturnsEnemyBackInSameColumn()
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(2));

            var resolver =
                new CombatAltarRecipientPositionResolver();

            var recipientPosition =
                resolver.Resolve(
                    donorPosition);

            Assert.That(
                recipientPosition.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                recipientPosition.Row,
                Is.EqualTo(
                    BoardRow.Back));

            Assert.That(
                recipientPosition.Column,
                Is.EqualTo(
                    new BoardColumn(2)));
        }

        [Test]
        public void Resolve_EnemyBack_ReturnsEnemyFrontInSameColumn()
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    new BoardColumn(4));

            var resolver =
                new CombatAltarRecipientPositionResolver();

            var recipientPosition =
                resolver.Resolve(
                    donorPosition);

            Assert.That(
                recipientPosition.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                recipientPosition.Row,
                Is.EqualTo(
                    BoardRow.Front));

            Assert.That(
                recipientPosition.Column,
                Is.EqualTo(
                    new BoardColumn(4)));
        }

        [Test]
        public void Resolve_WithInvalidDonorPosition_Throws()
        {
            var resolver =
                new CombatAltarRecipientPositionResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.Resolve(
                    default(BoardPosition)));
        }
    }
}