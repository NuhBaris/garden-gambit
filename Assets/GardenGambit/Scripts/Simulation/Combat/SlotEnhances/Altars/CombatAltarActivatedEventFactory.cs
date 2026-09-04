using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarActivatedEventFactory
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        public CombatAltarActivatedEventFactory(
            CombatEventMetadataFactory metadataFactory)
        {
            if (metadataFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(metadataFactory));
            }

            _metadataFactory =
                metadataFactory;
        }

        public CombatEvent Create(
            CombatStartedCombatEvent
                combatStartedEvent,
            CombatAltarTransferSnapshot snapshot)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(
                    nameof(snapshot));
            }

            if (!combatStartedEvent
                    .Metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            var metadata =
                _metadataFactory.CreateChild(
                    combatStartedEvent.Metadata);

            if (snapshot.IsSacrificialAltar)
            {
                return
                    new
                        SacrificialAltarActivatedCombatEvent(
                            metadata,
                            snapshot.DonorInstanceId,
                            snapshot.DonorPosition,
                            snapshot.RecipientInstanceId,
                            snapshot.RecipientPosition,
                            snapshot.TransferAmount);
            }

            return new WarAltarActivatedCombatEvent(
                metadata,
                snapshot.DonorInstanceId,
                snapshot.DonorPosition,
                snapshot.RecipientInstanceId,
                snapshot.RecipientPosition,
                snapshot.TransferAmount,
                snapshot.DonorPreviousHp);
        }
    }
}