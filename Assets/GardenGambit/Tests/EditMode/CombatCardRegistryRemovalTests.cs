using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardRegistryRemovalTests
    {
        [Test]
        public void RemoveCard_WithExistingInstanceId_RemovesAndReturnsSameCard()
        {
            var firstCard =
                CreateCard(
                    "card.first",
                    100);

            var removedCard =
                CreateCard(
                    "card.removed",
                    200);

            var lastCard =
                CreateCard(
                    "card.last",
                    300);

            var registry =
                new CombatCardRegistry(
                    new[]
                    {
                        firstCard,
                        removedCard,
                        lastCard
                    });

            var result =
                registry.RemoveCard(
                    removedCard.InstanceId);

            Assert.That(
                result,
                Is.SameAs(removedCard));

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Cards[0],
                Is.SameAs(firstCard));

            Assert.That(
                registry.Cards[1],
                Is.SameAs(lastCard));

            Assert.Throws<KeyNotFoundException>(
                () => registry.GetCard(
                    removedCard.InstanceId));
        }

        [Test]
        public void RemoveCard_UpdatesPreviouslyRetrievedReadOnlyView()
        {
            var firstCard =
                CreateCard(
                    "card.first",
                    100);

            var secondCard =
                CreateCard(
                    "card.second",
                    200);

            var registry =
                new CombatCardRegistry(
                    new[]
                    {
                        firstCard,
                        secondCard
                    });

            var readOnlyCards =
                registry.Cards;

            registry.RemoveCard(
                firstCard.InstanceId);

            Assert.That(
                readOnlyCards.Count,
                Is.EqualTo(1));

            Assert.That(
                readOnlyCards[0],
                Is.SameAs(secondCard));
        }

        [Test]
        public void RemoveCard_WithInvalidInstanceId_ThrowsWithoutChangingRegistry()
        {
            var card =
                CreateCard(
                    "card.existing",
                    100);

            var registry =
                new CombatCardRegistry(
                    new[] { card });

            Assert.Throws<ArgumentException>(
                () => registry.RemoveCard(
                    default(InstanceId)));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Cards[0],
                Is.SameAs(card));
        }

        [Test]
        public void RemoveCard_WithMissingInstanceId_ThrowsWithoutChangingRegistry()
        {
            var card =
                CreateCard(
                    "card.existing",
                    100);

            var registry =
                new CombatCardRegistry(
                    new[] { card });

            Assert.Throws<KeyNotFoundException>(
                () => registry.RemoveCard(
                    new InstanceId(999)));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Cards[0],
                Is.SameAs(card));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                5,
                5,
                0,
                1);
        }
    }
}