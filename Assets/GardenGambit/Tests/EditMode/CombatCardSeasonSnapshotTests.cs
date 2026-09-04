using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardSeasonSnapshotTests
    {
        [Test]
        public void
            LegacyConstructor_UsesUnspecifiedSeason()
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "legacy-card"),
                    new InstanceId(1),
                    new CardRank(5),
                    hpCapacity: 10,
                    currentHp: 8,
                    armor: 2,
                    attack: 4);

            Assert.That(
                card.Season,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                card.HasSpecifiedSeason,
                Is.False);

            Assert.That(
                card.IsSpring,
                Is.False);

            Assert.That(
                card.IsSummer,
                Is.False);

            Assert.That(
                card.IsAutumn,
                Is.False);

            Assert.That(
                card.IsWinter,
                Is.False);
        }

        [Test]
        public void
            SeasonConstructor_WithSummer_SetsSummerIdentity()
        {
            var card =
                CreateCard(
                    CombatCardSeason.Summer);

            Assert.That(
                card.Season,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                card.HasSpecifiedSeason,
                Is.True);

            Assert.That(
                card.IsSummer,
                Is.True);

            Assert.That(
                card.IsSpring,
                Is.False);

            Assert.That(
                card.IsAutumn,
                Is.False);

            Assert.That(
                card.IsWinter,
                Is.False);
        }

        [Test]
        public void
            SeasonConstructor_WithValueBelowRange_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateCard(
                        (CombatCardSeason)(-1)));
        }

        [Test]
        public void
            SeasonConstructor_WithValueAboveRange_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateCard(
                        (CombatCardSeason)5));
        }

        [Test]
        public void
            Snapshot_FromSummerCard_CopiesSeasonIdentity()
        {
            var card =
                CreateCard(
                    CombatCardSeason.Summer);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    CreatePosition());

            Assert.That(
                snapshot.Season,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                snapshot.HasSpecifiedSeason,
                Is.True);

            Assert.That(
                snapshot.IsSummer,
                Is.True);

            Assert.That(
                snapshot.IsSpring,
                Is.False);

            Assert.That(
                snapshot.IsAutumn,
                Is.False);

            Assert.That(
                snapshot.IsWinter,
                Is.False);

            Assert.That(
                snapshot.InstanceId,
                Is.EqualTo(
                    card.InstanceId));

            Assert.That(
                snapshot.Position,
                Is.EqualTo(
                    CreatePosition()));
        }

        [Test]
        public void
            Snapshot_FromLegacyCard_PreservesUnspecifiedSeason()
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "legacy-card"),
                    new InstanceId(2),
                    new CardRank(6),
                    hpCapacity: 12,
                    currentHp: 9,
                    armor: 1,
                    attack: 5);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    CreatePosition());

            Assert.That(
                snapshot.Season,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                snapshot.HasSpecifiedSeason,
                Is.False);

            Assert.That(
                snapshot.IsSpring,
                Is.False);

            Assert.That(
                snapshot.IsSummer,
                Is.False);

            Assert.That(
                snapshot.IsAutumn,
                Is.False);

            Assert.That(
                snapshot.IsWinter,
                Is.False);
        }

        private static CombatCardState CreateCard(
            CombatCardSeason season)
        {
            return new CombatCardState(
                new DefinitionId(
                    "season-card"),
                new InstanceId(1),
                new CardRank(5),
                season,
                hpCapacity: 10,
                currentHp: 8,
                armor: 2,
                attack: 4);
        }

        private static BoardPosition CreatePosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}