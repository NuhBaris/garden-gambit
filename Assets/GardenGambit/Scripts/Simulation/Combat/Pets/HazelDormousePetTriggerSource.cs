using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        HazelDormousePetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetNormalAttackTriggerSource
            _normalAttackTriggerSource;

        public HazelDormousePetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
        {
            Handler =
                new HazelDormousePetNormalAttackTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    targetDamageReductionRegistry);

            _normalAttackTriggerSource =
                new CombatPetNormalAttackTriggerSource(
                    Handler);
        }

        public HazelDormousePetNormalAttackTriggerHandler
            Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                Handler.UsageCommitter;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                Handler.TargetDamageReductionRegistry;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _normalAttackTriggerSource
                    .OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _normalAttackTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}
