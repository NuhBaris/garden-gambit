using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultResolutionProtectiveSealIntegrationTests
    {
        [Test]
        public void Resolve_PlayerSeal_ChangesDrawIntoPlayerVictory()
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
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(19));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resultEvent.PreventedDamageForPlayer,
                Is.EqualTo(1L));

            Assert.That(
                resultEvent.PreventedDamageForEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.PlayerDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.Zero);

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
                    new BattleHealth(1)));

            Assert.That(
                playerHealthEvent.Delta,
                Is.EqualTo(-19L));

            Assert.That(
                playerHealthEvent.ChangedAmount,
                Is.EqualTo(19L));

            Assert.That(
                playerHealthEvent.IsDamage,
                Is.True);

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
                    new BattleHealth(0)));

            Assert.That(
                enemyHealthEvent.Delta,
                Is.EqualTo(-20L));

            Assert.That(
                enemyHealthEvent.ChangedAmount,
                Is.EqualTo(20L));

            Assert.That(
                enemyHealthEvent.IsDamage,
                Is.True);

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
                    new BattleHealth(1)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(0)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(1L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(1L));

            Assert.That(
                completedEvent.IsPlayerVictory,
                Is.True);

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(1)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(0)));

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
                    enhanceKind:
                        CombatSlotEnhanceKind
                            .ProtectiveSeal),
                CreateSideState(
                    CombatSide.Enemy,
                    slotId: 2,
                    instanceId: 200,
                    definitionId: "enemy-card",
                    enhanceKind:
                        CombatSlotEnhanceKind.None));
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                int slotId,
                long instanceId,
                string definitionId,
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
                    new CardRank(10),
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
                new AttackMultiplier(2));
        }
    }
}