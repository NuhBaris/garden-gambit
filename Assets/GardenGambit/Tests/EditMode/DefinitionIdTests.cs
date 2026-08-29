using System;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class DefinitionIdTests
    {
        [Test]
        public void Constructor_WithValidValue_PreservesExactValue()
        {
            const string value = "card.sample";

            var definitionId = new DefinitionId(value);

            Assert.That(definitionId.Value, Is.EqualTo(value));
            Assert.That(definitionId.IsValid, Is.True);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var left = new DefinitionId("card.sample");
            var right = new DefinitionId("card.sample");

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left == right, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void DifferentCasing_IsNotEqual()
        {
            var lowerCase = new DefinitionId("card.sample");
            var upperCase = new DefinitionId("CARD.SAMPLE");

            Assert.That(lowerCase, Is.Not.EqualTo(upperCase));
            Assert.That(lowerCase != upperCase, Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_WithMissingValue_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new DefinitionId(value);
            });
        }
    }
}