using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimePandaTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var environment = new Environment();
            var registry = environment.FullRegistry();

            Assert.That(
                CombatPetDefinitionIds.PandaValue,
                Is.EqualTo("pet.panda"));
            Assert.That(
                CombatPetDefinitionIds.Panda,
                Is.EqualTo(new DefinitionId("pet.panda")));
            Assert.That(
                registry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .FullFactoryCount));

            var factory = registry.GetFactory(
                    CombatPetDefinitionIds.Panda)
                as PandaPetTriggerSourceFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(
                factory.ActivationUsageCommitter,
                Is.SameAs(
                    environment.Runtime
                        .PetUsageCommitter));
            Assert.That(
                factory.LimitedUsageCommitter,
                Is.SameAs(
                    environment.Runtime
                        .LimitedUsageCommitter));
            Assert.That(
                factory.TargetDamageReductionRegistry,
                Is.SameAs(
                    environment.Runtime
                        .TargetDamageReductionRegistry));
            Assert.That(
                environment.Runtime.FactoryCatalog
                    .LimitedUsageCommitter,
                Is.SameAs(
                    environment.Runtime
                        .LimitedUsageCommitter));
        }

        [Test]
        public void DeeperCatalogueLevelsPreservePandaRegistration()
        {
            var environment = new Environment();

            Assert.That(
                environment.RescueAwareRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .RescueAwareFactoryCount));
            Assert.That(
                environment.CompleteRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .CompleteFactoryCount));
            Assert.That(
                environment.EventLogAwareRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .EventLogAwareFactoryCount));
            Assert.That(
                environment.EventLogAwareRegistry().Contains(
                    CombatPetDefinitionIds.Panda),
                Is.True);
        }

        [Test]
        public void CatalogueLimitedUsageDependencyIsValidated()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new CombatPetTriggerSourceFactoryCatalog(
                    environment.Runtime.UsageCommitter,
                    environment.Runtime
                        .SourceDamageModifierRegistry,
                    environment.Runtime
                        .TargetDamageReductionRegistry,
                    environment.Runtime
                        .FinalRankModifierRegistry,
                    null));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeBuildsBothPandaSourcesWithSharedDependencies(
            CombatSide side)
        {
            var environment = new Environment(
                side,
                twoPandas: true);
            var sources = environment.Sources();

            Assert.That(sources.Count, Is.EqualTo(2));

            var owners = new HashSet<InstanceId>();

            foreach (var item in sources.Sources)
            {
                var source = item as PandaPetTriggerSource;

                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(
                    source.ActivationUsageCommitter,
                    Is.SameAs(
                        environment.Runtime
                            .PetUsageCommitter));
                Assert.That(
                    source.LimitedUsageCommitter,
                    Is.SameAs(
                        environment.Runtime
                            .LimitedUsageCommitter));
                Assert.That(
                    source.TargetDamageReductionRegistry,
                    Is.SameAs(
                        environment.Runtime
                            .TargetDamageReductionRegistry));
                Assert.That(
                    owners.Add(source.PetInstanceId),
                    Is.True);
            }

            Assert.That(
                owners.Contains(
                    environment.UpperPet.InstanceId),
                Is.True);
            Assert.That(
                owners.Contains(
                    environment.LowerPet.InstanceId),
                Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FullBattleReducesOnlyFirstTwoPositiveNormalDamageEvents(
            CombatSide side)
        {
            var environment = new Environment(side);
            var completed = environment.ResolveCombat();
            var hits = environment.TargetHits();

            Assert.That(completed, Is.Not.Null);
            Assert.That(
                completed.Outcome,
                Is.EqualTo(
                    side == CombatSide.Player
                        ? CombatOutcome.PlayerVictory
                        : CombatOutcome.EnemyVictory));
            Assert.That(hits.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(
                hits[0].Result.IncomingDamage,
                Is.EqualTo(2));
            Assert.That(
                hits[1].Result.IncomingDamage,
                Is.EqualTo(2));
            Assert.That(
                hits[2].Result.IncomingDamage,
                Is.EqualTo(3));
            Assert.That(
                environment.Runtime.PetUsageCommitter
                    .HasTriggered(
                        environment.UpperPet.InstanceId),
                Is.True);
            Assert.That(
                environment.Runtime.LimitedUsageRegistry
                    .GetUsageCount(
                        environment.UpperPet.InstanceId),
                Is.EqualTo(2));
            Assert.That(
                environment.Runtime
                    .TargetDamageReductionRegistry.Count,
                Is.Zero);
            Assert.That(
                environment.Queue.PendingCount,
                Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FullBattleWithTwoSeasonTypesDoesNotActivatePanda(
            CombatSide side)
        {
            var environment = new Environment(
                side,
                hasThreeSeasonTypes: false);

            environment.ResolveCombat();

            Assert.That(
                environment.TargetHits()[0]
                    .Result.IncomingDamage,
                Is.EqualTo(3));
            Assert.That(
                environment.Runtime.PetUsageRegistry.Count,
                Is.Zero);
            Assert.That(
                environment.Runtime.LimitedUsageRegistry
                    .TotalUsageCount,
                Is.Zero);
        }

        [Test]
        public void RebuiltRuntimeSourcesPreserveConsumedPandaUsage()
        {
            var environment = new Environment();

            environment.ResolveCombat();

            var attack =
                environment.CreateRecordedOpponentAttack();

            Assert.That(
                environment.Sources().DiscoverTriggers(
                    environment.State,
                    attack),
                Is.Empty);
            Assert.That(
                environment.Runtime.LimitedUsageRegistry
                    .TotalUsageCount,
                Is.EqualTo(2));
        }

        [Test]
        public void FreshRuntimeUsesIndependentBattleScopedPandaUsage()
        {
            var first = new Environment();
            var second = new Environment();

            first.ResolveCombat();

            Assert.That(
                second.UpperPet.InstanceId,
                Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(
                second.Target.InstanceId,
                Is.EqualTo(first.Target.InstanceId));
            Assert.That(
                second.Runtime.PetUsageRegistry.Count,
                Is.Zero);
            Assert.That(
                second.Runtime.LimitedUsageRegistry
                    .TotalUsageCount,
                Is.Zero);

            second.ResolveCombat();

            Assert.That(
                first.Runtime.LimitedUsageRegistry
                    .TotalUsageCount,
                Is.EqualTo(2));
            Assert.That(
                second.Runtime.LimitedUsageRegistry
                    .TotalUsageCount,
                Is.EqualTo(2));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatCardState Opponent;
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
            public readonly CombatHpGainResolver Hp;

            private readonly BoardPosition _targetPosition;
            private readonly BoardPosition _opponentPosition;

            public Environment(
                CombatSide side = CombatSide.Player,
                bool twoPandas = false,
                bool hasThreeSeasonTypes = true)
            {
                Side = side;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                _targetPosition = new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));
                _opponentPosition = new BoardPosition(
                    opposingSide,
                    BoardRow.Front,
                    new BoardColumn(1));

                Target = Card(
                    "test.panda_target",
                    1,
                    CombatCardSeason.Spring,
                    hp: 10,
                    attack: 0);
                var summer = Card(
                    "test.panda_summer",
                    2,
                    CombatCardSeason.Summer,
                    hp: 10,
                    attack: 10);
                var third = Card(
                    "test.panda_third",
                    3,
                    hasThreeSeasonTypes
                        ? CombatCardSeason.Seasonless
                        : CombatCardSeason.Spring,
                    hp: 10,
                    attack: 10);
                Opponent = Card(
                    "test.panda_opponent",
                    101,
                    CombatCardSeason.Winter,
                    hp: 10,
                    attack: 3);

                var own = MakeSide(
                    side,
                    new[] { Target, summer, third },
                    slotPrefix: 0);
                var opposing = MakeSide(
                    opposingSide,
                    new[] { Opponent },
                    slotPrefix: 100);

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.Panda,
                    new InstanceId(1002));

                if (twoPandas)
                {
                    LowerPet = new CombatPetState(
                        CombatPetDefinitionIds.Panda,
                        new InstanceId(1001));
                }

                var ownPetList =
                    LowerPet == null
                        ? new[] { UpperPet }
                        : new[] { UpperPet, LowerPet };
                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(ownPetList));
                var opposingPets = new CombatSidePetState(
                    opposingSide,
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
                        : opposingPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : opposingPets);

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
                Hp = new CombatHpGainResolver(
                    Metadata,
                    Log);
            }

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            public CombatPetTriggerSourceFactoryRegistry
                FullRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter);
            }

            public CombatPetTriggerSourceFactoryRegistry
                RescueAwareRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue);
            }

            public CombatPetTriggerSourceFactoryRegistry
                CompleteRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue,
                    Hp);
            }

            public CombatPetTriggerSourceFactoryRegistry
                EventLogAwareRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue,
                    Hp,
                    Log);
            }

            public CombatTriggerSourceRegistry Sources()
            {
                return Runtime.BuildSourceRegistry(
                    State,
                    Armor,
                    Attack,
                    new CombatCardLookup(Log));
            }

            public CombatCompletedCombatEvent ResolveCombat()
            {
                var runner = Runtime.CreateResolutionRunner(
                    State,
                    Metadata,
                    Log,
                    Queue);

                return runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 20,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }

            public List<DamageAppliedCombatEvent> TargetHits()
            {
                var hits = new List<DamageAppliedCombatEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    var damage =
                        combatEvent as DamageAppliedCombatEvent;

                    if (damage != null &&
                        damage.TargetInstanceId ==
                        Target.InstanceId)
                    {
                        hits.Add(damage);
                    }
                }

                return hits;
            }

            public NormalAttackCombatEvent
                CreateRecordedOpponentAttack()
            {
                var root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
                Log.Append(root);

                var targetPosition = new BoardPosition(
                    Side,
                    BoardRow.Front,
                    new BoardColumn(2));
                var target = State.GetSide(Side)
                    .GetCardAt(targetPosition);
                var attack = new NormalAttackCombatEvent(
                    Metadata.CreateChild(root.Metadata),
                    Opponent.InstanceId,
                    _opponentPosition,
                    target.InstanceId,
                    targetPosition,
                    3);

                Log.Append(attack);

                return attack;
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                IReadOnlyList<CombatCardState> cards,
                long slotPrefix)
            {
                var slots = new List<CombatSlotState>();

                for (var column = 1;
                     column <= 3;
                     column++)
                {
                    var frontPosition = new BoardPosition(
                        side,
                        BoardRow.Front,
                        new BoardColumn(column));

                    if (column <= cards.Count)
                    {
                        slots.Add(new CombatSlotState(
                            new SlotId(slotPrefix + column),
                            frontPosition,
                            cards[column - 1].InstanceId));
                    }
                    else
                    {
                        slots.Add(new CombatSlotState(
                            new SlotId(slotPrefix + column),
                            frontPosition));
                    }
                    slots.Add(new CombatSlotState(
                        new SlotId(slotPrefix + 10 + column),
                        new BoardPosition(
                            side,
                            BoardRow.Back,
                            new BoardColumn(column))));
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardState Card(
                string definition,
                long instanceId,
                CombatCardSeason season,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId(definition),
                    new InstanceId(instanceId),
                    new CardRank(2),
                    CombatCardSuit.Fruit,
                    season,
                    hp,
                    hp,
                    0,
                    attack);
            }
        }
    }
}
