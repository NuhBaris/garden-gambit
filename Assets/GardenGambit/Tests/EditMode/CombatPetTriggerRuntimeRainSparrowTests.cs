using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeRainSparrowTests
    {
        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Enemy, false)]
        [TestCase(CombatSide.Enemy, true)]
        public void
            CreateResolutionRunner_WithSacrificialAltar_ResolvesRainSparrowOnce(
                CombatSide side,
                bool resumeAfterBudgetExhaustion)
        {
            var state = CreateState(
                side,
                out var donor,
                out var recipient,
                out var pet);

            var runtime = new CombatPetTriggerRuntime();

            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());

            var eventLog = new CombatEventLog();
            var eventQueue = new CombatEventQueue(eventLog);

            var runner = runtime.CreateResolutionRunner(
                state,
                metadataFactory,
                eventLog,
                eventQueue);

            Assert.That(
                runner.UsesStagedNormalAttackByDefault,
                Is.True);

            CombatCompletedCombatEvent completedEvent;

            if (resumeAfterBudgetExhaustion)
            {
                Assert.Throws<InvalidOperationException>(
                    () => runner.StartAndResolveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 1,
                        maximumEventCountPerPass: 1,
                        maximumTriggerCountPerEvent: 100));

                Assert.That(runner.HasActiveCombat, Is.True);
                Assert.That(
                    runner.ActiveAltarExecutionStage,
                    Is.EqualTo(
                        CombatAltarActivationExecutionStage
                            .TransferApplied));

                Assert.That(donor.CurrentHp, Is.EqualTo(4));
                Assert.That(recipient.CurrentHp, Is.EqualTo(9));

                completedEvent = runner.ResumeActiveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }
            else
            {
                completedEvent = runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }

            Assert.That(completedEvent, Is.Not.Null);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(runner.ResolvedAltarActivationCount, Is.EqualTo(1));
            Assert.That(eventQueue.PendingCount, Is.Zero);

            Assert.That(recipient.HpCapacity, Is.EqualTo(14));
            Assert.That(recipient.CurrentHp, Is.EqualTo(9));
            Assert.That(recipient.Armor, Is.EqualTo(3));
            Assert.That(donor.CurrentHp, Is.Zero);

            Assert.That(
                runtime.UsageCommitter.HasTriggered(
                    pet.InstanceId,
                    recipient.InstanceId),
                Is.True);

            Assert.That(
                runtime.UsageCommitter.HasTriggered(
                    pet.InstanceId,
                    donor.InstanceId),
                Is.False);

            var altarIndex = FindSingleEventIndex(
                eventLog,
                CombatEventKind.SacrificialAltarActivated);

            var hpGainIndex = FindSingleEventIndex(
                eventLog,
                CombatEventKind.HpGain);

            var armorGainIndex = FindSingleEventIndex(
                eventLog,
                CombatEventKind.ArmorGain);

            var deathIndex = FindSingleEventIndex(
                eventLog,
                CombatEventKind.Death);

            var removalIndex = FindSingleEventIndex(
                eventLog,
                CombatEventKind.DeathRemoval);

            Assert.That(hpGainIndex, Is.GreaterThan(altarIndex));
            Assert.That(armorGainIndex, Is.GreaterThan(hpGainIndex));
            Assert.That(deathIndex, Is.GreaterThan(armorGainIndex));
            Assert.That(removalIndex, Is.GreaterThan(deathIndex));

            var hpGain =
                (HpGainCombatEvent)eventLog.Events[hpGainIndex];

            var armorGain =
                (ArmorGainCombatEvent)eventLog.Events[armorGainIndex];

            Assert.That(
                hpGain.SourceInstanceId,
                Is.EqualTo(donor.InstanceId));
            Assert.That(
                hpGain.TargetInstanceId,
                Is.EqualTo(recipient.InstanceId));
            Assert.That(hpGain.IsFromAnotherSource, Is.True);
            Assert.That(hpGain.IsHpStatGain, Is.True);
            Assert.That(hpGain.ActualGainedAmount, Is.EqualTo(4));

            Assert.That(
                hpGain.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    eventLog.Events[altarIndex].Metadata.EventId));

            Assert.That(
                armorGain.TargetInstanceId,
                Is.EqualTo(recipient.InstanceId));
            Assert.That(
                armorGain.TargetPosition,
                Is.EqualTo(hpGain.TargetPosition));
            Assert.That(armorGain.TargetSide, Is.EqualTo(side));
            Assert.That(armorGain.PreviousArmor, Is.EqualTo(2));
            Assert.That(armorGain.CurrentArmor, Is.EqualTo(3));
            Assert.That(armorGain.ActualGainedAmount, Is.EqualTo(1));

            Assert.That(
                armorGain.Metadata.ParentEventId.Value,
                Is.EqualTo(hpGain.Metadata.EventId));
            Assert.That(
                armorGain.Metadata.TriggerRootId,
                Is.EqualTo(hpGain.Metadata.TriggerRootId));

            var donorPosition = new BoardPosition(
                side,
                BoardRow.Back,
                new BoardColumn(1));

            Assert.That(
                state.GetSide(side).Board
                    .GetSlot(donorPosition).IsOccupied,
                Is.False);
        }

        private static CombatState CreateState(
            CombatSide side,
            out CombatCardState donor,
            out CombatCardState recipient,
            out CombatPetState pet)
        {
            var donorPosition = new BoardPosition(
                side,
                BoardRow.Back,
                new BoardColumn(1));

            var recipientPosition = new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));

            donor = CreateCard(100, currentHp: 4, armor: 0);
            recipient = CreateCard(200, currentHp: 5, armor: 2);

            pet = new CombatPetState(
                CombatPetDefinitionIds.RainSparrow,
                new InstanceId(1001));

            var ownerSide = new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            donorPosition,
                            donor.InstanceId,
                            CombatSlotEnhanceKind.SacrificialAltar),

                        new CombatSlotState(
                            new SlotId(2),
                            recipientPosition,
                            recipient.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[] { donor, recipient }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));

            var opposingSide = side == CombatSide.Player
                ? CombatSide.Enemy
                : CombatSide.Player;

            var emptySide = new CombatSideState(
                new CombatBoardState(
                    opposingSide,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));

            var ownerPets = new CombatSidePetState(
                side,
                new CombatPetRegistry(new[] { pet }));

            var emptyPets = new CombatSidePetState(
                opposingSide,
                new CombatPetRegistry(
                    Array.Empty<CombatPetState>()));

            return new CombatState(
                side == CombatSide.Player ? ownerSide : emptySide,
                side == CombatSide.Enemy ? ownerSide : emptySide,
                side == CombatSide.Player ? ownerPets : emptyPets,
                side == CombatSide.Enemy ? ownerPets : emptyPets);
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int currentHp,
            int armor)
        {
            return new CombatCardState(
                new DefinitionId($"test.card_{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(2),
                CombatCardSeason.Spring,
                hpCapacity: 10,
                currentHp: currentHp,
                armor: armor,
                attack: 3);
        }

        private static int FindSingleEventIndex(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            var foundIndex = -1;
            var count = 0;

            for (var index = 0; index < eventLog.Count; index++)
            {
                if (eventLog.Events[index].Kind != kind)
                {
                    continue;
                }

                foundIndex = index;
                count++;
            }

            Assert.That(
                count,
                Is.EqualTo(1),
                $"Expected exactly one {kind} event.");

            return foundIndex;
        }
    }
}