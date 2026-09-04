using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultCalculationWarBannerIntegrationTests
    {
        [Test]
        public void Resolve_AppliesWarBannerBeforeTargetProtectiveSeal()
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

            var resultResolver =
                new CombatResultCalculationResolver(
                    metadataFactory,
                    eventLog);

            var resultEvent =
                resultResolver.Resolve(
                    state,
                    combatStartedEvent);

            Assert.That(
                resultEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .CombatResultCalculated));

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
                resultEvent.PlayerDamageDelta,
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

            Assert.That(
                resultEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                resultEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                resultEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(
                    combatStartedEvent));

            Assert.That(
                eventLog.Events[1],
                Is.SameAs(
                    resultEvent));

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
                state.GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));
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