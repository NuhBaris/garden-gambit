using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class WhitePeacockPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        private readonly CombatArmorGainResolver
            _armorGainResolver;

        private readonly CombatAttackGainResolver
            _attackGainResolver;

        public WhitePeacockPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "White Peacock factory requires a " +
                    "valid Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (hpGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(hpGainResolver));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(attackGainResolver));
            }

            PetDefinitionId = petDefinitionId;
            _usageCommitter = usageCommitter;
            _hpGainResolver = hpGainResolver;
            _armorGainResolver = armorGainResolver;
            _attackGainResolver = attackGainResolver;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public CombatArmorGainResolver ArmorGainResolver =>
            _armorGainResolver;

        public CombatAttackGainResolver AttackGainResolver =>
            _attackGainResolver;

        public IEnumerable<ICombatTriggerSource>
            CreateSources(
                CombatSide side,
                CombatPetState pet)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "White Peacock source factory " +
                    "requires Player or Enemy side.");
            }

            if (pet == null)
            {
                throw new ArgumentNullException(
                    nameof(pet));
            }

            if (pet.DefinitionId != PetDefinitionId)
            {
                throw new ArgumentException(
                    "Pet DefinitionId does not match " +
                    "the White Peacock factory " +
                    "registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new WhitePeacockPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _hpGainResolver,
                    _armorGainResolver,
                    _attackGainResolver)
            };
        }
    }
}
