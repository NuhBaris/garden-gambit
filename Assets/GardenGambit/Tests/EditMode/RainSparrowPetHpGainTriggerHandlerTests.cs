using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class RainSparrowPetHpGainTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Enemy, false)]
        [TestCase(CombatSide.Enemy, true)]
        public void Resolve_WithEligibleGain_AddsArmorAndCommitsUsage(
            CombatSide petSide,
            bool heal)
        {
            var environment = CreateEnvironment(petSide: petSide);
            var hpGainEvent = Gain(environment, heal: heal);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    hpGainEvent),
                Is.True);

            Assert.That(environment.Card.Armor, Is.EqualTo(2));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));
            Assert.That(
                environment.UsageCommitter.UsageRegistry.Count,
                Is.Zero);

            environment.Handler.Resolve(
                environment.State,
                hpGainEvent);

            Assert.That(environment.Card.Armor, Is.EqualTo(3));
            Assert.That(environment.Card.CurrentHp, Is.EqualTo(5));
            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(heal ? 10 : 11));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Card.InstanceId),
                Is.True);

            Assert.That(environment.EventLog.Count, Is.EqualTo(3));

            var armorEvent =
                environment.EventLog.Events[2] as ArmorGainCombatEvent;

            Assert.That(armorEvent, Is.Not.Null);
            Assert.That(armorEvent.PreviousArmor, Is.EqualTo(2));
            Assert.That(armorEvent.CurrentArmor, Is.EqualTo(3));
            Assert.That(armorEvent.ActualGainedAmount, Is.EqualTo(1));

            Assert.That(
                armorEvent.TargetInstanceId,
                Is.EqualTo(environment.Card.InstanceId));

            Assert.That(
                armorEvent.TargetPosition,
                Is.EqualTo(environment.Position));

            Assert.That(
                armorEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(hpGainEvent.Metadata.EventId));

            Assert.That(
                armorEvent.Metadata.TriggerRootId,
                Is.EqualTo(hpGainEvent.Metadata.TriggerRootId));
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Autumn)]
        [TestCase(CombatCardSeason.Winter)]
        [TestCase(CombatCardSeason.Seasonless)]
        public void Resolve_WithNonSpringCard_DoesNotApplyOrConsume(
            CombatCardSeason season)
        {
            var environment = CreateEnvironment(season: season);
            var hpGainEvent = Gain(environment);

            AssertIgnored(environment, hpGainEvent);
        }

        [Test]
        public void Resolve_WithSelfGain_PreservesFirstExternalGain()
        {
            var environment = CreateEnvironment();

            var selfGain = Gain(
                environment,
                selfSource: true);

            AssertIgnored(environment, selfGain);

            var externalGain = Gain(environment);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    externalGain),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                externalGain);

            Assert.That(environment.Card.Armor, Is.EqualTo(3));
            Assert.That(
                environment.UsageCommitter.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithOpposingSideGain_DoesNotApplyOrConsume()
        {
            var environment = CreateEnvironment(
                petSide: CombatSide.Player,
                targetSide: CombatSide.Enemy);

            var hpGainEvent = Gain(environment);

            AssertIgnored(environment, hpGainEvent);
        }

        [Test]
        public void Resolve_UsesOneBonusPerCardInstance()
        {
            var environment = CreateEnvironment();

            var firstGain = Gain(environment);

            environment.Handler.Resolve(
                environment.State,
                firstGain);

            Assert.That(environment.Card.Armor, Is.EqualTo(3));

            AssertIgnored(environment, firstGain);

            var repeatedCardGain = Gain(environment);

            AssertIgnored(environment, repeatedCardGain);

            var otherCardGain = Gain(
                environment,
                secondCard: true);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    otherCardGain),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                otherCardGain);

            Assert.That(environment.Card.Armor, Is.EqualTo(3));
            Assert.That(environment.SecondCard.Armor, Is.EqualTo(3));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Card.InstanceId),
                Is.True);

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.SecondCard.InstanceId),
                Is.True);

            Assert.That(
                environment.UsageCommitter.UsageRegistry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WithTwoPets_UsesCurrentRowAndIndependentUsage()
        {
            var environment = CreateEnvironment();
            var frontGain = Gain(environment);

            AssertIgnored(
                environment,
                frontGain,
                environment.LowerHandler);

            environment.Handler.Resolve(
                environment.State,
                frontGain);

            Assert.That(environment.Card.Armor, Is.EqualTo(3));

            environment.State.GetSide(environment.Position.Side)
                .MoveCard(
                    environment.Position,
                    environment.BackPosition);

            var backGain = Gain(
                environment,
                position: environment.BackPosition);

            AssertIgnored(environment, backGain);

            Assert.That(
                environment.LowerHandler.CanTrigger(
                    environment.State,
                    backGain),
                Is.True);

            environment.LowerHandler.Resolve(
                environment.State,
                backGain);

            Assert.That(environment.Card.Armor, Is.EqualTo(4));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Card.InstanceId),
                Is.True);

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.LowerPet.InstanceId,
                    environment.Card.InstanceId),
                Is.True);

            Assert.That(
                environment.UsageCommitter.UsageRegistry.Count,
                Is.EqualTo(2));
        }

        [TestCase(BoardRow.Front)]
        [TestCase(BoardRow.Back)]
        public void Resolve_AfterMovement_RechecksCurrentPosition(
            BoardRow destinationRow)
        {
            var environment = CreateEnvironment();
            var hpGainEvent = Gain(environment);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    hpGainEvent),
                Is.True);

            var destination = destinationRow == BoardRow.Front
                ? environment.OtherFrontPosition
                : environment.BackPosition;

            environment.State.GetSide(environment.Position.Side)
                .MoveCard(environment.Position, destination);

            if (destinationRow == BoardRow.Back)
            {
                AssertIgnored(environment, hpGainEvent);
                return;
            }

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    hpGainEvent),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                hpGainEvent);

            Assert.That(environment.Card.Armor, Is.EqualTo(3));

            var armorEvent =
                environment.EventLog.Events[2] as ArmorGainCombatEvent;

            Assert.That(armorEvent, Is.Not.Null);
            Assert.That(
                armorEvent.TargetPosition,
                Is.EqualTo(destination));
            Assert.That(
                armorEvent.TargetInstanceId,
                Is.EqualTo(environment.Card.InstanceId));
        }

        [Test]
        public void Resolve_AfterDirectDelete_DoesNotBuffReplacementCard()
        {
            var environment = CreateEnvironment();
            var hpGainEvent = Gain(environment);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    hpGainEvent),
                Is.True);

            var deleteResolver = new CombatDirectDeleteResolver(
                environment.MetadataFactory,
                environment.EventLog);

            deleteResolver.ApplyDirectDelete(
                environment.State,
                hpGainEvent,
                environment.Position);

            environment.State.GetSide(environment.Position.Side)
                .MoveCard(
                    environment.SecondPosition,
                    environment.Position);

            AssertIgnored(environment, hpGainEvent);

            Assert.That(
                environment.SecondCard.Armor,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WithBoardCardStillAtDeathThreshold_AllowsBonus()
        {
            var environment = CreateEnvironment(currentHp: -2);
            var hpGainEvent = Gain(environment);

            Assert.That(environment.Card.CurrentHp, Is.EqualTo(-1));

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    hpGainEvent),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                hpGainEvent);

            Assert.That(environment.Card.CurrentHp, Is.EqualTo(-1));
            Assert.That(environment.Card.Armor, Is.EqualTo(3));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Card.InstanceId),
                Is.True);
        }

        [Test]
        public void Resolve_WhenArmorOverflows_PreservesUsageAndAllowsRetry()
        {
            var environment = CreateEnvironment(armor: int.MaxValue);
            var hpGainEvent = Gain(environment);

            Assert.Throws<OverflowException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    hpGainEvent));

            Assert.That(environment.Card.Armor, Is.EqualTo(int.MaxValue));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Card.InstanceId),
                Is.False);

            environment.Card.RemoveArmor(1);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    hpGainEvent),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                hpGainEvent);

            Assert.That(environment.Card.Armor, Is.EqualTo(int.MaxValue));
            Assert.That(environment.EventLog.Count, Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[2].Metadata.EventId.Value,
                Is.EqualTo(3L));

            Assert.That(
                environment.EventLog.Events[2].Metadata.SequenceNo.Value,
                Is.EqualTo(3L));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Card.InstanceId),
                Is.True);

            AssertIgnored(environment, hpGainEvent);
        }

        private static void AssertIgnored(
            TestEnvironment environment,
            HpGainCombatEvent sourceEvent,
            RainSparrowPetHpGainTriggerHandler handler = null)
        {
            handler = handler ?? environment.Handler;

            var previousArmor = environment.Card.Armor;
            var previousEventCount = environment.EventLog.Count;
            var previousUsageCount =
                environment.UsageCommitter.UsageRegistry.Count;

            Assert.That(
                handler.CanTrigger(environment.State, sourceEvent),
                Is.False);

            handler.Resolve(environment.State, sourceEvent);

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(previousArmor));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(previousEventCount));

            Assert.That(
                environment.UsageCommitter.UsageRegistry.Count,
                Is.EqualTo(previousUsageCount));
        }

        private static HpGainCombatEvent Gain(
            TestEnvironment environment,
            bool heal = false,
            bool selfSource = false,
            bool secondCard = false,
            BoardPosition? position = null)
        {
            var targetCard = secondCard
                ? environment.SecondCard
                : environment.Card;

            var targetPosition = position ??
                (secondCard
                    ? environment.SecondPosition
                    : environment.Position);

            var sourceInstanceId = selfSource
                ? targetCard.InstanceId
                : environment.UpperPet.InstanceId;

            var resolver = new CombatHpGainResolver(
                environment.MetadataFactory,
                environment.EventLog);

            return heal
                ? resolver.TryApplyHeal(
                    environment.State,
                    environment.RootEvent,
                    sourceInstanceId,
                    targetPosition,
                    requestedAmount: 1)
                : resolver.TryApplyHpStatGain(
                    environment.State,
                    environment.RootEvent,
                    sourceInstanceId,
                    targetPosition,
                    requestedAmount: 1);
        }

        private static TestEnvironment CreateEnvironment(
            CombatSide petSide = CombatSide.Player,
            CombatSide? targetSide = null,
            CombatCardSeason season = CombatCardSeason.Spring,
            int armor = 2,
            int currentHp = 4)
        {
            var cardSide = targetSide ?? petSide;

            var position = new BoardPosition(
                cardSide, BoardRow.Front, new BoardColumn(1));

            var secondPosition = new BoardPosition(
                cardSide, BoardRow.Front, new BoardColumn(2));

            var otherFrontPosition = new BoardPosition(
                cardSide, BoardRow.Front, new BoardColumn(3));

            var backPosition = new BoardPosition(
                cardSide, BoardRow.Back, new BoardColumn(1));

            var card = CreateCard(1, season, armor, currentHp);
            var secondCard = CreateCard(2, season, 2, 4);

            var cards = new[] { card, secondCard };

            var slots = new[]
            {
                new CombatSlotState(
                    new SlotId(1), position, card.InstanceId),
                new CombatSlotState(
                    new SlotId(2), secondPosition, secondCard.InstanceId),
                new CombatSlotState(
                    new SlotId(3), otherFrontPosition),
                new CombatSlotState(
                    new SlotId(4), backPosition)
            };

            var upperPet = new CombatPetState(
                new DefinitionId("test.rain_sparrow"),
                new InstanceId(101));

            var lowerPet = new CombatPetState(
                new DefinitionId("test.rain_sparrow"),
                new InstanceId(102));

            var pets = new[] { upperPet, lowerPet };

            var state = new CombatState(
                CreateSide(CombatSide.Player, cardSide, cards, slots),
                CreateSide(CombatSide.Enemy, cardSide, cards, slots),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        petSide == CombatSide.Player
                            ? pets
                            : Array.Empty<CombatPetState>())),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        petSide == CombatSide.Enemy
                            ? pets
                            : Array.Empty<CombatPetState>())));

            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());

            var eventLog = new CombatEventLog();

            var rootEvent = new CombatStartedCombatEvent(
                metadataFactory.CreateRoot());

            eventLog.Append(rootEvent);

            var usageCommitter = new CombatPetCardTriggerUsageCommitter(
                new CombatPetCardTriggerUsageRegistry());

            var armorResolver = new CombatArmorGainResolver(
                metadataFactory,
                eventLog);

            return new TestEnvironment
            {
                State = state,
                Card = card,
                SecondCard = secondCard,
                Position = position,
                SecondPosition = secondPosition,
                OtherFrontPosition = otherFrontPosition,
                BackPosition = backPosition,
                UpperPet = upperPet,
                LowerPet = lowerPet,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                RootEvent = rootEvent,
                UsageCommitter = usageCommitter,
                Handler = new RainSparrowPetHpGainTriggerHandler(
                    petSide,
                    upperPet.InstanceId,
                    usageCommitter,
                    armorResolver),
                LowerHandler = new RainSparrowPetHpGainTriggerHandler(
                    petSide,
                    lowerPet.InstanceId,
                    usageCommitter,
                    armorResolver)
            };
        }

        private static CombatCardState CreateCard(
            long instanceId,
            CombatCardSeason season,
            int armor,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId("test.spring_card"),
                new InstanceId(instanceId),
                new CardRank(2),
                season,
                hpCapacity: 10,
                currentHp: currentHp,
                armor: armor,
                attack: 2);
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatSide cardSide,
            CombatCardState[] cards,
            CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    side == cardSide
                        ? slots
                        : Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    side == cardSide
                        ? cards
                        : Array.Empty<CombatCardState>()),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }
            public CombatCardState Card { get; set; }
            public CombatCardState SecondCard { get; set; }
            public BoardPosition Position { get; set; }
            public BoardPosition SecondPosition { get; set; }
            public BoardPosition OtherFrontPosition { get; set; }
            public BoardPosition BackPosition { get; set; }
            public CombatPetState UpperPet { get; set; }
            public CombatPetState LowerPet { get; set; }
            public CombatEventMetadataFactory MetadataFactory { get; set; }
            public CombatEventLog EventLog { get; set; }
            public CombatStartedCombatEvent RootEvent { get; set; }
            public CombatPetCardTriggerUsageCommitter UsageCommitter
            {
                get;
                set;
            }
            public RainSparrowPetHpGainTriggerHandler Handler
            {
                get;
                set;
            }
            public RainSparrowPetHpGainTriggerHandler LowerHandler
            {
                get;
                set;
            }
        }
    }
}