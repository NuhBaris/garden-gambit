using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardRemovalReasonTests
    {
        [Test]
        public void Values_AreStableForPersistence()
        {
            Assert.That(
                (int)CombatCardRemovalReason.Unspecified,
                Is.EqualTo(0));

            Assert.That(
                (int)CombatCardRemovalReason.DeathRemoval,
                Is.EqualTo(1));

            Assert.That(
                (int)CombatCardRemovalReason.DirectDelete,
                Is.EqualTo(2));
        }
    }
}