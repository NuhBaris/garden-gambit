using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimePhoenixTests
    {
        [Test]
        public void Catalogue_RegistersStablePhoenixIdentityWithSharedResolvers()
        {
            var e = new Environment();

            Assert.That(
                CombatPetDefinitionIds.PhoenixValue,
                Is.EqualTo("pet.phoenix"));

            Assert.That(
                CombatPetDefinitionIds.Phoenix,
                Is.EqualTo(new DefinitionId("pet.phoenix")));

            var legacyRegistry =
                e.LegacyFactoryRegistry();

            Assert.That(
                legacyRegistry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .FullFactoryCount));

            Assert.That(
                legacyRegistry.Contains(
                    CombatPetDefinitionIds.Phoenix),
                Is.False);

            var registry =
                e.FullFactoryRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .RescueAwareFactoryCount));

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds.Phoenix)
                    as PhoenixPetTriggerSourceFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(
                factory.RescueResolver,
                Is.SameAs(e.Rescue));
            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds.Badger),
                Is.True);
            Assert.That(
                e.Runtime.FactoryRegistry.Count,
                Is.EqualTo(3));
            Assert.That(
                e.Runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds.Phoenix),
                Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsBothPhoenixSourcesWithSharedDependencies(
            CombatSide side)
        {
            var e = new Environment(side);
            var sources = e.Sources();

            Assert.That(sources.Count, Is.EqualTo(2));

            var owners = new HashSet<InstanceId>();

            foreach (var item in sources.Sources)
            {
                var source =
                    item as PhoenixPetTriggerSource;

                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(
                    source.UsageCommitter,
                    Is.SameAs(
                        e.Runtime.PetUsageCommitter));
                Assert.That(
                    source.RescueResolver,
                    Is.SameAs(e.Rescue));
                Assert.That(
                    owners.Add(source.PetInstanceId),
                    Is.True);
            }

            Assert.That(
                owners.Contains(e.UpperPet.InstanceId),
                Is.True);
            Assert.That(
                owners.Contains(e.LowerPet.InstanceId),
                Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FullBattle_FirstFriendlyDeathIsRescuedAndCombatContinues(
            CombatSide side)
        {
            var e = new Environment(side);
            var runner = e.CreateRunner();

            var completed = Start(runner);

            Assert.That(completed, Is.Not.Null);
            Assert.That(
                completed.Outcome,
                Is.EqualTo(
                    side == CombatSide.Player
                        ? CombatOutcome.PlayerVictory
                        : CombatOutcome.EnemyVictory));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(e.Target.IsAtDeathThreshold, Is.False);
            Assert.That(
                e.State.GetSide(side).Cards.Count,
                Is.EqualTo(1));
            Assert.That(
                e.State.GetOpposingSide(side).Cards.Count,
                Is.Zero);
            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.UpperPet.InstanceId),
                Is.True);
            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.LowerPet.InstanceId),
                Is.False);
            Assert.That(
                e.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));

            var rescues =
                Events<RescueCombatEvent>(e.Log);

            Assert.That(rescues.Count, Is.EqualTo(1));
            Assert.That(
                rescues[0].InstanceId,
                Is.EqualTo(e.Target.InstanceId));

            var targetDeaths =
                TargetEvents<DeathCombatEvent>(
                    e.Log,
                    e.Target.InstanceId);

            Assert.That(targetDeaths.Count, Is.EqualTo(1));
            Assert.That(
                rescues[0].Metadata.ParentEventId,
                Is.EqualTo(
                    targetDeaths[0].Metadata.EventId));
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void RebuiltRuntimeSources_PreserveConsumedPhoenixUsage()
        {
            var e = new Environment();
            var firstDeath = e.ApplyTargetDeath(3);
            var engine = new CombatTriggerEngine(
                e.State,
                e.Queue,
                e.Sources());

            engine.Drain(
                maximumEventCount: 30,
                maximumTriggerCountPerEvent: 30);

            Assert.That(firstDeath, Is.Not.Null);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(
                e.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));

            var secondDeath = e.ApplyTargetDeath(2);
            var rebuilt = e.Sources();

            Assert.That(
                rebuilt.DiscoverTriggers(
                    e.State,
                    secondDeath),
                Is.Empty);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(-1));
            Assert.That(
                Events<RescueCombatEvent>(e.Log).Count,
                Is.EqualTo(1));
            Assert.That(
                e.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FreshRuntime_AllowsSamePhoenixPetIdsInNewBattle()
        {
            var first = new Environment();
            Start(first.CreateRunner());

            var next = new Environment();

            Assert.That(
                next.UpperPet.InstanceId,
                Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(
                next.Runtime.PetUsageRegistry.Count,
                Is.Zero);

            Start(next.CreateRunner());

            Assert.That(first.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(next.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(
                next.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void LegacyRuntimeOverload_DoesNotSilentlyBuildPhoenix()
        {
            var e = new Environment();

            Assert.Throws<InvalidOperationException>(() =>
                e.Runtime.BuildSourceRegistry(
                    e.State,
                    e.Armor,
                    e.Attack,
                    new CombatCardLookup(e.Log)));

            Assert.That(
                e.Runtime.PetUsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RescueAwareOverloads_RejectNullRescueResolver()
        {
            var e = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                e.Runtime.FactoryCatalog.CreateRegistry(
                    e.Armor,
                    e.Attack,
                    new CombatCardLookup(e.Log),
                    e.Runtime.PetUsageCommitter,
                    null));

            Assert.Throws<ArgumentNullException>(() =>
                e.Runtime.BuildSourceRegistry(
                    e.State,
                    e.Armor,
                    e.Attack,
                    new CombatCardLookup(e.Log),
                    null));
        }

        private static CombatCompletedCombatEvent Start(
            CombatResolutionRunner runner)
        {
            return runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);
        }

        private static List<TEvent> Events<TEvent>(
            CombatEventLog log)
            where TEvent : CombatEvent
        {
            var result = new List<TEvent>();

            foreach (var item in log.Events)
            {
                var typed = item as TEvent;

                if (typed != null)
                {
                    result.Add(typed);
                }
            }

            return result;
        }

        private static List<TEvent> TargetEvents<TEvent>(
            CombatEventLog log,
            InstanceId targetInstanceId)
            where TEvent : CombatEvent
        {
            var result = new List<TEvent>();

            foreach (var item in log.Events)
            {
                var typed = item as TEvent;

                if (typed is DeathCombatEvent death &&
                    death.InstanceId == targetInstanceId)
                {
                    result.Add(typed);
                }
            }

            return result;
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatPetTriggerRuntime Runtime =
                new CombatPetTriggerRuntime();
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatEventQueue Queue;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatRescueResolver Rescue;
            private readonly BoardPosition _targetPosition;
            private readonly BoardPosition _opponentPosition;

            public Environment(
                CombatSide side = CombatSide.Player)
            {
                Side = side;

                var other =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                _targetPosition = new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));

                _opponentPosition = new BoardPosition(
                    other,
                    BoardRow.Front,
                    new BoardColumn(1));

                Target = Card(
                    "test.phoenix_target",
                    1,
                    hp: 2,
                    attack: 1);

                var opponent = Card(
                    "test.phoenix_opponent",
                    101,
                    hp: 1,
                    attack: 3);

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.Phoenix,
                    new InstanceId(1002));

                LowerPet = new CombatPetState(
                    CombatPetDefinitionIds.Phoenix,
                    new InstanceId(1001));

                var own = MakeSide(
                    side,
                    Target,
                    _targetPosition,
                    1);

                var opposing = MakeSide(
                    other,
                    opponent,
                    _opponentPosition,
                    101);

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { UpperPet, LowerPet }));

                var otherPets = new CombatSidePetState(
                    other,
                    new CombatPetRegistry(
                        Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? own
                        : opposing,
                    side == CombatSide.Enemy
                        ? own
                        : opposing,
                    side == CombatSide.Player
                        ? ownPets
                        : otherPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : otherPets);

                Queue = new CombatEventQueue(Log);
                Armor = new CombatArmorGainResolver(
                    Metadata,
                    Log);
                Attack = new CombatAttackGainResolver(
                    Metadata,
                    Log);
                Rescue = new CombatRescueResolver(
                    Metadata,
                    Log);
            }

            public CombatPetTriggerSourceFactoryRegistry
                LegacyFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter);
            }

            public CombatPetTriggerSourceFactoryRegistry
                FullFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue);
            }

            public CombatTriggerSourceRegistry Sources()
            {
                return Runtime.BuildSourceRegistry(
                    State,
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Rescue);
            }

            public CombatResolutionRunner CreateRunner()
            {
                return Runtime.CreateResolutionRunner(
                    State,
                    Metadata,
                    Log,
                    Queue);
            }

            public DeathCombatEvent ApplyTargetDeath(
                int damage)
            {
                var root =
                    new CombatStartedCombatEvent(
                        Metadata.CreateRoot());

                Log.Append(root);

                var applied =
                    new CombatDamageResolver(
                            Metadata,
                            Log)
                        .ApplyResolvedCardDamage(
                            State,
                            root,
                            _opponentPosition,
                            _targetPosition,
                            damage);

                var death =
                    new CombatDeathEventResolver(
                            Metadata,
                            Log)
                        .AppendFromDamage(applied);

                Assert.That(death, Is.Not.Null);

                return death;
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                CombatCardState card,
                BoardPosition front,
                long slotPrefix)
            {
                var back = new BoardPosition(
                    side,
                    BoardRow.Back,
                    front.Column);

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(slotPrefix),
                                front,
                                card.InstanceId),
                            new CombatSlotState(
                                new SlotId(slotPrefix + 1),
                                back)
                        }),
                    new CombatCardRegistry(
                        new[] { card }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardState Card(
                string definition,
                long instanceId,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId(definition),
                    new InstanceId(instanceId),
                    new CardRank(4),
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    attack);
            }
        }
    }
}
