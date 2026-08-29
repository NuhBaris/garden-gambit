using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatTriggerSourceKindTests
    {
        [Test]
        public void NumericValues_AreStableAndMatchLockedPriority()
        {
            Assert.That(
                (int)CombatTriggerSourceKind.Unspecified,
                Is.EqualTo(0));

            Assert.That(
                (int)CombatTriggerSourceKind.Slot,
                Is.EqualTo(1));

            Assert.That(
                (int)CombatTriggerSourceKind.Pet,
                Is.EqualTo(2));

            Assert.That(
                (int)CombatTriggerSourceKind.Card,
                Is.EqualTo(3));

            Assert.That(
                (int)CombatTriggerSourceKind
                    .NormalEnemySpecial,
                Is.EqualTo(4));

            Assert.That(
                CombatTriggerSourceKind.Slot <
                CombatTriggerSourceKind.Pet,
                Is.True);

            Assert.That(
                CombatTriggerSourceKind.Pet <
                CombatTriggerSourceKind.Card,
                Is.True);

            Assert.That(
                CombatTriggerSourceKind.Card <
                CombatTriggerSourceKind
                    .NormalEnemySpecial,
                Is.True);
        }
    }
}