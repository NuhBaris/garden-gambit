using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class WarBannerCountResolverTests
    {
        [Test]
        public void Resolve_WithLiveCardInWarBannerSlot_ReturnsOne()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            currentHp: 7)
                    });

            var state =
                CreateState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            var count =
                resolver.Resolve(
                    state,
                    CombatSide.Player);

            Assert.That(
                count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithMultipleLiveWarBannerSlots_ReturnsCount()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner),
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 2,
                            column: 2,
                            occupantInstanceId: 101,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            currentHp: 7),
                        CreateCard(
                            instanceId: 101,
                            currentHp: 3)
                    });

            var state =
                CreateState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            var count =
                resolver.Resolve(
                    state,
                    CombatSide.Player);

            Assert.That(
                count,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WithEmptyWarBannerSlot_DoesNotCount()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: null,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new CombatCardState[0]);

            var state =
                CreateState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            var count =
                resolver.Resolve(
                    state,
                    CombatSide.Player);

            Assert.That(
                count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithCardAtZeroHp_DoesNotCount()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            currentHp: 0)
                    });

            var state =
                CreateState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            var count =
                resolver.Resolve(
                    state,
                    CombatSide.Player);

            Assert.That(
                count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithCardBelowZeroHp_DoesNotCount()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            currentHp: -3)
                    });

            var state =
                CreateState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            var count =
                resolver.Resolve(
                    state,
                    CombatSide.Player);

            Assert.That(
                count,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithProtectiveSeal_DoesNotCount()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .ProtectiveSeal)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            currentHp: 7)
                    });

            var state =
                CreateState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            var count =
                resolver.Resolve(
                    state,
                    CombatSide.Player);

            Assert.That(
                count,
                Is.Zero);
        }

        [Test]
        public void Resolve_ForEnemySide_CountsOnlyEnemyBanners()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner),
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 2,
                            column: 2,
                            occupantInstanceId: 101,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new[]
                    {
                        CreateCard(100, 7),
                        CreateCard(101, 7)
                    });

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Enemy,
                            slotId: 3,
                            column: 1,
                            occupantInstanceId: 200,
                            CombatSlotEnhanceKind
                                .WarBanner)
                    },
                    new[]
                    {
                        CreateCard(200, 7)
                    });

            var state =
                CreateState(
                    playerSide,
                    enemySide);

            var resolver =
                new WarBannerCountResolver();

            var enemyCount =
                resolver.Resolve(
                    state,
                    CombatSide.Enemy);

            Assert.That(
                enemyCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var resolver =
                new WarBannerCountResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(
                    null,
                    CombatSide.Player));
        }

        [Test]
        public void Resolve_WithInvalidSide_Throws()
        {
            var state =
                CreateState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy));

            var resolver =
                new WarBannerCountResolver();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => resolver.Resolve(
                    state,
                    default(CombatSide)));
        }

        private static CombatState CreateState(
            CombatSideState playerSide,
            CombatSideState enemySide)
        {
            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return CreateSideState(
                side,
                new CombatSlotState[0],
                new CombatCardState[0]);
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                CombatSlotState[] slots,
                CombatCardState[] cards)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSlotState CreateSlot(
            CombatSide side,
            int slotId,
            int column,
            long? occupantInstanceId,
            CombatSlotEnhanceKind enhanceKind)
        {
            InstanceId? occupant = null;

            if (occupantInstanceId.HasValue)
            {
                occupant =
                    new InstanceId(
                        occupantInstanceId.Value);
            }

            return new CombatSlotState(
                new SlotId(slotId),
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(column)),
                occupant,
                enhanceKind);
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId("test-card"),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                currentHp,
                0,
                3);
        }
    }
}