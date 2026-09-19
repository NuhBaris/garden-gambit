using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        HazelDormousePetNormalAttackTriggerHandlerTests
    {
        [Test]
        public void Constructor_ValidatesAndExposesDependencies()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentNullException>(
                () => new
                    HazelDormousePetNormalAttackTriggerHandler(
                        environment.Side,
                        environment.Pet.InstanceId,
                        null,
                        environment.ReductionRegistry));

            Assert.Throws<ArgumentNullException>(
                () => new
                    HazelDormousePetNormalAttackTriggerHandler(
                        environment.Side,
                        environment.Pet.InstanceId,
                        environment.UsageCommitter,
                        null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                environment.Handler
                    .TargetDamageReductionRegistry,
                Is.SameAs(
                    environment.ReductionRegistry));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FirstNormalDamageToNutCard_IsReducedByOne(
            CombatSide side,
            BoardRow row)
        {
            var environment =
                new Environment(
                    side,
                    row);

            var attackEvent =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    attackEvent),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.ReductionRegistry
                    .GetRequests(
                        attackEvent.Metadata.EventId)
                    .Count,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.ReductionResolver
                    .ResolveDamage(
                        attackEvent,
                        incomingDamage: 5),
                Is.EqualTo(4));

            Assert.That(
                environment.UsageCommitter.HasTriggered(
                    environment.Pet.InstanceId,
                    environment.TargetCard.InstanceId),
                Is.True);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);
        }

        [TestCase(CombatCardSuit.Unspecified)]
        [TestCase(CombatCardSuit.Fruit)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Drink)]
        public void NonNutCard_DoesNotTrigger(
            CombatCardSuit suit)
        {
            var environment =
                new Environment(
                    targetSuit: suit);

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition));
        }

        [Test]
        public void OpposingSideTarget_DoesNotTrigger()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition,
                    environment.AttackerCard,
                    environment.AttackerPosition));
        }

        [Test]
        public void NutTargetInOtherRow_DoesNotTrigger()
        {
            var environment =
                new Environment();

            environment.AssertIgnored(
                environment.CreateAttack(
                    environment.OtherRowCard,
                    environment.OtherRowPosition));
        }

        [Test]
        public void SameNutCard_TriggersOnlyOnFirstDamage()
        {
            var environment =
                new Environment();

            var firstAttack =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            environment.Handler.Resolve(
                environment.State,
                firstAttack);

            environment.ReductionResolver.ResolveDamage(
                firstAttack,
                incomingDamage: 5);

            var secondAttack =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            environment.AssertIgnored(
                secondAttack,
                expectedUsageCount: 1);
        }

        [Test]
        public void DifferentNutCards_EachTriggerOnce()
        {
            var environment =
                new Environment();

            var firstAttack =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            var secondAttack =
                environment.CreateAttack(
                    environment.SecondTargetCard,
                    environment.SecondTargetPosition);

            environment.Handler.Resolve(
                environment.State,
                firstAttack);

            Assert.That(
                environment.ReductionResolver.ResolveDamage(
                    firstAttack,
                    incomingDamage: 5),
                Is.EqualTo(4));

            environment.Handler.Resolve(
                environment.State,
                secondAttack);

            Assert.That(
                environment.ReductionResolver.ResolveDamage(
                    secondAttack,
                    incomingDamage: 3),
                Is.EqualTo(2));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void RemovedTarget_DoesNotTriggerOrConsumeUsage()
        {
            var environment =
                new Environment();

            var attackEvent =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            environment.State.GetSide(
                    environment.Side)
                .RemoveCardFromCombat(
                    environment.TargetPosition);

            environment.AssertIgnored(
                attackEvent);
        }

        [Test]
        public void UnknownTargetIdentity_DoesNotTrigger()
        {
            var environment =
                new Environment();

            var attackEvent =
                environment.CreateAttack(
                    new InstanceId(999),
                    environment.TargetPosition,
                    baseDamage: 5);

            environment.AssertIgnored(
                attackEvent);
        }

        [Test]
        public void TargetIdentityMustOccupyEventPosition()
        {
            var environment =
                new Environment();

            var attackEvent =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.SecondTargetPosition);

            environment.AssertIgnored(
                attackEvent);
        }

        [Test]
        public void ZeroIncomingDamage_DoesNotConsumeFirstDamageUsage()
        {
            var environment =
                new Environment();

            var blockedAttack =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            environment.Handler.Resolve(
                environment.State,
                blockedAttack);

            Assert.That(
                environment.ReductionResolver.ResolveDamage(
                    blockedAttack,
                    incomingDamage: 0),
                Is.Zero);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            var damagingAttack =
                environment.CreateAttack(
                    environment.TargetCard,
                    environment.TargetPosition);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    damagingAttack),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                damagingAttack);

            Assert.That(
                environment.ReductionResolver.ResolveDamage(
                    damagingAttack,
                    incomingDamage: 2),
                Is.EqualTo(1));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        private sealed class Environment
        {
            private long _nextEventId = 2;

            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState TargetCard;
            public readonly CombatCardState SecondTargetCard;
            public readonly CombatCardState OtherRowCard;
            public readonly CombatCardState AttackerCard;
            public readonly CombatPetState Pet;
            public readonly CombatPetCardTriggerUsageRegistry
                UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter
                UsageCommitter;
            public readonly
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry;
            public readonly
                CombatNormalAttackTargetDamageReductionResolver
                ReductionResolver;
            public readonly
                HazelDormousePetNormalAttackTriggerHandler
                Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatCardSuit targetSuit =
                    CombatCardSuit.Nut)
            {
                Side = side;
                Row = row;

                TargetCard =
                    CreateCard(
                        1,
                        targetSuit);

                SecondTargetCard =
                    CreateCard(
                        2,
                        CombatCardSuit.Nut);

                OtherRowCard =
                    CreateCard(
                        3,
                        CombatCardSuit.Nut);

                AttackerCard =
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

                ReductionRegistry =
                    new
                        CombatNormalAttackTargetDamageReductionRegistry();

                ReductionResolver =
                    new
                        CombatNormalAttackTargetDamageReductionResolver(
                            ReductionRegistry,
                            UsageCommitter);

                Handler =
                    new
                        HazelDormousePetNormalAttackTriggerHandler(
                            side,
                            Pet.InstanceId,
                            UsageCommitter,
                            ReductionRegistry);
            }

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            public BoardRow OtherRow =>
                Row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

            public BoardPosition TargetPosition =>
                Position(
                    Side,
                    Row,
                    1);

            public BoardPosition SecondTargetPosition =>
                Position(
                    Side,
                    Row,
                    2);

            public BoardPosition OtherRowPosition =>
                Position(
                    Side,
                    OtherRow,
                    1);

            public BoardPosition AttackerPosition =>
                Position(
                    OpposingSide,
                    BoardRow.Front,
                    1);

            public NormalAttackCombatEvent CreateAttack(
                CombatCardState target,
                BoardPosition targetPosition,
                int baseDamage = 5)
            {
                return CreateAttack(
                    AttackerCard,
                    AttackerPosition,
                    target,
                    targetPosition,
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
                InstanceId targetInstanceId,
                BoardPosition targetPosition,
                int baseDamage)
            {
                return CreateAttack(
                    AttackerCard.InstanceId,
                    AttackerPosition,
                    targetInstanceId,
                    targetPosition,
                    baseDamage);
            }

            public void AssertIgnored(
                NormalAttackCombatEvent attackEvent,
                int expectedUsageCount = 0)
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
                    ReductionRegistry.HasRequests(
                        attackEvent.Metadata.EventId),
                    Is.False);

                Assert.That(
                    UsageRegistry.Count,
                    Is.EqualTo(
                        expectedUsageCount));
            }

            private NormalAttackCombatEvent CreateAttack(
                InstanceId attackerInstanceId,
                BoardPosition attackerPosition,
                InstanceId targetInstanceId,
                BoardPosition targetPosition,
                int baseDamage)
            {
                var eventId =
                    _nextEventId++;

                return new NormalAttackCombatEvent(
                    CreateMetadata(
                        eventId),
                    attackerInstanceId,
                    attackerPosition,
                    targetInstanceId,
                    targetPosition,
                    baseDamage);
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
                                TargetPosition,
                                TargetCard),
                            Slot(
                                2,
                                SecondTargetPosition,
                                SecondTargetCard),
                            Slot(
                                3,
                                OtherRowPosition,
                                OtherRowCard)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            TargetCard,
                            SecondTargetCard,
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
                                AttackerPosition,
                                AttackerCard)
                        }),
                    new CombatCardRegistry(
                        new[] { AttackerCard }),
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
                            "test.hazel_dormouse"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.hazel_dormouse"),
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
                        "test.hazel_dormouse_card"),
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

            private static CombatEventMetadata CreateMetadata(
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
}
