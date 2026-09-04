using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        NormalAttackCombatEventSeasonTests
    {
        [Test]
        public void
            LegacyConstructor_UsesUnspecifiedAttackerSeason()
        {
            var attackEvent =
                new NormalAttackCombatEvent(
                    CreateMetadata(),
                    new InstanceId(1),
                    CreatePlayerPosition(),
                    new InstanceId(2),
                    CreateEnemyPosition(),
                    baseDamage: 5);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                attackEvent
                    .HasSpecifiedAttackerSeason,
                Is.False);

            Assert.That(
                attackEvent.IsSpringAttack,
                Is.False);

            Assert.That(
                attackEvent.IsSummerAttack,
                Is.False);

            Assert.That(
                attackEvent.IsAutumnAttack,
                Is.False);

            Assert.That(
                attackEvent.IsWinterAttack,
                Is.False);
        }

        [Test]
        public void
            SeasonConstructor_WithSummer_SetsSummerAttack()
        {
            var attackEvent =
                CreateEvent(
                    CombatCardSeason.Summer);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                attackEvent
                    .HasSpecifiedAttackerSeason,
                Is.True);

            Assert.That(
                attackEvent.IsSummerAttack,
                Is.True);

            Assert.That(
                attackEvent.IsSpringAttack,
                Is.False);

            Assert.That(
                attackEvent.IsAutumnAttack,
                Is.False);

            Assert.That(
                attackEvent.IsWinterAttack,
                Is.False);

            Assert.That(
                attackEvent.AttackerSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                attackEvent.TargetSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(5));
        }

        [Test]
        public void
            SeasonConstructor_WithWinter_SetsWinterAttack()
        {
            var attackEvent =
                CreateEvent(
                    CombatCardSeason.Winter);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                attackEvent.IsWinterAttack,
                Is.True);

            Assert.That(
                attackEvent.IsSummerAttack,
                Is.False);
        }

        [Test]
        public void
            SeasonConstructor_WithValueBelowRange_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        (CombatCardSeason)(-1)));
        }

        [Test]
        public void
            SeasonConstructor_WithValueAboveRange_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateEvent(
                        (CombatCardSeason)5));
        }

        private static NormalAttackCombatEvent CreateEvent(
            CombatCardSeason attackerSeason)
        {
            return new NormalAttackCombatEvent(
                CreateMetadata(),
                new InstanceId(1),
                CreatePlayerPosition(),
                attackerSeason,
                new InstanceId(2),
                CreateEnemyPosition(),
                baseDamage: 5);
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                triggerRootId,
                triggerRootId);
        }

        private static BoardPosition
            CreatePlayerPosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static BoardPosition
            CreateEnemyPosition()
        {
            return new BoardPosition(
                CombatSide.Enemy,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}