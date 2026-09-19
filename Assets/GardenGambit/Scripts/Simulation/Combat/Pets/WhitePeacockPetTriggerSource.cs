using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class WhitePeacockPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetBattleStartTriggerSource
            _battleStartSource;

        public WhitePeacockPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver)
        {
            Handler =
                new WhitePeacockPetBattleStartTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    hpGainResolver,
                    armorGainResolver,
                    attackGainResolver);

            _battleStartSource =
                new CombatPetBattleStartTriggerSource(
                    Handler);
        }

        public WhitePeacockPetBattleStartTriggerHandler
            Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                Handler.UsageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            Handler.HpGainResolver;

        public CombatArmorGainResolver ArmorGainResolver =>
            Handler.ArmorGainResolver;

        public CombatAttackGainResolver AttackGainResolver =>
            Handler.AttackGainResolver;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _battleStartSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _battleStartSource.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}
