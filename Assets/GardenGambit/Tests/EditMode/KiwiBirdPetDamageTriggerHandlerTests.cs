using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        KiwiBirdPetDamageTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void StraightCard_FirstSurvivedNormalAttack_GainsOneArmor(
            CombatSide side,
            BoardRow row)
        {
            var environment =
                new Environment(
                    side,
                    row,
                    CombatPokerHand.Straight);

            var damageEvent =
                environment.CreateNormalAttackDamage();

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    damageEvent),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                damageEvent);

            Assert.That(
                environment.SourceCard.Armor,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            var armorGainEvent =
                environment.Log.Events[
                    environment.Log.Count - 1]
                as ArmorGainCombatEvent;

            Assert.That(
                armorGainEvent,
                Is.Not.Null);

            Assert.That(
                armorGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.SourceCard.InstanceId));

            Assert.That(
                armorGainEvent.ActualGainedAmount,
                Is.EqualTo(1));

            Assert.That(
                armorGainEvent.Metadata.ParentEventId,
                Is.EqualTo(
                    damageEvent.Metadata.EventId));
        }

        [TestCase(CombatPokerHand.Unspecified)]
        [TestCase(CombatPokerHand.HighCard)]
        [TestCase(CombatPokerHand.Pair)]
        [TestCase(CombatPokerHand.TwoPair)]
        [TestCase(CombatPokerHand.ThreeOfAKind)]
        [TestCase(CombatPokerHand.Flush)]
        [TestCase(CombatPokerHand.FullHouse)]
        [TestCase(CombatPokerHand.FourOfAKind)]
        [TestCase(CombatPokerHand.StraightFlush)]
        [TestCase(CombatPokerHand.FiveOfAKind)]
        [TestCase(CombatPokerHand.FlushHouse)]
        [TestCase(CombatPokerHand.FlushFive)]
        public void NonStraightLockedPoker_DoesNotTrigger(
            CombatPokerHand pokerHand)
        {
            var environment =
                new Environment(
                    CombatSide.Player,
                    BoardRow.Front,
                    pokerHand);

            environment.AssertIgnored(
                environment.CreateNormalAttackDamage());
        }

        [Test]
        public void AttackerAtDeathThreshold_DidNotSurviveAndDoesNotTrigger()
        {
            var environment =
                new Environment();

            var damageEvent =
                environment.CreateNormalAttackDamage();

            environment.SourceCard.ApplyIncomingDamage(
                environment.SourceCard.CurrentHp);

            environment.AssertIgnored(
                damageEvent);
        }

        [Test]
        public void EachCardCanTriggerOnceAcrossRepeatedAttacks()
        {
            var environment =
                new Environment();

            var firstDamage =
                environment.CreateNormalAttackDamage();

            environment.Handler.Resolve(
                environment.State,
                firstDamage);

            environment.AssertIgnored(
                environment.CreateNormalAttackDamage(),
                expectedUsageCount: 1);

            var secondDamage =
                environment.CreateNormalAttackDamage(
                    environment.SecondSourceCard,
                    environment.SecondSourcePosition);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    secondDamage),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                secondDamage);

            Assert.That(
                environment.SourceCard.Armor,
                Is.EqualTo(1));

            Assert.That(
                environment.SecondSourceCard.Armor,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void AttackFromOtherRow_DoesNotConsumeUsage()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateNormalAttackDamage(
                    environment.OtherRowCard,
                    environment.OtherRowPosition));
        }

        [Test]
        public void OpposingSideAttack_DoesNotConsumeUsage()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateOpposingNormalAttackDamage());
        }

        [Test]
        public void DirectDamageIsNotANormalAttackAndDoesNotTrigger()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateDirectDamage());
        }

        [Test]
        public void MissingBattleStartSnapshot_DoesNotTrigger()
        {
            var environment =
                new Environment(
                    includeSnapshot: false);

            environment.AssertIgnored(
                environment.CreateNormalAttackDamage());
        }

        [Test]
        public void LockedSnapshotRemainsAuthoritativeAfterRankChanges()
        {
            var environment =
                new Environment();

            environment.SourceCard.SetRank(
                new CardRank(14));

            var damageEvent =
                environment.CreateNormalAttackDamage();

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    damageEvent),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                damageEvent);

            Assert.That(
                environment.SourceCard.Armor,
                Is.EqualTo(1));
        }

        [Test]
        public void MovedSurvivingAttacker_IsFoundByIdentity()
        {
            var environment =
                new Environment();

            var damageEvent =
                environment.CreateNormalAttackDamage();

            environment.State.GetSide(
                    environment.Side)
                .MoveCard(
                    environment.SourcePosition,
                    environment.EmptyPosition);

            environment.Handler.Resolve(
                environment.State,
                damageEvent);

            Assert.That(
                environment.SourceCard.Armor,
                Is.EqualTo(1));

            var armorGainEvent =
                environment.Log.Events[
                    environment.Log.Count - 1]
                as ArmorGainCombatEvent;

            Assert.That(
                armorGainEvent.TargetPosition,
                Is.EqualTo(
                    environment.EmptyPosition));
        }

        [Test]
        public void RemovedAttacker_DoesNotTransferBonusToAnotherCard()
        {
            var environment =
                new Environment();

            var damageEvent =
                environment.CreateNormalAttackDamage();

            environment.State.GetSide(
                    environment.Side)
                .RemoveCardFromCombat(
                    environment.SourcePosition);

            environment.AssertIgnored(
                damageEvent);
        }

        [Test]
        public void ArmorOverflow_DoesNotConsumeUsageAndCanRetry()
        {
            var environment =
                new Environment();

            var damageEvent =
                environment.CreateNormalAttackDamage();

            environment.SourceCard.ApplyArmorGain(
                int.MaxValue);

            Assert.Throws<OverflowException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    damageEvent));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            environment.SourceCard.RemoveArmor(
                int.MaxValue);

            environment.Handler.Resolve(
                environment.State,
                damageEvent);

            Assert.That(
                environment.SourceCard.Armor,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Constructor_ValidatesAndExposesDependencies()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.ArmorGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.ArmorGainResolver,
                    null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                environment.Handler.ArmorGainResolver,
                Is.SameAs(
                    environment.ArmorGainResolver));

            Assert.That(
                environment.Handler.EventLog,
                Is.SameAs(
                    environment.Log));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState SourceCard;
            public readonly CombatCardState SecondSourceCard;
            public readonly CombatCardState OtherRowCard;
            public readonly CombatCardState TargetCard;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log;
            public readonly CombatEventMetadataFactory MetadataFactory;
            public readonly CombatPetCardTriggerUsageRegistry UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter UsageCommitter;
            public readonly CombatArmorGainResolver ArmorGainResolver;
            public readonly KiwiBirdPetDamageTriggerHandler Handler;

            private readonly CombatStartedCombatEvent
                _combatStartedEvent;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatPokerHand pokerHand =
                    CombatPokerHand.Straight,
                bool includeSnapshot = true)
            {
                Side = side;
                Row = row;

                Log = new CombatEventLog();

                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                SourceCard = CreateCard(1, 5);
                SecondSourceCard = CreateCard(2, 6);
                OtherRowCard = CreateCard(3, 7);
                TargetCard = CreateCard(4, 8, 100);

                var ownSide = CreateSideState(
                    side,
                    new[]
                    {
                        Slot(1, SourcePosition, SourceCard),
                        Slot(2, SecondSourcePosition, SecondSourceCard),
                        Slot(3, OtherRowPosition, OtherRowCard),
                        new CombatSlotState(
                            new SlotId(4),
                            EmptyPosition)
                    },
                    new[]
                    {
                        SourceCard,
                        SecondSourceCard,
                        OtherRowCard
                    });

                var opposingSide =
                    OpposingSide;

                var enemyState = CreateSideState(
                    opposingSide,
                    new[]
                    {
                        Slot(5, TargetPosition, TargetCard)
                    },
                    new[] { TargetCard });

                var playerPets = CreatePets(
                    CombatSide.Player);

                var enemyPets = CreatePets(
                    CombatSide.Enemy);

                Pet = (side == CombatSide.Player
                    ? playerPets
                    : enemyPets)[
                        row == BoardRow.Front ? 0 : 1];

                State = new CombatState(
                    side == CombatSide.Player
                        ? ownSide
                        : enemyState,
                    side == CombatSide.Player
                        ? enemyState
                        : ownSide,
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            playerPets)),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            enemyPets)));

                var snapshot =
                    new CombatBattleStartSnapshotResolver()
                        .Resolve(
                            State,
                            GetPokerHand(
                                CombatSide.Player,
                                BoardRow.Front,
                                pokerHand),
                            GetPokerHand(
                                CombatSide.Player,
                                BoardRow.Back,
                                pokerHand),
                            GetPokerHand(
                                CombatSide.Enemy,
                                BoardRow.Front,
                                pokerHand),
                            GetPokerHand(
                                CombatSide.Enemy,
                                BoardRow.Back,
                                pokerHand));

                _combatStartedEvent = includeSnapshot
                    ? new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot(),
                        snapshot)
                    : new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot());

                Log.Append(
                    _combatStartedEvent);

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                ArmorGainResolver =
                    new CombatArmorGainResolver(
                        MetadataFactory,
                        Log);

                Handler =
                    new KiwiBirdPetDamageTriggerHandler(
                        side,
                        Pet.InstanceId,
                        UsageCommitter,
                        ArmorGainResolver,
                        Log);
            }

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            public BoardRow OtherRow =>
                Row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

            public BoardPosition SourcePosition =>
                Position(
                    Side,
                    Row,
                    1);

            public BoardPosition SecondSourcePosition =>
                Position(
                    Side,
                    Row,
                    2);

            public BoardPosition OtherRowPosition =>
                Position(
                    Side,
                    OtherRow,
                    1);

            public BoardPosition EmptyPosition =>
                Position(
                    Side,
                    OtherRow,
                    2);

            public BoardPosition TargetPosition =>
                Position(
                    OpposingSide,
                    BoardRow.Front,
                    1);

            public DamageAppliedCombatEvent
                CreateNormalAttackDamage()
            {
                return CreateNormalAttackDamage(
                    SourceCard,
                    SourcePosition);
            }

            public DamageAppliedCombatEvent
                CreateNormalAttackDamage(
                    CombatCardState attacker,
                    BoardPosition attackerPosition)
            {
                var exchangeEvent =
                    CreateExchangeEvent(
                        attacker,
                        attackerPosition,
                        TargetCard,
                        TargetPosition);

                var attackEvent =
                    new NormalAttackCombatEvent(
                        MetadataFactory.CreateChild(
                            exchangeEvent.Metadata),
                        attacker.InstanceId,
                        attackerPosition,
                        TargetCard.InstanceId,
                        TargetPosition,
                        attacker.Attack);

                Log.Append(
                    attackEvent);

                return new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        attackEvent,
                        attackerPosition,
                        TargetPosition,
                        1);
            }

            public DamageAppliedCombatEvent
                CreateOpposingNormalAttackDamage()
            {
                var exchangeEvent =
                    CreateExchangeEvent(
                        SourceCard,
                        SourcePosition,
                        TargetCard,
                        TargetPosition);

                var attackEvent =
                    new NormalAttackCombatEvent(
                        MetadataFactory.CreateChild(
                            exchangeEvent.Metadata),
                        TargetCard.InstanceId,
                        TargetPosition,
                        SourceCard.InstanceId,
                        SourcePosition,
                        TargetCard.Attack);

                Log.Append(
                    attackEvent);

                return new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        attackEvent,
                        TargetPosition,
                        SourcePosition,
                        1);
            }

            public DamageAppliedCombatEvent
                CreateDirectDamage()
            {
                return new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        _combatStartedEvent,
                        SourcePosition,
                        TargetPosition,
                        1);
            }

            public void AssertIgnored(
                DamageAppliedCombatEvent damageEvent,
                int expectedUsageCount = 0)
            {
                var eventCount =
                    Log.Count;

                Assert.That(
                    Handler.CanTrigger(
                        State,
                        damageEvent),
                    Is.False);

                Handler.Resolve(
                    State,
                    damageEvent);

                Assert.That(
                    UsageRegistry.Count,
                    Is.EqualTo(
                        expectedUsageCount));

                Assert.That(
                    Log.Count,
                    Is.EqualTo(
                        eventCount));
            }

            private NormalAttackExchangeCombatEvent
                CreateExchangeEvent(
                    CombatCardState first,
                    BoardPosition firstPosition,
                    CombatCardState second,
                    BoardPosition secondPosition)
            {
                var playerCard =
                    firstPosition.Side == CombatSide.Player
                        ? first
                        : second;

                var playerPosition =
                    firstPosition.Side == CombatSide.Player
                        ? firstPosition
                        : secondPosition;

                var enemyCard =
                    firstPosition.Side == CombatSide.Enemy
                        ? first
                        : second;

                var enemyPosition =
                    firstPosition.Side == CombatSide.Enemy
                        ? firstPosition
                        : secondPosition;

                var exchangeEvent =
                    new NormalAttackExchangeCombatEvent(
                        MetadataFactory.CreateRoot(),
                        playerCard.InstanceId,
                        playerPosition,
                        playerCard.Attack,
                        enemyCard.InstanceId,
                        enemyPosition,
                        enemyCard.Attack);

                Log.Append(
                    exchangeEvent);

                return exchangeEvent;
            }

            private CombatPokerHand GetPokerHand(
                CombatSide side,
                BoardRow row,
                CombatPokerHand selectedHand)
            {
                return side == Side && row == Row
                    ? selectedHand
                    : CombatPokerHand.FlushFive;
            }

            private static CombatSideState CreateSideState(
                CombatSide side,
                CombatSlotState[] slots,
                CombatCardState[] cards)
            {
                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(
                        cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatSlotState Slot(
                long slotId,
                BoardPosition position,
                CombatCardState card)
            {
                return new CombatSlotState(
                    new SlotId(
                        slotId),
                    position,
                    card.InstanceId);
            }

            private static CombatPetState[] CreatePets(
                CombatSide side)
            {
                var baseId =
                    side == CombatSide.Player
                        ? 1000
                        : 2000;

                return new[]
                {
                    new CombatPetState(
                        new DefinitionId(
                            "test.kiwi_bird"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.kiwi_bird"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                int rank,
                int hp = 10)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.kiwi_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(
                        rank),
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    2);
            }

            private static BoardPosition Position(
                CombatSide side,
                BoardRow row,
                int column)
            {
                return new BoardPosition(
                    side,
                    row,
                    new BoardColumn(
                        column));
            }
        }
    }
}
