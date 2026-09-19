using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatBattleStartPokerSnapshotTests
    {
        [Test]
        public void PokerHand_UsesCanonicalStableValues()
        {
            Assert.That(
                Enum.GetValues(typeof(CombatPokerHand)).Length,
                Is.EqualTo(13));
            Assert.That((int)CombatPokerHand.Unspecified, Is.EqualTo(0));
            Assert.That((int)CombatPokerHand.HighCard, Is.EqualTo(1));
            Assert.That((int)CombatPokerHand.Pair, Is.EqualTo(2));
            Assert.That((int)CombatPokerHand.TwoPair, Is.EqualTo(3));
            Assert.That((int)CombatPokerHand.ThreeOfAKind, Is.EqualTo(4));
            Assert.That((int)CombatPokerHand.Straight, Is.EqualTo(5));
            Assert.That((int)CombatPokerHand.Flush, Is.EqualTo(6));
            Assert.That((int)CombatPokerHand.FullHouse, Is.EqualTo(7));
            Assert.That((int)CombatPokerHand.FourOfAKind, Is.EqualTo(8));
            Assert.That((int)CombatPokerHand.StraightFlush, Is.EqualTo(9));
            Assert.That((int)CombatPokerHand.FiveOfAKind, Is.EqualTo(10));
            Assert.That((int)CombatPokerHand.FlushHouse, Is.EqualTo(11));
            Assert.That((int)CombatPokerHand.FlushFive, Is.EqualTo(12));
        }

        [Test]
        public void SideSnapshot_StoresBothRowPokerHands()
        {
            var snapshot = new CombatBattleStartSideSnapshot(
                CombatSide.Player,
                Array.Empty<CombatBattleStartCardSnapshot>(),
                CombatPokerHand.Pair,
                CombatPokerHand.FlushHouse);

            Assert.That(
                snapshot.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Pair));
            Assert.That(
                snapshot.BackPokerHand,
                Is.EqualTo(CombatPokerHand.FlushHouse));
            Assert.That(snapshot.HasSpecifiedFrontPokerHand, Is.True);
            Assert.That(snapshot.HasSpecifiedBackPokerHand, Is.True);
            Assert.That(
                snapshot.GetPokerHand(BoardRow.Front),
                Is.EqualTo(CombatPokerHand.Pair));
            Assert.That(
                snapshot.GetPokerHand(BoardRow.Back),
                Is.EqualTo(CombatPokerHand.FlushHouse));
        }

        [Test]
        public void SideSnapshot_LegacyConstructor_UsesUnspecifiedHands()
        {
            var snapshot = new CombatBattleStartSideSnapshot(
                CombatSide.Enemy,
                Array.Empty<CombatBattleStartCardSnapshot>());

            Assert.That(
                snapshot.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(snapshot.HasSpecifiedFrontPokerHand, Is.False);
            Assert.That(snapshot.HasSpecifiedBackPokerHand, Is.False);
        }

        [TestCase(-1)]
        [TestCase(13)]
        public void SideSnapshot_WithInvalidFrontPokerHand_Throws(
            int rawPokerHand)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    Array.Empty<CombatBattleStartCardSnapshot>(),
                    (CombatPokerHand)rawPokerHand,
                    CombatPokerHand.HighCard));
        }

        [TestCase(-1)]
        [TestCase(13)]
        public void SideSnapshot_WithInvalidBackPokerHand_Throws(
            int rawPokerHand)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    Array.Empty<CombatBattleStartCardSnapshot>(),
                    CombatPokerHand.HighCard,
                    (CombatPokerHand)rawPokerHand));
        }

        [TestCase(0)]
        [TestCase(99)]
        public void SideSnapshot_GetPokerHandWithInvalidRow_Throws(
            int rawRow)
        {
            var snapshot = new CombatBattleStartSideSnapshot(
                CombatSide.Player,
                Array.Empty<CombatBattleStartCardSnapshot>());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                snapshot.GetPokerHand((BoardRow)rawRow));
        }

        [Test]
        public void Resolver_TransfersLockedHandsForBothSides()
        {
            var card = new CombatCardState(
                new DefinitionId("test.poker_snapshot.card"),
                new InstanceId(1),
                new CardRank(6),
                CombatCardSuit.Fruit,
                CombatCardSeason.Spring,
                10,
                7,
                2,
                4);

            var playerPosition = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));

            var state = new CombatState(
                CreateSide(
                    CombatSide.Player,
                    new[] { card },
                    new CombatSlotState(
                        new SlotId(1),
                        playerPosition,
                        card.InstanceId)),
                CreateEmptySide(CombatSide.Enemy));

            var snapshot =
                new CombatBattleStartSnapshotResolver().Resolve(
                    state,
                    CombatPokerHand.HighCard,
                    CombatPokerHand.TwoPair,
                    CombatPokerHand.StraightFlush,
                    CombatPokerHand.FlushFive);

            Assert.That(
                snapshot.Player.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.HighCard));
            Assert.That(
                snapshot.Player.BackPokerHand,
                Is.EqualTo(CombatPokerHand.TwoPair));
            Assert.That(
                snapshot.Enemy.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.StraightFlush));
            Assert.That(
                snapshot.Enemy.BackPokerHand,
                Is.EqualTo(CombatPokerHand.FlushFive));
            Assert.That(snapshot.Player.Count, Is.EqualTo(1));
            Assert.That(
                snapshot.Player.Cards[0].Suit,
                Is.EqualTo(CombatCardSuit.Fruit));
        }

        [Test]
        public void Resolver_LegacyOverload_UsesUnspecifiedHands()
        {
            var state = new CombatState(
                CreateEmptySide(CombatSide.Player),
                CreateEmptySide(CombatSide.Enemy));

            var snapshot =
                new CombatBattleStartSnapshotResolver().Resolve(state);

            Assert.That(
                snapshot.Player.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.Player.BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.Enemy.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.Enemy.BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
        }

        [Test]
        public void Resolver_ExplicitOverloadWithNullState_Throws()
        {
            var resolver = new CombatBattleStartSnapshotResolver();

            Assert.Throws<ArgumentNullException>(() => resolver.Resolve(
                null,
                CombatPokerHand.HighCard,
                CombatPokerHand.Pair,
                CombatPokerHand.TwoPair,
                CombatPokerHand.Flush));
        }

        private static CombatSideState CreateEmptySide(
            CombatSide side)
        {
            return CreateSide(
                side,
                Array.Empty<CombatCardState>());
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState[] cards,
            params CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(side, slots),
                new CombatCardRegistry(cards),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));
        }
    }
}
