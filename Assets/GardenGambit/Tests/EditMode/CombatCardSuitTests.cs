using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardSuitTests
    {
        [Test]
        public void Values_AreStableAndContainExactlyFourCanonicalSuits()
        {
            Assert.That(
                (int)CombatCardSuit.Unspecified,
                Is.Zero);
            Assert.That(
                (int)CombatCardSuit.Fruit,
                Is.EqualTo(1));
            Assert.That(
                (int)CombatCardSuit.Vegetable,
                Is.EqualTo(2));
            Assert.That(
                (int)CombatCardSuit.Nut,
                Is.EqualTo(3));
            Assert.That(
                (int)CombatCardSuit.Drink,
                Is.EqualTo(4));
            Assert.That(
                Enum.GetValues(
                        typeof(CombatCardSuit))
                    .Length,
                Is.EqualTo(5));
        }

        [TestCase(CombatCardSuit.Fruit)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Nut)]
        [TestCase(CombatCardSuit.Drink)]
        public void SuitAwareConstructor_PreservesSuitAndExistingState(
            CombatCardSuit suit)
        {
            var card = Card(suit);

            Assert.That(card.Suit, Is.EqualTo(suit));
            Assert.That(card.HasSpecifiedSuit, Is.True);
            Assert.That(
                card.IsFruit,
                Is.EqualTo(
                    suit == CombatCardSuit.Fruit));
            Assert.That(
                card.IsVegetable,
                Is.EqualTo(
                    suit == CombatCardSuit.Vegetable));
            Assert.That(
                card.IsNut,
                Is.EqualTo(
                    suit == CombatCardSuit.Nut));
            Assert.That(
                card.IsDrink,
                Is.EqualTo(
                    suit == CombatCardSuit.Drink));
            Assert.That(
                card.Season,
                Is.EqualTo(CombatCardSeason.Autumn));
            Assert.That(card.Rank.Value, Is.EqualTo(9));
            Assert.That(card.HpCapacity, Is.EqualTo(12));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
            Assert.That(card.Armor, Is.EqualTo(3));
            Assert.That(card.Attack, Is.EqualTo(5));
        }

        [Test]
        public void LegacyConstructors_DefaultSuitToUnspecified()
        {
            var withoutSeason =
                new CombatCardState(
                    new DefinitionId("test.legacy.no_season"),
                    new InstanceId(1),
                    new CardRank(4),
                    10,
                    8,
                    2,
                    3);

            var withSeason =
                new CombatCardState(
                    new DefinitionId("test.legacy.season"),
                    new InstanceId(2),
                    new CardRank(5),
                    CombatCardSeason.Winter,
                    11,
                    9,
                    1,
                    4);

            AssertUnspecified(withoutSeason);
            AssertUnspecified(withSeason);
            Assert.That(
                withoutSeason.Season,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));
            Assert.That(
                withSeason.Season,
                Is.EqualTo(CombatCardSeason.Winter));
        }

        [TestCase(-1)]
        [TestCase(5)]
        public void SuitAwareConstructor_RejectsUndefinedSuit(
            int rawSuit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Card((CombatCardSuit)rawSuit));
        }

        private static CombatCardState Card(
            CombatCardSuit suit)
        {
            return new CombatCardState(
                new DefinitionId("test.suit_card"),
                new InstanceId(10),
                new CardRank(9),
                suit,
                CombatCardSeason.Autumn,
                12,
                7,
                3,
                5);
        }

        private static void AssertUnspecified(
            CombatCardState card)
        {
            Assert.That(
                card.Suit,
                Is.EqualTo(
                    CombatCardSuit.Unspecified));
            Assert.That(card.HasSpecifiedSuit, Is.False);
            Assert.That(card.IsFruit, Is.False);
            Assert.That(card.IsVegetable, Is.False);
            Assert.That(card.IsNut, Is.False);
            Assert.That(card.IsDrink, Is.False);
        }
    }
}
