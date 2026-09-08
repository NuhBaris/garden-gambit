using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationExecutionStateTests
    {
        [Test]
        public void
            Constructor_WithNullAltarEvent_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionState(
                            null,
                            data.Preview));
        }

        [Test]
        public void
            Constructor_WithNullTransferPreview_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionState(
                            CreateMatchingEvent(data),
                            null));
        }

        [Test]
        public void
            Constructor_WithSacrificialAltar_ExposesInitialState()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                CreateMatchingEvent(
                    data);

            var state =
                new
                    CombatAltarActivationExecutionState(
                        altarEvent,
                        data.Preview);

            Assert.That(
                state.AltarEvent,
                Is.SameAs(
                    altarEvent));

            Assert.That(
                state.TransferPreview,
                Is.SameAs(
                    data.Preview));

            Assert.That(
                state.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                state.IsSacrificialAltar,
                Is.True);

            Assert.That(
                state.IsWarAltar,
                Is.False);

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            Constructor_WithWarAltar_ExposesInitialState()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var state =
                new
                    CombatAltarActivationExecutionState(
                        CreateMatchingEvent(data),
                        data.Preview);

            Assert.That(
                state.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                state.IsSacrificialAltar,
                Is.False);

            Assert.That(
                state.IsWarAltar,
                Is.True);

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            Constructor_WithSacrificialPreviewAndWarEvent_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var warEvent =
                new WarAltarActivatedCombatEvent(
                    CreateMetadata(),
                    data.Preview.DonorInstanceId,
                    data.Preview.DonorPosition,
                    data.Preview.RecipientInstanceId,
                    data.Preview.RecipientPosition,
                    transferredAttack: 6,
                    data.Preview.DonorPreviousHp);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionState(
                            warEvent,
                            data.Preview));
        }

        [Test]
        public void
            Constructor_WithWarPreviewAndSacrificialEvent_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var sacrificialEvent =
                new
                    SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        data.Preview.DonorInstanceId,
                        data.Preview.DonorPosition,
                        data.Preview.RecipientInstanceId,
                        data.Preview.RecipientPosition,
                        transferredHp: 4);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionState(
                            sacrificialEvent,
                            data.Preview));
        }

        [Test]
        public void
            Constructor_WithMismatchedDonor_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                new
                    SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(999),
                        data.Preview.DonorPosition,
                        data.Preview.RecipientInstanceId,
                        data.Preview.RecipientPosition,
                        data.Preview.TransferAmount);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionState(
                            altarEvent,
                            data.Preview));
        }

        [Test]
        public void
            Constructor_WithMismatchedTransferAmount_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                new
                    SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        data.Preview.DonorInstanceId,
                        data.Preview.DonorPosition,
                        data.Preview.RecipientInstanceId,
                        data.Preview.RecipientPosition,
                        data.Preview.TransferAmount + 1);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionState(
                            altarEvent,
                            data.Preview));
        }

        [Test]
        public void
            MarkTransferTriggersResolved_AdvancesStage()
        {
            var state =
                CreateExecutionState();

            state.MarkTransferTriggersResolved();

            Assert.That(
                state.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferTriggersResolved));

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            MarkDonorDeathStarted_BeforeTransferTriggersResolved_Throws()
        {
            var state =
                CreateExecutionState();

            Assert.Throws<InvalidOperationException>(
                () => state.MarkDonorDeathStarted());

            Assert.That(
                state.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));
        }

        [Test]
        public void
            MarkCompleted_BeforeDonorDeathStarted_Throws()
        {
            var state =
                CreateExecutionState();

            Assert.Throws<InvalidOperationException>(
                () => state.MarkCompleted());

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            OrderedTransitions_MarkExecutionCompleted()
        {
            var state =
                CreateExecutionState();

            state.MarkTransferTriggersResolved();
            state.MarkDonorDeathStarted();
            state.MarkCompleted();

            Assert.That(
                state.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .Completed));

            Assert.That(
                state.IsCompleted,
                Is.True);

            Assert.Throws<InvalidOperationException>(
                () =>
                    state
                        .MarkTransferTriggersResolved());

            Assert.Throws<InvalidOperationException>(
                () =>
                    state.MarkDonorDeathStarted());

            Assert.Throws<InvalidOperationException>(
                () => state.MarkCompleted());
        }

        private static
            CombatAltarActivationExecutionState
            CreateExecutionState()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            return new
                CombatAltarActivationExecutionState(
                    CreateMatchingEvent(data),
                    data.Preview);
        }

        private static CombatEvent
            CreateMatchingEvent(
                TestData data)
        {
            if (data.Preview.IsSacrificialAltar)
            {
                return new
                    SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        data.Preview.DonorInstanceId,
                        data.Preview.DonorPosition,
                        data.Preview.RecipientInstanceId,
                        data.Preview.RecipientPosition,
                        data.Preview.TransferAmount);
            }

            return new WarAltarActivatedCombatEvent(
                CreateMetadata(),
                data.Preview.DonorInstanceId,
                data.Preview.DonorPosition,
                data.Preview.RecipientInstanceId,
                data.Preview.RecipientPosition,
                data.Preview.TransferAmount,
                data.Preview.DonorPreviousHp);
        }

        private static TestData CreateData(
            CombatSlotEnhanceKind altarKind)
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var donorCard =
                CreateCard(
                    instanceId: 1,
                    hpCapacity: 10,
                    currentHp: 4,
                    attack: 6);

            var recipientCard =
                CreateCard(
                    instanceId: 2,
                    hpCapacity: 10,
                    currentHp: 5,
                    attack: 3);

            var recipient =
                new CombatAltarRecipient(
                    recipientPosition,
                    recipientCard);

            var context =
                new CombatAltarActivationContext(
                    altarKind,
                    donorPosition,
                    donorCard,
                    recipient);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            return new TestData
            {
                Preview =
                    new
                        CombatAltarTransferApplicationPreview(
                            snapshot)
            };
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int hpCapacity,
            int currentHp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(2),
                hpCapacity,
                currentHp,
                armor: 0,
                attack: attack);
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }

        private sealed class TestData
        {
            public CombatAltarTransferApplicationPreview
                Preview
            {
                get;
                set;
            }
        }
    }
}