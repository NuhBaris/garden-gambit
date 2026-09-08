using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarActivationExecutionState
    {
        public CombatAltarActivationExecutionState(
            CombatEvent altarEvent,
            CombatAltarTransferApplicationPreview
                transferPreview)
        {
            if (altarEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(altarEvent));
            }

            if (transferPreview == null)
            {
                throw new ArgumentNullException(
                    nameof(transferPreview));
            }

            ValidateMatchingActivation(
                altarEvent,
                transferPreview);

            AltarEvent =
                altarEvent;

            TransferPreview =
                transferPreview;

            Stage =
                CombatAltarActivationExecutionStage
                    .TransferApplied;
        }

        public CombatEvent AltarEvent
        {
            get;
        }

        public CombatAltarTransferApplicationPreview
            TransferPreview
        {
            get;
        }

        public CombatAltarActivationExecutionStage Stage
        {
            get;
            private set;
        }

        public bool IsSacrificialAltar =>
            TransferPreview.IsSacrificialAltar;

        public bool IsWarAltar =>
            TransferPreview.IsWarAltar;

        public bool IsCompleted =>
            Stage ==
            CombatAltarActivationExecutionStage
                .Completed;

        public void MarkTransferTriggersResolved()
        {
            EnsureStage(
                CombatAltarActivationExecutionStage
                    .TransferApplied);

            Stage =
                CombatAltarActivationExecutionStage
                    .TransferTriggersResolved;
        }

        public void MarkDonorDeathStarted()
        {
            EnsureStage(
                CombatAltarActivationExecutionStage
                    .TransferTriggersResolved);

            Stage =
                CombatAltarActivationExecutionStage
                    .DonorDeathStarted;
        }

        public void MarkCompleted()
        {
            EnsureStage(
                CombatAltarActivationExecutionStage
                    .DonorDeathStarted);

            Stage =
                CombatAltarActivationExecutionStage
                    .Completed;
        }

        private void EnsureStage(
            CombatAltarActivationExecutionStage
                expectedStage)
        {
            if (Stage == expectedStage)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Altar activation execution is at " +
                $"{Stage}, but {expectedStage} was " +
                $"required.");
        }

        private static void
            ValidateMatchingActivation(
                CombatEvent altarEvent,
                CombatAltarTransferApplicationPreview
                    transferPreview)
        {
            var sacrificialEvent =
                altarEvent as
                    SacrificialAltarActivatedCombatEvent;

            if (transferPreview.IsSacrificialAltar)
            {
                if (sacrificialEvent == null)
                {
                    throw new ArgumentException(
                        "Sacrificial Altar transfer preview " +
                        "requires a Sacrificial Altar " +
                        "activation event.",
                        nameof(altarEvent));
                }

                ValidateCommonValues(
                    sacrificialEvent.DonorInstanceId,
                    sacrificialEvent.DonorPosition,
                    sacrificialEvent.RecipientInstanceId,
                    sacrificialEvent.RecipientPosition,
                    transferPreview,
                    altarEvent);

                if (sacrificialEvent.TransferredHp !=
                    transferPreview.TransferAmount)
                {
                    throw new ArgumentException(
                        "Sacrificial Altar event transfer " +
                        "amount does not match the transfer " +
                        "preview.",
                        nameof(altarEvent));
                }

                return;
            }

            var warEvent =
                altarEvent as
                    WarAltarActivatedCombatEvent;

            if (warEvent == null)
            {
                throw new ArgumentException(
                    "War Altar transfer preview requires " +
                    "a War Altar activation event.",
                    nameof(altarEvent));
            }

            ValidateCommonValues(
                warEvent.DonorInstanceId,
                warEvent.DonorPosition,
                warEvent.RecipientInstanceId,
                warEvent.RecipientPosition,
                transferPreview,
                altarEvent);

            if (warEvent.TransferredAttack !=
                transferPreview.TransferAmount)
            {
                throw new ArgumentException(
                    "War Altar event transfer amount does " +
                    "not match the transfer preview.",
                    nameof(altarEvent));
            }

            if (warEvent.DonorPreviousHp !=
                transferPreview.DonorPreviousHp)
            {
                throw new ArgumentException(
                    "War Altar event donor HP does not " +
                    "match the transfer preview.",
                    nameof(altarEvent));
            }
        }

        private static void ValidateCommonValues(
            GardenGambit.Domain.Identity.InstanceId
                donorInstanceId,
            BoardPosition donorPosition,
            GardenGambit.Domain.Identity.InstanceId
                recipientInstanceId,
            BoardPosition recipientPosition,
            CombatAltarTransferApplicationPreview
                transferPreview,
            CombatEvent altarEvent)
        {
            if (donorInstanceId !=
                    transferPreview.DonorInstanceId ||
                donorPosition !=
                    transferPreview.DonorPosition ||
                recipientInstanceId !=
                    transferPreview.RecipientInstanceId ||
                recipientPosition !=
                    transferPreview.RecipientPosition)
            {
                throw new ArgumentException(
                    "Altar activation event does not match " +
                    "the transfer preview.",
                    nameof(altarEvent));
            }
        }
    }
}