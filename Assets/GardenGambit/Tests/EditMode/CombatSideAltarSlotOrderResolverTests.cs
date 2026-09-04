using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideAltarSlotOrderResolverTests
    {
        [Test]
        public void Resolve_WithNullSideState_Throws()
        {
            var resolver =
                new CombatSideAltarSlotOrderResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(null));
        }

        [Test]
        public void Resolve_WithUnorderedAltars_OrdersLeftToRightAndFrontBeforeBack()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 3,
                        CombatSlotEnhanceKind
                            .WarAltar),
                    CreateSlot(
                        2,
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 1,
                        CombatSlotEnhanceKind
                            .SacrificialAltar),
                    CreateSlot(
                        3,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 2,
                        CombatSlotEnhanceKind
                            .WarAltar),
                    CreateSlot(
                        4,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 1,
                        CombatSlotEnhanceKind
                            .SacrificialAltar),
                    CreateSlot(
                        5,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 5,
                        CombatSlotEnhanceKind
                            .WarAltar));

            var resolver =
                new CombatSideAltarSlotOrderResolver();

            var positions =
                resolver.Resolve(
                    sideState);

            Assert.That(
                positions.Count,
                Is.EqualTo(5));

            AssertPosition(
                positions[0],
                CombatSide.Player,
                BoardRow.Front,
                column: 1);

            AssertPosition(
                positions[1],
                CombatSide.Player,
                BoardRow.Back,
                column: 1);

            AssertPosition(
                positions[2],
                CombatSide.Player,
                BoardRow.Front,
                column: 2);

            AssertPosition(
                positions[3],
                CombatSide.Player,
                BoardRow.Back,
                column: 3);

            AssertPosition(
                positions[4],
                CombatSide.Player,
                BoardRow.Front,
                column: 5);
        }

        [Test]
        public void Resolve_WithNonAltarEnhances_ExcludesThem()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 1,
                        CombatSlotEnhanceKind.None),
                    CreateSlot(
                        2,
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 1,
                        CombatSlotEnhanceKind
                            .ProtectiveSeal),
                    CreateSlot(
                        3,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 2,
                        CombatSlotEnhanceKind
                            .WarBanner),
                    CreateSlot(
                        4,
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 4,
                        CombatSlotEnhanceKind
                            .SacrificialAltar),
                    CreateSlot(
                        5,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 5,
                        CombatSlotEnhanceKind
                            .WarAltar));

            var positions =
                new CombatSideAltarSlotOrderResolver()
                    .Resolve(sideState);

            Assert.That(
                positions.Count,
                Is.EqualTo(2));

            AssertPosition(
                positions[0],
                CombatSide.Player,
                BoardRow.Back,
                column: 4);

            AssertPosition(
                positions[1],
                CombatSide.Player,
                BoardRow.Front,
                column: 5);
        }

        [Test]
        public void Resolve_WithEmptyAltarSlots_IncludesTheirPositions()
        {
            var frontPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            var backPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(2));

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new CombatSlotState(
                        new SlotId(1),
                        frontPosition,
                        null,
                        CombatSlotEnhanceKind
                            .SacrificialAltar),
                    new CombatSlotState(
                        new SlotId(2),
                        backPosition,
                        null,
                        CombatSlotEnhanceKind
                            .WarAltar));

            var positions =
                new CombatSideAltarSlotOrderResolver()
                    .Resolve(sideState);

            Assert.That(
                positions.Count,
                Is.EqualTo(2));

            Assert.That(
                positions[0],
                Is.EqualTo(frontPosition));

            Assert.That(
                positions[1],
                Is.EqualTo(backPosition));
        }

        [Test]
        public void Resolve_WithEnemySide_PreservesEnemyPositionsAndOrdering()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Enemy,
                    CreateSlot(
                        1,
                        CombatSide.Enemy,
                        BoardRow.Back,
                        column: 4,
                        CombatSlotEnhanceKind
                            .WarAltar),
                    CreateSlot(
                        2,
                        CombatSide.Enemy,
                        BoardRow.Back,
                        column: 2,
                        CombatSlotEnhanceKind
                            .SacrificialAltar),
                    CreateSlot(
                        3,
                        CombatSide.Enemy,
                        BoardRow.Front,
                        column: 2,
                        CombatSlotEnhanceKind
                            .WarAltar));

            var positions =
                new CombatSideAltarSlotOrderResolver()
                    .Resolve(sideState);

            Assert.That(
                positions.Count,
                Is.EqualTo(3));

            AssertPosition(
                positions[0],
                CombatSide.Enemy,
                BoardRow.Front,
                column: 2);

            AssertPosition(
                positions[1],
                CombatSide.Enemy,
                BoardRow.Back,
                column: 2);

            AssertPosition(
                positions[2],
                CombatSide.Enemy,
                BoardRow.Back,
                column: 4);
        }

        [Test]
        public void Resolve_WithoutAltars_ReturnsEmptyList()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 1,
                        CombatSlotEnhanceKind.None),
                    CreateSlot(
                        2,
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 1,
                        CombatSlotEnhanceKind
                            .ProtectiveSeal),
                    CreateSlot(
                        3,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 2,
                        CombatSlotEnhanceKind
                            .WarBanner));

            var positions =
                new CombatSideAltarSlotOrderResolver()
                    .Resolve(sideState);

            Assert.That(
                positions,
                Is.Not.Null);

            Assert.That(
                positions.Count,
                Is.Zero);
        }

        [Test]
        public void Resolve_ReturnsReadOnlyList()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 1,
                        CombatSlotEnhanceKind
                            .SacrificialAltar));

            var positions =
                new CombatSideAltarSlotOrderResolver()
                    .Resolve(sideState);

            var mutablePositions =
                positions as
                    IList<BoardPosition>;

            Assert.That(
                mutablePositions,
                Is.Not.Null);

            Assert.Throws<NotSupportedException>(
                () => mutablePositions.Add(
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        new BoardColumn(1))));
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                params CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSlotState CreateSlot(
            int slotId,
            CombatSide side,
            BoardRow row,
            int column,
            CombatSlotEnhanceKind enhanceKind)
        {
            return new CombatSlotState(
                new SlotId(slotId),
                new BoardPosition(
                    side,
                    row,
                    new BoardColumn(column)),
                null,
                enhanceKind);
        }

        private static void AssertPosition(
            BoardPosition position,
            CombatSide expectedSide,
            BoardRow expectedRow,
            int column)
        {
            Assert.That(
                position.Side,
                Is.EqualTo(expectedSide));

            Assert.That(
                position.Row,
                Is.EqualTo(expectedRow));

            Assert.That(
                position.Column,
                Is.EqualTo(
                    new BoardColumn(column)));
        }
    }
}