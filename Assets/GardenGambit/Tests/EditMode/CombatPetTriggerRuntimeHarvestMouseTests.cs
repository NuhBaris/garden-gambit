using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeHarvestMouseTests
    {
        [Test]
        public void Constructor_RejectsNullPetUsageRegistry()
        {
            Assert.Throws<ArgumentNullException>(() => CreateRuntime(null));
        }

        [Test]
        public void Constructor_UsesInjectedPetRegistryAndKeepsCardScopedRegistrySeparate()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var petId = new InstanceId(1001);
            registry.TryRegister(petId);
            var runtime = CreateRuntime(registry);
            Assert.That(runtime.PetUsageRegistry, Is.SameAs(registry));
            Assert.That(runtime.PetUsageCommitter.UsageRegistry, Is.SameAs(registry));
            Assert.That(runtime.PetUsageCommitter.HasTriggered(petId), Is.True);
            Assert.That(runtime.UsageRegistry.Count, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void FullBuildSourceRegistry_RejectsMissingDependencies(int missing)
        {
            var e = new Scenario();
            var armor = new CombatArmorGainResolver(e.Metadata, e.Log);
            var attack = new CombatAttackGainResolver(e.Metadata, e.Log);
            var lookup = new CombatCardLookup(e.Log);
            Assert.Throws<ArgumentNullException>(() => e.Runtime.BuildSourceRegistry(
                missing == 0 ? null : e.State, missing == 1 ? null : armor,
                missing == 2 ? null : attack, missing == 3 ? null : lookup));
            Assert.That(e.Log.Count, Is.Zero);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void FullBuildSourceRegistry_SharesDependenciesAndRebuildDoesNotResetUse()
        {
            var e = new Scenario();
            var armor = new CombatArmorGainResolver(e.Metadata, e.Log);
            var attack = new CombatAttackGainResolver(e.Metadata, e.Log);
            var lookup = new CombatCardLookup(e.Log);
            var first = e.Runtime.BuildSourceRegistry(e.State, armor, attack, lookup);
            Assert.That(first.Count, Is.EqualTo(1));
            var mouse = first.Sources[0] as HarvestMousePetTriggerSource;
            Assert.That(mouse, Is.Not.Null);
            Assert.That(mouse.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(mouse.AttackGainResolver, Is.SameAs(attack));
            Assert.That(mouse.CardLookup, Is.SameAs(lookup));
            mouse.UsageCommitter.TryCommit(e.Mouse.InstanceId, () => { });
            var rebuilt = e.Runtime.BuildSourceRegistry(e.State, armor, attack, lookup);
            var rebuiltMouse = (HarvestMousePetTriggerSource)rebuilt.Sources[0];
            Assert.That(rebuiltMouse, Is.Not.SameAs(mouse));
            Assert.That(rebuiltMouse.UsageCommitter.HasTriggered(e.Mouse.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeRunner_NormalCombat_ResolvesHarvestMouseWithoutManualSourceRegistration(CombatSide side)
        {
            var e = new Scenario(side);
            var result = e.Start();
            Assert.That(result, Is.Not.Null);
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            Assert.That(e.Runner.UsesStagedNormalAttackByDefault, Is.True);
            AssertMouseAppliedExactlyOnce(e);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(new CombatCardLookup(e.Log).Get(e.State, e.Donor.InstanceId).IsRemoved, Is.True);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeRunner_OverflowDuringDeathThenResume_DoesNotRepeatDeathOrGain(CombatSide side)
        {
            var e = new Scenario(side);
            e.Target.ApplyAttackGain(int.MaxValue - e.Target.Attack);
            Assert.Throws<OverflowException>(() => e.Start());
            Assert.That(e.Runner.HasActiveCombat, Is.True);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.Mouse.InstanceId), Is.False);
            Assert.That(e.FindEvents<AttackGainCombatEvent>().Count, Is.Zero);
            Assert.That(e.Donor.CurrentHp, Is.LessThanOrEqualTo(0));
            Assert.That(e.Target.Attack, Is.EqualTo(int.MaxValue));
            var sourceDeaths = FindSourceDeaths(e);
            Assert.That(sourceDeaths.Count, Is.EqualTo(1));
            var originalDeath = sourceDeaths[0];

            e.Target.ReduceAttack(int.MaxValue - 2);
            var completed = e.Runner.ResumeActiveCombat(
                maximumExchangeCountPerColumn: 20, maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);

            Assert.That(completed, Is.Not.Null);
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            AssertMouseAppliedExactlyOnce(e);
            Assert.That(FindSourceDeaths(e).Count, Is.EqualTo(1));
            Assert.That(FindSourceDeaths(e)[0], Is.SameAs(originalDeath));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeRunner_AltarSupportsHarvestMouseAndRainSparrowTogether(CombatSide side)
        {
            var e = new Scenario(side, includeRainSparrow: true);
            Assert.That(e.Start(), Is.Not.Null);
            AssertMouseAppliedExactlyOnce(e);
            var armorGains = e.FindEvents<ArmorGainCombatEvent>();
            Assert.That(armorGains.Count, Is.EqualTo(1));
            Assert.That(armorGains[0].TargetInstanceId, Is.EqualTo(e.SpringRecipient.InstanceId));
            Assert.That(armorGains[0].ActualGainedAmount, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.Rain.InstanceId, e.SpringRecipient.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            var hpGain = e.Log.GetEvent(armorGains[0].Metadata.ParentEventId.Value) as HpGainCombatEvent;
            Assert.That(hpGain, Is.Not.Null);
            Assert.That(hpGain.TargetInstanceId, Is.EqualTo(e.SpringRecipient.InstanceId));
            Assert.That(armorGains[0].Metadata.TriggerRootId, Is.EqualTo(hpGain.Metadata.TriggerRootId));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void SeparateRuntimes_DoNotCarryHarvestMouseUsageIntoAnotherBattle()
        {
            var first = new Scenario();
            var second = new Scenario();
            Assert.That(first.Mouse.InstanceId, Is.EqualTo(second.Mouse.InstanceId));
            Assert.That(first.Runtime.PetUsageRegistry, Is.Not.SameAs(second.Runtime.PetUsageRegistry));
            Assert.That(first.Start(), Is.Not.Null);
            Assert.That(second.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(second.Start(), Is.Not.Null);
            AssertMouseAppliedExactlyOnce(first);
            AssertMouseAppliedExactlyOnce(second);
        }

        [Test]
        public void InjectedUsedPetRegistry_IsRespectedByRuntimeRunner()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            registry.TryRegister(new InstanceId(1001));
            var e = new Scenario(runtime: CreateRuntime(registry));
            Assert.That(e.Start(), Is.Not.Null);
            Assert.That(FindSourceDeaths(e).Count, Is.EqualTo(1));
            Assert.That(e.FindEvents<AttackGainCombatEvent>(), Is.Empty);
            Assert.That(e.Target.Attack, Is.EqualTo(2));
            Assert.That(e.Runtime.PetUsageRegistry, Is.SameAs(registry));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        private static CombatPetTriggerRuntime CreateRuntime(CombatPetTriggerUsageRegistry registry)
        {
            return new CombatPetTriggerRuntime(new CombatPetCardTriggerUsageRegistry(),
                new CombatNormalAttackSourceDamageModifierRegistry(),
                new CombatNormalAttackTargetDamageReductionRegistry(),
                new CombatFinalRankModifierRegistry(), registry);
        }

        private static List<DeathCombatEvent> FindSourceDeaths(Scenario e)
        {
            return e.FindEvents<DeathCombatEvent>().FindAll(death => death.InstanceId == e.Donor.InstanceId);
        }

        private static void AssertMouseAppliedExactlyOnce(Scenario e)
        {
            Assert.That(e.Target.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.Mouse.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            var gains = e.FindEvents<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(1));
            var gain = gains[0];
            Assert.That(gain.TargetInstanceId, Is.EqualTo(e.Target.InstanceId));
            Assert.That(gain.PreviousAttack, Is.EqualTo(2));
            Assert.That(gain.CurrentAttack, Is.EqualTo(3));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
            var death = e.Log.GetEvent(gain.Metadata.ParentEventId.Value) as DeathCombatEvent;
            Assert.That(death, Is.Not.Null);
            Assert.That(death.InstanceId, Is.EqualTo(e.Donor.InstanceId));
            Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(death.Metadata.TriggerRootId));
        }

        private sealed class Scenario
        {
            public readonly CombatPetTriggerRuntime Runtime;
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventQueue Queue;
            public readonly CombatState State;
            public readonly CombatPetState Mouse;
            public readonly CombatPetState Rain;
            public readonly CombatCardState Donor;
            public readonly CombatCardState Target;
            public readonly CombatCardState SpringRecipient;
            public readonly CombatResolutionRunner Runner;

            public Scenario(CombatSide side = CombatSide.Player, bool includeRainSparrow = false,
                CombatPetTriggerRuntime runtime = null)
            {
                Runtime = runtime ?? new CombatPetTriggerRuntime();
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Donor = Card(1, CombatCardSeason.Autumn, hp: 1, attack: 0);
                Target = Card(2, CombatCardSeason.Autumn, hp: 10, attack: 2);
                SpringRecipient = includeRainSparrow ? Card(3, CombatCardSeason.Spring, hp: 10, attack: 1) : null;
                var opponent = Card(101, CombatCardSeason.Winter, hp: 5, attack: 2);
                Mouse = new CombatPetState(CombatPetDefinitionIds.HarvestMouse, new InstanceId(1001));
                Rain = includeRainSparrow ? new CombatPetState(CombatPetDefinitionIds.RainSparrow, new InstanceId(1002)) : null;
                var owner = CreateSide(side, Donor, Target, SpringRecipient, includeRainSparrow);
                var other = CreateSide(otherSide, opponent, null, null, false);
                var ownerPets = new CombatSidePetState(side,
                    new CombatPetRegistry(includeRainSparrow ? new[] { Mouse, Rain } : new[] { Mouse }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? owner : other,
                    side == CombatSide.Enemy ? owner : other,
                    side == CombatSide.Player ? ownerPets : otherPets,
                    side == CombatSide.Enemy ? ownerPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Runner = Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);
            }

            public CombatCompletedCombatEvent Start()
            {
                return Runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 20,
                    maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }

            public List<T> FindEvents<T>() where T : CombatEvent
            {
                var result = new List<T>();
                foreach (var combatEvent in Log.Events)
                {
                    var typed = combatEvent as T;
                    if (typed != null) { result.Add(typed); }
                }
                return result;
            }

            private static CombatCardState Card(long id, CombatCardSeason season, int hp, int attack)
            {
                return new CombatCardState(new DefinitionId("test.runtime_mouse_card"), new InstanceId(id),
                    new CardRank(2), season, hp, hp, 0, attack);
            }

            private static CombatSideState CreateSide(CombatSide side, CombatCardState first,
                CombatCardState second, CombatCardState back, bool altar)
            {
                var cards = new List<CombatCardState> { first };
                if (second != null) { cards.Add(second); }
                if (back != null) { cards.Add(back); }
                var slots = new List<CombatSlotState>();
                for (var column = 1; column <= 5; column++)
                {
                    foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                    {
                        var occupant = row == BoardRow.Front ? (column == 1 ? first : column == 2 ? second : null)
                            : column == 1 ? back : null;
                        var position = new BoardPosition(side, row, new BoardColumn(column));
                        var slotValue = (side == CombatSide.Player ? 0 : 100) + column * 2 + (row == BoardRow.Front ? 0 : 1);
                        slots.Add(new CombatSlotState(new SlotId(slotValue), position,
                            occupant == null ? (InstanceId?)null : occupant.InstanceId,
                            altar && row == BoardRow.Front && column == 1
                                ? CombatSlotEnhanceKind.SacrificialAltar : CombatSlotEnhanceKind.None));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
