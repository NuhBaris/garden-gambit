using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSlotStateAltarTests
    {
        [Test]
        public void Constructor_WithSacrificialAltar_SetsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    new InstanceId(100),
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind
                        .SacrificialAltar));

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasSacrificialAltar,
                Is.True);

            Assert.That(
                slot.HasWarAltar,
                Is.False);

            Assert.That(
                slot.HasWarBanner,
                Is.False);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.False);
        }

        [Test]
        public void Constructor_WithWarAltar_SetsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    new InstanceId(100),
                    CombatSlotEnhanceKind.WarAltar);

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind.WarAltar));

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasWarAltar,
                Is.True);

            Assert.That(
                slot.HasSacrificialAltar,
                Is.False);

            Assert.That(
                slot.HasWarBanner,
                Is.False);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.False);
        }

        [Test]
        public void Constructor_WithEmptySacrificialAltar_AllowsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    occupantInstanceId: null,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .SacrificialAltar);

            Assert.That(
                slot.IsOccupied,
                Is.False);

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasSacrificialAltar,
                Is.True);

            Assert.That(
                slot.HasWarAltar,
                Is.False);
        }

        [Test]
        public void Constructor_WithEmptyWarAltar_AllowsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    occupantInstanceId: null,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .WarAltar);

            Assert.That(
                slot.IsOccupied,
                Is.False);

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasWarAltar,
                Is.True);

            Assert.That(
                slot.HasSacrificialAltar,
                Is.False);
        }

        [Test]
        public void RemoveOccupant_PreservesSacrificialAltarSnapshot()
        {
            AssertRemovalPreservesEnhance(
                CombatSlotEnhanceKind
                    .SacrificialAltar);
        }

        [Test]
        public void RemoveOccupant_PreservesWarAltarSnapshot()
        {
            AssertRemovalPreservesEnhance(
                CombatSlotEnhanceKind.WarAltar);
        }

        private static void
            AssertRemovalPreservesEnhance(
                CombatSlotEnhanceKind enhanceKind)
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
                    enhanceKind);

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
                    enhanceKind));

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasSacrificialAltar,
                Is.EqualTo(
                    enhanceKind ==
                    CombatSlotEnhanceKind
                        .SacrificialAltar));

            Assert.That(
                slot.HasWarAltar,
                Is.EqualTo(
                    enhanceKind ==
                    CombatSlotEnhanceKind.WarAltar));
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