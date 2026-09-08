using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetNormalAttackRowScopeTests
    {
        [Test]
        public void
            SunBird_UpperPet_AcceptsFrontAndRejectsBack()
        {
            var sunBird =
                CreatePet(
                    "sun-bird",
                    1001);

            var state =
                CreateState(
                    new[]
                    {
                        sunBird
                    });

            var handler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        sunBird.InstanceId,
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry());

            var frontAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerSide:
                        CombatSide.Player,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            var backAttack =
                CreateAttackEvent(
                    eventId: 3,
                    attackerSide:
                        CombatSide.Player,
                    attackerRow:
                        BoardRow.Back,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            Assert.That(
                handler.CanTrigger(
                    state,
                    frontAttack),
                Is.True);

            Assert.That(
                handler.CanTrigger(
                    state,
                    backAttack),
                Is.False);
        }

        [Test]
        public void
            SunBird_LowerPet_AcceptsBackAndRejectsFront()
        {
            var upperPet =
                CreatePet(
                    "upper-pet",
                    1001);

            var lowerSunBird =
                CreatePet(
                    "lower-sun-bird",
                    1002);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerSunBird
                    });

            var handler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        lowerSunBird.InstanceId,
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry());

            var frontAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerSide:
                        CombatSide.Player,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            var backAttack =
                CreateAttackEvent(
                    eventId: 3,
                    attackerSide:
                        CombatSide.Player,
                    attackerRow:
                        BoardRow.Back,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            Assert.That(
                handler.CanTrigger(
                    state,
                    frontAttack),
                Is.False);

            Assert.That(
                handler.CanTrigger(
                    state,
                    backAttack),
                Is.True);
        }

        [Test]
        public void
            SunBird_LowerPet_BackResolve_AddsModifierAndCommitsUsage()
        {
            var upperPet =
                CreatePet(
                    "upper-pet",
                    1001);

            var lowerSunBird =
                CreatePet(
                    "lower-sun-bird",
                    1002);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerSunBird
                    });

            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        lowerSunBird.InstanceId,
                        usageCommitter,
                        modifierRegistry);

            var backAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerSide:
                        CombatSide.Player,
                    attackerRow:
                        BoardRow.Back,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            handler.Resolve(
                state,
                backAttack);

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    backAttack.Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                usageCommitter.HasTriggered(
                    lowerSunBird.InstanceId,
                    backAttack.AttackerInstanceId),
                Is.True);
        }

        [Test]
        public void
            PolarFerret_UpperPet_AcceptsFrontAndRejectsBack()
        {
            var polarFerret =
                CreatePet(
                    "polar-ferret",
                    1001);

            var state =
                CreateState(
                    new[]
                    {
                        polarFerret
                    });

            var handler =
                new
                    PolarFerretPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        polarFerret.InstanceId,
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackTargetDamageReductionRegistry());

            var frontTargetAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerSide:
                        CombatSide.Enemy,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            var backTargetAttack =
                CreateAttackEvent(
                    eventId: 3,
                    attackerSide:
                        CombatSide.Enemy,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Back,
                    targetSeason:
                        CombatCardSeason.Winter);

            Assert.That(
                handler.CanTrigger(
                    state,
                    frontTargetAttack),
                Is.True);

            Assert.That(
                handler.CanTrigger(
                    state,
                    backTargetAttack),
                Is.False);
        }

        [Test]
        public void
            PolarFerret_LowerPet_AcceptsBackAndRejectsFront()
        {
            var upperPet =
                CreatePet(
                    "upper-pet",
                    1001);

            var lowerPolarFerret =
                CreatePet(
                    "lower-polar-ferret",
                    1002);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerPolarFerret
                    });

            var handler =
                new
                    PolarFerretPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        lowerPolarFerret.InstanceId,
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackTargetDamageReductionRegistry());

            var frontTargetAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerSide:
                        CombatSide.Enemy,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Front,
                    targetSeason:
                        CombatCardSeason.Winter);

            var backTargetAttack =
                CreateAttackEvent(
                    eventId: 3,
                    attackerSide:
                        CombatSide.Enemy,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Back,
                    targetSeason:
                        CombatCardSeason.Winter);

            Assert.That(
                handler.CanTrigger(
                    state,
                    frontTargetAttack),
                Is.False);

            Assert.That(
                handler.CanTrigger(
                    state,
                    backTargetAttack),
                Is.True);
        }

        [Test]
        public void
            PolarFerret_LowerPet_BackResolve_RegistersReductionRequest()
        {
            var upperPet =
                CreatePet(
                    "upper-pet",
                    1001);

            var lowerPolarFerret =
                CreatePet(
                    "lower-polar-ferret",
                    1002);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerPolarFerret
                    });

            var usageCommitter =
                CreateUsageCommitter();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var handler =
                new
                    PolarFerretPetNormalAttackTriggerHandler(
                        CombatSide.Player,
                        lowerPolarFerret.InstanceId,
                        usageCommitter,
                        reductionRegistry);

            var backTargetAttack =
                CreateAttackEvent(
                    eventId: 2,
                    attackerSide:
                        CombatSide.Enemy,
                    attackerRow:
                        BoardRow.Front,
                    attackerSeason:
                        CombatCardSeason.Summer,
                    targetRow:
                        BoardRow.Back,
                    targetSeason:
                        CombatCardSeason.Winter);

            handler.Resolve(
                state,
                backTargetAttack);

            var requests =
                reductionRegistry.GetRequests(
                    backTargetAttack.Metadata.EventId);

            Assert.That(
                requests.Count,
                Is.EqualTo(1));

            Assert.That(
                requests[0].PetInstanceId,
                Is.EqualTo(
                    lowerPolarFerret.InstanceId));

            Assert.That(
                requests[0].TargetCardInstanceId,
                Is.EqualTo(
                    backTargetAttack.TargetInstanceId));

            Assert.That(
                usageCommitter.HasTriggered(
                    lowerPolarFerret.InstanceId,
                    backTargetAttack.TargetInstanceId),
                Is.False);
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets)
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        Array.Empty<
                            CombatPetState>())));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<
                        CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<
                        CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
        }

        private static
            CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new
                CombatPetCardTriggerUsageCommitter(
                    new
                        CombatPetCardTriggerUsageRegistry());
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                long eventId,
                CombatSide attackerSide,
                BoardRow attackerRow,
                CombatCardSeason attackerSeason,
                BoardRow targetRow,
                CombatCardSeason targetSeason)
        {
            var targetSide =
                attackerSide ==
                CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            var attackerInstanceId =
                attackerSide ==
                CombatSide.Player
                    ? new InstanceId(1)
                    : new InstanceId(101);

            var targetInstanceId =
                attackerSide ==
                CombatSide.Player
                    ? new InstanceId(101)
                    : new InstanceId(1);

            return new NormalAttackCombatEvent(
                CreateMetadata(
                    eventId),
                attackerInstanceId,
                new BoardPosition(
                    attackerSide,
                    attackerRow,
                    new BoardColumn(1)),
                attackerSeason,
                targetInstanceId,
                new BoardPosition(
                    targetSide,
                    targetRow,
                    new BoardColumn(1)),
                targetSeason,
                baseDamage: 5);
        }

        private static CombatEventMetadata
            CreateMetadata(
                long eventId)
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(
                    eventId),
                new CombatSequenceNumber(
                    eventId),
                triggerRootId,
                triggerRootId);
        }
    }
}