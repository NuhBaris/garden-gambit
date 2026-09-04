using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackTargetSeasonTests
    {
        [Test]
        public void
            AppendExchangeAttacks_CopiesOpposingTargetSeasons()
        {
            var environment =
                CreateEnvironment();

            environment.Resolver
                .AppendExchangeAttacks(
                    environment.ExchangeEvent,
                    CombatCardSeason.Summer,
                    CombatCardSeason.Winter);

            var playerAttackEvent =
                environment.EventLog.Events[1]
                    as NormalAttackCombatEvent;

            var enemyAttackEvent =
                environment.EventLog.Events[2]
                    as NormalAttackCombatEvent;

            Assert.That(
                playerAttackEvent,
                Is.Not.Null);

            Assert.That(
                enemyAttackEvent,
                Is.Not.Null);

            Assert.That(
                playerAttackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                playerAttackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                enemyAttackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                enemyAttackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Summer));
        }

        [Test]
        public void
            AppendAttack_WithTargetSeason_CopiesBothSeasons()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Player,
                    CombatCardSeason.Autumn,
                    CombatCardSeason.Winter);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Autumn));

            Assert.That(
                attackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                attackEvent.IsAutumnAttack,
                Is.True);

            Assert.That(
                attackEvent.IsWinterTarget,
                Is.True);
        }

        [Test]
        public void
            AppendAttack_WithoutSeasons_UsesUnspecified()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Player);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                attackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                attackEvent.HasSpecifiedTargetSeason,
                Is.False);
        }

        [Test]
        public void
            AppendAttack_WithOnlyAttackerSeason_LeavesTargetUnspecified()
        {
            var environment =
                CreateEnvironment();

            var attackEvent =
                environment.Resolver.AppendAttack(
                    environment.ExchangeEvent,
                    CombatSide.Enemy,
                    CombatCardSeason.Spring);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Spring));

            Assert.That(
                attackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Unspecified));

            Assert.That(
                attackEvent.IsSpringAttack,
                Is.True);

            Assert.That(
                attackEvent.HasSpecifiedTargetSeason,
                Is.False);
        }

        [Test]
        public void
            AppendAttack_WithInvalidTargetSeason_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                    () =>
                        environment.Resolver
                            .AppendAttack(
                                environment.ExchangeEvent,
                                CombatSide.Player,
                                CombatCardSeason.Summer,
                                (CombatCardSeason)999));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            Constructor_WithWinterTarget_SetsTargetFlags()
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            var attackEvent =
                new NormalAttackCombatEvent(
                    metadata,
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    CombatCardSeason.Summer,
                    new InstanceId(101),
                    CreatePosition(
                        CombatSide.Enemy),
                    CombatCardSeason.Winter,
                    baseDamage: 5);

            Assert.That(
                attackEvent.HasSpecifiedTargetSeason,
                Is.True);

            Assert.That(
                attackEvent.IsWinterTarget,
                Is.True);

            Assert.That(
                attackEvent.IsSpringTarget,
                Is.False);

            Assert.That(
                attackEvent.IsSummerTarget,
                Is.False);

            Assert.That(
                attackEvent.IsAutumnTarget,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var exchangeMetadata =
                metadataFactory.CreateRoot();

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    exchangeMetadata,
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    playerAttack: 5,
                    new InstanceId(101),
                    CreatePosition(
                        CombatSide.Enemy),
                    enemyAttack: 7);

            eventLog.Append(
                exchangeEvent);

            var resolver =
                new CombatNormalAttackEventResolver(
                    metadataFactory,
                    eventLog);

            return new TestEnvironment
            {
                EventLog =
                    eventLog,

                ExchangeEvent =
                    exchangeEvent,

                Resolver =
                    resolver
            };
        }

        private static BoardPosition
            CreatePosition(
                CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private sealed class TestEnvironment
        {
            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public NormalAttackExchangeCombatEvent
                ExchangeEvent
            {
                get;
                set;
            }

            public CombatNormalAttackEventResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}