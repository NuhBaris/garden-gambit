using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardStateZeroHpTests
    {
        [Test]
        public void SetCurrentHpToZero_WithPositiveHp_ReturnsPreviousHpAndSetsZero()
        {
            var card =
                CreateCard(
                    currentHp: 6);

            var previousHp =
                card.SetCurrentHpToZero();

            Assert.That(
                previousHp,
                Is.EqualTo(6));

            Assert.That(
                card.CurrentHp,
                Is.Zero);

            Assert.That(
                card.IsAtDeathThreshold,
                Is.True);

            Assert.That(
                card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                card.Armor,
                Is.EqualTo(2));

            Assert.That(
                card.Attack,
                Is.EqualTo(4));

            Assert.That(
                card.Rank,
                Is.EqualTo(
                    new CardRank(3)));

            Assert.That(
                card.InstanceId,
                Is.EqualTo(
                    new InstanceId(100)));

            Assert.That(
                card.DefinitionId,
                Is.EqualTo(
                    new DefinitionId("test-card")));
        }

        [Test]
        public void SetCurrentHpToZero_WithOneHp_SetsExactZero()
        {
            var card =
                CreateCard(
                    currentHp: 1);

            var previousHp =
                card.SetCurrentHpToZero();

            Assert.That(
                previousHp,
                Is.EqualTo(1));

            Assert.That(
                card.CurrentHp,
                Is.Zero);

            Assert.That(
                card.IsAtDeathThreshold,
                Is.True);

            Assert.That(
                card.HpCapacity,
                Is.EqualTo(10));
        }

        [Test]
        public void SetCurrentHpToZero_WhenAlreadyZero_ThrowsWithoutChangingCard()
        {
            var card =
                CreateCard(
                    currentHp: 0);

            Assert.Throws<InvalidOperationException>(
                () => card.SetCurrentHpToZero());

            Assert.That(
                card.CurrentHp,
                Is.Zero);

            Assert.That(
                card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                card.Armor,
                Is.EqualTo(2));

            Assert.That(
                card.Attack,
                Is.EqualTo(4));

            Assert.That(
                card.IsAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void SetCurrentHpToZero_WhenBelowZero_ThrowsWithoutChangingCard()
        {
            var card =
                CreateCard(
                    currentHp: -3);

            Assert.Throws<InvalidOperationException>(
                () => card.SetCurrentHpToZero());

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(-3));

            Assert.That(
                card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                card.Armor,
                Is.EqualTo(2));

            Assert.That(
                card.Attack,
                Is.EqualTo(4));

            Assert.That(
                card.IsAtDeathThreshold,
                Is.True);
        }

        private static CombatCardState CreateCard(
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId("test-card"),
                new InstanceId(100),
                new CardRank(3),
                10,
                currentHp,
                2,
                4);
        }
    }
}