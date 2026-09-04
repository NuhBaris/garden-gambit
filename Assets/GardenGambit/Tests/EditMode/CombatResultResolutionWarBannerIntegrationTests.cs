using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultResolutionWarBannerIntegrationTests
    {
        [Test]
        public void Resolve_WarBannerAndProtectiveSeal_CompleteCombinedPipeline()
        {
            var state =
                CreateState();

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var startResolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            var combatStartedEvent =
                startResolver.Start(
                    state);

            var resolver =
                new CombatResultResolutionResolver(
                    metadataFactory,
                    eventLog);

            var completedEvent =
                resolver.Resolve(
                    state,
                    combatStartedEvent);

            Assert.That(
                eventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(
                    combatStartedEvent));

            var resultEvent =
                eventLog.Events[1]
                    as CombatResultCalculatedCombatEvent;

            Assert.That(
                resultEvent,
                Is.Not.Null);

            Assert.That(
                resultEvent
                    .PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(20));

            Assert.That(
                resultEvent
                    .EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.PreventedDamageForPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(19));

            Assert.That(
                resultEvent.PreventedDamageForEnemy,
                Is.EqualTo(1L));

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                resultEvent.HasAnyDamageModification,
                Is.True);

            var playerHealthEvent =
                eventLog.Events[2]
                    as BattleHealthChangedCombatEvent;

            Assert.That(
                playerHealthEvent,
                Is.Not.Null);

            Assert.That(
                playerHealthEvent.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                playerHealthEvent.PreviousBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                playerHealthEvent.CurrentBattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                playerHealthEvent.Delta,
                Is.EqualTo(-2L));

            Assert.That(
                playerHealthEvent.ChangedAmount,
                Is.EqualTo(2L));

            var enemyHealthEvent =
                eventLog.Events[3]
                    as BattleHealthChangedCombatEvent;

            Assert.That(
                enemyHealthEvent,
                Is.Not.Null);

            Assert.That(
                enemyHealthEvent.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                enemyHealthEvent.PreviousBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                enemyHealthEvent.CurrentBattleHealth,
                Is.EqualTo(
                    new BattleHealth(1)));

            Assert.That(
                enemyHealthEvent.Delta,
                Is.EqualTo(-19L));

            Assert.That(
                enemyHealthEvent.ChangedAmount,
                Is.EqualTo(19L));

            Assert.That(
                eventLog.Events[4],
                Is.SameAs(
                    completedEvent));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(1)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(17L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(17L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.True);

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(18)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(1)));

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(1)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .AttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(1)));

            Assert.That(
                completedEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                completedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    resultEvent.Metadata.EventId));

            Assert.That(
                completedEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.TriggerRootId));
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player,
                    slotId: 1,
                    instanceId: 100,
                    definitionId: "player-card",
                    rank: 10,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .WarBanner),
                CreateSideState(
                    CombatSide.Enemy,
                    slotId: 2,
                    instanceId: 200,
                    definitionId: "enemy-card",
                    rank: 2,
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .ProtectiveSeal));
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                int slotId,
                long instanceId,
                string definitionId,
                int rank,
                CombatSlotEnhanceKind enhanceKind)
        {
            var position =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));

            var card =
                new CombatCardState(
                    new DefinitionId(definitionId),
                    new InstanceId(instanceId),
                    new CardRank(rank),
                    7,
                    7,
                    0,
                    3);

            var slot =
                new CombatSlotState(
                    new SlotId(slotId),
                    position,
                    card.InstanceId,
                    enhanceKind);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        slot
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(20),
                new AttackMultiplier(1));
        }
    }
}