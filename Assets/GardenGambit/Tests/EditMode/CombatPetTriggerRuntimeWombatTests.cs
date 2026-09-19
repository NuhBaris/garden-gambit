using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeWombatTests
    {
        [Test]
        public void Catalogue_RegistersStableWombatIdentityWithSharedDependencies()
        {
            var e = new Battle();
            Assert.That(CombatPetDefinitionIds.WombatValue, Is.EqualTo("pet.wombat"));
            Assert.That(CombatPetDefinitionIds.Wombat.IsValid, Is.True);
            Assert.That(CombatPetDefinitionIds.Wombat, Is.EqualTo(new DefinitionId(CombatPetDefinitionIds.WombatValue)));
            Assert.That(new HashSet<DefinitionId>
            {
                CombatPetDefinitionIds.SunBird, CombatPetDefinitionIds.PolarFerret,
                CombatPetDefinitionIds.MuskCat, CombatPetDefinitionIds.RainSparrow,
                CombatPetDefinitionIds.HarvestMouse, CombatPetDefinitionIds.Wombat
            }.Count, Is.EqualTo(6));
            var attack = new CombatAttackGainResolver(e.Metadata, e.Log);
            var armor = new CombatArmorGainResolver(e.Metadata, e.Log);
            var registry = e.Runtime.FactoryCatalog.CreateRegistry(
                armor, attack, new CombatCardLookup(e.Log), e.Runtime.PetUsageCommitter);
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            var factory = registry.GetFactory(CombatPetDefinitionIds.Wombat) as WombatPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.PetDefinitionId, Is.EqualTo(CombatPetDefinitionIds.Wombat));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(factory.AttackGainResolver, Is.SameAs(attack));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry().Contains(CombatPetDefinitionIds.Wombat), Is.False);
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry(armor).Contains(CombatPetDefinitionIds.Wombat), Is.False);
        }

        [Test]
        public void RuntimeSourceRebuild_KeepsUsedWombatState()
        {
            var e = new Battle();
            var armor = new CombatArmorGainResolver(e.Metadata, e.Log);
            var attack = new CombatAttackGainResolver(e.Metadata, e.Log);
            var lookup = new CombatCardLookup(e.Log);
            var first = e.Runtime.BuildSourceRegistry(e.State, armor, attack, lookup);
            Assert.That(first.Count, Is.EqualTo(1));
            var source = first.Sources[0] as WombatPetTriggerSource;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Wombat.InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(source.AttackGainResolver, Is.SameAs(attack));
            source.UsageCommitter.TryCommit(e.Wombat.InstanceId, () => { });
            var rebuilt = e.Runtime.BuildSourceRegistry(e.State, armor, attack, lookup);
            var rebuiltSource = (WombatPetTriggerSource)rebuilt.Sources[0];
            Assert.That(rebuiltSource, Is.Not.SameAs(source));
            Assert.That(rebuiltSource.UsageCommitter.HasTriggered(e.Wombat.InstanceId), Is.True);
            Assert.That(e.Start(), Is.Not.Null);
            Assert.That(e.Find<AttackGainCombatEvent>(), Is.Empty);
            Assert.That(e.Target.Attack, Is.EqualTo(2));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void NormalCombat_ResolvesWinterDeathAndSummerTargetThroughRuntime(CombatSide side)
        {
            var e = new Battle(side);
            Assert.That(e.Start(), Is.Not.Null);
            AssertWombatGain(e);
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Runner.UsesStagedNormalAttackByDefault, Is.True);
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            Assert.That(new CombatCardLookup(e.Log).Get(e.State, e.Donor.InstanceId).IsRemoved, Is.True);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void DeathOverflowThenResume_DoesNotRepeatDeathOrAttackGain(CombatSide side)
        {
            var e = new Battle(side);
            e.Target.ApplyAttackGain(int.MaxValue - e.Target.Attack);
            Assert.Throws<OverflowException>(() => e.Start());
            Assert.That(e.Runner.HasActiveCombat, Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(e.Find<AttackGainCombatEvent>(), Is.Empty);
            var deaths = e.Find<DeathCombatEvent>().FindAll(item => item.InstanceId == e.Donor.InstanceId);
            Assert.That(deaths.Count, Is.EqualTo(1));
            var originalDeath = deaths[0];
            e.Target.ReduceAttack(int.MaxValue - 2);
            Assert.That(e.Runner.ResumeActiveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100), Is.Not.Null);
            AssertWombatGain(e);
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(1));
            deaths = e.Find<DeathCombatEvent>().FindAll(item => item.InstanceId == e.Donor.InstanceId);
            Assert.That(deaths.Count, Is.EqualTo(1));
            Assert.That(deaths[0], Is.SameAs(originalDeath));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void WombatAndRainSparrow_ResolveTheirAltarTriggersInTheSameBattle(CombatSide side)
        {
            var e = new Battle(side, Companion.RainSparrow);
            Assert.That(e.Start(), Is.Not.Null);
            AssertWombatGain(e);
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(1));
            var armorGains = e.Find<ArmorGainCombatEvent>();
            Assert.That(armorGains.Count, Is.EqualTo(1));
            Assert.That(armorGains[0].TargetInstanceId, Is.EqualTo(e.BackFirst.InstanceId));
            Assert.That(armorGains[0].ActualGainedAmount, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.CompanionPet.InstanceId, e.BackFirst.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            var hpGain = e.Log.GetEvent(armorGains[0].Metadata.ParentEventId.Value) as HpGainCombatEvent;
            Assert.That(hpGain, Is.Not.Null);
            Assert.That(hpGain.TargetInstanceId, Is.EqualTo(e.BackFirst.InstanceId));
            Assert.That(armorGains[0].Metadata.TriggerRootId, Is.EqualTo(hpGain.Metadata.TriggerRootId));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void WombatAndHarvestMouse_ShareRegistryButKeepIndependentPetUses(CombatSide side)
        {
            var e = new Battle(side, Companion.HarvestMouse);
            Assert.That(e.Start(), Is.Not.Null);
            AssertWombatGain(e);
            Assert.That(e.BackSecond.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.CompanionPet.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            var gains = e.Find<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(2));
            var harvestGains = gains.FindAll(item => item.TargetInstanceId == e.BackSecond.InstanceId);
            Assert.That(harvestGains.Count, Is.EqualTo(1));
            var death = e.Log.GetEvent(harvestGains[0].Metadata.ParentEventId.Value) as DeathCombatEvent;
            Assert.That(death, Is.Not.Null);
            Assert.That(death.InstanceId, Is.EqualTo(e.BackFirst.InstanceId));
            Assert.That(death.Position.Row, Is.EqualTo(BoardRow.Back));
            Assert.That(harvestGains[0].Metadata.TriggerRootId, Is.EqualTo(death.Metadata.TriggerRootId));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void NewRuntime_GivesSamePetIdItsOwnBattleUsage()
        {
            var first = new Battle();
            var second = new Battle();
            Assert.That(first.Wombat.InstanceId, Is.EqualTo(second.Wombat.InstanceId));
            Assert.That(first.Start(), Is.Not.Null);
            Assert.That(second.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(second.Start(), Is.Not.Null);
            AssertWombatGain(first);
            AssertWombatGain(second);
            Assert.That(first.Runtime.PetUsageRegistry, Is.Not.SameAs(second.Runtime.PetUsageRegistry));
        }

        private static void AssertWombatGain(Battle e)
        {
            Assert.That(e.Target.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.Wombat.InstanceId), Is.True);
            var gains = e.Find<AttackGainCombatEvent>().FindAll(item => item.TargetInstanceId == e.Target.InstanceId);
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(gains[0].PreviousAttack, Is.EqualTo(2));
            Assert.That(gains[0].CurrentAttack, Is.EqualTo(3));
            var death = e.Log.GetEvent(gains[0].Metadata.ParentEventId.Value) as DeathCombatEvent;
            Assert.That(death, Is.Not.Null);
            Assert.That(death.InstanceId, Is.EqualTo(e.Donor.InstanceId));
            Assert.That(death.Position.Row, Is.EqualTo(BoardRow.Front));
            Assert.That(gains[0].Metadata.TriggerRootId, Is.EqualTo(death.Metadata.TriggerRootId));
        }

        private enum Companion { None, RainSparrow, HarvestMouse }

        private sealed class Battle
        {
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventQueue Queue;
            public readonly CombatState State;
            public readonly CombatResolutionRunner Runner;
            public readonly CombatPetState Wombat;
            public readonly CombatPetState CompanionPet;
            public readonly CombatCardState Donor;
            public readonly CombatCardState Target;
            public readonly CombatCardState BackFirst;
            public readonly CombatCardState BackSecond;

            public Battle(CombatSide side = CombatSide.Player, Companion companion = Companion.None)
            {
                Donor = Card(1, CombatCardSeason.Winter, 1, 0);
                Target = Card(2, CombatCardSeason.Summer, 10, 2);
                Wombat = new CombatPetState(CombatPetDefinitionIds.Wombat, new InstanceId(1001));
                if (companion == Companion.RainSparrow)
                {
                    BackFirst = Card(3, CombatCardSeason.Spring, 10, 1);
                    CompanionPet = new CombatPetState(CombatPetDefinitionIds.RainSparrow, new InstanceId(1002));
                }
                else if (companion == Companion.HarvestMouse)
                {
                    BackFirst = Card(3, CombatCardSeason.Autumn, 1, 0);
                    BackSecond = Card(4, CombatCardSeason.Autumn, 10, 2);
                    CompanionPet = new CombatPetState(CombatPetDefinitionIds.HarvestMouse, new InstanceId(1002));
                }
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var own = CreateSide(side, Donor, Target, BackFirst, BackSecond,
                    companion == Companion.RainSparrow ? (BoardRow?)BoardRow.Front :
                    companion == Companion.HarvestMouse ? (BoardRow?)BoardRow.Back : null);
                var other = CreateSide(otherSide, Card(101, CombatCardSeason.Winter, 5, 2), null, null, null, null);
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(
                    CompanionPet == null ? new[] { Wombat } : new[] { Wombat, CompanionPet }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : other, side == CombatSide.Enemy ? own : other,
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Runner = Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);
            }

            public CombatCompletedCombatEvent Start() => Runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 20, maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);

            public List<T> Find<T>() where T : CombatEvent
            {
                var result = new List<T>();
                foreach (var combatEvent in Log.Events)
                {
                    var typed = combatEvent as T;
                    if (typed != null) { result.Add(typed); }
                }
                return result;
            }

            private static CombatCardState Card(long id, CombatCardSeason season, int hp, int attack) =>
                new CombatCardState(new DefinitionId("test.runtime_wombat_card"), new InstanceId(id), new CardRank(2),
                    season, hp, hp, 0, attack);

            private static CombatSideState CreateSide(CombatSide side, CombatCardState first, CombatCardState second,
                CombatCardState backFirst, CombatCardState backSecond, BoardRow? altarRow)
            {
                var cards = new List<CombatCardState> { first };
                if (second != null) { cards.Add(second); }
                if (backFirst != null) { cards.Add(backFirst); }
                if (backSecond != null) { cards.Add(backSecond); }
                var slots = new List<CombatSlotState>();
                for (var column = 1; column <= 5; column++)
                {
                    foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                    {
                        var card = row == BoardRow.Front ? (column == 1 ? first : column == 2 ? second : null) :
                            (column == 1 ? backFirst : column == 2 ? backSecond : null);
                        var position = new BoardPosition(side, row, new BoardColumn(column));
                        var value = (side == CombatSide.Player ? 0 : 100) + column * 2 + (row == BoardRow.Front ? 0 : 1);
                        slots.Add(new CombatSlotState(new SlotId(value), position,
                            card == null ? (InstanceId?)null : card.InstanceId,
                            column == 1 && altarRow == row ? CombatSlotEnhanceKind.SacrificialAltar : CombatSlotEnhanceKind.None));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
