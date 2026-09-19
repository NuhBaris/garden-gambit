using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        FruitBatPetDamageTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FruitCard_FirstSurvivedNormalAttack_GainsOneHpStat(
            CombatSide side,
            BoardRow row)
        {
            var environment =
                new Environment(
                    side,
                    row);

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
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.SourceCard.CurrentHp,
                Is.EqualTo(11));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            var hpGainEvent =
                environment.Log.Events[
                    environment.Log.Count - 1]
                as HpGainCombatEvent;

            Assert.That(
                hpGainEvent,
                Is.Not.Null);

            Assert.That(
                hpGainEvent.SourceInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            Assert.That(
                hpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.SourceCard.InstanceId));

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(1));

            Assert.That(
                hpGainEvent.Metadata.ParentEventId,
                Is.EqualTo(
                    damageEvent.Metadata.EventId));
        }

        [TestCase(CombatCardSuit.Unspecified)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Nut)]
        [TestCase(CombatCardSuit.Drink)]
        public void NonFruitCard_DoesNotTrigger(
            CombatCardSuit suit)
        {
            var environment =
                new Environment(
                    sourceSuit: suit);

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
        public void EachFruitCardCanTriggerOnceAcrossRepeatedAttacks()
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

            environment.Handler.Resolve(
                environment.State,
                secondDamage);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.SecondSourceCard.HpCapacity,
                Is.EqualTo(11));

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
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            var hpGainEvent =
                environment.Log.Events[
                    environment.Log.Count - 1]
                as HpGainCombatEvent;

            Assert.That(
                hpGainEvent.TargetPosition,
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
        public void HpOverflow_DoesNotConsumeUsage()
        {
            var environment =
                new Environment(
                    sourceHpCapacity: int.MaxValue,
                    sourceCurrentHp: 10);

            var damageEvent =
                environment.CreateNormalAttackDamage();

            Assert.Throws<OverflowException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    damageEvent));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            environment.SourceCard.SetCurrentHpToZero();

            environment.AssertIgnored(
                damageEvent);
        }

        [Test]
        public void UnloggedDamageCopy_IsRejectedWithoutConsumingUsage()
        {
            var environment =
                new Environment();

            var damageEvent =
                environment.CreateNormalAttackDamage();

            var copy =
                new DamageAppliedCombatEvent(
                    damageEvent.Metadata,
                    damageEvent.SourceInstanceId,
                    damageEvent.SourcePosition,
                    damageEvent.TargetInstanceId,
                    damageEvent.TargetPosition,
                    damageEvent.Result);

            Assert.Throws<ArgumentException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    copy));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            environment.Handler.Resolve(
                environment.State,
                damageEvent);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));
        }

        [Test]
        public void Constructor_ValidatesAndExposesDependencies()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                environment.Handler.HpGainResolver,
                Is.SameAs(
                    environment.HpGainResolver));

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
            public readonly CombatStartedCombatEvent RootEvent;
            public readonly CombatPetCardTriggerUsageRegistry UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter UsageCommitter;
            public readonly CombatHpGainResolver HpGainResolver;
            public readonly FruitBatPetDamageTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatCardSuit sourceSuit =
                    CombatCardSuit.Fruit,
                int sourceHpCapacity = 10,
                int sourceCurrentHp = 10)
            {
                Side = side;
                Row = row;

                SourceCard = CreateCard(
                    1,
                    sourceSuit,
                    sourceHpCapacity,
                    sourceCurrentHp);

                SecondSourceCard = CreateCard(
                    2,
                    CombatCardSuit.Fruit,
                    10,
                    10);

                OtherRowCard = CreateCard(
                    3,
                    CombatCardSuit.Fruit,
                    10,
                    10);

                TargetCard = CreateCard(
                    4,
                    CombatCardSuit.Vegetable,
                    100,
                    100);

                var ownState =
                    CreateOwnSideState();

                var opposingState =
                    CreateOpposingSideState();

                var playerPets =
                    CreatePets(
                        CombatSide.Player);

                var enemyPets =
                    CreatePets(
                        CombatSide.Enemy);

                Pet = (side == CombatSide.Player
                    ? playerPets
                    : enemyPets)[
                        row == BoardRow.Front ? 0 : 1];

                State = new CombatState(
                    side == CombatSide.Player
                        ? ownState
                        : opposingState,
                    side == CombatSide.Player
                        ? opposingState
                        : ownState,
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            playerPets)),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            enemyPets)));

                Log = new CombatEventLog();

                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                RootEvent =
                    new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot());

                Log.Append(
                    RootEvent);

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                HpGainResolver =
                    new CombatHpGainResolver(
                        MetadataFactory,
                        Log);

                Handler =
                    new FruitBatPetDamageTriggerHandler(
                        side,
                        Pet.InstanceId,
                        UsageCommitter,
                        HpGainResolver,
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
                        RootEvent,
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

            private CombatSideState CreateOwnSideState()
            {
                return new CombatSideState(
                    new CombatBoardState(
                        Side,
                        new[]
                        {
                            Slot(1, SourcePosition, SourceCard),
                            Slot(2, SecondSourcePosition, SecondSourceCard),
                            Slot(3, OtherRowPosition, OtherRowCard),
                            new CombatSlotState(
                                new SlotId(4),
                                EmptyPosition)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            SourceCard,
                            SecondSourceCard,
                            OtherRowCard
                        }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private CombatSideState CreateOpposingSideState()
            {
                return new CombatSideState(
                    new CombatBoardState(
                        OpposingSide,
                        new[]
                        {
                            Slot(5, TargetPosition, TargetCard)
                        }),
                    new CombatCardRegistry(
                        new[] { TargetCard }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
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
                            "test.fruit_bat"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.fruit_bat"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                CombatCardSuit suit,
                int hpCapacity,
                int currentHp)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.fruit_bat_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(5),
                    suit,
                    CombatCardSeason.Spring,
                    hpCapacity,
                    currentHp,
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
