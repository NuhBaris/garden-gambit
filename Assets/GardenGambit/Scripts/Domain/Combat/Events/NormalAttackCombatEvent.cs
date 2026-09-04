using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        NormalAttackCombatEvent :
        CombatEvent
    {
        public NormalAttackCombatEvent(
            CombatEventMetadata metadata,
            InstanceId attackerInstanceId,
            BoardPosition attackerPosition,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int baseDamage)
            : this(
                metadata,
                attackerInstanceId,
                attackerPosition,
                CombatCardSeason.Unspecified,
                targetInstanceId,
                targetPosition,
                CombatCardSeason.Unspecified,
                baseDamage)
        {
        }

        public NormalAttackCombatEvent(
            CombatEventMetadata metadata,
            InstanceId attackerInstanceId,
            BoardPosition attackerPosition,
            CombatCardSeason attackerSeason,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int baseDamage)
            : this(
                metadata,
                attackerInstanceId,
                attackerPosition,
                attackerSeason,
                targetInstanceId,
                targetPosition,
                CombatCardSeason.Unspecified,
                baseDamage)
        {
        }

        public NormalAttackCombatEvent(
            CombatEventMetadata metadata,
            InstanceId attackerInstanceId,
            BoardPosition attackerPosition,
            CombatCardSeason attackerSeason,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            CombatCardSeason targetSeason,
            int baseDamage)
            : base(
                metadata,
                CombatEventKind.NormalAttack)
        {
            ValidateMetadata(
                metadata);

            if (!attackerInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Normal attack requires a valid " +
                    "attacker InstanceId.",
                    nameof(attackerInstanceId));
            }

            if (!attackerPosition.IsValid)
            {
                throw new ArgumentException(
                    "Normal attack requires a valid " +
                    "attacker position.",
                    nameof(attackerPosition));
            }

            ValidateSeason(
                attackerSeason,
                nameof(attackerSeason));

            if (!targetInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Normal attack requires a valid " +
                    "target InstanceId.",
                    nameof(targetInstanceId));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "Normal attack requires a valid " +
                    "target position.",
                    nameof(targetPosition));
            }

            ValidateSeason(
                targetSeason,
                nameof(targetSeason));

            if (attackerInstanceId ==
                targetInstanceId)
            {
                throw new ArgumentException(
                    "Normal attack attacker and target " +
                    "must be different card instances.",
                    nameof(targetInstanceId));
            }

            if (attackerPosition.Side ==
                targetPosition.Side)
            {
                throw new ArgumentException(
                    "Normal attack attacker and target " +
                    "must belong to opposing sides.",
                    nameof(targetPosition));
            }

            if (baseDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(baseDamage),
                    baseDamage,
                    "Normal attack base damage cannot " +
                    "be negative.");
            }

            AttackerInstanceId =
                attackerInstanceId;

            AttackerPosition =
                attackerPosition;

            AttackerSeason =
                attackerSeason;

            TargetInstanceId =
                targetInstanceId;

            TargetPosition =
                targetPosition;

            TargetSeason =
                targetSeason;

            BaseDamage =
                baseDamage;
        }

        public InstanceId AttackerInstanceId
        {
            get;
        }

        public BoardPosition AttackerPosition
        {
            get;
        }

        public CombatSide AttackerSide =>
            AttackerPosition.Side;

        public CombatCardSeason AttackerSeason
        {
            get;
        }

        public bool HasSpecifiedAttackerSeason =>
            AttackerSeason !=
            CombatCardSeason.Unspecified;

        public bool IsSpringAttack =>
            AttackerSeason ==
            CombatCardSeason.Spring;

        public bool IsSummerAttack =>
            AttackerSeason ==
            CombatCardSeason.Summer;

        public bool IsAutumnAttack =>
            AttackerSeason ==
            CombatCardSeason.Autumn;

        public bool IsWinterAttack =>
            AttackerSeason ==
            CombatCardSeason.Winter;

        public InstanceId TargetInstanceId
        {
            get;
        }

        public BoardPosition TargetPosition
        {
            get;
        }

        public CombatSide TargetSide =>
            TargetPosition.Side;

        public CombatCardSeason TargetSeason
        {
            get;
        }

        public bool HasSpecifiedTargetSeason =>
            TargetSeason !=
            CombatCardSeason.Unspecified;

        public bool IsSpringTarget =>
            TargetSeason ==
            CombatCardSeason.Spring;

        public bool IsSummerTarget =>
            TargetSeason ==
            CombatCardSeason.Summer;

        public bool IsAutumnTarget =>
            TargetSeason ==
            CombatCardSeason.Autumn;

        public bool IsWinterTarget =>
            TargetSeason ==
            CombatCardSeason.Winter;

        public int BaseDamage
        {
            get;
        }

        public bool IsPlayerAttack =>
            AttackerSide ==
            CombatSide.Player;

        public bool IsEnemyAttack =>
            AttackerSide ==
            CombatSide.Enemy;

        private static void ValidateMetadata(
            CombatEventMetadata metadata)
        {
            if (!metadata.HasParent)
            {
                throw new ArgumentException(
                    "Normal Attack event must have a " +
                    "parent exchange event.",
                    nameof(metadata));
            }

            if (metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Normal Attack event cannot be a " +
                    "trigger-root event.",
                    nameof(metadata));
            }
        }

        private static void ValidateSeason(
            CombatCardSeason season,
            string parameterName)
        {
            if (season >=
                    CombatCardSeason.Unspecified &&
                season <=
                    CombatCardSeason.Winter)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(
                parameterName,
                season,
                "Normal Attack card season must be " +
                "Unspecified, Spring, Summer, Autumn " +
                "or Winter.");
        }
    }
}