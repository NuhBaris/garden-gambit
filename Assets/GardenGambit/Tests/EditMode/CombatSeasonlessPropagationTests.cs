using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSeasonlessPropagationTests
    {
        [Test]
        public void
            Seasonless_UsesStableDistinctEnumValue()
        {
            Assert.That(
                (int)CombatCardSeason.Seasonless,
                Is.EqualTo(5));

            Assert.That(
                CombatCardSeason.Seasonless,
                Is.Not.EqualTo(
                    CombatCardSeason.Unspecified));
        }

        [Test]
        public void
            CardState_WithSeasonless_ExposesSeasonlessIdentity()
        {
            var card =
                CreateCard(
                    "seasonless-card",
                    new InstanceId(1),
                    CombatCardSeason.Seasonless);

            Assert.That(
                card.Season,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                card.HasSpecifiedSeason,
                Is.True);

            Assert.That(
                card.IsSeasonless,
                Is.True);

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
            BattleStartSnapshot_FromSeasonlessCard_PreservesIdentity()
        {
            var position =
                CreatePosition(
                    CombatSide.Player);

            var card =
                CreateCard(
                    "seasonless-card",
                    new InstanceId(1),
                    CombatCardSeason.Seasonless);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    position);

            Assert.That(
                snapshot.Season,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                snapshot.HasSpecifiedSeason,
                Is.True);

            Assert.That(
                snapshot.IsSeasonless,
                Is.True);

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

        [Test]
        public void
            NormalAttackEvent_WithSeasonlessSides_ExposesBothIdentities()
        {
            var attackEvent =
                new NormalAttackCombatEvent(
                    CreateChildMetadata(),
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    CombatCardSeason.Seasonless,
                    new InstanceId(101),
                    CreatePosition(
                        CombatSide.Enemy),
                    CombatCardSeason.Seasonless,
                    baseDamage: 3);

            Assert.That(
                attackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                attackEvent.TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                attackEvent.HasSpecifiedAttackerSeason,
                Is.True);

            Assert.That(
                attackEvent.HasSpecifiedTargetSeason,
                Is.True);

            Assert.That(
                attackEvent.IsSeasonlessAttack,
                Is.True);

            Assert.That(
                attackEvent.IsSeasonlessTarget,
                Is.True);

            Assert.That(
                attackEvent.IsSummerAttack,
                Is.False);

            Assert.That(
                attackEvent.IsWinterTarget,
                Is.False);
        }

        [Test]
        public void
            EventResolver_WithSeasonlessPlayer_CopiesAttackerAndTargetSeasons()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    metadataFactory.CreateRoot(),
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player),
                    playerAttack: 3,
                    new InstanceId(101),
                    CreatePosition(
                        CombatSide.Enemy),
                    enemyAttack: 2);

            eventLog.Append(
                exchangeEvent);

            var resolver =
                new CombatNormalAttackEventResolver(
                    metadataFactory,
                    eventLog);

            var batch =
                resolver.AppendExchangeAttacks(
                    exchangeEvent,
                    CombatCardSeason.Seasonless,
                    CombatCardSeason.Winter);

            Assert.That(
                batch.PlayerAttackEvent
                    .AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                batch.PlayerAttackEvent
                    .TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                batch.EnemyAttackEvent
                    .AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                batch.EnemyAttackEvent
                    .TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                batch.PlayerAttackEvent
                    .IsSeasonlessAttack,
                Is.True);

            Assert.That(
                batch.EnemyAttackEvent
                    .IsSeasonlessTarget,
                Is.True);
        }

        [Test]
        public void
            Preparation_WithSeasonlessPlayer_CopiesSeasonIntoAttackBatch()
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "player-seasonless",
                    new InstanceId(1),
                    CombatCardSeason.Seasonless);

            var enemyCard =
                CreateCard(
                    "enemy-winter",
                    new InstanceId(101),
                    CombatCardSeason.Winter);

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        playerCard,
                        playerPosition,
                        new SlotId(1)),
                    CreateSide(
                        CombatSide.Enemy,
                        enemyCard,
                        enemyPosition,
                        new SlotId(2)));

            var eventLog =
                new CombatEventLog();

            var resolver =
                new
                    CombatNormalAttackPreparationResolver(
                        CreateMetadataFactory(),
                        eventLog);

            var batch =
                resolver.Prepare(
                    state,
                    playerPosition,
                    enemyPosition);

            Assert.That(
                batch.PlayerAttackEvent
                    .AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                batch.PlayerAttackEvent
                    .TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                batch.EnemyAttackEvent
                    .AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Winter));

            Assert.That(
                batch.EnemyAttackEvent
                    .TargetSeason,
                Is.EqualTo(
                    CombatCardSeason.Seasonless));

            Assert.That(
                batch.PlayerAttackEvent
                    .IsSeasonlessAttack,
                Is.True);

            Assert.That(
                batch.EnemyAttackEvent
                    .IsSeasonlessTarget,
                Is.True);
        }

        private static CombatCardState CreateCard(
            string definitionId,
            InstanceId instanceId,
            CombatCardSeason season)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                instanceId,
                new CardRank(5),
                season,
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 3);
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState card,
            BoardPosition position,
            SlotId slotId)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
                            position,
                            card.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private static CombatEventMetadata
            CreateChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }
    }
}