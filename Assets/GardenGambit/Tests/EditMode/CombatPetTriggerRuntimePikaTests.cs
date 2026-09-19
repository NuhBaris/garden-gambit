using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimePikaTests
    {
        [Test]
        public void Catalogue_RegistersStablePikaIdentityAndSharedDependencies()
        {
            var e = new Battle();
            Assert.That(CombatPetDefinitionIds.PikaValue, Is.EqualTo("pet.pika"));
            Assert.That(CombatPetDefinitionIds.Pika.IsValid, Is.True);
            Assert.That(CombatPetDefinitionIds.Pika, Is.EqualTo(new DefinitionId("pet.pika")));
            var ids = new HashSet<DefinitionId>
            {
                CombatPetDefinitionIds.SunBird, CombatPetDefinitionIds.PolarFerret,
                CombatPetDefinitionIds.MuskCat, CombatPetDefinitionIds.RainSparrow,
                CombatPetDefinitionIds.HarvestMouse, CombatPetDefinitionIds.Wombat,
                CombatPetDefinitionIds.Ladybug, CombatPetDefinitionIds.Pika
            };
            Assert.That(ids.Count, Is.EqualTo(8));
            var registry = e.FullCatalogue();
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            foreach (var id in ids) { Assert.That(registry.Contains(id), Is.True); }
            var factory = registry.GetFactory(CombatPetDefinitionIds.Pika) as PikaPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.PetDefinitionId, Is.EqualTo(CombatPetDefinitionIds.Pika));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(factory.FinalRankModifierRegistry, Is.SameAs(e.Runtime.FinalRankModifierRegistry));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry().Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry(e.Armor).Count, Is.EqualTo(4));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Pika), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneSourceWithCorrectOwnerAndSharedDependencies(CombatSide side)
        {
            var e = new Battle(side);
            var factory = (PikaPetTriggerSourceFactory)e.FullCatalogue().GetFactory(CombatPetDefinitionIds.Pika);
            var sources = new List<ICombatTriggerSource>(factory.CreateSources(side, e.UpperPet));
            Assert.That(sources.Count, Is.EqualTo(1));
            var source = sources[0] as PikaPetTriggerSource;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.UpperPet.InstanceId));
            Assert.That(source.Handler, Is.TypeOf<PikaPetBattleEndTriggerHandler>());
            Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(source.FinalRankModifierRegistry, Is.SameAs(e.Runtime.FinalRankModifierRegistry));
            Assert.That(source.OrderKeyProvider, Is.Not.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Factory_RejectsMissingDependency(bool missingUsage)
        {
            var e = new Battle();
            Assert.Throws<ArgumentNullException>(() => new PikaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Pika, missingUsage ? null : e.Runtime.UsageCommitter,
                missingUsage ? e.Runtime.FinalRankModifierRegistry : null));
        }

        [Test]
        public void Factory_RejectsInvalidDefinitionId()
        {
            var e = new Battle();
            Assert.Throws<ArgumentException>(() => new PikaPetTriggerSourceFactory(
                default(DefinitionId), e.Runtime.UsageCommitter, e.Runtime.FinalRankModifierRegistry));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Factory_RejectsInvalidSourceRequest(int invalid)
        {
            var e = new Battle();
            var factory = (PikaPetTriggerSourceFactory)e.FullCatalogue().GetFactory(CombatPetDefinitionIds.Pika);
            if (invalid == 0)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new List<ICombatTriggerSource>(
                    factory.CreateSources((CombatSide)99, e.UpperPet)));
            }
            else if (invalid == 1)
            {
                Assert.Throws<ArgumentNullException>(() => new List<ICombatTriggerSource>(
                    factory.CreateSources(e.Side, null)));
            }
            else
            {
                var pet = new CombatPetState(CombatPetDefinitionIds.MuskCat, new InstanceId(1003));
                Assert.Throws<ArgumentException>(() => new List<ICombatTriggerSource>(factory.CreateSources(e.Side, pet)));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Source_RejectsMissingDependency(bool missingUsage)
        {
            var e = new Battle();
            Assert.Throws<ArgumentNullException>(() => new PikaPetTriggerSource(e.Side, e.UpperPet.InstanceId,
                missingUsage ? null : e.Runtime.UsageCommitter,
                missingUsage ? e.Runtime.FinalRankModifierRegistry : null));
        }

        [TestCase(CombatSide.Player, 3)]
        [TestCase(CombatSide.Player, 4)]
        [TestCase(CombatSide.Player, 5)]
        [TestCase(CombatSide.Enemy, 3)]
        [TestCase(CombatSide.Enemy, 4)]
        [TestCase(CombatSide.Enemy, 5)]
        public void NormalCombat_AppliesThresholdBonusBeforeResultMultiplier(CombatSide side, int count)
        {
            var e = new Battle(side, count, multiplier: 2);
            var completed = e.Start();
            var bonus = count >= 4 ? count : 0;
            AssertResult(e, completed, (2 * count + bonus) * 2);
            Assert.That(e.Find<NormalAttackExchangeCombatEvent>().Count, Is.GreaterThan(0));
            Assert.That(e.Find<DeathCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(bonus));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            for (var column = 1; column <= count; column++)
            {
                var card = e.Card(BoardRow.Front, column);
                Assert.That(e.Runtime.FinalRankModifierRegistry.GetTotalModifier(card.InstanceId), Is.EqualTo(count >= 4 ? 1 : 0));
                Assert.That(card.Rank.Value, Is.EqualTo(2));
            }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void LowerPika_BuffsOnlyBackRowThroughRuntime(CombatSide side)
        {
            var e = new Battle(side, 5, 4, Roster.Lower);
            AssertResult(e, e.Start(), 22);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(4));
            for (var column = 1; column <= 5; column++)
            {
                Assert.That(e.Runtime.FinalRankModifierRegistry.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.Zero);
                if (column <= 4)
                {
                    var id = e.Card(BoardRow.Back, column).InstanceId;
                    Assert.That(e.Runtime.FinalRankModifierRegistry.GetTotalModifier(id), Is.EqualTo(1));
                    Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, id), Is.True);
                }
            }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void PikaAndMuskCat_ContributeIndependentlyToSameBattleResult(CombatSide side)
        {
            var e = new Battle(side, 4, 1, Roster.Musk);
            AssertResult(e, e.Start(), 18);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(5));
            for (var column = 1; column <= 4; column++)
            {
                Assert.That(e.Runtime.FinalRankModifierRegistry.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(1));
            }
            var lower = e.Card(BoardRow.Back, 1);
            Assert.That(e.Runtime.FinalRankModifierRegistry.GetTotalModifier(lower.InstanceId), Is.EqualTo(4));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, lower.InstanceId), Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void TwoPikas_ResolveBothRowsWithIndependentUsage(CombatSide side)
        {
            var e = new Battle(side, 4, 4, Roster.Both);
            AssertResult(e, e.Start(), 24);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(8));
            for (var column = 1; column <= 4; column++)
            {
                Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId,
                    e.Card(BoardRow.Front, column).InstanceId), Is.True);
                Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId,
                    e.Card(BoardRow.Back, column).InstanceId), Is.True);
            }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void BattleEndOverflowThenResume_DoesNotRepeatAttackDeathOrBonus(CombatSide side)
        {
            var e = new Battle(side);
            var last = e.Card(BoardRow.Front, 4);
            e.Runtime.FinalRankModifierRegistry.AddModifier(last.InstanceId, int.MaxValue);
            Assert.Throws<OverflowException>(() => e.Start());
            Assert.That(e.Runner.HasActiveCombat, Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.FinalRankModifierRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Find<CombatCompletedCombatEvent>(), Is.Empty);
            var attacks = e.Find<NormalAttackExchangeCombatEvent>().Count;
            Assert.That(attacks, Is.GreaterThan(0));
            var deaths = e.Find<DeathCombatEvent>();
            var ends = e.Find<BattleEndStartedCombatEvent>();
            Assert.That(deaths.Count, Is.EqualTo(1));
            Assert.That(ends.Count, Is.EqualTo(1));
            e.Runtime.FinalRankModifierRegistry.AddModifier(last.InstanceId, -int.MaxValue);
            var completed = e.Runner.ResumeActiveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);
            AssertResult(e, completed, 12);
            Assert.That(e.Find<NormalAttackExchangeCombatEvent>().Count, Is.EqualTo(attacks));
            Assert.That(e.Find<DeathCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<DeathCombatEvent>()[0], Is.SameAs(deaths[0]));
            Assert.That(e.Find<BattleEndStartedCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<BattleEndStartedCombatEvent>()[0], Is.SameAs(ends[0]));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(4));
            for (var column = 1; column <= 4; column++)
            {
                Assert.That(e.Runtime.FinalRankModifierRegistry.GetTotalModifier(e.Card(BoardRow.Front, column).InstanceId), Is.EqualTo(1));
            }
        }

        [Test]
        public void RebuildingSources_PreservesUsedStateAndFinalRankModifiers()
        {
            var e = new Battle();
            var first = e.Sources();
            Assert.That(first.Count, Is.EqualTo(1));
            var source = first.Sources[0] as PikaPetTriggerSource;
            Assert.That(source, Is.Not.Null);
            for (var column = 1; column <= 4; column++)
            {
                var id = e.Card(BoardRow.Front, column).InstanceId;
                source.UsageCommitter.TryCommit(e.UpperPet.InstanceId, id,
                    () => source.FinalRankModifierRegistry.AddModifier(id, 1));
            }
            var rebuilt = (PikaPetTriggerSource)e.Sources().Sources[0];
            Assert.That(rebuilt, Is.Not.SameAs(source));
            Assert.That(rebuilt.UsageCommitter, Is.SameAs(source.UsageCommitter));
            Assert.That(rebuilt.FinalRankModifierRegistry, Is.SameAs(source.FinalRankModifierRegistry));
            AssertResult(e, e.Start(), 12);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(4));
        }

        [Test]
        public void NewRuntime_WithSameInstanceIdsStartsWithIndependentState()
        {
            var first = new Battle();
            var second = new Battle();
            Assert.That(first.UpperPet.InstanceId, Is.EqualTo(second.UpperPet.InstanceId));
            AssertResult(first, first.Start(), 12);
            Assert.That(second.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(second.Runtime.FinalRankModifierRegistry.Count, Is.Zero);
            AssertResult(second, second.Start(), 12);
            Assert.That(second.Runtime.UsageRegistry, Is.Not.SameAs(first.Runtime.UsageRegistry));
            Assert.That(second.Runtime.FinalRankModifierRegistry, Is.Not.SameAs(first.Runtime.FinalRankModifierRegistry));
        }

        private static void AssertResult(Battle e, CombatCompletedCombatEvent completed, int resultDamage)
        {
            Assert.That(completed, Is.Not.Null);
            Assert.That(completed.Outcome, Is.EqualTo(e.Side == CombatSide.Player ? CombatOutcome.PlayerVictory : CombatOutcome.EnemyVictory));
            Assert.That(completed.PlayerBattleHealth.Value, Is.EqualTo(e.Side == CombatSide.Player ? 100 : 100 - resultDamage));
            Assert.That(completed.EnemyBattleHealth.Value, Is.EqualTo(e.Side == CombatSide.Enemy ? 100 : 100 - resultDamage));
            Assert.That(e.Runner.FinalRankModifierRegistry, Is.SameAs(e.Runtime.FinalRankModifierRegistry));
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(e.Find<CombatCompletedCombatEvent>().Count, Is.EqualTo(1));
        }

        private enum Roster { Upper, Lower, Both, Musk }

        private sealed class Battle
        {
            public readonly CombatSide Side;
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventQueue Queue;
            public readonly CombatState State;
            public readonly CombatResolutionRunner Runner;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatCardLookup Lookup;

            public Battle(CombatSide side = CombatSide.Player, int front = 4, int back = 0,
                Roster roster = Roster.Upper, int multiplier = 1)
            {
                Side = side;
                UpperPet = new CombatPetState(roster == Roster.Lower ? CombatPetDefinitionIds.SunBird : CombatPetDefinitionIds.Pika,
                    new InstanceId(1001));
                if (roster != Roster.Upper)
                {
                    LowerPet = new CombatPetState(roster == Roster.Musk ? CombatPetDefinitionIds.MuskCat : CombatPetDefinitionIds.Pika,
                        new InstanceId(1002));
                }
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var own = CreateSide(side, front, back, false, multiplier);
                var other = CreateSide(otherSide, 1, 0, true, 1);
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(
                    LowerPet == null ? new[] { UpperPet } : new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : other, side == CombatSide.Enemy ? own : other,
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
                Lookup = new CombatCardLookup(Log);
                Runner = Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);
            }

            public CombatPetTriggerSourceFactoryRegistry FullCatalogue() => Runtime.FactoryCatalog.CreateRegistry(
                Armor, Attack, Lookup, Runtime.PetUsageCommitter);
            public CombatTriggerSourceRegistry Sources() => Runtime.BuildSourceRegistry(State, Armor, Attack, Lookup);
            public CombatCardState Card(BoardRow row, int column) => State.GetSide(Side).GetCardAt(
                new BoardPosition(Side, row, new BoardColumn(column)));
            public CombatCompletedCombatEvent Start() => Runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);

            public List<T> Find<T>() where T : CombatEvent
            {
                var events = new List<T>();
                foreach (var item in Log.Events) { if (item is T typed) { events.Add(typed); } }
                return events;
            }

            private static CombatSideState CreateSide(CombatSide side, int front, int back, bool opponent, int multiplier)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();
                for (var column = 1; column <= 5; column++)
                {
                    foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                    {
                        var id = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 5) + column;
                        CombatCardState card = null;
                        if (column <= (row == BoardRow.Front ? front : back))
                        {
                            card = new CombatCardState(new DefinitionId("test.runtime_pika_card"), new InstanceId(id),
                                new CardRank(2), CombatCardSeason.Winter, opponent ? 1 : 10, opponent ? 1 : 10, 0, opponent ? 0 : 2);
                            cards.Add(card);
                        }
                        slots.Add(new CombatSlotState(new SlotId(id), new BoardPosition(side, row, new BoardColumn(column)),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(100), new AttackMultiplier(multiplier));
            }
        }
    }
}
