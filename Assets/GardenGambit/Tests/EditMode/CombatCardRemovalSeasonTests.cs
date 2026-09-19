using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardRemovalSeasonTests
    {
        [TestCase(CombatCardSeason.Unspecified, false)]
        [TestCase(CombatCardSeason.Spring, false)]
        [TestCase(CombatCardSeason.Summer, false)]
        [TestCase(CombatCardSeason.Autumn, false)]
        [TestCase(CombatCardSeason.Winter, false)]
        [TestCase(CombatCardSeason.Seasonless, false)]
        [TestCase(CombatCardSeason.Unspecified, true)]
        [TestCase(CombatCardSeason.Spring, true)]
        [TestCase(CombatCardSeason.Summer, true)]
        [TestCase(CombatCardSeason.Autumn, true)]
        [TestCase(CombatCardSeason.Winter, true)]
        [TestCase(CombatCardSeason.Seasonless, true)]
        public void Removal_PreservesSeasonThroughActiveAndRemovedLookup(
            CombatCardSeason season, bool directDelete)
        {
            var environment = CreateEnvironment(CombatSide.Player, season);
            var lookup = new CombatCardLookup(environment.EventLog);
            var active = lookup.Get(environment.State, environment.Target.InstanceId);

            Assert.That(active.IsActive, Is.True);
            Assert.That(active.ActiveCard, Is.SameAs(environment.Target));
            Assert.That(active.Season, Is.EqualTo(season));

            CombatEvent removalEvent;
            if (directDelete)
            {
                removalEvent = new CombatDirectDeleteResolver(
                    environment.MetadataFactory, environment.EventLog)
                    .ApplyDirectDelete(environment.State, environment.RootEvent, environment.TargetPosition);
            }
            else
            {
                var deathEvent = AppendDeath(environment);
                removalEvent = new CombatDeathRemovalResolver(
                    environment.MetadataFactory, environment.EventLog)
                    .TryApplyRemoval(environment.State, deathEvent);
            }

            Assert.That(removalEvent, Is.Not.Null);
            var removed = lookup.Get(environment.State, environment.Target.InstanceId);
            var tombstone = environment.EventLog.CardTombstones.Get(environment.Target.InstanceId);

            Assert.That(removed.IsRemoved, Is.True);
            Assert.That(removed.ActiveCard, Is.Null);
            Assert.That(removed.Tombstone, Is.SameAs(tombstone));
            Assert.That(removed.InstanceId, Is.EqualTo(environment.Target.InstanceId));
            Assert.That(removed.Season, Is.EqualTo(season));
            Assert.That(tombstone.Season, Is.EqualTo(season));
            Assert.That(removed.Position, Is.EqualTo(environment.TargetPosition));
            Assert.That(removed.RemovalReason, Is.EqualTo(directDelete
                ? CombatCardRemovalReason.DirectDelete
                : CombatCardRemovalReason.DeathRemoval));
            Assert.That(tombstone.RemovalMetadata.EventId, Is.EqualTo(removalEvent.Metadata.EventId));
            Assert.That(environment.EventLog.CardTombstones.Count, Is.EqualTo(1));
            Assert.That(environment.State.GetSide(environment.TargetPosition.Side).Cards.Count, Is.Zero);
            Assert.That(environment.State.GetSide(environment.TargetPosition.Side)
                .Board.GetSlot(environment.TargetPosition).IsOccupied, Is.False);
            Assert.That(environment.EventLog.Count, Is.EqualTo(directDelete ? 2 : 4));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void QueuedPetDeathTrigger_AfterDirectDelete_CanStillReadAutumnSeason(CombatSide side)
        {
            var environment = CreateEnvironment(side, CombatCardSeason.Autumn);
            var deathEvent = AppendDeath(environment);
            var lookup = new CombatCardLookup(environment.EventLog);
            var observer = new AutumnDeathObserver(side, environment.Pet.InstanceId, lookup);
            var deleteHandler = new DeleteOnDeathHandler(new CombatDirectDeleteResolver(
                environment.MetadataFactory, environment.EventLog));

            var sources = new CombatTriggerSourceRegistry(new ICombatTriggerSource[]
            {
                new CombatPetDeathTriggerSource(observer),
                new CombatTriggerHandlerSource(
                    new FixedCombatTriggerOrderKeyProvider(new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Slot, side, horizontalOrder: 0, verticalOrder: 0)),
                    deleteHandler)
            });
            var queue = new CombatEventQueue(environment.EventLog);
            var engine = new CombatTriggerEngine(environment.State, queue, sources);

            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.EqualTo(4));
            Assert.That(observer.CanCallCount, Is.EqualTo(1));
            Assert.That(observer.WasActiveAtDiscovery, Is.True);
            Assert.That(observer.ResolveCallCount, Is.EqualTo(1));
            Assert.That(observer.WasRemovedAtResolution, Is.True);
            Assert.That(observer.SeasonAtResolution, Is.EqualTo(CombatCardSeason.Autumn));
            Assert.That(observer.ObservedEvent, Is.SameAs(deathEvent));
            Assert.That(observer.ObservedPet, Is.SameAs(environment.Pet));
            Assert.That(deleteHandler.ResolveCallCount, Is.EqualTo(1));
            Assert.That(environment.EventLog.Count, Is.EqualTo(4));
            Assert.That(environment.EventLog.Events[3].Kind, Is.EqualTo(CombatEventKind.DirectDelete));
            Assert.That(environment.EventLog.Events[3].Metadata.ParentEventId.Value,
                Is.EqualTo(deathEvent.Metadata.EventId));
            Assert.That(environment.EventLog.CardTombstones.Get(environment.Target.InstanceId).Season,
                Is.EqualTo(CombatCardSeason.Autumn));

            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.Zero);
            Assert.That(observer.ResolveCallCount, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
        }

        private static DeathCombatEvent AppendDeath(TestEnvironment environment)
        {
            var damage = new CombatDamageResolver(environment.MetadataFactory, environment.EventLog)
                .ApplyResolvedCardDamage(environment.State, environment.RootEvent,
                    environment.AttackerPosition, environment.TargetPosition, incomingDamage: 3);
            var death = new CombatDeathEventResolver(environment.MetadataFactory, environment.EventLog)
                .AppendFromDamage(damage);
            Assert.That(death, Is.Not.Null);
            return death;
        }

        private static TestEnvironment CreateEnvironment(CombatSide side, CombatCardSeason season)
        {
            var opposingSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
            var targetPosition = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
            var attackerPosition = new BoardPosition(opposingSide, BoardRow.Front, new BoardColumn(1));
            var target = new CombatCardState(
                new DefinitionId("test.removal_target"), new InstanceId(1), new CardRank(2), season,
                hpCapacity: 3, currentHp: 3, armor: 0, attack: 1);
            var attacker = new CombatCardState(
                new DefinitionId("test.removal_attacker"), new InstanceId(2), new CardRank(2),
                CombatCardSeason.Summer, hpCapacity: 5, currentHp: 5, armor: 0, attack: 3);
            var owner = CreateSide(target, targetPosition, new SlotId(1));
            var other = CreateSide(attacker, attackerPosition, new SlotId(2));
            var pet = new CombatPetState(new DefinitionId("test.autumn_observer"), new InstanceId(1001));
            var ownerPets = new CombatSidePetState(side, new CombatPetRegistry(new[] { pet }));
            var otherPets = new CombatSidePetState(opposingSide,
                new CombatPetRegistry(Array.Empty<CombatPetState>()));
            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            var eventLog = new CombatEventLog();
            var root = new CombatStartedCombatEvent(metadataFactory.CreateRoot());
            eventLog.Append(root);

            return new TestEnvironment
            {
                State = new CombatState(
                    side == CombatSide.Player ? owner : other,
                    side == CombatSide.Enemy ? owner : other,
                    side == CombatSide.Player ? ownerPets : otherPets,
                    side == CombatSide.Enemy ? ownerPets : otherPets),
                Target = target,
                TargetPosition = targetPosition,
                AttackerPosition = attackerPosition,
                Pet = pet,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                RootEvent = root
            };
        }

        private static CombatSideState CreateSide(CombatCardState card, BoardPosition position, SlotId slotId)
        {
            return new CombatSideState(
                new CombatBoardState(position.Side, new[] { new CombatSlotState(slotId, position, card.InstanceId) }),
                new CombatCardRegistry(new[] { card }),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));
        }

        private sealed class DeleteOnDeathHandler : CombatEventTriggerHandler<DeathCombatEvent>
        {
            private readonly CombatDirectDeleteResolver _resolver;

            public DeleteOnDeathHandler(CombatDirectDeleteResolver resolver)
            {
                _resolver = resolver;
            }

            public int ResolveCallCount { get; private set; }

            protected override bool CanTriggerTyped(CombatState state, DeathCombatEvent sourceEvent)
            {
                return true;
            }

            protected override void ResolveTyped(CombatState state, DeathCombatEvent sourceEvent)
            {
                _resolver.ApplyDirectDelete(state, sourceEvent, sourceEvent.Position);
                ResolveCallCount++;
            }
        }

        private sealed class AutumnDeathObserver : CombatPetDeathTriggerHandler
        {
            private readonly CombatCardLookup _lookup;

            public AutumnDeathObserver(CombatSide side, InstanceId petInstanceId, CombatCardLookup lookup)
                : base(side, petInstanceId)
            {
                _lookup = lookup;
            }

            public int CanCallCount { get; private set; }
            public int ResolveCallCount { get; private set; }
            public bool WasActiveAtDiscovery { get; private set; }
            public bool WasRemovedAtResolution { get; private set; }
            public CombatCardSeason SeasonAtResolution { get; private set; }
            public DeathCombatEvent ObservedEvent { get; private set; }
            public CombatPetState ObservedPet { get; private set; }

            protected override bool CanTriggerOnDeath(CombatPetDeathContext context, CombatPetState pet)
            {
                CanCallCount++;
                var card = _lookup.Get(context.State, context.SourceEvent.InstanceId);
                WasActiveAtDiscovery = card.IsActive;
                return context.SourceEvent.Position.Side == Side && card.Season == CombatCardSeason.Autumn;
            }

            protected override void ResolveOnDeath(CombatPetDeathContext context, CombatPetState pet)
            {
                var card = _lookup.Get(context.State, context.SourceEvent.InstanceId);
                ResolveCallCount++;
                WasRemovedAtResolution = card.IsRemoved;
                SeasonAtResolution = card.Season;
                ObservedEvent = context.SourceEvent;
                ObservedPet = pet;
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }
            public CombatCardState Target { get; set; }
            public BoardPosition TargetPosition { get; set; }
            public BoardPosition AttackerPosition { get; set; }
            public CombatPetState Pet { get; set; }
            public CombatEventMetadataFactory MetadataFactory { get; set; }
            public CombatEventLog EventLog { get; set; }
            public CombatStartedCombatEvent RootEvent { get; set; }
        }
    }
}
