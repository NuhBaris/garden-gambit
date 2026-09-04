using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideResultContributionWarBannerTests
    {
        [Test]
        public void Resolve_WithOneWarBanner_UsesIncreasedMultiplier()
        {
            var sideState =
                CreateSideState(
                    attackMultiplier: 2,
                    new[]
                    {
                        CreateSlot(
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
                            rank: 3,
                            currentHp: 7)
                    });

            var resolver =
                new CombatSideResultContributionResolver();

            var contribution =
                resolver.Resolve(
                    sideState);

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(9));

            Assert.That(
                sideState.AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(2)));
        }

        [Test]
        public void Resolve_WithMultipleWarBanners_AddsAllBonuses()
        {
            var sideState =
                CreateSideState(
                    attackMultiplier: 1,
                    new[]
                    {
                        CreateSlot(
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner),
                        CreateSlot(
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
                            rank: 2,
                            currentHp: 7),
                        CreateCard(
                            instanceId: 101,
                            rank: 4,
                            currentHp: 7)
                    });

            var resolver =
                new CombatSideResultContributionResolver();

            var contribution =
                resolver.Resolve(
                    sideState);

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(18));

            Assert.That(
                sideState.AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(1)));
        }

        [Test]
        public void Resolve_WithEmptyWarBanner_DoesNotIncreaseMultiplier()
        {
            var sideState =
                CreateSideState(
                    attackMultiplier: 2,
                    new[]
                    {
                        CreateSlot(
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: null,
                            CombatSlotEnhanceKind
                                .WarBanner),
                        CreateSlot(
                            slotId: 2,
                            column: 2,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            rank: 3,
                            currentHp: 7)
                    });

            var resolver =
                new CombatSideResultContributionResolver();

            var contribution =
                resolver.Resolve(
                    sideState);

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(6));
        }

        [Test]
        public void Resolve_WithDeadWarBannerOccupant_DoesNotIncreaseMultiplier()
        {
            var sideState =
                CreateSideState(
                    attackMultiplier: 2,
                    new[]
                    {
                        CreateSlot(
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .WarBanner),
                        CreateSlot(
                            slotId: 2,
                            column: 2,
                            occupantInstanceId: 101,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            rank: 4,
                            currentHp: 0),
                        CreateCard(
                            instanceId: 101,
                            rank: 3,
                            currentHp: 7)
                    });

            var resolver =
                new CombatSideResultContributionResolver();

            var contribution =
                resolver.Resolve(
                    sideState);

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(6));
        }

        [Test]
        public void Resolve_WithProtectiveSeal_DoesNotIncreaseMultiplier()
        {
            var sideState =
                CreateSideState(
                    attackMultiplier: 2,
                    new[]
                    {
                        CreateSlot(
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
                            rank: 3,
                            currentHp: 7)
                    });

            var resolver =
                new CombatSideResultContributionResolver();

            var contribution =
                resolver.Resolve(
                    sideState);

            Assert.That(
                contribution.FinalResultContribution,
                Is.EqualTo(6));

            Assert.That(
                sideState.AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(2)));
        }

        private static CombatSideState
            CreateSideState(
                int attackMultiplier,
                CombatSlotState[] slots,
                CombatCardState[] cards)
        {
            return new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    attackMultiplier));
        }

        private static CombatSlotState CreateSlot(
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
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(column)),
                occupant,
                enhanceKind);
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId("test-card"),
                new InstanceId(instanceId),
                new CardRank(rank),
                7,
                currentHp,
                0,
                3);
        }
    }
}