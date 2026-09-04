using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSlotStateWarBannerTests
    {
        [Test]
        public void Constructor_WithWarBanner_SetsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    new InstanceId(100),
                    CombatSlotEnhanceKind.WarBanner);

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind.WarBanner));

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasWarBanner,
                Is.True);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.False);

            Assert.That(
                slot.IsOccupied,
                Is.True);
        }

        [Test]
        public void Constructor_WithEmptyWarBanner_AllowsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    occupantInstanceId: null,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .WarBanner);

            Assert.That(
                slot.IsOccupied,
                Is.False);

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasWarBanner,
                Is.True);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.False);
        }

        [Test]
        public void Constructor_WithProtectiveSeal_DoesNotSetWarBanner()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    new InstanceId(100),
                    CombatSlotEnhanceKind
                        .ProtectiveSeal);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.True);

            Assert.That(
                slot.HasWarBanner,
                Is.False);

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind
                        .ProtectiveSeal));
        }

        [Test]
        public void RemoveOccupant_PreservesWarBannerSnapshot()
        {
            var position =
                CreatePosition();

            var occupantInstanceId =
                new InstanceId(100);

            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    position,
                    occupantInstanceId,
                    CombatSlotEnhanceKind.WarBanner);

            var board =
                new CombatBoardState(
                    CombatSide.Player,
                    new[]
                    {
                        slot
                    });

            var removedInstanceId =
                board.RemoveOccupant(
                    position);

            Assert.That(
                removedInstanceId,
                Is.EqualTo(
                    occupantInstanceId));

            Assert.That(
                slot.IsOccupied,
                Is.False);

            Assert.That(
                slot.OccupantInstanceId.HasValue,
                Is.False);

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind.WarBanner));

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasWarBanner,
                Is.True);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.False);
        }

        private static BoardPosition
            CreatePosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}