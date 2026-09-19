using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        OtterPetNormalAttackTriggerHandlerTests
    {
        [Test]
        public void Constructor_ValidatesAndExposesDependencies()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentNullException>(
                () => new OtterPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.AttackGainResolver));

            Assert.Throws<ArgumentNullException>(
                () => new OtterPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                environment.Handler.AttackGainResolver,
                Is.SameAs(
                    environment.AttackGainResolver));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FirstDrinkNormalAttack_GainsOneAttackAfterSnapshot(
            CombatSide side,
            BoardRow row)
        {
            var environment =
                new Environment(
                    side,
                    row);

            var attackEvent =
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent),
                Is.True);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            var gains =
                environment.AttackGains();

            Assert.That(
                gains.Count,
                Is.EqualTo(1));

            Assert.That(
                gains[0].TargetInstanceId,
                Is.EqualTo(
                    environment.SourceCard.InstanceId));

            Assert.That(
                gains[0].TargetPosition,
                Is.EqualTo(
                    environment.SourcePosition));

            Assert.That(
                gains[0].PreviousAttack,
                Is.EqualTo(5));

            Assert.That(
                gains[0].CurrentAttack,
                Is.EqualTo(6));

            Assert.That(
                gains[0].Metadata.ParentEventId.Value,
                Is.EqualTo(
                    attackEvent.Metadata.EventId));

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(6));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(5));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.Pet.InstanceId,
                    environment.SourceCard.InstanceId),
                Is.True);
        }

        [TestCase(CombatCardSuit.Unspecified)]
        [TestCase(CombatCardSuit.Fruit)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Nut)]
        public void NonDrinkCard_DoesNotTrigger(
            CombatCardSuit suit)
        {
            var environment =
                new Environment(
                    sourceSuit: suit);

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition));
        }

        [Test]
        public void OpposingSideAttack_DoesNotTrigger()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition,
                    environment.SourceCard,
                    environment.SourcePosition));
        }

        [Test]
        public void DrinkAttackFromOtherRow_DoesNotTrigger()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.OtherRowCard,
                    environment.OtherRowPosition));
        }

        [Test]
        public void SameDrinkCard_TriggersOnlyOnFirstAttack()
        {
            var environment =
                new Environment();

            var firstAttack =
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition);

            environment.Handler.Resolve(
                environment.State,
                firstAttack);

            var secondAttack =
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition);

            environment.AssertIgnored(
                secondAttack,
                expectedUsageCount: 1,
                expectedGainCount: 1);

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(6));
        }

        [Test]
        public void DifferentDrinkCards_EachTriggerOnce()
        {
            var environment =
                new Environment();

            var firstAttack =
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition);

            var secondAttack =
                environment.CreateAttack(
                    environment.SecondSourceCard,
                    environment.SecondSourcePosition);

            environment.Handler.Resolve(
                environment.State,
                firstAttack);

            environment.Handler.Resolve(
                environment.State,
                secondAttack);

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(6));

            Assert.That(
                environment.SecondSourceCard.Attack,
                Is.EqualTo(6));

            Assert.That(
                environment.AttackGains().Count,
                Is.EqualTo(2));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void RemovedAttacker_DoesNotTriggerOrConsumeUsage()
        {
            var environment =
                new Environment();

            var attackEvent =
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition);

            environment.State.GetSide(
                    environment.Side)
                .RemoveCardFromCombat(
                    environment.SourcePosition);

            environment.AssertIgnored(
                attackEvent);
        }

        [Test]
        public void UnknownAttackerIdentity_DoesNotTrigger()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateAttack(
                    new InstanceId(999),
                    environment.SourcePosition,
                    baseDamage: 5));
        }

        [Test]
        public void AttackerIdentityMustOccupyEventPosition()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SecondSourcePosition));
        }

        [Test]
        public void AttackOverflow_DoesNotConsumeUsageOrAppendGain()
        {
            var environment =
                new Environment();

            var attackEvent =
                environment.CreateAttack(
                    environment.SourceCard,
                    environment.SourcePosition);

            environment.SourceCard.ApplyAttackGain(
                int.MaxValue -
                environment.SourceCard.Attack);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent),
                Is.True);

            Assert.Throws<OverflowException>(
                () => environment.Handler.Resolve(
                    environment.State,
                    attackEvent));

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(
                    int.MaxValue));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(5));

            Assert.That(
                environment.AttackGains(),
                Is.Empty);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);
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
            public readonly CombatPetCardTriggerUsageRegistry
                UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter
                UsageCommitter;
            public readonly CombatEventMetadataFactory
                MetadataFactory;
            public readonly CombatEventLog Log;
            public readonly CombatAttackGainResolver
                AttackGainResolver;
            public readonly OtterPetNormalAttackTriggerHandler
                Handler;

            private readonly CombatStartedCombatEvent
                _rootEvent;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatCardSuit sourceSuit =
                    CombatCardSuit.Drink)
            {
                Side = side;
                Row = row;

                SourceCard =
                    CreateCard(
                        1,
                        sourceSuit);

                SecondSourceCard =
                    CreateCard(
                        2,
                        CombatCardSuit.Drink);

                OtherRowCard =
                    CreateCard(
                        3,
                        CombatCardSuit.Drink);

                TargetCard =
                    CreateCard(
                        4,
                        CombatCardSuit.Fruit);

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

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                Log =
                    new CombatEventLog();

                _rootEvent =
                    new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot());

                Log.Append(
                    _rootEvent);

                AttackGainResolver =
                    new CombatAttackGainResolver(
                        MetadataFactory,
                        Log);

                Handler =
                    new OtterPetNormalAttackTriggerHandler(
                        side,
                        Pet.InstanceId,
                        UsageCommitter,
                        AttackGainResolver);
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

            public BoardPosition TargetPosition =>
                Position(
                    OpposingSide,
                    BoardRow.Front,
                    1);

            public NormalAttackCombatEvent CreateAttack(
                CombatCardState attacker,
                BoardPosition attackerPosition,
                int baseDamage = 5)
            {
                return CreateAttack(
                    attacker,
                    attackerPosition,
                    TargetCard,
                    TargetPosition,
                    baseDamage);
            }

            public NormalAttackCombatEvent CreateAttack(
                CombatCardState attacker,
                BoardPosition attackerPosition,
                CombatCardState target,
                BoardPosition targetPosition,
                int baseDamage = 5)
            {
                return CreateAttack(
                    attacker.InstanceId,
                    attackerPosition,
                    target.InstanceId,
                    targetPosition,
                    baseDamage);
            }

            public NormalAttackCombatEvent CreateAttack(
                InstanceId attackerInstanceId,
                BoardPosition attackerPosition,
                int baseDamage)
            {
                return CreateAttack(
                    attackerInstanceId,
                    attackerPosition,
                    TargetCard.InstanceId,
                    TargetPosition,
                    baseDamage);
            }

            public List<AttackGainCombatEvent> AttackGains()
            {
                var gains =
                    new List<AttackGainCombatEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    var gain =
                        combatEvent as AttackGainCombatEvent;

                    if (gain != null)
                    {
                        gains.Add(
                            gain);
                    }
                }

                return gains;
            }

            public void AssertIgnored(
                NormalAttackCombatEvent attackEvent,
                int expectedUsageCount = 0,
                int expectedGainCount = 0)
            {
                Assert.That(
                    Handler.CanTrigger(
                        State,
                        attackEvent),
                    Is.False);

                Handler.Resolve(
                    State,
                    attackEvent);

                Assert.That(
                    UsageRegistry.Count,
                    Is.EqualTo(
                        expectedUsageCount));

                Assert.That(
                    AttackGains().Count,
                    Is.EqualTo(
                        expectedGainCount));
            }

            private NormalAttackCombatEvent CreateAttack(
                InstanceId attackerInstanceId,
                BoardPosition attackerPosition,
                InstanceId targetInstanceId,
                BoardPosition targetPosition,
                int baseDamage)
            {
                var attackEvent =
                    new NormalAttackCombatEvent(
                        MetadataFactory.CreateChild(
                            _rootEvent.Metadata),
                        attackerInstanceId,
                        attackerPosition,
                        targetInstanceId,
                        targetPosition,
                        baseDamage);

                Log.Append(
                    attackEvent);

                return attackEvent;
            }

            private CombatSideState CreateOwnSideState()
            {
                return new CombatSideState(
                    new CombatBoardState(
                        Side,
                        new[]
                        {
                            Slot(
                                1,
                                SourcePosition,
                                SourceCard),
                            Slot(
                                2,
                                SecondSourcePosition,
                                SecondSourceCard),
                            Slot(
                                3,
                                OtherRowPosition,
                                OtherRowCard)
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
                            Slot(
                                4,
                                TargetPosition,
                                TargetCard)
                        }),
                    new CombatCardRegistry(
                        new[] { TargetCard }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
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
                            "test.otter"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.otter"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                CombatCardSuit suit)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.otter_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(5),
                    suit,
                    CombatCardSeason.Spring,
                    10,
                    10,
                    0,
                    5);
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
