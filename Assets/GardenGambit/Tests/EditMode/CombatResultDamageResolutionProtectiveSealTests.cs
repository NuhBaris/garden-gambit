using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultDamageResolutionProtectiveSealTests
    {
        [Test]
        public void Resolve_PlayerSeal_ReducesDamageToPlayer()
        {
            var state =
                CreateState(
                    CreateSingleCardSide(
                        CombatSide.Player,
                        instanceId: 100,
                        rank: 2,
                        attackMultiplier: 1,
                        CombatSlotEnhanceKind
                            .ProtectiveSeal),
                    CreateSingleCardSide(
                        CombatSide.Enemy,
                        instanceId: 200,
                        rank: 10,
                        attackMultiplier: 2,
                        CombatSlotEnhanceKind.None));

            var calculation =
                CreateCalculation(
                    state);

            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    state,
                    calculation);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(19));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.EqualTo(1L));

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.EqualTo(2));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(2));

            Assert.That(
                resolution.PreventedDamageForEnemy,
                Is.Zero);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.True);
        }

        [Test]
        public void Resolve_EnemySeal_ReducesDamageToEnemy()
        {
            var state =
                CreateState(
                    CreateSingleCardSide(
                        CombatSide.Player,
                        instanceId: 100,
                        rank: 10,
                        attackMultiplier: 2,
                        CombatSlotEnhanceKind.None),
                    CreateSingleCardSide(
                        CombatSide.Enemy,
                        instanceId: 200,
                        rank: 2,
                        attackMultiplier: 1,
                        CombatSlotEnhanceKind
                            .ProtectiveSeal));

            var calculation =
                CreateCalculation(
                    state);

            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    state,
                    calculation);

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(19));

            Assert.That(
                resolution.PreventedDamageForEnemy,
                Is.EqualTo(1L));

            Assert.That(
                resolution.EnemyDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithSealsOnBothSides_AppliesEachToOwnIncomingDamage()
        {
            var state =
                CreateState(
                    CreateSingleCardSide(
                        CombatSide.Player,
                        instanceId: 100,
                        rank: 10,
                        attackMultiplier: 2,
                        CombatSlotEnhanceKind
                            .ProtectiveSeal),
                    CreateSingleCardSide(
                        CombatSide.Enemy,
                        instanceId: 200,
                        rank: 10,
                        attackMultiplier: 2,
                        CombatSlotEnhanceKind
                            .ProtectiveSeal));

            var calculation =
                CreateCalculation(
                    state);

            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    state,
                    calculation);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(19));

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(19));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.EqualTo(1L));

            Assert.That(
                resolution.PreventedDamageForEnemy,
                Is.EqualTo(1L));

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.True);
        }

        [Test]
        public void Resolve_WithTwoPlayerSeals_AppliesSequentialRounding()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    attackMultiplier: 1,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: 100,
                            CombatSlotEnhanceKind
                                .ProtectiveSeal),
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 2,
                            column: 2,
                            occupantInstanceId: 101,
                            CombatSlotEnhanceKind
                                .ProtectiveSeal)
                    },
                    new[]
                    {
                        CreateCard(
                            instanceId: 100,
                            rank: 2,
                            currentHp: 7),
                        CreateCard(
                            instanceId: 101,
                            rank: 2,
                            currentHp: 7)
                    });

            var enemySide =
                CreateSingleCardSide(
                    CombatSide.Enemy,
                    instanceId: 200,
                    rank: 10,
                    attackMultiplier: 10,
                    CombatSlotEnhanceKind.None);

            var state =
                CreateState(
                    playerSide,
                    enemySide);

            var calculation =
                CreateCalculation(
                    state);

            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    state,
                    calculation);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(100));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(91));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.EqualTo(9L));

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.EqualTo(-9L));
        }

        [Test]
        public void Resolve_WithEmptySeal_DoesNotReduceDamage()
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    attackMultiplier: 1,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            slotId: 1,
                            column: 1,
                            occupantInstanceId: null,
                            CombatSlotEnhanceKind
                                .ProtectiveSeal)
                    },
                    new CombatCardState[0]);

            var enemySide =
                CreateSingleCardSide(
                    CombatSide.Enemy,
                    instanceId: 200,
                    rank: 10,
                    attackMultiplier: 2,
                    CombatSlotEnhanceKind.None);

            var state =
                CreateState(
                    playerSide,
                    enemySide);

            var calculation =
                CreateCalculation(
                    state);

            var resolver =
                new CombatResultDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    state,
                    calculation);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.Zero);

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.Zero);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void Resolve_WithProtectiveSeal_DoesNotMutateState()
        {
            var playerSide =
                CreateSingleCardSide(
                    CombatSide.Player,
                    instanceId: 100,
                    rank: 2,
                    attackMultiplier: 1,
                    CombatSlotEnhanceKind
                        .ProtectiveSeal);

            var enemySide =
                CreateSingleCardSide(
                    CombatSide.Enemy,
                    instanceId: 200,
                    rank: 10,
                    attackMultiplier: 2,
                    CombatSlotEnhanceKind.None);

            var state =
                CreateState(
                    playerSide,
                    enemySide);

            var playerCard =
                playerSide.Cards.GetCard(
                    new InstanceId(100));

            var enemyCard =
                enemySide.Cards.GetCard(
                    new InstanceId(200));

            var playerSlot =
                playerSide.Board.GetSlot(
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)));

            var calculation =
                CreateCalculation(
                    state);

            var resolver =
                new CombatResultDamageResolutionResolver();

            resolver.Resolve(
                state,
                calculation);

            Assert.That(
                playerCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                playerSide.BattleHealth,
                Is.EqualTo(
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue)));

            Assert.That(
                enemySide.BattleHealth,
                Is.EqualTo(
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue)));

            Assert.That(
                playerSlot.IsOccupied,
                Is.True);

            Assert.That(
                playerSlot.OccupantInstanceId.Value,
                Is.EqualTo(
                    new InstanceId(100)));

            Assert.That(
                playerSlot.HasProtectiveSeal,
                Is.True);
        }

        private static CombatResultDamageCalculation
            CreateCalculation(
                CombatState state)
        {
            var resolver =
                new CombatResultDamageResolver();

            return resolver.Resolve(
                state);
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
            CreateSingleCardSide(
                CombatSide side,
                long instanceId,
                int rank,
                int attackMultiplier,
                CombatSlotEnhanceKind enhanceKind)
        {
            return CreateSideState(
                side,
                attackMultiplier,
                new[]
                {
                    CreateSlot(
                        side,
                        slotId:
                            side == CombatSide.Player
                                ? 1
                                : 101,
                        column: 1,
                        occupantInstanceId:
                            instanceId,
                        enhanceKind:
                            enhanceKind)
                },
                new[]
                {
                    CreateCard(
                        instanceId,
                        rank,
                        currentHp: 7)
                });
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                int attackMultiplier,
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
                    attackMultiplier));
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