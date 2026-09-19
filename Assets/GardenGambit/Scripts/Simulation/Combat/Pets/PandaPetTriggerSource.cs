using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class PandaPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetBattleStartTriggerSource
            _battleStartTriggerSource;

        private readonly CombatTriggerSourceRegistry
            _sources;

        public PandaPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter
                activationUsageCommitter,
            CombatPetLimitedTriggerUsageCommitter
                limitedUsageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
        {
            if (activationUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(activationUsageCommitter));
            }

            if (limitedUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(limitedUsageCommitter));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageReductionRegistry));
            }

            BattleStartHandler =
                new PandaPetBattleStartTriggerHandler(
                    side,
                    petInstanceId,
                    activationUsageCommitter);
            NormalAttackHandler =
                new PandaPetNormalAttackTriggerHandler(
                    side,
                    petInstanceId,
                    activationUsageCommitter,
                    limitedUsageCommitter,
                    targetDamageReductionRegistry);

            _battleStartTriggerSource =
                new CombatPetBattleStartTriggerSource(
                    BattleStartHandler);

            _sources = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[]
                {
                    _battleStartTriggerSource,
                    new CombatPetNormalAttackTriggerSource(
                        NormalAttackHandler)
                });
        }

        public PandaPetBattleStartTriggerHandler
            BattleStartHandler
        {
            get;
        }

        public PandaPetNormalAttackTriggerHandler
            NormalAttackHandler
        {
            get;
        }

        public CombatSide Side =>
            BattleStartHandler.Side;

        public InstanceId PetInstanceId =>
            BattleStartHandler.PetInstanceId;

        public CombatPetTriggerUsageCommitter
            ActivationUsageCommitter =>
                BattleStartHandler
                    .ActivationUsageCommitter;

        public CombatPetLimitedTriggerUsageCommitter
            LimitedUsageCommitter =>
                NormalAttackHandler
                    .LimitedUsageCommitter;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                NormalAttackHandler
                    .TargetDamageReductionRegistry;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _battleStartTriggerSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _sources.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}
