using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeMacaqueTests
    {
        [Test]
        public void Catalogue_RegistersMacaqueWithSharedDependencies()
        {
            var e = new Environment();

            Assert.That(
                CombatPetDefinitionIds.MacaqueValue,
                Is.EqualTo("pet.macaque"));

            Assert.That(
                CombatPetDefinitionIds.Macaque,
                Is.EqualTo(new DefinitionId("pet.macaque")));

            var registry = e.Runtime.FactoryCatalog.CreateRegistry(
                e.Armor,
                e.Attack,
                new CombatCardLookup(e.Log),
                e.Runtime.PetUsageCommitter);

            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));

            var factory = registry.GetFactory(
                CombatPetDefinitionIds.Macaque)
                as MacaquePetTriggerSourceFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(
                factory.AttackGainResolver,
                Is.SameAs(e.Attack));

            Assert.That(e.Runtime.FactoryRegistry.Count, Is.EqualTo(3));
            Assert.That(
                e.Runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds.Macaque),
                Is.False);

            var armorRegistry = e.Runtime.FactoryCatalog.CreateRegistry(
                e.Armor);

            Assert.That(armorRegistry.Count, Is.EqualTo(4));
            Assert.That(
                armorRegistry.Contains(CombatPetDefinitionIds.Macaque),
                Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void BuildSources_CreatesBothMacaquesWithSharedUsage(
            CombatSide side)
        {
            var e = new Environment(side);
            var registry = e.Sources();

            Assert.That(registry.Count, Is.EqualTo(2));

            var petIds = new HashSet<InstanceId>();

            foreach (var item in registry.Sources)
            {
                var source = item as MacaquePetTriggerSource;

                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(
                    source.UsageCommitter,
                    Is.SameAs(e.Runtime.PetUsageCommitter));
                Assert.That(
                    source.AttackGainResolver,
                    Is.SameAs(e.Attack));
                Assert.That(petIds.Add(source.PetInstanceId), Is.True);
            }

            Assert.That(petIds.Contains(e.UpperPet.InstanceId), Is.True);
            Assert.That(petIds.Contains(e.LowerPet.InstanceId), Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_ResolvesBothSnapshotGroupsInPetAndColumnOrder(
            CombatSide side)
        {
            var e = new Environment(side);
            var runner = e.CreateRunner();

            Assert.That(runner.UsesStagedNormalAttackByDefault, Is.True);

            var completed = Start(runner);

            Assert.That(completed, Is.Not.Null);

            foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
            {
                for (var column = 1; column <= 3; column++)
                {
                    var card = e.Card(row, column);

                    Assert.That(card.Attack, Is.EqualTo(4));
                    Assert.That(card.CurrentHp, Is.EqualTo(5));
                    Assert.That(card.HpCapacity, Is.EqualTo(10));
                    Assert.That(card.Armor, Is.Zero);
                    Assert.That(card.Rank.Value, Is.EqualTo(7));
                }
            }

            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.UpperPet.InstanceId),
                Is.True);

            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.LowerPet.InstanceId),
                Is.True);

            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);

            var petStage = e.PetStage();
            Assert.That(petStage.HasBattleStartSnapshot, Is.True);

            var gains = e.Events<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(6));

            for (var index = 0; index < gains.Count; index++)
            {
                var expectedRow = index < 3
                    ? BoardRow.Front
                    : BoardRow.Back;

                var expectedColumn = index % 3 + 1;
                var gain = gains[index];

                Assert.That(
                    gain.TargetInstanceId,
                    Is.EqualTo(
                        e.Card(expectedRow, expectedColumn).InstanceId));

                Assert.That(
                    gain.TargetPosition.Row,
                    Is.EqualTo(expectedRow));

                Assert.That(
                    gain.TargetPosition.Column.Value,
                    Is.EqualTo(expectedColumn));

                Assert.That(gain.ActualGainedAmount, Is.EqualTo(2));

                Assert.That(
                    gain.Metadata.ParentEventId.Value,
                    Is.EqualTo(petStage.Metadata.EventId));

                Assert.That(
                    gain.Metadata.TriggerRootId,
                    Is.EqualTo(petStage.Metadata.TriggerRootId));

                Assert.That(
                    gain.Metadata.SequenceNo.Value,
                    Is.LessThan(completed.Metadata.SequenceNo.Value));
            }

            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(2)]
        public void Runtime_WithoutQualifyingSnapshotGroupDoesNotBuff(
            int matchingCount)
        {
            var e = new Environment(matchingCount: matchingCount);
            var runner = e.CreateRunner();

            Assert.That(Start(runner), Is.Not.Null);

            foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
            {
                for (var column = 1; column <= 3; column++)
                {
                    Assert.That(
                        e.Card(row, column).Attack,
                        Is.EqualTo(2));
                }
            }

            Assert.That(e.Events<AttackGainCombatEvent>(), Is.Empty);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_ResumePreservesUpperBonusAndRetriesLowerBatch(
            CombatSide side)
        {
            var e = new Environment(side);
            var overflowTarget = e.Card(BoardRow.Back, 3);

            overflowTarget.ApplyAttackGain(
                int.MaxValue - 1 - overflowTarget.Attack);

            var runner = e.CreateRunner();

            Assert.Throws<OverflowException>(() => Start(runner));

            Assert.That(runner.HasActiveCombat, Is.True);

            for (var column = 1; column <= 3; column++)
            {
                Assert.That(
                    e.Card(BoardRow.Front, column).Attack,
                    Is.EqualTo(4));
            }

            Assert.That(e.Card(BoardRow.Back, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Card(BoardRow.Back, 2).Attack, Is.EqualTo(2));
            Assert.That(
                overflowTarget.Attack,
                Is.EqualTo(int.MaxValue - 1));

            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.UpperPet.InstanceId),
                Is.True);

            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.LowerPet.InstanceId),
                Is.False);

            var firstGains = e.Events<AttackGainCombatEvent>();
            Assert.That(firstGains.Count, Is.EqualTo(3));

            var petStage = e.PetStage();

            overflowTarget.ReduceAttack(int.MaxValue - 3);

            var completed = runner.ResumeActiveCombat(
                maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(completed, Is.Not.Null);

            foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
            {
                for (var column = 1; column <= 3; column++)
                {
                    Assert.That(
                        e.Card(row, column).Attack,
                        Is.EqualTo(4));
                }
            }

            var allGains = e.Events<AttackGainCombatEvent>();
            Assert.That(allGains.Count, Is.EqualTo(6));

            for (var index = 0; index < firstGains.Count; index++)
            {
                Assert.That(allGains[index], Is.SameAs(firstGains[index]));
            }

            Assert.That(e.PetStage(), Is.SameAs(petStage));

            Assert.That(
                e.Events<CombatStartedCombatEvent>().Count,
                Is.EqualTo(1));

            Assert.That(
                e.Events<CombatCompletedCombatEvent>().Count,
                Is.EqualTo(1));

            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void RebuiltSources_PreserveCompletedUsage()
        {
            var e = new Environment();
            Start(e.CreateRunner());

            Assert.That(
                e.Sources().DiscoverTriggers(e.State, e.PetStage()),
                Is.Empty);

            Assert.That(
                e.Events<AttackGainCombatEvent>().Count,
                Is.EqualTo(6));

            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void FreshRuntime_AllowsSamePetIdsInNewBattle()
        {
            var first = new Environment();
            Start(first.CreateRunner());

            var second = new Environment();

            Assert.That(
                second.UpperPet.InstanceId,
                Is.EqualTo(first.UpperPet.InstanceId));

            Assert.That(second.Runtime.PetUsageRegistry.Count, Is.Zero);

            Start(second.CreateRunner());

            Assert.That(
                first.Card(BoardRow.Front, 1).Attack,
                Is.EqualTo(4));

            Assert.That(
                second.Card(BoardRow.Front, 1).Attack,
                Is.EqualTo(4));

            Assert.That(
                second.Events<AttackGainCombatEvent>().Count,
                Is.EqualTo(6));

            Assert.That(second.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
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

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;

            public readonly CombatPetTriggerRuntime Runtime =
                new CombatPetTriggerRuntime();

            public readonly CombatEventLog Log = new CombatEventLog();

            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            public readonly CombatEventQueue Queue;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatArmorGainResolver Armor;

            public Environment(
                CombatSide side = CombatSide.Player,
                int matchingCount = 3)
            {
                Side = side;

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.Macaque,
                    new InstanceId(1002));

                LowerPet = new CombatPetState(
                    CombatPetDefinitionIds.Macaque,
                    new InstanceId(1001));

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

                var own = MakeSide(side, true, matchingCount);
                var opposing = MakeSide(otherSide, false, matchingCount);

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(new[] { UpperPet, LowerPet }));

                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Enemy ? own : opposing,
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);

                Queue = new CombatEventQueue(Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
            }

            public CombatCardState Card(BoardRow row, int column) =>
                State.GetSide(Side).GetCardAt(
                    new BoardPosition(Side, row, new BoardColumn(column)));

            public CombatTriggerSourceRegistry Sources() =>
                Runtime.BuildSourceRegistry(
                    State,
                    Armor,
                    Attack,
                    new CombatCardLookup(Log));

            public CombatResolutionRunner CreateRunner() =>
                Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);

            public List<T> Events<T>() where T : CombatEvent
            {
                var result = new List<T>();

                foreach (var item in Log.Events)
                {
                    if (item is T typed)
                    {
                        result.Add(typed);
                    }
                }

                return result;
            }

            public BattleStartStageStartedCombatEvent PetStage()
            {
                BattleStartStageStartedCombatEvent result = null;

                foreach (var stage in Events<BattleStartStageStartedCombatEvent>())
                {
                    if (!stage.IsPetStage)
                    {
                        continue;
                    }

                    Assert.That(result, Is.Null, "Pet stage must occur once.");
                    result = stage;
                }

                Assert.That(result, Is.Not.Null);
                return result;
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                bool occupied,
                int matchingCount)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();

                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var prefix = (side == CombatSide.Player ? 0 : 100)
                            + (row == BoardRow.Front ? 0 : 10);

                        CombatCardState card = null;

                        if (occupied && column <= 3)
                        {
                            card = new CombatCardState(
                                new DefinitionId("test.runtime_macaque_card"),
                                new InstanceId(prefix + 6 - column),
                                new CardRank(
                                    column <= matchingCount ? 7 : 8 + column),
                                CombatCardSeason.Spring,
                                hpCapacity: 10,
                                currentHp: 5,
                                armor: 0,
                                attack: 2);

                            cards.Add(card);
                        }

                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(side, row, new BoardColumn(column)),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(100),
                    new AttackMultiplier(1));
            }
        }
    }
}