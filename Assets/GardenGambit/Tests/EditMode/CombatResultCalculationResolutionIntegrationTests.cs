using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultCalculationResolutionIntegrationTests
    {
        [Test]
        public void Resolve_UsesResolutionSnapshotInProductionPipeline()
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
                new CombatResultCalculationResolver(
                    metadataFactory,
                    eventLog);

            var resultEvent =
                resolver.Resolve(
                    state,
                    combatStartedEvent);

            Assert.That(
                resultEvent.Resolution.IsValid,
                Is.True);

            Assert.That(
                resultEvent.Calculation.IsValid,
                Is.True);

            Assert.That(
                resultEvent.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(6));

            Assert.That(
                resultEvent.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(2));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resultEvent.PlayerDamageDelta,
                Is.Zero);

            Assert.That(
                resultEvent.EnemyDamageDelta,
                Is.Zero);

            Assert.That(
                resultEvent.HasAnyDamageModification,
                Is.False);

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
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
                    new SlotId(1),
                    new InstanceId(100),
                    "player-card",
                    rank: 3,
                    attackMultiplier: 2),
                CreateSideState(
                    CombatSide.Enemy,
                    new SlotId(2),
                    new InstanceId(200),
                    "enemy-card",
                    rank: 2,
                    attackMultiplier: 1));
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                SlotId slotId,
                InstanceId instanceId,
                string definitionId,
                int rank,
                int attackMultiplier)
        {
            var position =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));

            var card =
                new CombatCardState(
                    new DefinitionId(definitionId),
                    instanceId,
                    new CardRank(rank),
                    7,
                    7,
                    0,
                    3);

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
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    attackMultiplier));
        }
    }
}