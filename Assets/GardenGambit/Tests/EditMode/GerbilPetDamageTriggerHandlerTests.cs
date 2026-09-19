using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        GerbilPetDamageTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front, 2)]
        [TestCase(CombatSide.Player, BoardRow.Back, 6)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 2)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 6)]
        public void EligibleRank_FirstSurvivedNormalAttack_GainsOneAttack(
            CombatSide side,
            BoardRow row,
            int rank)
        {
            var environment =
                new Environment(
                    side,
                    row,
                    rank);

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
                environment.SourceCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            var attackGainEvent =
                environment.Log.Events[
                    environment.Log.Count - 1]
                as AttackGainCombatEvent;

            Assert.That(
                attackGainEvent,
                Is.Not.Null);

            Assert.That(
                attackGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.SourceCard.InstanceId));

            Assert.That(
                attackGainEvent.ActualGainedAmount,
                Is.EqualTo(1));

            Assert.That(
                attackGainEvent.Metadata.ParentEventId,
                Is.EqualTo(
                    damageEvent.Metadata.EventId));
        }

        [TestCase(7)]
        [TestCase(10)]
        [TestCase(14)]
        public void RankOutsideTwoThroughSix_DoesNotTrigger(
            int rank)
        {
            var environment =
                new Environment(
                    sourceRank: rank);

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
        public void EachEligibleCardCanTriggerOnceAcrossRepeatedAttacks()
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
                environment.SourceCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.SecondSourceCard.Attack,
                Is.EqualTo(3));

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
                environment.SourceCard.Attack,
                Is.EqualTo(3));

            var attackGainEvent =
                environment.Log.Events[
                    environment.Log.Count - 1]
                as AttackGainCombatEvent;

            Assert.That(
                attackGainEvent.TargetPosition,
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
        public void AttackOverflow_DoesNotConsumeUsage()
        {
            var environment =
                new Environment(
                    sourceAttack: int.MaxValue);

            var damageEvent =
                environment.CreateNormalAttackDamage();

            Assert.Throws<OverflowException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    damageEvent));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(
                    int.MaxValue));

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
                environment.SourceCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void Constructor_ValidatesAndExposesDependencies()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentNullException>(
                () => new GerbilPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.AttackGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new GerbilPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new GerbilPetDamageTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.AttackGainResolver,
                    null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                environment.Handler.AttackGainResolver,
                Is.SameAs(
                    environment.AttackGainResolver));

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
            public readonly CombatAttackGainResolver AttackGainResolver;
            public readonly GerbilPetDamageTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int sourceRank = 5,
                int sourceAttack = 2)
            {
                Side = side;
                Row = row;

                SourceCard = CreateCard(
                    1,
                    sourceRank,
                    sourceAttack);

                SecondSourceCard = CreateCard(
                    2,
                    5,
                    2);

                OtherRowCard = CreateCard(
                    3,
                    5,
                    2);

                TargetCard = CreateCard(
                    4,
                    8,
                    2,
                    hp: 100);

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

                AttackGainResolver =
                    new CombatAttackGainResolver(
                        MetadataFactory,
                        Log);

                Handler =
                    new GerbilPetDamageTriggerHandler(
                        side,
                        Pet.InstanceId,
                        UsageCommitter,
                        AttackGainResolver,
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
                            "test.gerbil"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.gerbil"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                int rank,
                int attack,
                int hp = 10)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.gerbil_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(
                        rank),
                    CombatCardSuit.Unspecified,
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    attack);
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
