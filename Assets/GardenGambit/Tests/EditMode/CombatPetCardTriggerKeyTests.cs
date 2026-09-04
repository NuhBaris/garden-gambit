using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetCardTriggerKeyTests
    {
        [Test]
        public void Constructor_WithValidIds_SetsState()
        {
            var petInstanceId =
                new InstanceId(1);

            var cardInstanceId =
                new InstanceId(101);

            var key =
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    cardInstanceId);

            Assert.That(
                key.PetInstanceId,
                Is.EqualTo(
                    petInstanceId));

            Assert.That(
                key.CardInstanceId,
                Is.EqualTo(
                    cardInstanceId));

            Assert.That(
                key.IsValid,
                Is.True);
        }

        [Test]
        public void Constructor_WithInvalidPetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetCardTriggerKey(
                        default(InstanceId),
                        new InstanceId(101)));
        }

        [Test]
        public void Constructor_WithInvalidCardInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetCardTriggerKey(
                        new InstanceId(1),
                        default(InstanceId)));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var key =
                default(CombatPetCardTriggerKey);

            Assert.That(
                key.IsValid,
                Is.False);
        }

        [Test]
        public void Equals_WithSameIds_ReturnsTrue()
        {
            var first =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(101));

            var second =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(101));

            Assert.That(
                first.Equals(second),
                Is.True);

            Assert.That(
                first.Equals(
                    (object)second),
                Is.True);

            Assert.That(
                first.GetHashCode(),
                Is.EqualTo(
                    second.GetHashCode()));
        }

        [Test]
        public void Equals_WithDifferentPetInstanceId_ReturnsFalse()
        {
            var first =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(101));

            var second =
                new CombatPetCardTriggerKey(
                    new InstanceId(2),
                    new InstanceId(101));

            Assert.That(
                first.Equals(second),
                Is.False);
        }

        [Test]
        public void Equals_WithDifferentCardInstanceId_ReturnsFalse()
        {
            var first =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(101));

            var second =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(102));

            Assert.That(
                first.Equals(second),
                Is.False);
        }

        [Test]
        public void EqualityOperators_UseValueEquality()
        {
            var first =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(101));

            var equal =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(101));

            var different =
                new CombatPetCardTriggerKey(
                    new InstanceId(1),
                    new InstanceId(102));

            Assert.That(
                first == equal,
                Is.True);

            Assert.That(
                first != equal,
                Is.False);

            Assert.That(
                first == different,
                Is.False);

            Assert.That(
                first != different,
                Is.True);
        }

        [Test]
        public void ToString_ContainsBothInstanceIds()
        {
            var petInstanceId =
                new InstanceId(1);

            var cardInstanceId =
                new InstanceId(101);

            var key =
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    cardInstanceId);

            var text =
                key.ToString();

            Assert.That(
                text,
                Does.Contain(
                    petInstanceId.ToString()));

            Assert.That(
                text,
                Does.Contain(
                    cardInstanceId.ToString()));
        }
    }
}