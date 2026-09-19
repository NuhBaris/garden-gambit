using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        JackalopePetBattleEndTriggerHandlerTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Constructor_RejectsMissingDependency(
            bool missingUsage)
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new JackalopePetBattleEndTriggerHandler(
                    environment.Side,
                    environment.UpperPet.InstanceId,
                    missingUsage
                        ? null
                        : environment.Usage,
                    missingUsage
                        ? environment.Modifiers
                        : null));
        }

        [Test]
        public void Constructor_RejectsInvalidSide()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new JackalopePetBattleEndTriggerHandler(
                    (CombatSide)99,
                    environment.UpperPet.InstanceId,
                    environment.Usage,
                    environment.Modifiers));
        }

        [Test]
        public void Constructor_RejectsInvalidPetInstanceId()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new JackalopePetBattleEndTriggerHandler(
                    environment.Side,
                    default(InstanceId),
                    environment.Usage,
                    environment.Modifiers));
        }

        [TestCase(CombatPokerHand.Unspecified, 0)]
        [TestCase(CombatPokerHand.HighCard, 3)]
        [TestCase(CombatPokerHand.Pair, 2)]
        [TestCase(CombatPokerHand.TwoPair, 1)]
        [TestCase(CombatPokerHand.ThreeOfAKind, 0)]
        [TestCase(CombatPokerHand.Straight, 0)]
        [TestCase(CombatPokerHand.Flush, 0)]
        [TestCase(CombatPokerHand.FullHouse, 0)]
        [TestCase(CombatPokerHand.FourOfAKind, 0)]
        [TestCase(CombatPokerHand.StraightFlush, 0)]
        [TestCase(CombatPokerHand.FiveOfAKind, 0)]
        [TestCase(CombatPokerHand.FlushHouse, 0)]
        [TestCase(CombatPokerHand.FlushFive, 0)]
        public void LockedPokerHand_AwardsCanonicalBonus(
            CombatPokerHand pokerHand,
            int expectedBonus)
        {
            var environment = new Environment(
                frontPokerHand: pokerHand,
                frontLiving: 1);

            var handler =
                environment.Handler(
                    BoardRow.Front);

            Assert.That(
                handler.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                handler.FinalRankModifierRegistry,
                Is.SameAs(environment.Modifiers));
            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    environment.Event),
                Is.EqualTo(expectedBonus > 0));

            handler.Resolve(
                environment.State,
                environment.Event);

            var card =
                environment.Card(
                    BoardRow.Front,
                    1);

            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    card.InstanceId),
                Is.EqualTo(expectedBonus));
            Assert.That(
                environment.Usage.HasTriggered(
                    environment.UpperPet.InstanceId,
                    card.InstanceId),
                Is.EqualTo(expectedBonus > 0));
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(5, true)]
        public void LivingCount_RequiresAtLeastOneTarget(
            int livingCount,
            bool expectedCanTrigger)
        {
            var environment = new Environment(
                frontPokerHand: CombatPokerHand.HighCard,
                frontLiving: livingCount);

            var handler =
                environment.Handler(
                    BoardRow.Front);

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    environment.Event),
                Is.EqualTo(expectedCanTrigger));

            handler.Resolve(
                environment.State,
                environment.Event);

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        environment.Card(
                            BoardRow.Front,
                            column).InstanceId),
                    Is.EqualTo(
                        column <= livingCount
                            ? 3
                            : 0));
            }
        }

        [TestCase(CombatSide.Player, BoardRow.Front, 3)]
        [TestCase(CombatSide.Player, BoardRow.Back, 2)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 3)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 2)]
        public void Resolve_UsesOnlyOwningSideAndAffectedRow(
            CombatSide side,
            BoardRow row,
            int expectedBonus)
        {
            var environment = new Environment(
                side,
                CombatPokerHand.HighCard,
                CombatPokerHand.Pair,
                5,
                5);

            environment.Handler(row).Resolve(
                environment.State,
                environment.Event);

            foreach (var slot in
                     environment.State.GetSide(side).Board.Slots)
            {
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        slot.OccupantInstanceId.Value),
                    Is.EqualTo(
                        slot.Position.Row == row
                            ? expectedBonus
                            : 0));
            }

            foreach (var slot in
                     environment.State.GetOpposingSide(side)
                         .Board.Slots)
            {
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        slot.OccupantInstanceId.Value),
                    Is.Zero);
            }
        }

        [Test]
        public void BattleEndWithoutSnapshot_DoesNotTrigger()
        {
            var environment = new Environment();
            var handler =
                environment.Handler(
                    BoardRow.Front);

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    environment.LegacyEvent),
                Is.False);

            handler.Resolve(
                environment.State,
                environment.LegacyEvent);

            Assert.That(environment.Modifiers.Count, Is.Zero);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void DeathThresholdOccupant_IsNotBuffed(
            int currentHp)
        {
            var environment = new Environment(
                frontLiving: 5);
            var card =
                environment.Card(
                    BoardRow.Front,
                    5);

            card.SetCurrentHpToZero();
            if (currentHp < 0)
            {
                card.ApplyIncomingDamage(-currentHp);
            }

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            Assert.That(card.CurrentHp, Is.EqualTo(currentHp));
            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    card.InstanceId),
                Is.Zero);
            Assert.That(environment.Modifiers.Count, Is.EqualTo(4));
        }

        [Test]
        public void RegisteredCardWithoutBoardSlot_IsNotBuffed()
        {
            var environment = new Environment(
                frontLiving: 5);
            var position = new BoardPosition(
                environment.Side,
                BoardRow.Front,
                new BoardColumn(1));
            var removed =
                environment.State.GetSide(environment.Side)
                    .RemoveCard(position);

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    removed.InstanceId),
                Is.Zero);
            Assert.That(environment.Modifiers.Count, Is.EqualTo(4));
        }

        [Test]
        public void Resolve_RechecksLivingTargetsAfterDiscovery()
        {
            var environment = new Environment(
                frontLiving: 4);
            var handler =
                environment.Handler(
                    BoardRow.Front);

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    environment.Event),
                Is.True);

            environment.Card(BoardRow.Front, 4)
                .SetCurrentHpToZero();

            handler.Resolve(
                environment.State,
                environment.Event);

            Assert.That(environment.Modifiers.Count, Is.EqualTo(3));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void ResolveTwice_DoesNotRepeatBonus()
        {
            var environment = new Environment();
            var handler =
                environment.Handler(
                    BoardRow.Front);

            handler.Resolve(environment.State, environment.Event);
            handler.Resolve(environment.State, environment.Event);

            Assert.That(
                handler.CanTrigger(
                    environment.State,
                    environment.Event),
                Is.False);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(4));

            for (var column = 1;
                 column <= 4;
                 column++)
            {
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        environment.Card(
                            BoardRow.Front,
                            column).InstanceId),
                    Is.EqualTo(3));
            }
        }

        [Test]
        public void UsedCardIsSkipped_WhileOthersReceiveBonus()
        {
            var environment = new Environment();
            var first =
                environment.Card(
                    BoardRow.Front,
                    1);

            environment.Usage.TryCommit(
                environment.UpperPet.InstanceId,
                first.InstanceId,
                () => environment.Modifiers.AddModifier(
                    first.InstanceId,
                    3));

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(4));

            for (var column = 1;
                 column <= 4;
                 column++)
            {
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        environment.Card(
                            BoardRow.Front,
                            column).InstanceId),
                    Is.EqualTo(3));
            }
        }

        [Test]
        public void TwoJackalopes_KeepIndependentRowUses()
        {
            var environment = new Environment(
                frontPokerHand: CombatPokerHand.HighCard,
                backPokerHand: CombatPokerHand.Pair,
                frontLiving: 4,
                backLiving: 4);

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);
            environment.Handler(BoardRow.Back).Resolve(
                environment.State,
                environment.Event);

            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(8));

            for (var column = 1;
                 column <= 4;
                 column++)
            {
                var frontCard =
                    environment.Card(
                        BoardRow.Front,
                        column);
                var backCard =
                    environment.Card(
                        BoardRow.Back,
                        column);

                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        frontCard.InstanceId),
                    Is.EqualTo(3));
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        backCard.InstanceId),
                    Is.EqualTo(2));
                Assert.That(
                    environment.Usage.HasTriggered(
                        environment.UpperPet.InstanceId,
                        backCard.InstanceId),
                    Is.False);
            }
        }

        [Test]
        public void Resolve_CommitsTargetsLeftToRight()
        {
            var environment = new Environment(
                frontLiving: 5);

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            for (var index = 0;
                 index < 5;
                 index++)
            {
                Assert.That(
                    environment.Usage.UsageRegistry
                        .Keys[index].CardInstanceId,
                    Is.EqualTo(
                        environment.Card(
                            BoardRow.Front,
                            index + 1).InstanceId));
            }
        }

        [Test]
        public void OverflowOnLastTarget_LeavesPendingTargetsUntouched()
        {
            var environment = new Environment();
            var last =
                environment.Card(
                    BoardRow.Front,
                    4);

            environment.Modifiers.AddModifier(
                last.InstanceId,
                int.MaxValue - 2);

            Assert.Throws<OverflowException>(() =>
                environment.Handler(BoardRow.Front).Resolve(
                    environment.State,
                    environment.Event));

            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
            Assert.That(environment.Modifiers.Count, Is.EqualTo(1));

            for (var column = 1;
                 column < 4;
                 column++)
            {
                Assert.That(
                    environment.Modifiers.GetTotalModifier(
                        environment.Card(
                            BoardRow.Front,
                            column).InstanceId),
                    Is.Zero);
            }

            environment.Modifiers.AddModifier(
                last.InstanceId,
                -(int.MaxValue - 2));

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(4));
        }

        [Test]
        public void Bonus_AddsAfterExistingFinalRankContribution()
        {
            var environment = new Environment(
                frontLiving: 1);
            var card =
                environment.Card(
                    BoardRow.Front,
                    1);

            environment.Modifiers.AddModifier(
                card.InstanceId,
                4);

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            Assert.That(card.Rank.Value, Is.EqualTo(2));
            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    card.InstanceId),
                Is.EqualTo(7));
            Assert.That(
                new CombatFinalRankContributionResolver(
                        environment.Modifiers)
                    .Resolve(
                        card.InstanceId,
                        40),
                Is.EqualTo(47));
        }

        [Test]
        public void NonBattleEndEvent_DoesNotTrigger()
        {
            var environment = new Environment();
            var started =
                new CombatStartedCombatEvent(
                    new CombatEventMetadataFactory(
                            new CombatEventIdAllocator(),
                            new CombatSequenceNumberAllocator())
                        .CreateRoot());

            Assert.That(
                environment.Handler(BoardRow.Front)
                    .CanTrigger(
                        environment.State,
                        started),
                Is.False);
            Assert.That(environment.Modifiers.Count, Is.Zero);
        }

        [Test]
        public void RankChangesAfterSnapshot_DoNotChangeLockedPokerBonus()
        {
            var environment = new Environment(
                frontPokerHand: CombatPokerHand.Pair,
                frontLiving: 1);
            var card =
                environment.Card(
                    BoardRow.Front,
                    1);

            card.SetRank(new CardRank(14));

            environment.Handler(BoardRow.Front).Resolve(
                environment.State,
                environment.Event);

            Assert.That(card.Rank.Value, Is.EqualTo(14));
            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    card.InstanceId),
                Is.EqualTo(2));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState UpperPet =
                Pet(1001);
            public readonly CombatPetState LowerPet =
                Pet(1002);
            public readonly CombatPetCardTriggerUsageCommitter
                Usage =
                    new CombatPetCardTriggerUsageCommitter(
                        new CombatPetCardTriggerUsageRegistry());
            public readonly CombatFinalRankModifierRegistry
                Modifiers =
                    new CombatFinalRankModifierRegistry();
            public readonly BattleEndStartedCombatEvent Event;
            public readonly BattleEndStartedCombatEvent LegacyEvent;

            public Environment(
                CombatSide side = CombatSide.Player,
                CombatPokerHand frontPokerHand =
                    CombatPokerHand.HighCard,
                CombatPokerHand backPokerHand =
                    CombatPokerHand.Pair,
                int frontLiving = 4,
                int backLiving = 5)
            {
                Side = side;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                var own =
                    CreateSide(
                        side,
                        frontLiving,
                        backLiving);
                var opposing =
                    CreateSide(
                        opposingSide,
                        5,
                        5);

                var ownPets =
                    new CombatSidePetState(
                        side,
                        new CombatPetRegistry(
                            new[]
                            {
                                UpperPet,
                                LowerPet
                            }));
                var opposingPets =
                    new CombatSidePetState(
                        opposingSide,
                        new CombatPetRegistry(
                            Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? own
                        : opposing,
                    side == CombatSide.Enemy
                        ? own
                        : opposing,
                    side == CombatSide.Player
                        ? ownPets
                        : opposingPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : opposingPets);

                var snapshotResolver =
                    new CombatBattleStartSnapshotResolver();

                var snapshot =
                    snapshotResolver.Resolve(
                        State,
                        side == CombatSide.Player
                            ? frontPokerHand
                            : CombatPokerHand.FlushFive,
                        side == CombatSide.Player
                            ? backPokerHand
                            : CombatPokerHand.FlushFive,
                        side == CombatSide.Enemy
                            ? frontPokerHand
                            : CombatPokerHand.FlushFive,
                        side == CombatSide.Enemy
                            ? backPokerHand
                            : CombatPokerHand.FlushFive);

                var metadata =
                    new CombatEventMetadata(
                        new CombatEventId(2),
                        new CombatSequenceNumber(2),
                        new CombatEventId(1),
                        new CombatEventId(1));

                Event =
                    new BattleEndStartedCombatEvent(
                        metadata,
                        snapshot);

                LegacyEvent =
                    new BattleEndStartedCombatEvent(
                        metadata);
            }

            public JackalopePetBattleEndTriggerHandler Handler(
                BoardRow row)
            {
                return new JackalopePetBattleEndTriggerHandler(
                    Side,
                    row == BoardRow.Front
                        ? UpperPet.InstanceId
                        : LowerPet.InstanceId,
                    Usage,
                    Modifiers);
            }

            public CombatCardState Card(
                BoardRow row,
                int column)
            {
                return State.GetSide(Side).Cards.GetCard(
                    new InstanceId(
                        CardId(
                            Side,
                            row,
                            column)));
            }

            private static CombatPetState Pet(
                long instanceId)
            {
                return new CombatPetState(
                    new DefinitionId(
                        "test.pet.jackalope"),
                    new InstanceId(instanceId));
            }

            private static long CardId(
                CombatSide side,
                BoardRow row,
                int column)
            {
                return (side == CombatSide.Player
                           ? 0
                           : 100) +
                       (row == BoardRow.Front
                           ? 0
                           : 5) +
                       column;
            }

            private static CombatSideState CreateSide(
                CombatSide side,
                int frontLiving,
                int backLiving)
            {
                var cards =
                    new List<CombatCardState>();
                var slots =
                    new List<CombatSlotState>();

                for (var column = 5;
                     column >= 1;
                     column--)
                {
                    foreach (var row in
                             new[]
                             {
                                 BoardRow.Back,
                                 BoardRow.Front
                             })
                    {
                        var instanceId =
                            CardId(
                                side,
                                row,
                                column);

                        var livingCount =
                            row == BoardRow.Front
                                ? frontLiving
                                : backLiving;

                        var card =
                            new CombatCardState(
                                new DefinitionId(
                                    "test.jackalope.card"),
                                new InstanceId(instanceId),
                                new CardRank(2),
                                5,
                                column <= livingCount
                                    ? 5
                                    : 0,
                                0,
                                2);

                        cards.Add(card);
                        slots.Add(
                            new CombatSlotState(
                                new SlotId(instanceId),
                                new BoardPosition(
                                    side,
                                    row,
                                    new BoardColumn(column)),
                                card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(
                        cards),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));
            }
        }
    }
}
