using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatPetState
    {
        public CombatPetState(
            DefinitionId definitionId,
            InstanceId instanceId)
        {
            if (!definitionId.IsValid)
            {
                throw new ArgumentException(
                    "Combat Pet requires a valid " +
                    "DefinitionId.",
                    nameof(definitionId));
            }

            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Combat Pet requires a valid " +
                    "InstanceId.",
                    nameof(instanceId));
            }

            DefinitionId =
                definitionId;

            InstanceId =
                instanceId;
        }

        public DefinitionId DefinitionId
        {
            get;
        }

        public InstanceId InstanceId
        {
            get;
        }
    }
}