using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        OtterPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetNormalAttackTriggerSource
            _normalAttackTriggerSource;

        public OtterPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver)
        {
            Handler =
                new OtterPetNormalAttackTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    attackGainResolver);

            _normalAttackTriggerSource =
                new CombatPetNormalAttackTriggerSource(
                    Handler);
        }

        public OtterPetNormalAttackTriggerHandler
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

        public CombatAttackGainResolver
            AttackGainResolver =>
                Handler.AttackGainResolver;

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
