using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class RainSparrowPetTriggerSourceTests
    {
        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Enemy, false)]
        [TestCase(CombatSide.Enemy, true)]
        public void
            Drain_SelfThenExternalHpGain_GrantsArmorOnlyOnFirstExternalGain(
                CombatSide side,
                bool isHpStatGain)
        {
            var position = new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));

            var card = new CombatCardState(
                new DefinitionId("test-spring-card"),
                new InstanceId(1),
                new CardRank(5),
                CombatCardSeason.Spring,
                hpCapacity: 10,
                currentHp: 5,
                armor: 2,
                attack: 2);

            var pet = new CombatPetState(
                new DefinitionId("rain-sparrow"),
                new InstanceId(1001));

            var state = new CombatState(
                CreateSide(CombatSide.Player, card, position),
                CreateSide(CombatSide.Enemy, card, position),
                CreatePets(CombatSide.Player, side, pet),
                CreatePets(CombatSide.Enemy, side, pet));

            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());

            var eventLog = new CombatEventLog();

            var rootEvent = new CombatStartedCombatEvent(
                metadataFactory.CreateRoot());

            eventLog.Append(rootEvent);

            var hpGainResolver = new CombatHpGainResolver(
                metadataFactory,
                eventLog);

            var armorGainResolver = new CombatArmorGainResolver(
                metadataFactory,
                eventLog);

            var usageCommitter =
                new CombatPetCardTriggerUsageCommitter(
                    new CombatPetCardTriggerUsageRegistry());

            var source = new RainSparrowPetTriggerSource(
                side,
                pet.InstanceId,
                usageCommitter,
                armorGainResolver);

            var eventQueue = new CombatEventQueue(eventLog);

            var engine = new CombatTriggerEngine(
                state,
                eventQueue,
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        source
                    }));

            // Self-source gain must not grant Armor or consume usage.
            var selfGain = ApplyGain(
                hpGainResolver,
                state,
                rootEvent,
                card.InstanceId,
                position,
                isHpStatGain);

            Assert.That(selfGain, Is.Not.Null);
            Assert.That(selfGain.IsSelfSource, Is.True);

            Assert.That(
                engine.Drain(
                    maximumEventCount: 10,
                    maximumTriggerCountPerEvent: 10),
                Is.EqualTo(2));

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(eventLog.Count, Is.EqualTo(2));

            Assert.That(
                usageCommitter.HasTriggered(
                    pet.InstanceId,
                    card.InstanceId),
                Is.False);

            // A distinct source identity represents external HP gain.
            var externalSourceId = new InstanceId(2001);

            var firstExternalGain = ApplyGain(
                hpGainResolver,
                state,
                rootEvent,
                externalSourceId,
                position,
                isHpStatGain);

            Assert.That(firstExternalGain, Is.Not.Null);
            Assert.That(
                firstExternalGain.IsFromAnotherSource,
                Is.True);
            Assert.That(
                firstExternalGain.IsHpStatGain,
                Is.EqualTo(isHpStatGain));

            // The bonus must be applied when the queue processes HP_GAIN.
            Assert.That(card.Armor, Is.EqualTo(2));

            Assert.That(
                engine.Drain(
                    maximumEventCount: 10,
                    maximumTriggerCountPerEvent: 10),
                Is.EqualTo(2));

            Assert.That(card.Armor, Is.EqualTo(3));
            Assert.That(eventLog.Count, Is.EqualTo(4));

            Assert.That(
                usageCommitter.HasTriggered(
                    pet.InstanceId,
                    card.InstanceId),
                Is.True);

            Assert.That(
                eventLog.Events[2],
                Is.SameAs(firstExternalGain));

            Assert.That(
                eventLog.Events[3],
                Is.TypeOf<ArmorGainCombatEvent>());

            var armorGain =
                (ArmorGainCombatEvent)eventLog.Events[3];

            Assert.That(
                armorGain.TargetInstanceId,
                Is.EqualTo(card.InstanceId));
            Assert.That(
                armorGain.TargetPosition,
                Is.EqualTo(position));
            Assert.That(armorGain.PreviousArmor, Is.EqualTo(2));
            Assert.That(armorGain.CurrentArmor, Is.EqualTo(3));
            Assert.That(armorGain.ActualGainedAmount, Is.EqualTo(1));

            Assert.That(
                armorGain.Metadata.ParentEventId.Value,
                Is.EqualTo(firstExternalGain.Metadata.EventId));

            Assert.That(
                armorGain.Metadata.TriggerRootId,
                Is.EqualTo(rootEvent.Metadata.TriggerRootId));

            // A later external gain for the same card grants no bonus.
            var secondExternalGain = ApplyGain(
                hpGainResolver,
                state,
                rootEvent,
                externalSourceId,
                position,
                isHpStatGain);

            Assert.That(secondExternalGain, Is.Not.Null);

            Assert.That(
                engine.Drain(
                    maximumEventCount: 10,
                    maximumTriggerCountPerEvent: 10),
                Is.EqualTo(1));

            Assert.That(card.Armor, Is.EqualTo(3));
            Assert.That(eventLog.Count, Is.EqualTo(5));
            Assert.That(
                eventLog.Events[4],
                Is.SameAs(secondExternalGain));

            Assert.That(
                engine.Drain(
                    maximumEventCount: 10,
                    maximumTriggerCountPerEvent: 10),
                Is.Zero);

            Assert.That(eventQueue.PendingCount, Is.Zero);
            Assert.That(card.Armor, Is.EqualTo(3));
            Assert.That(eventLog.Count, Is.EqualTo(5));
        }

        private static HpGainCombatEvent ApplyGain(
            CombatHpGainResolver resolver,
            CombatState state,
            CombatEvent parentEvent,
            InstanceId sourceInstanceId,
            BoardPosition position,
            bool isHpStatGain)
        {
            if (isHpStatGain)
            {
                return resolver.TryApplyHpStatGain(
                    state,
                    parentEvent,
                    sourceInstanceId,
                    position,
                    requestedAmount: 1);
            }

            return resolver.TryApplyHeal(
                state,
                parentEvent,
                sourceInstanceId,
                position,
                requestedAmount: 1);
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState card,
            BoardPosition position)
        {
            var ownsCard = side == position.Side;

            var slots = ownsCard
                ? new[]
                {
                    new CombatSlotState(
                        new SlotId(1),
                        position,
                        card.InstanceId)
                }
                : Array.Empty<CombatSlotState>();

            var cards = ownsCard
                ? new[] { card }
                : Array.Empty<CombatCardState>();

            return new CombatSideState(
                new CombatBoardState(side, slots),
                new CombatCardRegistry(cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSidePetState CreatePets(
            CombatSide side,
            CombatSide ownerSide,
            CombatPetState pet)
        {
            var pets = side == ownerSide
                ? new[] { pet }
                : Array.Empty<CombatPetState>();

            return new CombatSidePetState(
                side,
                new CombatPetRegistry(pets));
        }
    }
}