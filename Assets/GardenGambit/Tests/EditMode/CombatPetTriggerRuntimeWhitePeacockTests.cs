using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeWhitePeacockTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var e = new Environment();

            Assert.That(
                CombatPetDefinitionIds.WhitePeacockValue,
                Is.EqualTo("pet.white_peacock"));
            Assert.That(
                CombatPetDefinitionIds.WhitePeacock,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.white_peacock")));

            var registry = e.FullFactoryRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .CompleteFactoryCount));

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds
                            .WhitePeacock)
                    as WhitePeacockPetTriggerSourceFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    e.Runtime.PetUsageCommitter));
            Assert.That(
                factory.HpGainResolver,
                Is.SameAs(e.Hp));
            Assert.That(
                factory.ArmorGainResolver,
                Is.SameAs(e.Armor));
            Assert.That(
                factory.AttackGainResolver,
                Is.SameAs(e.Attack));
            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds.Phoenix),
                Is.True);
            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds.Badger),
                Is.True);

            var rescueAware =
                e.RescueAwareFactoryRegistry();

            Assert.That(
                rescueAware.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .RescueAwareFactoryCount));
            Assert.That(
                rescueAware.Contains(
                    CombatPetDefinitionIds
                        .WhitePeacock),
                Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsBothSourcesWithSharedDependencies(
            CombatSide side)
        {
            var e = new Environment(
                side,
                twoWhitePeacocks: true);
            var sources = e.Sources();

            Assert.That(sources.Count, Is.EqualTo(2));

            var owners = new HashSet<InstanceId>();

            foreach (var item in sources.Sources)
            {
                var source =
                    item as WhitePeacockPetTriggerSource;

                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(
                    source.UsageCommitter,
                    Is.SameAs(
                        e.Runtime.PetUsageCommitter));
                Assert.That(
                    source.HpGainResolver,
                    Is.SameAs(e.Hp));
                Assert.That(
                    source.ArmorGainResolver,
                    Is.SameAs(e.Armor));
                Assert.That(
                    source.AttackGainResolver,
                    Is.SameAs(e.Attack));
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
        public void FullBattle_BonusPrecedesAttacksAndCombatCompletes(
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

            for (var column = 1;
                 column <= 4;
                 column++)
            {
                var card = e.OwnCard(column);

                Assert.That(card.HpCapacity, Is.EqualTo(6));
                Assert.That(card.CurrentHp, Is.EqualTo(6));
                Assert.That(card.Armor, Is.EqualTo(1));
                Assert.That(card.Attack, Is.EqualTo(2));
            }

            Assert.That(
                e.State.GetOpposingSide(side).Cards.Count,
                Is.Zero);
            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.UpperPet.InstanceId),
                Is.True);
            Assert.That(
                e.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
            Assert.That(
                e.Events<HpGainCombatEvent>().Count,
                Is.EqualTo(4));
            Assert.That(
                e.Events<ArmorGainCombatEvent>().Count,
                Is.EqualTo(4));
            Assert.That(
                e.Events<AttackGainCombatEvent>().Count,
                Is.EqualTo(4));

            foreach (var gain in
                     e.Events<HpGainCombatEvent>())
            {
                Assert.That(
                    gain.SourceInstanceId,
                    Is.EqualTo(
                        e.UpperPet.InstanceId));
                Assert.That(gain.IsSelfSource, Is.False);
            }

            var ownAttacks = e.OwnAttacks();

            Assert.That(
                ownAttacks.Count,
                Is.EqualTo(4));

            foreach (var attack in ownAttacks)
            {
                Assert.That(
                    attack.BaseDamage,
                    Is.EqualTo(2));
            }

            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void CompleteRuntime_BuildsPhoenixAndWhitePeacockTogether()
        {
            var e = new Environment(
                lowerPhoenix: true);
            var sources = e.Sources();

            Assert.That(sources.Count, Is.EqualTo(2));
            Assert.That(
                sources.Sources[0],
                Is.TypeOf<WhitePeacockPetTriggerSource>());
            Assert.That(
                sources.Sources[1],
                Is.TypeOf<PhoenixPetTriggerSource>());

            var phoenix =
                (PhoenixPetTriggerSource)
                    sources.Sources[1];

            Assert.That(
                phoenix.RescueResolver,
                Is.SameAs(e.Rescue));
            Assert.That(
                phoenix.UsageCommitter,
                Is.SameAs(
                    e.Runtime.PetUsageCommitter));
        }

        [Test]
        public void FreshRuntime_AllowsSamePetAndCardIdsInNewBattle()
        {
            var first = new Environment();
            Start(first.CreateRunner());

            var next = new Environment();

            Assert.That(
                next.UpperPet.InstanceId,
                Is.EqualTo(
                    first.UpperPet.InstanceId));
            Assert.That(
                next.OwnCard(1).InstanceId,
                Is.EqualTo(
                    first.OwnCard(1).InstanceId));
            Assert.That(
                next.Runtime.PetUsageRegistry.Count,
                Is.Zero);

            Start(next.CreateRunner());

            Assert.That(
                first.OwnCard(1).Attack,
                Is.EqualTo(2));
            Assert.That(
                next.OwnCard(1).Attack,
                Is.EqualTo(2));
            Assert.That(
                next.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void CompleteOverloads_RejectNullHpGainResolver()
        {
            var e = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                e.Runtime.FactoryCatalog.CreateRegistry(
                    e.Armor,
                    e.Attack,
                    new CombatCardLookup(e.Log),
                    e.Runtime.PetUsageCommitter,
                    e.Rescue,
                    null));

            Assert.Throws<ArgumentNullException>(() =>
                e.Runtime.BuildSourceRegistry(
                    e.State,
                    e.Armor,
                    e.Attack,
                    new CombatCardLookup(e.Log),
                    e.Rescue,
                    null));
        }

        [Test]
        public void RescueAwareLegacyOverload_DoesNotSilentlyBuildWhitePeacock()
        {
            var e = new Environment();

            Assert.Throws<InvalidOperationException>(() =>
                e.Runtime.BuildSourceRegistry(
                    e.State,
                    e.Armor,
                    e.Attack,
                    new CombatCardLookup(e.Log),
                    e.Rescue));

            Assert.That(
                e.Runtime.PetUsageRegistry.Count,
                Is.Zero);
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

        private sealed class Environment
        {
            private static readonly CombatCardSuit[] Suits =
            {
                CombatCardSuit.Fruit,
                CombatCardSuit.Vegetable,
                CombatCardSuit.Nut,
                CombatCardSuit.Drink
            };

            public readonly CombatSide Side;
            public readonly CombatState State;
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
            public readonly CombatHpGainResolver Hp;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatRescueResolver Rescue;

            public Environment(
                CombatSide side = CombatSide.Player,
                bool twoWhitePeacocks = false,
                bool lowerPhoenix = false)
            {
                Side = side;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.WhitePeacock,
                    new InstanceId(1002));

                if (twoWhitePeacocks)
                {
                    LowerPet = new CombatPetState(
                        CombatPetDefinitionIds.WhitePeacock,
                        new InstanceId(1001));
                }
                else if (lowerPhoenix)
                {
                    LowerPet = new CombatPetState(
                        CombatPetDefinitionIds.Phoenix,
                        new InstanceId(1001));
                }

                var own = MakeSide(
                    side,
                    own: true);
                var opposing = MakeSide(
                    opposingSide,
                    own: false);

                var ownPetList =
                    LowerPet == null
                        ? new[] { UpperPet }
                        : new[] { UpperPet, LowerPet };

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(ownPetList));
                var opposingPets =
                    new CombatSidePetState(
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
                Hp = new CombatHpGainResolver(
                    Metadata,
                    Log);
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
                RescueAwareFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue);
            }

            public CombatPetTriggerSourceFactoryRegistry
                FullFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue,
                    Hp);
            }

            public CombatTriggerSourceRegistry Sources()
            {
                return Runtime.BuildSourceRegistry(
                    State,
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Rescue,
                    Hp);
            }

            public CombatResolutionRunner CreateRunner()
            {
                return Runtime.CreateResolutionRunner(
                    State,
                    Metadata,
                    Log,
                    Queue);
            }

            public CombatCardState OwnCard(
                int column)
            {
                return State.GetSide(Side)
                    .GetCardAt(
                        new BoardPosition(
                            Side,
                            BoardRow.Front,
                            new BoardColumn(column)));
            }

            public List<TEvent> Events<TEvent>()
                where TEvent : CombatEvent
            {
                var result = new List<TEvent>();

                foreach (var item in Log.Events)
                {
                    var typed = item as TEvent;

                    if (typed != null)
                    {
                        result.Add(typed);
                    }
                }

                return result;
            }

            public List<NormalAttackCombatEvent>
                OwnAttacks()
            {
                var result =
                    new List<NormalAttackCombatEvent>();

                foreach (var attack in
                         Events<NormalAttackCombatEvent>())
                {
                    if (attack.AttackerSide == Side)
                    {
                        result.Add(attack);
                    }
                }

                return result;
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                bool own)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var prefix = own ? 0L : 100L;

                for (var column = 1;
                     column <= 5;
                     column++)
                {
                    var front = new BoardPosition(
                        side,
                        BoardRow.Front,
                        new BoardColumn(column));
                    var back = new BoardPosition(
                        side,
                        BoardRow.Back,
                        new BoardColumn(column));

                    if (column <= 4)
                    {
                        var instanceId =
                            new InstanceId(
                                prefix + column);
                        var card = new CombatCardState(
                            new DefinitionId(
                                $"test.white_peacock_runtime." +
                                $"{prefix + column}"),
                            instanceId,
                            new CardRank(4),
                            Suits[column - 1],
                            CombatCardSeason.Spring,
                            own ? 5 : 1,
                            own ? 5 : 1,
                            0,
                            own ? 1 : 0);

                        cards.Add(card);
                        slots.Add(
                            new CombatSlotState(
                                new SlotId(column),
                                front,
                                instanceId));
                    }
                    else
                    {
                        slots.Add(
                            new CombatSlotState(
                                new SlotId(column),
                                front));
                    }

                    slots.Add(
                        new CombatSlotState(
                            new SlotId(10 + column),
                            back));
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }
        }
    }
}
