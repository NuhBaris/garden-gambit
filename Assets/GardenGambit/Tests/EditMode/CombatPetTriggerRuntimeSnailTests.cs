using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeSnailTests
    {
        [Test]
        public void Catalogue_RegistersSnailWithSharedDependenciesAndPreservesLegacyOverloads()
        {
            var e = new Environment();
            Assert.That(CombatPetDefinitionIds.SnailValue, Is.EqualTo("pet.snail"));
            Assert.That(CombatPetDefinitionIds.Snail, Is.EqualTo(new DefinitionId("pet.snail")));
            var registry = e.Runtime.FactoryCatalog.CreateRegistry(
                e.Armor, e.Attack, new CombatCardLookup(e.Log), e.Runtime.PetUsageCommitter);
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            var factory = registry.GetFactory(CombatPetDefinitionIds.Snail) as SnailPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(factory.ArmorGainResolver, Is.SameAs(e.Armor));
            Assert.That(registry.Contains(CombatPetDefinitionIds.Alpaca), Is.True);
            Assert.That(registry.Contains(CombatPetDefinitionIds.Macaque), Is.True);
            Assert.That(e.Runtime.FactoryRegistry.Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Snail), Is.False);
            var armorOnly = e.Runtime.FactoryCatalog.CreateRegistry(e.Armor);
            Assert.That(armorOnly.Count, Is.EqualTo(4));
            Assert.That(armorOnly.Contains(CombatPetDefinitionIds.Snail), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsBothPetSourcesWithSharedUsage(CombatSide side)
        {
            var e = new Environment(side);
            var sources = e.Sources();
            Assert.That(sources.Count, Is.EqualTo(2));
            var owners = new HashSet<InstanceId>();
            foreach (var item in sources.Sources)
            {
                var source = item as SnailPetTriggerSource;
                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
                Assert.That(source.ArmorGainResolver, Is.SameAs(e.Armor));
                Assert.That(owners.Add(source.PetInstanceId), Is.True);
            }
            Assert.That(owners.Contains(e.UpperPet.InstanceId), Is.True);
            Assert.That(owners.Contains(e.LowerPet.InstanceId), Is.True);
        }

        [TestCase(CombatSide.Player, 3, 4)]
        [TestCase(CombatSide.Enemy, 3, 4)]
        [TestCase(CombatSide.Player, 8, 1)]
        [TestCase(CombatSide.Enemy, 8, 1)]
        public void FullBattle_ResolvesArmorBreakOnceAndStillRemovesThresholdCard(
            CombatSide side, int opponentAttack, int expectedHits)
        {
            var e = new Environment(side, opponentAttack);
            var runner = e.Runtime.CreateResolutionRunner(e.State, e.Metadata, e.Log, e.Queue);
            Assert.That(runner.UsesStagedNormalAttackByDefault, Is.True);
            var completed = runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);
            Assert.That(completed.Outcome, Is.EqualTo(side == CombatSide.Player
                ? CombatOutcome.EnemyVictory : CombatOutcome.PlayerVictory));
            Assert.That(e.State.GetSide(side).Cards.Count, Is.Zero);
            Assert.That(e.State.GetOpposingSide(side).Cards.Count, Is.EqualTo(1));
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            var gains = e.Events<ArmorGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(gains[0].TargetInstanceId, Is.EqualTo(e.Target.InstanceId));
            Assert.That(gains[0].ActualGainedAmount, Is.EqualTo(2));
            var hits = e.TargetHits();
            Assert.That(hits.Count, Is.EqualTo(expectedHits));
            Assert.That(hits[0].Result.PreviousArmor, Is.EqualTo(3));
            Assert.That(hits[0].Result.CurrentArmor, Is.Zero);
            Assert.That(gains[0].Metadata.ParentEventId, Is.EqualTo(hits[0].Metadata.EventId));
            Assert.That(gains[0].Metadata.TriggerRootId, Is.EqualTo(hits[0].Metadata.TriggerRootId));
            if (expectedHits > 1)
            {
                Assert.That(hits[1].Result.PreviousArmor, Is.EqualTo(2));
                Assert.That(hits[1].Result.ArmorAbsorbed, Is.EqualTo(2));
                Assert.That(hits[1].Result.HpDamage, Is.EqualTo(1));
            }
            else
            {
                Assert.That(hits[0].Result.CurrentHp, Is.Zero);
                Assert.That(e.Target.CurrentHp, Is.Zero);
                Assert.That(e.Target.Armor, Is.EqualTo(2));
            }
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.UpperPet.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.LowerPet.InstanceId), Is.False);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Events<CombatCompletedCombatEvent>().Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeBuiltSources_ResumeFailedGainWithoutRepeatingDamage(CombatSide side)
        {
            var e = new Environment(side);
            var damage = e.AppendDamage(4);
            e.Target.ApplyArmorGain(int.MaxValue);
            var engine = new CombatTriggerEngine(e.State, e.Queue, e.Sources());
            Assert.Throws<OverflowException>(() => engine.Drain(20, 20));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            e.Target.RemoveArmor(int.MaxValue);
            engine.Drain(20, 20);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(e.TargetHits()[0], Is.SameAs(damage));
            Assert.That(engine.HasActiveBatch, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [Test]
        public void RebuiltRuntimeSources_PreserveConsumedUsage()
        {
            var e = new Environment();
            var damage = e.AppendDamage(3);
            new CombatTriggerEngine(e.State, e.Queue, e.Sources()).Drain(20, 20);
            Assert.That(e.Sources().DiscoverTriggers(e.State, damage), Is.Empty);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Events<ArmorGainCombatEvent>().Count, Is.EqualTo(1));
        }

        [Test]
        public void FreshRuntime_AllowsSamePetIdsInNewBattle()
        {
            var first = new Environment();
            first.AppendDamage(3);
            new CombatTriggerEngine(first.State, first.Queue, first.Sources()).Drain(20, 20);
            var next = new Environment();
            Assert.That(next.UpperPet.InstanceId, Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(next.Runtime.PetUsageRegistry.Count, Is.Zero);
            next.AppendDamage(3);
            new CombatTriggerEngine(next.State, next.Queue, next.Sources()).Drain(20, 20);
            Assert.That(next.Target.Armor, Is.EqualTo(2));
            Assert.That(next.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(next.Events<ArmorGainCombatEvent>().Count, Is.EqualTo(1));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatPetState UpperPet = new CombatPetState(CombatPetDefinitionIds.Snail, new InstanceId(1002));
            public readonly CombatPetState LowerPet = new CombatPetState(CombatPetDefinitionIds.Snail, new InstanceId(1001));
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventQueue Queue;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatArmorGainResolver Armor;
            private readonly BoardPosition _targetPosition;
            private readonly BoardPosition _sourcePosition;

            public Environment(CombatSide side = CombatSide.Player, int opponentAttack = 3)
            {
                Side = side;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                _targetPosition = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
                _sourcePosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                Target = new CombatCardState(new DefinitionId("test.snail_runtime_target"), new InstanceId(1),
                    new CardRank(4), CombatCardSeason.Spring, 5, 5, 3, 0);
                var opponent = new CombatCardState(new DefinitionId("test.snail_runtime_opponent"), new InstanceId(2),
                    new CardRank(4), CombatCardSeason.Spring, 5, 5, 0, opponentAttack);
                var own = MakeSide(side, Target);
                var opposing = MakeSide(other, opponent);
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(other, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Enemy ? own : opposing,
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
            }

            public CombatTriggerSourceRegistry Sources() =>
                Runtime.BuildSourceRegistry(State, Armor, Attack, new CombatCardLookup(Log));

            public DamageAppliedCombatEvent AppendDamage(int amount)
            {
                var root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(root);
                return new CombatDamageResolver(Metadata, Log).ApplyResolvedCardDamage(
                    State, root, _sourcePosition, _targetPosition, amount);
            }

            public List<T> Events<T>() where T : CombatEvent
            {
                var result = new List<T>();
                foreach (var item in Log.Events) { if (item is T typed) { result.Add(typed); } }
                return result;
            }

            public List<DamageAppliedCombatEvent> TargetHits()
            {
                var result = new List<DamageAppliedCombatEvent>();
                foreach (var item in Events<DamageAppliedCombatEvent>())
                { if (item.TargetInstanceId == Target.InstanceId) { result.Add(item); } }
                return result;
            }

            private static CombatSideState MakeSide(CombatSide side, CombatCardState card)
            {
                var slots = new List<CombatSlotState>();
                foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                {
                    for (var column = 1; column <= 5; column++)
                    {
                        var slotId = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        slots.Add(new CombatSlotState(new SlotId(slotId),
                            new BoardPosition(side, row, new BoardColumn(column)),
                            row == BoardRow.Front && column == 1 ? card.InstanceId : (InstanceId?)null));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots),
                    new CombatCardRegistry(new[] { card }), new BattleHealth(100), new AttackMultiplier(1));
            }
        }
    }
}
