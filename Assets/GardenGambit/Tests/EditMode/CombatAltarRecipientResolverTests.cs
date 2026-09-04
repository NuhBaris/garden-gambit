using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarRecipientResolverTests
    {
        [Test]
        public void TryResolve_PlayerFrontDonor_ReturnsBackRecipient()
        {
            var donorCard =
                CreateCard(
                    instanceId: 100,
                    currentHp: 7);

            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 5);

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Front,
                            slotId: 1,
                            occupantInstanceId: 100),
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Back,
                            slotId: 2,
                            occupantInstanceId: 200)
                    },
                    new[]
                    {
                        donorCard,
                        recipientCard
                    });

            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front);

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    donorPosition);

            Assert.That(
                recipient,
                Is.Not.Null);

            Assert.That(
                recipient.Position,
                Is.EqualTo(
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Back)));

            Assert.That(
                recipient.Card,
                Is.SameAs(
                    recipientCard));

            Assert.That(
                recipient.InstanceId,
                Is.EqualTo(
                    new InstanceId(200)));
        }

        [Test]
        public void TryResolve_EnemyBackDonor_ReturnsFrontRecipient()
        {
            var donorCard =
                CreateCard(
                    instanceId: 100,
                    currentHp: 7);

            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 5);

            var sideState =
                CreateSideState(
                    CombatSide.Enemy,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            slotId: 1,
                            occupantInstanceId: 200),
                        CreateSlot(
                            CombatSide.Enemy,
                            BoardRow.Back,
                            slotId: 2,
                            occupantInstanceId: 100)
                    },
                    new[]
                    {
                        donorCard,
                        recipientCard
                    });

            var donorPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Back);

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    donorPosition);

            Assert.That(
                recipient,
                Is.Not.Null);

            Assert.That(
                recipient.Position,
                Is.EqualTo(
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Front)));

            Assert.That(
                recipient.Card,
                Is.SameAs(
                    recipientCard));
        }

        [Test]
        public void TryResolve_WithEmptyDonorSlot_ReturnsNull()
        {
            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 5);

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Front,
                            slotId: 1,
                            occupantInstanceId: null),
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Back,
                            slotId: 2,
                            occupantInstanceId: 200)
                    },
                    new[]
                    {
                        recipientCard
                    });

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front));

            Assert.That(
                recipient,
                Is.Null);
        }

        [Test]
        public void TryResolve_WithMissingDonorSlot_ReturnsNull()
        {
            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 5);

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Back,
                            slotId: 2,
                            occupantInstanceId: 200)
                    },
                    new[]
                    {
                        recipientCard
                    });

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front));

            Assert.That(
                recipient,
                Is.Null);
        }

        [Test]
        public void TryResolve_WithEmptyRecipientSlot_ReturnsNull()
        {
            var donorCard =
                CreateCard(
                    instanceId: 100,
                    currentHp: 7);

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Front,
                            slotId: 1,
                            occupantInstanceId: 100),
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Back,
                            slotId: 2,
                            occupantInstanceId: null)
                    },
                    new[]
                    {
                        donorCard
                    });

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front));

            Assert.That(
                recipient,
                Is.Null);
        }

        [Test]
        public void TryResolve_WithMissingRecipientSlot_ReturnsNull()
        {
            var donorCard =
                CreateCard(
                    instanceId: 100,
                    currentHp: 7);

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Front,
                            slotId: 1,
                            occupantInstanceId: 100)
                    },
                    new[]
                    {
                        donorCard
                    });

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front));

            Assert.That(
                recipient,
                Is.Null);
        }

        [Test]
        public void TryResolve_WithDeathThresholdRecipient_ReturnsRecipient()
        {
            var donorCard =
                CreateCard(
                    instanceId: 100,
                    currentHp: 7);

            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 0);

            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new[]
                    {
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Front,
                            slotId: 1,
                            occupantInstanceId: 100),
                        CreateSlot(
                            CombatSide.Player,
                            BoardRow.Back,
                            slotId: 2,
                            occupantInstanceId: 200)
                    },
                    new[]
                    {
                        donorCard,
                        recipientCard
                    });

            var resolver =
                new CombatAltarRecipientResolver();

            var recipient =
                resolver.TryResolve(
                    sideState,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front));

            Assert.That(
                recipient,
                Is.Not.Null);

            Assert.That(
                recipient.Card,
                Is.SameAs(
                    recipientCard));

            Assert.That(
                recipient.Card.IsAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void TryResolve_WithNullSideState_Throws()
        {
            var resolver =
                new CombatAltarRecipientResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.TryResolve(
                    null,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front)));
        }

        [Test]
        public void TryResolve_WithInvalidDonorPosition_Throws()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new CombatSlotState[0],
                    new CombatCardState[0]);

            var resolver =
                new CombatAltarRecipientResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.TryResolve(
                    sideState,
                    default(BoardPosition)));
        }

        [Test]
        public void TryResolve_WithDifferentSidePosition_Throws()
        {
            var sideState =
                CreateSideState(
                    CombatSide.Player,
                    new CombatSlotState[0],
                    new CombatCardState[0]);

            var resolver =
                new CombatAltarRecipientResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.TryResolve(
                    sideState,
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Front)));
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
            BoardRow row,
            int slotId,
            long? occupantInstanceId)
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
                CreatePosition(
                    side,
                    row),
                occupant);
        }

        private static BoardPosition CreatePosition(
            CombatSide side,
            BoardRow row)
        {
            return new BoardPosition(
                side,
                row,
                new BoardColumn(3));
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