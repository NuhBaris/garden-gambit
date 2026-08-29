using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatTriggerOrderKeyTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsProperties()
        {
            var key =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Player,
                    2,
                    1);

            Assert.That(
                key.SourceKind,
                Is.EqualTo(
                    CombatTriggerSourceKind.Card));

            Assert.That(
                key.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                key.HorizontalOrder,
                Is.EqualTo(2));

            Assert.That(
                key.VerticalOrder,
                Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithUnspecifiedSourceKind_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Unspecified,
                        CombatSide.Player,
                        0,
                        0));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        default(CombatSide),
                        0,
                        0));
        }

        [Test]
        public void Constructor_WithNegativeHorizontalOrder_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        -1,
                        0));
        }

        [Test]
        public void Constructor_WithNegativeVerticalOrder_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        0,
                        -1));
        }

        [Test]
        public void CompareTo_SourceKindTakesPriorityOverPosition()
        {
            var slotKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Slot,
                    CombatSide.Enemy,
                    99,
                    99);

            var petKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Pet,
                    CombatSide.Player,
                    0,
                    0);

            Assert.That(
                slotKey.CompareTo(petKey),
                Is.LessThan(0));

            Assert.That(
                petKey.CompareTo(slotKey),
                Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_HorizontalOrderUsesLeftToRightPriority()
        {
            var leftKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Enemy,
                    0,
                    99);

            var rightKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Player,
                    1,
                    0);

            Assert.That(
                leftKey.CompareTo(rightKey),
                Is.LessThan(0));

            Assert.That(
                rightKey.CompareTo(leftKey),
                Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_VerticalOrderUsesTopToBottomPriority()
        {
            var topKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Enemy,
                    2,
                    0);

            var bottomKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Player,
                    2,
                    1);

            Assert.That(
                topKey.CompareTo(bottomKey),
                Is.LessThan(0));

            Assert.That(
                bottomKey.CompareTo(topKey),
                Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_WithEqualCategoryAndPosition_UsesPlayerFirst()
        {
            var playerKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Player,
                    2,
                    1);

            var enemyKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Enemy,
                    2,
                    1);

            Assert.That(
                playerKey.CompareTo(enemyKey),
                Is.LessThan(0));

            Assert.That(
                enemyKey.CompareTo(playerKey),
                Is.GreaterThan(0));
        }

        [Test]
        public void Equality_WithSameValues_IsStable()
        {
            var first =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Pet,
                    CombatSide.Player,
                    3,
                    0);

            var second =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Pet,
                    CombatSide.Player,
                    3,
                    0);

            var different =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Pet,
                    CombatSide.Player,
                    4,
                    0);

            Assert.That(
                first.CompareTo(second),
                Is.Zero);

            Assert.That(
                first,
                Is.EqualTo(second));

            Assert.That(
                first == second,
                Is.True);

            Assert.That(
                first != second,
                Is.False);

            Assert.That(
                first.GetHashCode(),
                Is.EqualTo(
                    second.GetHashCode()));

            Assert.That(
                first,
                Is.Not.EqualTo(different));
        }

        [Test]
        public void IsValid_WithConstructedKey_ReturnsTrue()
        {
            var key =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Player,
                    0,
                    0);

            Assert.That(
                key.IsValid,
                Is.True);
        }

        [Test]
        public void IsValid_WithDefaultKey_ReturnsFalse()
        {
            var key =
                default(CombatTriggerOrderKey);

            Assert.That(
                key.IsValid,
                Is.False);
        }

    }
}