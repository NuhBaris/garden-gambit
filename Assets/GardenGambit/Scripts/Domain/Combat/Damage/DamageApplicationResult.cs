using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct DamageApplicationResult
    {
        private readonly bool _isInitialized;

        internal DamageApplicationResult(
            int incomingDamage,
            int armorAbsorbed,
            int hpDamage,
            int previousArmor,
            int currentArmor,
            int previousHp,
            int currentHp)
        {
            if (incomingDamage < 0 ||
                armorAbsorbed < 0 ||
                hpDamage < 0)
            {
                throw new ArgumentException(
                    "Damage amounts cannot be negative.");
            }

            if (previousArmor < 0 ||
                currentArmor < 0)
            {
                throw new ArgumentException(
                    "Armor values cannot be negative.");
            }

            if ((long)armorAbsorbed + hpDamage !=
                incomingDamage)
            {
                throw new ArgumentException(
                    "Armor and HP damage must equal " +
                    "incoming damage.");
            }

            if ((long)previousArmor - armorAbsorbed !=
                currentArmor)
            {
                throw new ArgumentException(
                    "Current Armor does not match " +
                    "the absorbed amount.");
            }

            if ((long)previousHp - hpDamage !=
                currentHp)
            {
                throw new ArgumentException(
                    "Current HP does not match HP damage.");
            }

            IncomingDamage = incomingDamage;
            ArmorAbsorbed = armorAbsorbed;
            HpDamage = hpDamage;
            PreviousArmor = previousArmor;
            CurrentArmor = currentArmor;
            PreviousHp = previousHp;
            CurrentHp = currentHp;
            _isInitialized = true;
        }

        public int IncomingDamage { get; }

        public int ArmorAbsorbed { get; }

        public int HpDamage { get; }

        public int PreviousArmor { get; }

        public int CurrentArmor { get; }

        public int PreviousHp { get; }

        public int CurrentHp { get; }

        public bool IsValid
        {
            get
            {
                if (!_isInitialized ||
                    IncomingDamage < 0 ||
                    ArmorAbsorbed < 0 ||
                    HpDamage < 0 ||
                    PreviousArmor < 0 ||
                    CurrentArmor < 0)
                {
                    return false;
                }

                if ((long)ArmorAbsorbed + HpDamage !=
                    IncomingDamage)
                {
                    return false;
                }

                if ((long)PreviousArmor - ArmorAbsorbed !=
                    CurrentArmor)
                {
                    return false;
                }

                return (long)PreviousHp - HpDamage ==
                       CurrentHp;
            }
        }

        public bool HasPositiveDamage =>
            IncomingDamage > 0;

        public bool WasFullyAbsorbedByArmor =>
            IncomingDamage > 0 &&
            HpDamage == 0;

        public bool EnteredDeathThreshold =>
            PreviousHp > 0 &&
            CurrentHp <= 0;
    }
}