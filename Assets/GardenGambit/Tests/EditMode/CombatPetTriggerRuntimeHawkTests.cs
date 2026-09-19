using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeHawkTests
    {
        [Test]
        public void Catalogue_RegistersHawkWithSharedDependencies()
        {
            var e = new Environment();

            Assert.That(
                CombatPetDefinitionIds.HawkValue,
                Is.EqualTo("pet.hawk"));

            Assert.That(
                CombatPetDefinitionIds.Hawk,
                Is.EqualTo(new DefinitionId("pet.hawk")));

            var registry = e.Runtime.FactoryCatalog.CreateRegistry(
                e.Armor,
                e.Attack,
                new CombatCardLookup(e.Log),
                e.Runtime.PetUsageCommitter);

            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));

            var factory = registry.GetFactory(
                CombatPetDefinitionIds.Hawk) as HawkPetTriggerSourceFactory;

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
                    CombatPetDefinitionIds.Hawk),
                Is.False);

            var armorRegistry = e.Runtime.FactoryCatalog.CreateRegistry(
                e.Armor);

            Assert.That(armorRegistry.Count, Is.EqualTo(4));
            Assert.That(
                armorRegistry.Contains(CombatPetDefinitionIds.Hawk),
                Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void BuildSources_CreatesBothHawksWithSharedUsage(
            CombatSide side)
        {
            var e = new Environment(side);
            var registry = e.Sources();

            Assert.That(registry.Count, Is.EqualTo(2));

            var petIds = new HashSet<InstanceId>();

            foreach (var item in registry.Sources)
            {
                var source = item as HawkPetTriggerSource;
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
        public void Runtime_StartCompletesWithIndependentRowBonuses(
            CombatSide side)
        {
            var e = new Environment(side);
            var runner = e.CreateRunner();

            Assert.That(runner.UsesStagedNormalAttackByDefault, Is.True);

            var completed = Start(runner);

            Assert.That(completed, Is.Not.Null);
            Assert.That(e.Front.Attack, Is.EqualTo(8));
            Assert.That(e.Back.Attack, Is.EqualTo(8));
            Assert.That(e.Front.CurrentHp, Is.EqualTo(5));
            Assert.That(e.Back.CurrentHp, Is.EqualTo(5));

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

            var gains = e.Events<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(2));

            // Pet slot order takes precedence over the numeric InstanceIds.
            Assert.That(
                gains[0].TargetInstanceId,
                Is.EqualTo(e.Front.InstanceId));
            Assert.That(
                gains[1].TargetInstanceId,
                Is.EqualTo(e.Back.InstanceId));

            var petStage = e.PetStage();

            foreach (var gain in gains)
            {
                Assert.That(gain.ActualGainedAmount, Is.EqualTo(3));
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

        [TestCase(BoardRow.Front)]
        [TestCase(BoardRow.Back)]
        public void Runtime_EmptyPetRowCompletesWithoutBonus(BoardRow emptyRow)
        {
            var e = new Environment();

            e.State.GetSide(e.Side).RemoveCardFromCombat(
                e.Position(emptyRow));

            var runner = e.CreateRunner();
            Assert.That(Start(runner), Is.Not.Null);

            var remaining = emptyRow == BoardRow.Front ? e.Back : e.Front;

            Assert.That(remaining.Attack, Is.EqualTo(8));
            Assert.That(e.Events<AttackGainCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_ResumeDoesNotRepeatSuccessfulUpperHawk(
            CombatSide side)
        {
            var e = new Environment(side);

            e.Back.ApplyAttackGain(int.MaxValue - 1 - e.Back.Attack);

            var runner = e.CreateRunner();

            Assert.Throws<OverflowException>(() => Start(runner));

            Assert.That(runner.HasActiveCombat, Is.True);
            Assert.That(e.Front.Attack, Is.EqualTo(8));
            Assert.That(e.Back.Attack, Is.EqualTo(int.MaxValue - 1));

            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.UpperPet.InstanceId),
                Is.True);

            Assert.That(
                e.Runtime.PetUsageCommitter.HasTriggered(
                    e.LowerPet.InstanceId),
                Is.False);

            var gainsBeforeResume = e.Events<AttackGainCombatEvent>();
            Assert.That(gainsBeforeResume.Count, Is.EqualTo(1));
            var firstGain = gainsBeforeResume[0];
            var petStage = e.PetStage();

            // Restore the original Attack without reapplying the first effect.
            e.Back.ReduceAttack(int.MaxValue - 6);

            var completed = runner.ResumeActiveCombat(
                maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(completed, Is.Not.Null);
            Assert.That(e.Front.Attack, Is.EqualTo(8));
            Assert.That(e.Back.Attack, Is.EqualTo(8));

            var gains = e.Events<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(gains[0], Is.SameAs(firstGain));
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
        public void RebuiltSources_PreserveCompletedStartUsage()
        {
            var e = new Environment();
            Start(e.CreateRunner());

            var rebuilt = e.Sources();

            Assert.That(
                rebuilt.DiscoverTriggers(e.State, e.PetStage()),
                Is.Empty);

            Assert.That(e.Events<AttackGainCombatEvent>().Count, Is.EqualTo(2));
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

            Assert.That(first.Front.Attack, Is.EqualTo(8));
            Assert.That(second.Front.Attack, Is.EqualTo(8));
            Assert.That(second.Back.Attack, Is.EqualTo(8));
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
            public readonly CombatCardState Front;
            public readonly CombatCardState Back;
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

            public Environment(CombatSide side = CombatSide.Player)
            {
                Side = side;
                Front = CreateCard(1);
                Back = CreateCard(2);

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.Hawk,
                    new InstanceId(1002));

                LowerPet = new CombatPetState(
                    CombatPetDefinitionIds.Hawk,
                    new InstanceId(1001));

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

                var own = MakeSide(side, true);
                var opposing = MakeSide(otherSide, false);

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

            public BoardPosition Position(BoardRow row) =>
                new BoardPosition(Side, row, new BoardColumn(1));

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

            private CombatSideState MakeSide(CombatSide side, bool occupied)
            {
                var slots = new List<CombatSlotState>();

                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var slotId = (side == CombatSide.Player ? 0 : 100)
                            + (row == BoardRow.Front ? 0 : 10)
                            + column;

                        InstanceId? occupant = null;

                        if (occupied && column == 1)
                        {
                            occupant = row == BoardRow.Front
                                ? Front.InstanceId
                                : Back.InstanceId;
                        }

                        slots.Add(new CombatSlotState(
                            new SlotId(slotId),
                            new BoardPosition(
                                side, row, new BoardColumn(column)),
                            occupant));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(
                        occupied
                            ? new[] { Front, Back }
                            : Array.Empty<CombatCardState>()),
                    new BattleHealth(100),
                    new AttackMultiplier(1));
            }

            private static CombatCardState CreateCard(long id)
            {
                return new CombatCardState(
                    new DefinitionId("test.runtime_hawk_card"),
                    new InstanceId(id),
                    new CardRank(4),
                    CombatCardSeason.Spring,
                    hpCapacity: 10,
                    currentHp: 5,
                    armor: 0,
                    attack: 5);
            }
        }
    }
}