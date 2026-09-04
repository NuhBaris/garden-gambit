using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationContextResolverTests
    {
        [Test]
        public void TryResolve_WithSacrificialAltar_ReturnsContext()
        {
            var environment =
                CreateEnvironment(
                    CombatSide.Player,
                    BoardRow.Front,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Not.Null);

            Assert.That(
                context.IsSacrificialAltar,
                Is.True);

            Assert.That(
                context.IsWarAltar,
                Is.False);

            Assert.That(
                context.DonorCard,
                Is.SameAs(
                    environment.DonorCard));

            Assert.That(
                context.RecipientCard,
                Is.SameAs(
                    environment.RecipientCard));

            Assert.That(
                context.DonorPosition,
                Is.EqualTo(
                    environment.DonorPosition));

            Assert.That(
                context.RecipientPosition,
                Is.EqualTo(
                    environment.RecipientPosition));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void TryResolve_WithWarAltar_ReturnsContext()
        {
            var environment =
                CreateEnvironment(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    CombatSlotEnhanceKind.WarAltar);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Not.Null);

            Assert.That(
                context.IsWarAltar,
                Is.True);

            Assert.That(
                context.IsSacrificialAltar,
                Is.False);

            Assert.That(
                context.DonorCard,
                Is.SameAs(
                    environment.DonorCard));

            Assert.That(
                context.RecipientCard,
                Is.SameAs(
                    environment.RecipientCard));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.DonorCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void TryResolve_WithMissingDonorSlot_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    includeDonorSlot: false,
                    donorOccupied: false);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Null);
        }

        [Test]
        public void TryResolve_WithEmptyDonorSlot_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    donorOccupied: false);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Null);

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void TryResolve_WithNonAltarEnhance_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    donorEnhanceKind:
                        CombatSlotEnhanceKind
                            .ProtectiveSeal);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void TryResolve_WithDeathThresholdDonor_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    donorCurrentHp: 0);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void TryResolve_WithMissingRecipientSlot_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    includeRecipientSlot: false,
                    recipientOccupied: false);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(7));
        }

        [Test]
        public void TryResolve_WithEmptyRecipientSlot_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    recipientOccupied: false);

            var resolver =
                new CombatAltarActivationContextResolver();

            var context =
                resolver.TryResolve(
                    environment.SideState,
                    environment.DonorPosition);

            Assert.That(
                context,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.DonorCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void TryResolve_WithNullSideState_Throws()
        {
            var environment =
                CreateEnvironment();

            var resolver =
                new CombatAltarActivationContextResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.TryResolve(
                    null,
                    environment.DonorPosition));
        }

        [Test]
        public void TryResolve_WithInvalidDonorPosition_Throws()
        {
            var environment =
                CreateEnvironment();

            var resolver =
                new CombatAltarActivationContextResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.TryResolve(
                    environment.SideState,
                    default(BoardPosition)));
        }

        [Test]
        public void TryResolve_WithDifferentSidePosition_Throws()
        {
            var environment =
                CreateEnvironment(
                    side: CombatSide.Player);

            var invalidPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    environment.DonorPosition.Row,
                    environment.DonorPosition.Column);

            var resolver =
                new CombatAltarActivationContextResolver();

            Assert.Throws<ArgumentException>(
                () => resolver.TryResolve(
                    environment.SideState,
                    invalidPosition));
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatSide side =
                    CombatSide.Player,
                BoardRow donorRow =
                    BoardRow.Front,
                CombatSlotEnhanceKind
                    donorEnhanceKind =
                        CombatSlotEnhanceKind
                            .SacrificialAltar,
                bool includeDonorSlot = true,
                bool donorOccupied = true,
                bool includeRecipientSlot = true,
                bool recipientOccupied = true,
                int donorCurrentHp = 7)
        {
            var donorPosition =
                new BoardPosition(
                    side,
                    donorRow,
                    new BoardColumn(3));

            var recipientRow =
                donorRow == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

            var recipientPosition =
                new BoardPosition(
                    side,
                    recipientRow,
                    new BoardColumn(3));

            CombatCardState donorCard = null;
            CombatCardState recipientCard = null;

            var slots =
                new List<CombatSlotState>();

            var cards =
                new List<CombatCardState>();

            if (donorOccupied)
            {
                donorCard =
                    CreateCard(
                        instanceId: 100,
                        currentHp:
                            donorCurrentHp);

                cards.Add(
                    donorCard);
            }

            if (includeDonorSlot)
            {
                slots.Add(
                    new CombatSlotState(
                        new SlotId(1),
                        donorPosition,
                        donorCard == null
                            ? (InstanceId?)null
                            : donorCard.InstanceId,
                        donorEnhanceKind));
            }

            if (recipientOccupied)
            {
                recipientCard =
                    CreateCard(
                        instanceId: 200,
                        currentHp: 5);

                cards.Add(
                    recipientCard);
            }

            if (includeRecipientSlot)
            {
                slots.Add(
                    new CombatSlotState(
                        new SlotId(2),
                        recipientPosition,
                        recipientCard == null
                            ? (InstanceId?)null
                            : recipientCard.InstanceId));
            }

            var sideState =
                new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(
                        cards),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            return new TestEnvironment
            {
                SideState = sideState,
                DonorPosition = donorPosition,
                RecipientPosition =
                    recipientPosition,
                DonorCard = donorCard,
                RecipientCard = recipientCard
            };
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

        private sealed class TestEnvironment
        {
            public CombatSideState SideState
            {
                get;
                set;
            }

            public BoardPosition DonorPosition
            {
                get;
                set;
            }

            public BoardPosition RecipientPosition
            {
                get;
                set;
            }

            public CombatCardState DonorCard
            {
                get;
                set;
            }

            public CombatCardState RecipientCard
            {
                get;
                set;
            }
        }
    }
}