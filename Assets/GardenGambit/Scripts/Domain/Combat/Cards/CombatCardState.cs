using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatCardState
    {
        public CombatCardState(
            DefinitionId definitionId,
            InstanceId instanceId,
            CardRank rank,
            int hpCapacity,
            int currentHp,
            int armor,
            int attack)
        {
            if (!definitionId.IsValid)
            {
                throw new ArgumentException(
                    "Combat card requires a valid DefinitionId.",
                    nameof(definitionId));
            }

            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Combat card requires a valid InstanceId.",
                    nameof(instanceId));
            }

            if (!rank.IsValid)
            {
                throw new ArgumentException(
                    "Combat card requires a valid CardRank.",
                    nameof(rank));
            }

            if (hpCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hpCapacity),
                    hpCapacity,
                    "HP capacity must be greater than zero.");
            }

            if (currentHp > hpCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHp),
                    currentHp,
                    "Current HP cannot exceed HP capacity.");
            }

            if (armor < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(armor),
                    armor,
                    "Armor cannot be negative.");
            }

            if (attack < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attack),
                    attack,
                    "Attack cannot be negative.");
            }

            DefinitionId = definitionId;
            InstanceId = instanceId;
            Rank = rank;
            HpCapacity = hpCapacity;
            CurrentHp = currentHp;
            Armor = armor;
            Attack = attack;
        }

        public DefinitionId DefinitionId { get; }

        public InstanceId InstanceId { get; }

        public CardRank Rank { get; private set; }

        public int HpCapacity { get; private set; }

        public int CurrentHp { get; private set; }

        public int Armor { get; private set; }

        public int Attack { get; private set; }

        public DamageApplicationResult PreviewIncomingDamage(
    int incomingDamage)
        {
            if (incomingDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incomingDamage),
                    incomingDamage,
                    "Incoming damage cannot be negative.");
            }

            var previousArmor = Armor;
            var previousHp = CurrentHp;

            var armorAbsorbed =
                Math.Min(previousArmor, incomingDamage);

            var currentArmor =
                previousArmor - armorAbsorbed;

            var hpDamage =
                incomingDamage - armorAbsorbed;

            var currentHp = checked(
                previousHp - hpDamage);

            return new DamageApplicationResult(
                incomingDamage,
                armorAbsorbed,
                hpDamage,
                previousArmor,
                currentArmor,
                previousHp,
                currentHp);
        }

        public DamageApplicationResult ApplyIncomingDamage(
            int incomingDamage)
        {
            var result =
                PreviewIncomingDamage(incomingDamage);

            Armor = result.CurrentArmor;
            CurrentHp = result.CurrentHp;

            return result;
        }

        public int RescueToOneHp()
        {
            if (!IsAtDeathThreshold)
            {
                throw new InvalidOperationException(
                    "Only a card at the death threshold " +
                    "can be rescued.");
            }

            var previousHp = CurrentHp;

            CurrentHp = 1;

            return previousHp;
        }

        public int Heal(int requestedAmount)
        {
            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Heal amount cannot be negative.");
            }

            var missingHp =
                (long)HpCapacity - CurrentHp;

            var actualRestoredAmount = (int)Math.Min(
                requestedAmount,
                missingHp);

            var currentHp = checked(
                CurrentHp + actualRestoredAmount);

            CurrentHp = currentHp;

            return actualRestoredAmount;
        }

        public int ApplyHpStatGain(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "HP stat gain cannot be negative.");
            }

            var hpCapacity = checked(
                HpCapacity + amount);

            var currentHp = checked(
                CurrentHp + amount);

            HpCapacity = hpCapacity;
            CurrentHp = currentHp;

            return amount;
        }

        public int ApplyArmorGain(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Armor gain cannot be negative.");
            }

            var armor = checked(
                Armor + amount);

            Armor = armor;

            return amount;
        }

        public int RemoveArmor(int requestedAmount)
        {
            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Armor removal amount cannot be negative.");
            }

            var actualRemovedAmount =
                Math.Min(Armor, requestedAmount);

            Armor -= actualRemovedAmount;

            return actualRemovedAmount;
        }

        public int ApplyAttackGain(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Attack gain cannot be negative.");
            }

            var attack = checked(
                Attack + amount);

            Attack = attack;

            return amount;
        }

        public int ReduceAttack(int requestedAmount)
        {
            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Attack reduction amount cannot be negative.");
            }

            var actualReducedAmount =
                Math.Min(Attack, requestedAmount);

            Attack -= actualReducedAmount;

            return actualReducedAmount;
        }

        public CardRank SetRank(CardRank rank)
        {
            if (!rank.IsValid)
            {
                throw new ArgumentException(
                    "A valid CardRank is required.",
                    nameof(rank));
            }

            var previousRank = Rank;

            Rank = rank;

            return previousRank;
        }

        public bool IsAtDeathThreshold =>
            CurrentHp <= 0;
    }
}