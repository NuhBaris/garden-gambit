using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        IguanaPetDeathTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front, 11)]
        [TestCase(CombatSide.Player, BoardRow.Back, 14)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 11)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 14)]
        public void EligibleRank_FirstNormalKillGainsOneHpStat(
            CombatSide side,
            BoardRow row,
            int rank)
        {
            var environment = new Environment(
                side,
                row,
                rank);

            var death = environment.CreateNormalKill();

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    death),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                death);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.SourceCard.CurrentHp,
                Is.EqualTo(11));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            var gain = environment.Log.Events[
                    environment.Log.Count - 1]
                as HpGainCombatEvent;

            Assert.That(gain, Is.Not.Null);
            Assert.That(
                gain.SourceInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));
            Assert.That(
                gain.TargetInstanceId,
                Is.EqualTo(
                    environment.SourceCard.InstanceId));
            Assert.That(
                gain.ActualGainedAmount,
                Is.EqualTo(1));
            Assert.That(
                gain.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    death.Metadata.EventId));
        }

        [TestCase(2)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(10)]
        public void RankBelowJackDoesNotTrigger(
            int rank)
        {
            var environment = new Environment(
                sourceRank: rank);

            environment.AssertIgnored(
                environment.CreateNormalKill());
        }

        [Test]
        public void EachEligibleCardCanTriggerOnceAcrossKills()
        {
            var environment = new Environment();

            var first = environment.CreateNormalKill();
            environment.Handler.Resolve(
                environment.State,
                first);

            var second = environment.CreateNormalKill(
                environment.SourceCard,
                environment.SourcePosition,
                environment.SecondTargetCard,
                environment.SecondTargetPosition);

            environment.AssertIgnored(
                second,
                expectedUsageCount: 1);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));
        }

        [Test]
        public void DifferentEligibleCardsUseIndependentCounters()
        {
            var environment = new Environment();

            var first = environment.CreateNormalKill();
            environment.Handler.Resolve(
                environment.State,
                first);

            var second = environment.CreateNormalKill(
                environment.SecondSourceCard,
                environment.SecondSourcePosition,
                environment.SecondTargetCard,
                environment.SecondTargetPosition);

            environment.Handler.Resolve(
                environment.State,
                second);

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
        public void KillerInOtherRowDoesNotConsumeUsage()
        {
            var environment = new Environment();

            environment.AssertIgnored(
                environment.CreateNormalKill(
                    environment.OtherRowCard,
                    environment.OtherRowPosition,
                    environment.TargetCard,
                    environment.TargetPosition));
        }

        [Test]
        public void DirectDamageKillIsNotNormalKill()
        {
            var environment = new Environment();

            environment.AssertIgnored(
                environment.CreateDirectDamageKill());
        }

        [Test]
        public void MovedKillerIsFoundByIdentity()
        {
            var environment = new Environment();
            var death = environment.CreateNormalKill();

            environment.State.GetSide(
                    environment.Side)
                .MoveCard(
                    environment.SourcePosition,
                    environment.EmptyPosition);

            environment.Handler.Resolve(
                environment.State,
                death);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            var gain = environment.Log.Events[
                    environment.Log.Count - 1]
                as HpGainCombatEvent;

            Assert.That(
                gain.TargetPosition,
                Is.EqualTo(
                    environment.EmptyPosition));
        }

        [Test]
        public void RemovedKillerDoesNotTransferGain()
        {
            var environment = new Environment();
            var death = environment.CreateNormalKill();

            environment.State.GetSide(
                    environment.Side)
                .RemoveCardFromCombat(
                    environment.SourcePosition);

            environment.AssertIgnored(
                death);
        }

        [Test]
        public void KillerAtDeathThresholdDoesNotTrigger()
        {
            var environment = new Environment();
            var death = environment.CreateNormalKill();

            environment.SourceCard.SetCurrentHpToZero();

            environment.AssertIgnored(
                death);
        }

        [Test]
        public void UnloggedDeathCopyDoesNotConsumeUsage()
        {
            var environment = new Environment();
            var death = environment.CreateNormalKill();

            var copy = new DeathCombatEvent(
                death.Metadata,
                death.InstanceId,
                death.Position,
                death.PreviousHp,
                death.CurrentHp);

            Assert.Throws<ArgumentException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    copy));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            environment.Handler.Resolve(
                environment.State,
                death);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));
        }

        [Test]
        public void ConstructorValidatesAndExposesDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetDeathTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetDeathTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetDeathTriggerHandler(
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
            public readonly CombatCardState SecondTargetCard;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log;
            public readonly CombatEventMetadataFactory MetadataFactory;
            public readonly CombatStartedCombatEvent RootEvent;
            public readonly CombatPetCardTriggerUsageRegistry UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter UsageCommitter;
            public readonly CombatHpGainResolver HpGainResolver;
            public readonly IguanaPetDeathTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int sourceRank = 12)
            {
                Side = side;
                Row = row;

                SourceCard = CreateCard(1, sourceRank, 10, 5);
                SecondSourceCard = CreateCard(2, 13, 10, 5);
                OtherRowCard = CreateCard(3, 14, 10, 5);
                TargetCard = CreateCard(4, 8, 5, 1);
                SecondTargetCard = CreateCard(5, 8, 5, 1);

                var ownState = CreateOwnSideState();
                var opposingState = CreateOpposingSideState();
                var playerPets = CreatePets(CombatSide.Player);
                var enemyPets = CreatePets(CombatSide.Enemy);

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
                        new CombatPetRegistry(playerPets)),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(enemyPets)));

                Log = new CombatEventLog();
                MetadataFactory = new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
                RootEvent = new CombatStartedCombatEvent(
                    MetadataFactory.CreateRoot());
                Log.Append(RootEvent);

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();
                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);
                HpGainResolver = new CombatHpGainResolver(
                    MetadataFactory,
                    Log);
                Handler = new IguanaPetDeathTriggerHandler(
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
                Position(Side, Row, 1);

            public BoardPosition SecondSourcePosition =>
                Position(Side, Row, 2);

            public BoardPosition OtherRowPosition =>
                Position(Side, OtherRow, 1);

            public BoardPosition EmptyPosition =>
                Position(Side, OtherRow, 2);

            public BoardPosition TargetPosition =>
                Position(OpposingSide, BoardRow.Front, 1);

            public BoardPosition SecondTargetPosition =>
                Position(OpposingSide, BoardRow.Front, 2);

            public DeathCombatEvent CreateNormalKill()
            {
                return CreateNormalKill(
                    SourceCard,
                    SourcePosition,
                    TargetCard,
                    TargetPosition);
            }

            public DeathCombatEvent CreateNormalKill(
                CombatCardState source,
                BoardPosition sourcePosition,
                CombatCardState target,
                BoardPosition targetPosition)
            {
                var attack = new NormalAttackCombatEvent(
                    MetadataFactory.CreateChild(
                        RootEvent.Metadata),
                    source.InstanceId,
                    sourcePosition,
                    target.InstanceId,
                    targetPosition,
                    source.Attack);
                Log.Append(attack);

                var damage = new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        attack,
                        sourcePosition,
                        targetPosition,
                        target.CurrentHp);

                return new CombatDeathEventResolver(
                        MetadataFactory,
                        Log)
                    .AppendFromDamage(damage);
            }

            public DeathCombatEvent CreateDirectDamageKill()
            {
                var damage = new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        RootEvent,
                        SourcePosition,
                        TargetPosition,
                        TargetCard.CurrentHp);

                return new CombatDeathEventResolver(
                        MetadataFactory,
                        Log)
                    .AppendFromDamage(damage);
            }

            public void AssertIgnored(
                DeathCombatEvent deathEvent,
                int expectedUsageCount = 0)
            {
                var eventCount = Log.Count;

                Assert.That(
                    Handler.CanTrigger(
                        State,
                        deathEvent),
                    Is.False);

                Handler.Resolve(
                    State,
                    deathEvent);

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
                            Slot(5, TargetPosition, TargetCard),
                            Slot(6, SecondTargetPosition, SecondTargetCard)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            TargetCard,
                            SecondTargetCard
                        }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatSlotState Slot(
                long slotId,
                BoardPosition position,
                CombatCardState card)
            {
                return new CombatSlotState(
                    new SlotId(slotId),
                    position,
                    card.InstanceId);
            }

            private static CombatPetState[] CreatePets(
                CombatSide side)
            {
                var baseId = side == CombatSide.Player
                    ? 1000
                    : 2000;

                return new[]
                {
                    new CombatPetState(
                        new DefinitionId("test.iguana"),
                        new InstanceId(baseId + 1)),
                    new CombatPetState(
                        new DefinitionId("test.iguana"),
                        new InstanceId(baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                int rank,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId("test.iguana_card"),
                    new InstanceId(instanceId),
                    new CardRank(rank),
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
                    new BoardColumn(column));
            }
        }
    }
}
