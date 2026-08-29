using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSlotStateEnhanceTests
    {
        [Test]
        public void Constructor_WithoutEnhance_UsesNone()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    new InstanceId(100));

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind.None));

            Assert.That(
                slot.HasEnhance,
                Is.False);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.False);
        }

        [Test]
        public void Constructor_WithProtectiveSeal_SetsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    new InstanceId(100),
                    CombatSlotEnhanceKind
                        .ProtectiveSeal);

            Assert.That(
                slot.EnhanceKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind
                        .ProtectiveSeal));

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.True);

            Assert.That(
                slot.IsOccupied,
                Is.True);
        }

        [Test]
        public void Constructor_WithProtectiveSealAndNoOccupant_AllowsSnapshot()
        {
            var slot =
                new CombatSlotState(
                    new SlotId(1),
                    CreatePosition(),
                    occupantInstanceId: null,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .ProtectiveSeal);

            Assert.That(
                slot.IsOccupied,
                Is.False);

            Assert.That(
                slot.HasEnhance,
                Is.True);

            Assert.That(
                slot.HasProtectiveSeal,
                Is.True);
        }

        [Test]
        public void Constructor_WithUnsupportedEnhanceKind_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSlotState(
                        new SlotId(1),
                        CreatePosition(),
                        new InstanceId(100),
                        (CombatSlotEnhanceKind)999));
        }

        [Test]
        public void RemoveOccupant_PreservesProtectiveSealSnapshot()
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
                    CombatSlotEnhanceKind
                        .ProtectiveSeal);

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
                    CombatSlotEnhanceKind
                        .ProtectiveSeal));

            Assert.That(
                slot.HasProtectiveSeal,
                Is.True);
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