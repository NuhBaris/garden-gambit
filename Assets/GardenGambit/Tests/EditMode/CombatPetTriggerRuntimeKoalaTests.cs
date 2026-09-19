using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeKoalaTests
    {
        [Test]
        public void Catalogue_RegistersStableKoalaIdentityAndSharedDependencies()
        {
            var e = new Environment();
            Assert.That(CombatPetDefinitionIds.KoalaValue, Is.EqualTo("pet.koala"));
            Assert.That(CombatPetDefinitionIds.Koala, Is.EqualTo(new DefinitionId("pet.koala")));
            var registry = e.FullFactoryRegistry();
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            var factory = registry.GetFactory(CombatPetDefinitionIds.Koala) as KoalaPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(factory.TargetDamageReductionRegistry, Is.SameAs(e.Runtime.TargetDamageReductionRegistry));
            Assert.That(registry.Contains(CombatPetDefinitionIds.Snail), Is.True);
            Assert.That(registry.Contains(CombatPetDefinitionIds.Alpaca), Is.True);
            Assert.That(e.Runtime.FactoryRegistry.Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Koala), Is.False);
            var armorOnly = e.Runtime.FactoryCatalog.CreateRegistry(e.Armor);
            Assert.That(armorOnly.Count, Is.EqualTo(4));
            Assert.That(armorOnly.Contains(CombatPetDefinitionIds.Koala), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsBothKoalaSourcesWithSharedRegistries(CombatSide side)
        {
            var e = new Environment(side);
            var sources = e.Sources();
            Assert.That(sources.Count, Is.EqualTo(2));
            var owners = new HashSet<InstanceId>();
            foreach (var item in sources.Sources)
            {
                var source = item as KoalaPetTriggerSource;
                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
                Assert.That(source.TargetDamageReductionRegistry,
                    Is.SameAs(e.Runtime.TargetDamageReductionRegistry));
                Assert.That(owners.Add(source.PetInstanceId), Is.True);
            }
            Assert.That(owners.Contains(e.UpperPet.InstanceId), Is.True);
            Assert.That(owners.Contains(e.LowerPet.InstanceId), Is.True);
        }

        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Enemy, true)]
        [TestCase(CombatSide.Enemy, false)]
        public void FullBattle_ReducesFirstNormalDamageOnlyForEnhancedSlot(
            CombatSide side, bool enhanced)
        {
            var e = new Environment(side, enhanced: enhanced, opponentAttack: 3);
            var runner = e.CreateRunner();
            var completed = Start(runner);
            Assert.That(completed, Is.Not.Null);
            Assert.That(completed.Outcome, Is.EqualTo(side == CombatSide.Player
                ? CombatOutcome.PlayerVictory : CombatOutcome.EnemyVictory));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(enhanced ? 8 : 7));
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.State.GetOpposingSide(side).Cards.Count, Is.Zero);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(
                e.UpperPet.InstanceId, e.Target.InstanceId), Is.EqualTo(enhanced));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(
                e.LowerPet.InstanceId, e.Target.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(enhanced ? 1 : 0));
            Assert.That(e.Runtime.TargetDamageReductionRegistry.Count, Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            var hit = e.OpponentHit();
            Assert.That(hit.Result.IncomingDamage, Is.EqualTo(enhanced ? 2 : 3));
            Assert.That(hit.Result.HpDamage, Is.EqualTo(enhanced ? 2 : 3));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FullBattle_ZeroDamageLeavesKoalaCardUsageAvailable(CombatSide side)
        {
            var e = new Environment(side, enhanced: true, opponentAttack: 0);
            var completed = Start(e.CreateRunner());
            Assert.That(completed, Is.Not.Null);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(10));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(
                e.UpperPet.InstanceId, e.Target.InstanceId), Is.False);
            Assert.That(e.Runtime.TargetDamageReductionRegistry.Count, Is.Zero);
            Assert.That(e.OpponentHit().Result.IncomingDamage, Is.Zero);
        }

        [Test]
        public void RebuiltRuntimeSources_PreserveConsumedCardUsage()
        {
            var e = new Environment();
            Start(e.CreateRunner());
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(
                e.UpperPet.InstanceId, e.Target.InstanceId), Is.True);
            var attack = e.CreateRecordedOpponentAttack(baseDamage: 3);
            Assert.That(e.Sources().DiscoverTriggers(e.State, attack), Is.Empty);
            Assert.That(e.Runtime.TargetDamageReductionRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void FreshRuntime_AllowsSamePetAndCardIdsInNewBattle()
        {
            var first = new Environment();
            Start(first.CreateRunner());
            var next = new Environment();
            Assert.That(next.UpperPet.InstanceId, Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(next.Target.InstanceId, Is.EqualTo(first.Target.InstanceId));
            Assert.That(next.Runtime.UsageRegistry.Count, Is.Zero);
            Start(next.CreateRunner());
            Assert.That(first.Target.CurrentHp, Is.EqualTo(8));
            Assert.That(next.Target.CurrentHp, Is.EqualTo(8));
            Assert.That(next.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        private static CombatCompletedCombatEvent Start(CombatResolutionRunner runner) =>
            runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventQueue Queue;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            private readonly BoardPosition _targetPosition;
            private readonly BoardPosition _opponentPosition;

            public Environment(CombatSide side = CombatSide.Player,
                bool enhanced = true, int opponentAttack = 3)
            {
                Side = side;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                _targetPosition = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
                _opponentPosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                Target = Card("test.koala_target", 1, hp: 10, attack: 1);
                var opponent = Card("test.koala_opponent", 101, hp: 1, attack: opponentAttack);
                UpperPet = new CombatPetState(CombatPetDefinitionIds.Koala, new InstanceId(1002));
                LowerPet = new CombatPetState(CombatPetDefinitionIds.Koala, new InstanceId(1001));
                var own = MakeSide(side, Target, _targetPosition, 1,
                    enhanced ? CombatSlotEnhanceKind.ProtectiveSeal : CombatSlotEnhanceKind.None);
                var opposing = MakeSide(other, opponent, _opponentPosition, 101, CombatSlotEnhanceKind.None);
                var ownPets = new CombatSidePetState(side,
                    new CombatPetRegistry(new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(other,
                    new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Enemy ? own : opposing,
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
            }

            public CombatPetTriggerSourceFactoryRegistry FullFactoryRegistry() =>
                Runtime.FactoryCatalog.CreateRegistry(Armor, Attack,
                    new CombatCardLookup(Log), Runtime.PetUsageCommitter);

            public CombatTriggerSourceRegistry Sources() =>
                Runtime.BuildSourceRegistry(State, Armor, Attack, new CombatCardLookup(Log));

            public CombatResolutionRunner CreateRunner() =>
                Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);

            public DamageAppliedCombatEvent OpponentHit()
            {
                foreach (var item in Log.Events)
                {
                    var damage = item as DamageAppliedCombatEvent;
                    if (damage != null && damage.TargetInstanceId == Target.InstanceId)
                    {
                        return damage;
                    }
                }
                throw new InvalidOperationException("Opponent damage event was not found.");
            }

            public NormalAttackCombatEvent CreateRecordedOpponentAttack(int baseDamage)
            {
                var root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(root);
                var attack = new NormalAttackCombatEvent(Metadata.CreateChild(root.Metadata),
                    new InstanceId(101), _opponentPosition, Target.InstanceId,
                    _targetPosition, baseDamage);
                Log.Append(attack);
                return attack;
            }

            private static CombatSideState MakeSide(CombatSide side, CombatCardState card,
                BoardPosition front, long slotPrefix, CombatSlotEnhanceKind enhance)
            {
                var back = new BoardPosition(side, BoardRow.Back, front.Column);
                return new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(slotPrefix), front, card.InstanceId, enhance),
                    new CombatSlotState(new SlotId(slotPrefix + 1), back) }),
                    new CombatCardRegistry(new[] { card }), new BattleHealth(20), new AttackMultiplier(1));
            }

            private static CombatCardState Card(string definition, long id, int hp, int attack) =>
                new CombatCardState(new DefinitionId(definition), new InstanceId(id),
                    new CardRank(2), CombatCardSeason.Spring, hp, hp, 0, attack);
        }
    }
}
