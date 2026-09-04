namespace GardenGambit.Domain.Combat
{
    public enum CombatEventKind
    {
        Unspecified = 0,

        CombatStarted = 1,

        ColumnStarted = 2,

        NormalAttackExchange = 3,

        NormalAttack = 4,

        DamageApplied = 5,

        HpGain = 6,

        Death = 7,

        Rescue = 8,

        DeathRemoval = 9,

        DirectDelete = 10,

        CardAdvanced = 11,

        CombatResultCalculated = 12,

        BattleHealthChanged = 13,

        CombatCompleted = 14,

        SacrificialAltarActivated = 15,

        WarAltarActivated = 16,

        BattleStartStageStarted = 17
    }
}