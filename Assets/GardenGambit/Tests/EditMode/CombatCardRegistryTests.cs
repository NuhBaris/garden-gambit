using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardRegistryTests
    {
        [Test]
        public void Constructor_WithCards_PreservesOrderAndCreatesReadOnlyList()
        {
            var firstCard =
                CreateCard(1, "card.first");

            var secondCard =
                CreateCard(2, "card.second");

            var registry = new CombatCardRegistry(
                new[] { firstCard, secondCard });

            Assert.That(registry.Count, Is.EqualTo(2));
            Assert.That(
                registry.Cards[0],
                Is.SameAs(firstCard));

            Assert.That(
                registry.Cards[1],
                Is.SameAs(secondCard));

            var collection =
                (ICollection<CombatCardState>)
                registry.Cards;

            Assert.That(collection.IsReadOnly, Is.True);
        }

        [Test]
        public void Constructor_WithEmptyCollection_AllowsEmptyRegistry()
        {
            var registry = new CombatCardRegistry(
                new CombatCardState[0]);

            Assert.That(registry.Count, Is.Zero);
            Assert.That(registry.Cards, Is.Empty);
        }

        [Test]
        public void Constructor_WithNullCollection_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new CombatCardRegistry(null);
            });
        }

        [Test]
        public void Constructor_WithNullCard_Throws()
        {
            var cards = new CombatCardState[]
            {
                null
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatCardRegistry(cards);
            });
        }

        [Test]
        public void Constructor_WithDuplicateInstanceId_Throws()
        {
            var cards = new[]
            {
                CreateCard(1, "card.first"),
                CreateCard(1, "card.second")
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatCardRegistry(cards);
            });
        }

        [Test]
        public void Constructor_WithSameDefinitionAndDifferentInstances_AllowsCards()
        {
            var cards = new[]
            {
                CreateCard(1, "card.shared"),
                CreateCard(2, "card.shared")
            };

            var registry =
                new CombatCardRegistry(cards);

            Assert.That(registry.Count, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_WithMoreThanTenCards_AllowsRegistry()
        {
            var cards =
                new List<CombatCardState>();

            for (var index = 1; index <= 11; index++)
            {
                cards.Add(CreateCard(
                    index,
                    $"card.{index}"));
            }

            var registry =
                new CombatCardRegistry(cards);

            Assert.That(registry.Count, Is.EqualTo(11));
        }

        [Test]
        public void GetCard_WithExistingInstanceId_ReturnsCard()
        {
            var expectedCard =
                CreateCard(1, "card.first");

            var registry = new CombatCardRegistry(
                new[]
                {
                    expectedCard,
                    CreateCard(2, "card.second")
                });

            var result =
                registry.GetCard(new InstanceId(1));

            Assert.That(result, Is.SameAs(expectedCard));
        }

        [Test]
        public void GetCard_WithMissingInstanceId_Throws()
        {
            var registry = new CombatCardRegistry(
                new[]
                {
                    CreateCard(1, "card.first")
                });

            Assert.Throws<KeyNotFoundException>(
                () => registry.GetCard(
                    new InstanceId(999)));
        }

        [Test]
        public void GetCard_WithInvalidInstanceId_Throws()
        {
            var registry = new CombatCardRegistry(
                new CombatCardState[0]);

            Assert.Throws<ArgumentException>(
                () => registry.GetCard(
                    default(InstanceId)));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            string definitionId)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                7,
                2,
                3);
        }
    }
}