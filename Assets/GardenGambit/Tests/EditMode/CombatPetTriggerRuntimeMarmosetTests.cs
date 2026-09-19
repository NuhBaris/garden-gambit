using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeMarmosetTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var environment = new Environment();
            var registry = environment.CompleteFactoryRegistry();

            Assert.That(
                CombatPetDefinitionIds.MarmosetValue,
                Is.EqualTo("pet.marmoset"));
            Assert.That(
                CombatPetDefinitionIds.Marmoset,
                Is.EqualTo(new DefinitionId("pet.marmoset")));
            Assert.That(
                registry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .CompleteFactoryCount));

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds.Marmoset)
                    as MarmosetPetTriggerSourceFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.PetUsageCommitter));
            Assert.That(
                factory.HpGainResolver,
                Is.SameAs(environment.Hp));
            Assert.That(
                factory.ArmorGainResolver,
                Is.SameAs(environment.Armor));

            var rescueAware =
                environment.RescueAwareFactoryRegistry();

            Assert.That(
                rescueAware.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .RescueAwareFactoryCount));
            Assert.That(
                rescueAware.Contains(
                    CombatPetDefinitionIds.Marmoset),
                Is.False);
        }

        [Test]
        public void EventLogAwareRegistry_PreservesMarmosetRegistration()
        {
            var environment = new Environment();
            var registry =
                environment.EventLogAwareFactoryRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .EventLogAwareFactoryCount));
            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds.Marmoset),
                Is.True);
            Assert.That(
                registry.GetFactory(
                    CombatPetDefinitionIds.Marmoset),
                Is.TypeOf<
                    MarmosetPetTriggerSourceFactory>());
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsBothSourcesWithSharedDependencies(
            CombatSide side)
        {
            var environment = new Environment(
                side,
                twoMarmosets: true);
            var sources = environment.Sources();

            Assert.That(sources.Count, Is.EqualTo(2));

            var owners = new HashSet<InstanceId>();

            foreach (var item in sources.Sources)
            {
                var source = item as MarmosetPetTriggerSource;

                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(
                    source.UsageCommitter,
                    Is.SameAs(
                        environment.Runtime
                            .PetUsageCommitter));
                Assert.That(
                    source.HpGainResolver,
                    Is.SameAs(environment.Hp));
                Assert.That(
                    source.ArmorGainResolver,
                    Is.SameAs(environment.Armor));
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
        public void FullBattle_AppliesLeftmostLowestRankBonusBeforeAttack(
            CombatSide side)
        {
            var environment = new Environment(side);
            var runner = environment.CreateRunner();

            var completed = Start(runner);

            AssertCompleted(
                environment,
                runner,
                completed);

            var target = environment.OwnCard(
                BoardRow.Front,
                2);
            var tiedCard = environment.OwnCard(
                BoardRow.Front,
                4);

            Assert.That(target.HpCapacity, Is.EqualTo(11));
            Assert.That(target.CurrentHp, Is.EqualTo(6));
            Assert.That(target.Armor, Is.EqualTo(1));
            Assert.That(tiedCard.HpCapacity, Is.EqualTo(10));
            Assert.That(tiedCard.CurrentHp, Is.EqualTo(5));
            Assert.That(tiedCard.Armor, Is.Zero);

            var hpGains =
                environment.Events<HpGainCombatEvent>();
            var armorGains =
                environment.Events<ArmorGainCombatEvent>();

            Assert.That(hpGains.Count, Is.EqualTo(1));
            Assert.That(armorGains.Count, Is.EqualTo(1));
            Assert.That(
                hpGains[0].TargetInstanceId,
                Is.EqualTo(target.InstanceId));
            Assert.That(
                hpGains[0].SourceInstanceId,
                Is.EqualTo(
                    environment.UpperPet.InstanceId));
            Assert.That(
                armorGains[0].TargetInstanceId,
                Is.EqualTo(target.InstanceId));

            var petStage = environment.PetStage();

            Assert.That(
                hpGains[0].Metadata.ParentEventId.Value,
                Is.EqualTo(petStage.Metadata.EventId));
            Assert.That(
                armorGains[0].Metadata.ParentEventId.Value,
                Is.EqualTo(petStage.Metadata.EventId));

            var ownAttacks = environment.OwnAttacks();

            Assert.That(ownAttacks.Count, Is.EqualTo(1));
            Assert.That(
                armorGains[0].Metadata.SequenceNo.Value,
                Is.LessThan(
                    ownAttacks[0].Metadata.SequenceNo.Value));
            Assert.That(
                environment.Runtime.PetUsageCommitter
                    .HasTriggered(
                        environment.UpperPet.InstanceId),
                Is.True);
            Assert.That(
                environment.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Resume_RetainsOriginalSnapshotTarget(
            CombatSide side)
        {
            var environment = new Environment(
                side,
                overflowTarget: true);
            var runner = environment.CreateRunner();

            Assert.Throws<OverflowException>(() =>
                Start(runner));

            Assert.That(runner.HasActiveCombat, Is.True);
            Assert.That(
                environment.Runtime.PetUsageCommitter
                    .HasTriggered(
                        environment.UpperPet.InstanceId),
                Is.False);
            Assert.That(
                environment.Events<HpGainCombatEvent>(),
                Is.Empty);
            Assert.That(
                environment.Events<ArmorGainCombatEvent>(),
                Is.Empty);

            var originalTarget = environment.OwnCard(
                BoardRow.Front,
                2);
            var newCurrentLowest = environment.OwnCard(
                BoardRow.Front,
                1);

            originalTarget.RemoveArmor(int.MaxValue);
            originalTarget.SetRank(new CardRank(14));
            newCurrentLowest.SetRank(new CardRank(2));

            var completed = runner.ResumeActiveCombat(
                maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            AssertCompleted(
                environment,
                runner,
                completed);
            Assert.That(
                originalTarget.HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                originalTarget.Armor,
                Is.EqualTo(1));
            Assert.That(
                newCurrentLowest.HpCapacity,
                Is.EqualTo(10));
            Assert.That(
                newCurrentLowest.Armor,
                Is.Zero);
            Assert.That(
                environment.Events<HpGainCombatEvent>().Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Events<ArmorGainCombatEvent>().Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Events<
                    CombatStartedCombatEvent>().Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Events<
                    CombatCompletedCombatEvent>().Count,
                Is.EqualTo(1));
        }

        [Test]
        public void RescueAwareLegacyOverload_DoesNotBuildMarmoset()
        {
            var environment = new Environment();

            Assert.Throws<InvalidOperationException>(() =>
                environment.Runtime.BuildSourceRegistry(
                    environment.State,
                    environment.Armor,
                    environment.Attack,
                    new CombatCardLookup(environment.Log),
                    environment.Rescue));

            Assert.That(
                environment.Runtime.PetUsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RebuiltSources_PreserveCompletedUsage()
        {
            var environment = new Environment();

            Start(environment.CreateRunner());

            Assert.That(
                environment.Sources().DiscoverTriggers(
                    environment.State,
                    environment.PetStage()),
                Is.Empty);
            Assert.That(
                environment.Events<HpGainCombatEvent>().Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Events<ArmorGainCombatEvent>().Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FreshRuntime_AllowsSameIdsInNewBattle()
        {
            var first = new Environment();
            Start(first.CreateRunner());

            var second = new Environment();

            Assert.That(
                second.UpperPet.InstanceId,
                Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(
                second.OwnCard(
                    BoardRow.Front,
                    2).InstanceId,
                Is.EqualTo(
                    first.OwnCard(
                        BoardRow.Front,
                        2).InstanceId));
            Assert.That(
                second.Runtime.PetUsageRegistry.Count,
                Is.Zero);

            Start(second.CreateRunner());

            Assert.That(
                first.OwnCard(
                    BoardRow.Front,
                    2).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                second.OwnCard(
                    BoardRow.Front,
                    2).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                second.Runtime.PetUsageRegistry.Count,
                Is.EqualTo(1));
        }

        private static CombatCompletedCombatEvent Start(
            CombatResolutionRunner runner)
        {
            return runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);
        }

        private static void AssertCompleted(
            Environment environment,
            CombatResolutionRunner runner,
            CombatCompletedCombatEvent completed)
        {
            Assert.That(completed, Is.Not.Null);
            Assert.That(
                completed.Outcome,
                Is.EqualTo(
                    environment.Side == CombatSide.Player
                        ? CombatOutcome.PlayerVictory
                        : CombatOutcome.EnemyVictory));
            Assert.That(
                environment.State.GetOpposingSide(
                    environment.Side).Cards.Count,
                Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(
                environment.Queue.PendingCount,
                Is.Zero);
        }

        private sealed class Environment
        {
            private static readonly int[] Ranks =
            {
                5,
                2,
                4,
                2,
                3
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
                bool twoMarmosets = false,
                bool overflowTarget = false)
            {
                Side = side;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.Marmoset,
                    new InstanceId(1002));

                if (twoMarmosets)
                {
                    LowerPet = new CombatPetState(
                        CombatPetDefinitionIds.Marmoset,
                        new InstanceId(1001));
                }

                var own = MakeSide(
                    side,
                    own: true,
                    overflowTarget: overflowTarget);
                var opposing = MakeSide(
                    opposingSide,
                    own: false,
                    overflowTarget: false);

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
                CompleteFactoryRegistry()
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
                EventLogAwareFactoryRegistry()
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
                BoardRow row,
                int column)
            {
                return State.GetSide(Side)
                    .GetCardAt(
                        new BoardPosition(
                            Side,
                            row,
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

            public List<NormalAttackCombatEvent> OwnAttacks()
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

            public BattleStartStageStartedCombatEvent PetStage()
            {
                BattleStartStageStartedCombatEvent result = null;

                foreach (var stage in
                         Events<
                             BattleStartStageStartedCombatEvent>())
                {
                    if (!stage.IsPetStage)
                    {
                        continue;
                    }

                    Assert.That(
                        result,
                        Is.Null,
                        "Pet stage must occur once.");
                    result = stage;
                }

                Assert.That(result, Is.Not.Null);

                return result;
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                bool own,
                bool overflowTarget)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var prefix =
                    side == CombatSide.Player
                        ? 0L
                        : 100L;

                foreach (var row in new[]
                         {
                             BoardRow.Front,
                             BoardRow.Back
                         })
                {
                    for (var column = 1;
                         column <= 5;
                         column++)
                    {
                        var position = new BoardPosition(
                            side,
                            row,
                            new BoardColumn(column));
                        var rowOffset =
                            row == BoardRow.Front
                                ? 0L
                                : 10L;
                        var slotId = new SlotId(
                            prefix + rowOffset + column);
                        CombatCardState card = null;

                        if (own)
                        {
                            var instanceId = new InstanceId(
                                prefix + rowOffset + column);
                            var armor =
                                overflowTarget &&
                                row == BoardRow.Front &&
                                column == 2
                                    ? int.MaxValue
                                    : 0;

                            card = new CombatCardState(
                                new DefinitionId(
                                    "test.marmoset_runtime.card"),
                                instanceId,
                                new CardRank(Ranks[column - 1]),
                                CombatCardSeason.Spring,
                                10,
                                5,
                                armor,
                                2);
                        }
                        else if (
                            row == BoardRow.Front &&
                            column == 2)
                        {
                            var instanceId = new InstanceId(
                                prefix + column);

                            card = new CombatCardState(
                                new DefinitionId(
                                    "test.marmoset_runtime.opponent"),
                                instanceId,
                                new CardRank(2),
                                CombatCardSeason.Winter,
                                1,
                                1,
                                0,
                                0);
                        }

                        if (card != null)
                        {
                            cards.Add(card);
                        }

                        slots.Add(
                            new CombatSlotState(
                                slotId,
                                position,
                                card == null
                                    ? (InstanceId?)null
                                    : card.InstanceId));
                    }
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
