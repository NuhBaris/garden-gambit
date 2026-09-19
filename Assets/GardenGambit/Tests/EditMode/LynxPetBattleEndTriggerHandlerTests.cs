using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class LynxPetBattleEndTriggerHandlerTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Constructor_RejectsMissingDependency(bool missingUsage)
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new LynxPetBattleEndTriggerHandler(
                e.Side, e.UpperPet.InstanceId, missingUsage ? null : e.Usage,
                missingUsage ? e.Modifiers : null));
        }

        [Test]
        public void Constructor_RejectsInvalidSide()
        {
            var e = new Environment();
            Assert.Throws<ArgumentOutOfRangeException>(() => new LynxPetBattleEndTriggerHandler(
                (CombatSide)99, e.UpperPet.InstanceId, e.Usage, e.Modifiers));
        }

        [Test]
        public void Constructor_RejectsInvalidPetId()
        {
            var e = new Environment();
            Assert.Throws<ArgumentException>(() => new LynxPetBattleEndTriggerHandler(
                e.Side, default(InstanceId), e.Usage, e.Modifiers));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void LivingCount_AwardsEachLivingEvenRankCardWithoutMinimumCount(int livingCount)
        {
            var e = new Environment(frontLiving: livingCount);
            var handler = e.Handler(BoardRow.Front);
            Assert.That(handler.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(handler.FinalRankModifierRegistry, Is.SameAs(e.Modifiers));
            Assert.That(handler.CanTrigger(e.State, e.Event), Is.EqualTo(livingCount > 0));
            Assert.That(e.Modifiers.Count, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            handler.Resolve(e.State, e.Event);
            for (var column = 1; column <= 5; column++)
            {
                var expected = column <= livingCount ? 2 : 0;
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(expected));
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Back, column).InstanceId), Is.Zero);
            }
            Assert.That(e.Modifiers.Count, Is.EqualTo(livingCount));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(e.Modifiers.Count));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Resolve_UsesOnlyOwningSideAndAffectedRow(CombatSide side, BoardRow row)
        {
            var e = new Environment(side, 5, 5);
            e.Handler(row).Resolve(e.State, e.Event);
            foreach (var slot in e.State.GetSide(side).Board.Slots)
            {
                Assert.That(e.Modifiers.GetTotalModifier(slot.OccupantInstanceId.Value),
                    Is.EqualTo(slot.Position.Row == row ? 2 : 0));
            }
            foreach (var slot in e.State.GetOpposingSide(side).Board.Slots)
            {
                Assert.That(e.Modifiers.GetTotalModifier(slot.OccupantInstanceId.Value), Is.Zero);
            }
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(5));
        }

        [Test]
        public void ResolveTwice_DoesNotRepeatBonus()
        {
            var e = new Environment();
            var handler = e.Handler(BoardRow.Front);
            handler.Resolve(e.State, e.Event);
            Assert.That(handler.CanTrigger(e.State, e.Event), Is.False);
            handler.Resolve(e.State, e.Event);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            for (var column = 1; column <= 4; column++)
            {
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(2));
            }
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void DeathThresholdOccupant_IsNeitherCountedNorBuffed(int currentHp)
        {
            var e = new Environment(frontLiving: 5);
            var card = e.Card(BoardRow.Front, 5);
            card.SetCurrentHpToZero();
            if (currentHp < 0) { card.ApplyIncomingDamage(-currentHp); }
            Assert.That(card.CurrentHp, Is.EqualTo(currentHp));
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.Count, Is.EqualTo(4));
            Assert.That(e.Modifiers.GetTotalModifier(card.InstanceId), Is.Zero);
            Assert.That(e.Usage.HasTriggered(e.UpperPet.InstanceId, card.InstanceId), Is.False);
        }

        [TestCase(4)]
        [TestCase(5)]
        public void LivingCardWithoutBoardSlot_IsNotCountedOrBuffed(int initialLiving)
        {
            var e = new Environment(frontLiving: initialLiving);
            var removed = e.Card(BoardRow.Front, 1);
            e.State.GetSide(e.Side).RemoveCard(new BoardPosition(e.Side, BoardRow.Front, new BoardColumn(1)));
            Assert.That(e.State.GetSide(e.Side).Cards.GetCard(removed.InstanceId), Is.SameAs(removed));
            var handler = e.Handler(BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, e.Event), Is.True);
            handler.Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.Count, Is.EqualTo(initialLiving - 1));
            Assert.That(e.Modifiers.GetTotalModifier(removed.InstanceId), Is.Zero);
        }

        [Test]
        public void Resolve_RechecksLivingTargetsAfterDiscovery()
        {
            var e = new Environment();
            var handler = e.Handler(BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, e.Event), Is.True);
            e.Card(BoardRow.Front, 4).SetCurrentHpToZero();
            handler.Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.Count, Is.EqualTo(3));
            Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, 4).InstanceId), Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(3));
        }

        [Test]
        public void UsedCardIsSkipped_WhileOtherEligibleCardsReceiveBonus()
        {
            var e = new Environment();
            var first = e.Card(BoardRow.Front, 1);
            e.Usage.TryCommit(e.UpperPet.InstanceId, first.InstanceId,
                () => e.Modifiers.AddModifier(first.InstanceId, 2));
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            for (var column = 1; column <= 4; column++)
            {
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(2));
            }
        }

        [Test]
        public void TwoLynxes_KeepIndependentUsesForTheirRows()
        {
            var e = new Environment(frontLiving: 4, backLiving: 4);
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(e.Handler(BoardRow.Back).CanTrigger(e.State, e.Event), Is.True);
            e.Handler(BoardRow.Back).Resolve(e.State, e.Event);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(8));
            for (var column = 1; column <= 4; column++)
            {
                var upperId = e.Card(BoardRow.Front, column).InstanceId;
                var lowerId = e.Card(BoardRow.Back, column).InstanceId;
                Assert.That(e.Usage.HasTriggered(e.UpperPet.InstanceId, upperId), Is.True);
                Assert.That(e.Usage.HasTriggered(e.LowerPet.InstanceId, lowerId), Is.True);
                Assert.That(e.Usage.HasTriggered(e.UpperPet.InstanceId, lowerId), Is.False);
                Assert.That(e.Modifiers.GetTotalModifier(upperId), Is.EqualTo(2));
                Assert.That(e.Modifiers.GetTotalModifier(lowerId), Is.EqualTo(2));
            }
        }

        [Test]
        public void Resolve_CommitsLeftToRightDespiteReversedSlotStorage()
        {
            var e = new Environment(frontLiving: 5);
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            for (var index = 0; index < 5; index++)
            {
                Assert.That(e.Usage.UsageRegistry.Keys[index].CardInstanceId,
                    Is.EqualTo(e.Card(BoardRow.Front, index + 1).InstanceId));
            }
        }

        [Test]
        public void OverflowOnLastTarget_LeavesAllPendingTargetsUntouchedAndAllowsRetry()
        {
            var e = new Environment();
            var last = e.Card(BoardRow.Front, 4);
            e.Modifiers.AddModifier(last.InstanceId, int.MaxValue - 1);
            var handler = e.Handler(BoardRow.Front);
            Assert.Throws<OverflowException>(() => handler.Resolve(e.State, e.Event));
            Assert.That(e.Modifiers.Count, Is.EqualTo(1));
            Assert.That(e.Modifiers.GetTotalModifier(last.InstanceId), Is.EqualTo(int.MaxValue - 1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            for (var column = 1; column < 4; column++)
            {
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.Zero);
            }
            e.Modifiers.AddModifier(last.InstanceId, -(int.MaxValue - 1));
            handler.Resolve(e.State, e.Event);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            for (var column = 1; column <= 4; column++)
            {
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(2));
            }
        }

        [Test]
        public void FinalRankBonus_AddsToExistingModifierAfterStructuralContribution()
        {
            var e = new Environment();
            var card = e.Card(BoardRow.Front, 1);
            card.SetRank(new CardRank(14));
            e.Modifiers.AddModifier(card.InstanceId, 3);
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(card.Rank.Value, Is.EqualTo(14));
            Assert.That(e.Modifiers.GetTotalModifier(card.InstanceId), Is.EqualTo(5));
            Assert.That(new CombatFinalRankContributionResolver(e.Modifiers).Resolve(card.InstanceId, 42), Is.EqualTo(47));
            Assert.That(card.CurrentHp, Is.EqualTo(5));
            Assert.That(card.Armor, Is.Zero);
            Assert.That(card.Attack, Is.EqualTo(2));
        }

        [Test]
        public void CombatStartedEvent_DoesNotTriggerBattleEndBonus()
        {
            var e = new Environment();
            var started = new CombatStartedCombatEvent(new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator()).CreateRoot());
            Assert.That(e.Handler(BoardRow.Front).CanTrigger(e.State, started), Is.False);
            Assert.That(e.Modifiers.Count, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [TestCase(2, true)]
        [TestCase(3, false)]
        [TestCase(4, true)]
        [TestCase(5, false)]
        [TestCase(6, true)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(9, false)]
        [TestCase(10, true)]
        [TestCase(11, false)]
        [TestCase(12, true)]
        [TestCase(13, false)]
        [TestCase(14, true)]
        public void RankRange_OnlyEvenCurrentRanksReceiveBonus(int rank, bool eligible)
        {
            var e = new Environment(frontLiving: 1);
            var card = e.Card(BoardRow.Front, 1);
            card.SetRank(new CardRank(rank));
            var handler = e.Handler(BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, e.Event), Is.EqualTo(eligible));
            handler.Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.GetTotalModifier(card.InstanceId), Is.EqualTo(eligible ? 2 : 0));
            Assert.That(e.Usage.HasTriggered(e.UpperPet.InstanceId, card.InstanceId), Is.EqualTo(eligible));
            Assert.That(card.Rank.Value, Is.EqualTo(rank));
        }

        [TestCase(3, 4)]
        [TestCase(4, 3)]
        public void Resolve_UsesRankAtResolutionTime(int before, int after)
        {
            var e = new Environment(frontLiving: 1);
            var card = e.Card(BoardRow.Front, 1);
            card.SetRank(new CardRank(before));
            var handler = e.Handler(BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, e.Event), Is.EqualTo(before == 4));
            card.SetRank(new CardRank(after));
            handler.Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.GetTotalModifier(card.InstanceId), Is.EqualTo(after == 4 ? 2 : 0));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(after == 4 ? 1 : 0));
        }

        [TestCase(3, 1)]
        [TestCase(4, 3)]
        public void ExistingFinalRankModifier_DoesNotChangeParityCondition(int rank, int expectedModifier)
        {
            var e = new Environment(frontLiving: 1);
            var card = e.Card(BoardRow.Front, 1);
            card.SetRank(new CardRank(rank));
            e.Modifiers.AddModifier(card.InstanceId, 1);
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.GetTotalModifier(card.InstanceId), Is.EqualTo(expectedModifier));
            Assert.That(e.Usage.HasTriggered(e.UpperPet.InstanceId, card.InstanceId), Is.EqualTo(rank == 4));
        }

        [Test]
        public void AlreadyUsedTargetAtModifierLimit_DoesNotBlockOtherTargets()
        {
            var e = new Environment();
            var used = e.Card(BoardRow.Front, 4);
            e.Usage.TryCommit(e.UpperPet.InstanceId, used.InstanceId,
                () => e.Modifiers.AddModifier(used.InstanceId, int.MaxValue));
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(e.Modifiers.GetTotalModifier(used.InstanceId), Is.EqualTo(int.MaxValue));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            for (var column = 1; column <= 3; column++)
            {
                Assert.That(e.Modifiers.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(2));
            }
        }

        [Test]
        public void MixedRanks_OnlyEligibleCardsReceiveBonusInColumnOrder()
        {
            var e = new Environment(frontLiving: 5);
            var ranks = new[] { 4, 3, 12, 11, 14 };
            for (var index = 0; index < ranks.Length; index++)
            {
                e.Card(BoardRow.Front, index + 1).SetRank(new CardRank(ranks[index]));
            }
            e.Handler(BoardRow.Front).Resolve(e.State, e.Event);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(3));
            for (var index = 0; index < ranks.Length; index++)
            {
                var card = e.Card(BoardRow.Front, index + 1);
                Assert.That(e.Modifiers.GetTotalModifier(card.InstanceId), Is.EqualTo(index % 2 == 0 ? 2 : 0));
            }
            for (var index = 0; index < 3; index++)
            {
                Assert.That(e.Usage.UsageRegistry.Keys[index].CardInstanceId,
                    Is.EqualTo(e.Card(BoardRow.Front, index * 2 + 1).InstanceId));
            }
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState UpperPet = Pet(1001);
            public readonly CombatPetState LowerPet = Pet(1002);
            public readonly CombatPetCardTriggerUsageCommitter Usage =
                new CombatPetCardTriggerUsageCommitter(new CombatPetCardTriggerUsageRegistry());
            public readonly CombatFinalRankModifierRegistry Modifiers = new CombatFinalRankModifierRegistry();
            public readonly BattleEndStartedCombatEvent Event = new BattleEndStartedCombatEvent(
                new CombatEventMetadata(new CombatEventId(2), new CombatSequenceNumber(2),
                    new CombatEventId(1), new CombatEventId(1)));

            public Environment(CombatSide side = CombatSide.Player, int frontLiving = 4, int backLiving = 5)
            {
                Side = side;
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var own = CreateSide(side, frontLiving, backLiving);
                var other = CreateSide(otherSide, 5, 5);
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : other,
                    side == CombatSide.Enemy ? own : other,
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);
            }

            public LynxPetBattleEndTriggerHandler Handler(BoardRow row) => new LynxPetBattleEndTriggerHandler(
                Side, row == BoardRow.Front ? UpperPet.InstanceId : LowerPet.InstanceId, Usage, Modifiers);

            public CombatCardState Card(BoardRow row, int column) => State.GetSide(Side).Cards.GetCard(
                new InstanceId(CardId(Side, row, column)));

            private static CombatPetState Pet(long id) => new CombatPetState(new DefinitionId("test.pet.lynx"), new InstanceId(id));

            private static long CardId(CombatSide side, BoardRow row, int column) =>
                (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 5) + column;

            private static CombatSideState CreateSide(CombatSide side, int frontLiving, int backLiving)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();
                for (var column = 5; column >= 1; column--)
                {
                    foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                    {
                        var id = CardId(side, row, column);
                        var card = new CombatCardState(new DefinitionId("test.lynx.card"), new InstanceId(id),
                            new CardRank(4), 5, column <= (row == BoardRow.Front ? frontLiving : backLiving) ? 5 : 0, 0, 2);
                        cards.Add(card);
                        slots.Add(new CombatSlotState(new SlotId(id),
                            new BoardPosition(side, row, new BoardColumn(column)), card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
