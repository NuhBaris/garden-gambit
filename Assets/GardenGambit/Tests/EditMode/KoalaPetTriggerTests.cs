using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class KoalaPetTriggerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FactorySource_RegistersReductionForCorrectOwnerWithoutEarlyUsage(CombatSide side, BoardRow row)
        {
            var e = new Environment(side, row);
            var source = e.Source(row);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pet(row).InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.TargetDamageReductionRegistry, Is.SameAs(e.Reductions));
            var attack = e.Attack(e.Position(row, 1));
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(source.DiscoverTriggers(e.State, attack));
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            candidates[0].Trigger.Resolve(e.State, attack);
            Assert.That(e.Reductions.GetRequests(attack.Metadata.EventId).Count, Is.EqualTo(1));
            Assert.That(e.Used(row, e.Target), Is.False);
            Assert.That(e.Resolve(attack, 5), Is.EqualTo(4));
            Assert.That(e.Used(row, e.Target), Is.True);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [TestCase(CombatSlotEnhanceKind.WarBanner)]
        [TestCase(CombatSlotEnhanceKind.SacrificialAltar)]
        [TestCase(CombatSlotEnhanceKind.WarAltar)]
        public void OtherSupportedEnhances_AlsoQualify(CombatSlotEnhanceKind kind)
        {
            var e = new Environment(enhance: kind);
            var attack = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, attack);
            Assert.That(e.Resolve(attack, 3), Is.EqualTo(2));
        }

        [Test]
        public void UnenhancedSlot_DoesNotRegisterOrConsumeUsage()
        {
            var e = new Environment(enhance: CombatSlotEnhanceKind.None);
            var attack = e.Attack(e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, attack), Is.False);
            handler.Resolve(e.State, attack);
            Assert.That(e.Resolve(attack, 3), Is.EqualTo(3));
            Assert.That(e.Used(e.Row, e.Target), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WrongRowOrSide_DoesNotProtectTarget(bool oppositeSide)
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            KoalaPetNormalAttackTriggerHandler handler;
            if (oppositeSide)
            {
                handler = new KoalaPetNormalAttackTriggerHandler(CombatSide.Enemy,
                    new InstanceId(2002), e.Usage, e.Reductions);
            }
            else { handler = e.Source(BoardRow.Back).Handler; }
            Assert.That(handler.CanTrigger(e.State, attack), Is.False);
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [TestCase(0, 0, false)]
        [TestCase(1, 0, true)]
        [TestCase(5, 4, true)]
        public void Usage_IsCommittedOnlyForActualReduction(int damage, int expected, bool used)
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, attack);
            Assert.That(e.Resolve(attack, damage), Is.EqualTo(expected));
            Assert.That(e.Used(e.Row, e.Target), Is.EqualTo(used));
            Assert.That(e.Reductions.Count, Is.Zero);
            if (!used)
            {
                var next = e.Attack(e.Position(e.Row, 1));
                e.Source(e.Row).Handler.Resolve(e.State, next);
                Assert.That(e.Resolve(next, 2), Is.EqualTo(1));
                Assert.That(e.Used(e.Row, e.Target), Is.True);
            }
        }

        [Test]
        public void RebuiltSource_DoesNotRestoreUsedCardOpportunity()
        {
            var e = new Environment();
            var first = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, first);
            Assert.That(e.Resolve(first, 3), Is.EqualTo(2));
            var next = e.Attack(e.Position(e.Row, 1));
            var rebuilt = e.Source(e.Row);
            Assert.That(rebuilt.DiscoverTriggers(e.State, next), Is.Empty);
            rebuilt.Handler.Resolve(e.State, next);
            Assert.That(e.Resolve(next, 3), Is.EqualTo(3));
        }

        [Test]
        public void SamePet_HasSeparateOpportunityForEachCard()
        {
            var e = new Environment();
            foreach (var column in new[] { 1, 2 })
            {
                var attack = e.Attack(e.Position(e.Row, column));
                e.Source(e.Row).Handler.Resolve(e.State, attack);
                Assert.That(e.Resolve(attack, 3), Is.EqualTo(2));
            }
            Assert.That(e.Used(e.Row, e.Target), Is.True);
            Assert.That(e.Used(e.Row, e.Second), Is.True);
        }

        [Test]
        public void DifferentPet_CanProtectSameCardAfterRowMove()
        {
            var e = new Environment();
            var first = e.Attack(e.Position(BoardRow.Front, 1));
            e.Source(BoardRow.Front).Handler.Resolve(e.State, first);
            e.Resolve(first, 3);
            e.State.GetSide(e.Side).MoveCard(e.Position(BoardRow.Front, 1), e.Position(BoardRow.Back, 1));
            var next = e.Attack(e.Position(BoardRow.Back, 1));
            e.Source(BoardRow.Back).Handler.Resolve(e.State, next);
            Assert.That(e.Resolve(next, 3), Is.EqualTo(2));
            Assert.That(e.Used(BoardRow.Front, e.Target), Is.True);
            Assert.That(e.Used(BoardRow.Back, e.Target), Is.True);
        }

        [Test]
        public void RemovedTargetAndReplacement_DoNotInheritPreparedAttackEligibility()
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            e.State.GetSide(e.Side).RemoveCardFromCombat(e.Position(e.Row, 1));
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Row, 2), e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, attack), Is.False);
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [Test]
        public void Resolve_RechecksTargetAfterDiscovery()
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, attack), Is.True);
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Row, 1), e.Position(BoardRow.Back, 1));
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [Test]
        public void RepeatedResolve_DoesNotDuplicateRequest()
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            handler.Resolve(e.State, attack); handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.EqualTo(1));
            Assert.That(e.Resolve(attack, 5), Is.EqualTo(4));
        }

        [Test]
        public void NonNormalEvent_DoesNotRegisterReduction()
        {
            var e = new Environment();
            Assert.That(e.Source(e.Row).DiscoverTriggers(e.State, e.Root), Is.Empty);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [Test]
        public void ConstructorsAndFactory_RejectInvalidDependenciesAndRegistration()
        {
            var e = new Environment();
            var pet = e.Pet(e.Row);
            Assert.Throws<ArgumentNullException>(() => new KoalaPetNormalAttackTriggerHandler(e.Side, pet.InstanceId, null, e.Reductions));
            Assert.Throws<ArgumentNullException>(() => new KoalaPetNormalAttackTriggerHandler(e.Side, pet.InstanceId, e.Usage, null));
            Assert.Throws<ArgumentException>(() => new KoalaPetTriggerSourceFactory(default(DefinitionId), e.Usage, e.Reductions));
            var factory = e.Factory();
            Assert.Throws<ArgumentOutOfRangeException>(() => new List<ICombatTriggerSource>(factory.CreateSources((CombatSide)99, pet)));
            Assert.Throws<ArgumentNullException>(() => new List<ICombatTriggerSource>(factory.CreateSources(e.Side, null)));
            var other = new CombatPetState(new DefinitionId("test.other"), pet.InstanceId);
            Assert.Throws<ArgumentException>(() => new List<ICombatTriggerSource>(factory.CreateSources(e.Side, other)));
        }

        [Test]
        public void Reduction_PrecedesArmorAndHpApplication()
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, attack);
            var reduced = e.Resolve(attack, 5);
            var damage = new CombatDamageResolver(e.Metadata, e.Log).ApplyResolvedCardDamage(
                e.State, attack, e.SourcePosition, attack.TargetPosition, reduced);
            Assert.That(damage.Result.IncomingDamage, Is.EqualTo(4));
            Assert.That(damage.Result.ArmorAbsorbed, Is.EqualTo(3));
            Assert.That(damage.Result.HpDamage, Is.EqualTo(1));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Target.Armor, Is.Zero);
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatCardState Second;
            public readonly BoardPosition SourcePosition;
            public readonly CombatPetCardTriggerUsageCommitter Usage = new CombatPetCardTriggerUsageCommitter(new CombatPetCardTriggerUsageRegistry());
            public readonly CombatNormalAttackTargetDamageReductionRegistry Reductions = new CombatNormalAttackTargetDamageReductionRegistry();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            private readonly CombatPetState[] _pets;
            public Environment(CombatSide side = CombatSide.Player, BoardRow row = BoardRow.Front,
                CombatSlotEnhanceKind enhance = CombatSlotEnhanceKind.ProtectiveSeal)
            {
                Side = side; Row = row;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                SourcePosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                Target = Card(1); Second = Card(3);
                var attacker = Card(2);
                var own = new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(1), Position(row, 1), Target.InstanceId, enhance),
                    new CombatSlotState(new SlotId(3), Position(row, 2), Second.InstanceId, enhance),
                    new CombatSlotState(new SlotId(4), Position(row == BoardRow.Front ? BoardRow.Back : BoardRow.Front, 1), null, enhance) }),
                    new CombatCardRegistry(new[] { Target, Second }), new BattleHealth(20), new AttackMultiplier(1));
                var opposing = new CombatSideState(new CombatBoardState(other, new[] {
                    new CombatSlotState(new SlotId(2), SourcePosition, attacker.InstanceId) }),
                    new CombatCardRegistry(new[] { attacker }), new BattleHealth(20), new AttackMultiplier(1));
                var playerPets = Pets(1000); var enemyPets = Pets(2000);
                _pets = side == CombatSide.Player ? playerPets : enemyPets;
                State = new CombatState(side == CombatSide.Player ? own : opposing, side == CombatSide.Player ? opposing : own,
                    new CombatSidePetState(CombatSide.Player, new CombatPetRegistry(playerPets)),
                    new CombatSidePetState(CombatSide.Enemy, new CombatPetRegistry(enemyPets)));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot()); Log.Append(Root);
            }
            public BoardPosition Position(BoardRow row, int column) => new BoardPosition(Side, row, new BoardColumn(column));
            public CombatPetState Pet(BoardRow row) => _pets[row == BoardRow.Front ? 0 : 1];
            public bool Used(BoardRow row, CombatCardState card) => Usage.HasTriggered(Pet(row).InstanceId, card.InstanceId);
            public KoalaPetTriggerSourceFactory Factory() => new KoalaPetTriggerSourceFactory(Pet(Row).DefinitionId, Usage, Reductions);
            public KoalaPetTriggerSource Source(BoardRow row)
            {
                var sources = new List<ICombatTriggerSource>(Factory().CreateSources(Side, Pet(row)));
                Assert.That(sources.Count, Is.EqualTo(1));
                return (KoalaPetTriggerSource)sources[0];
            }
            public NormalAttackCombatEvent Attack(BoardPosition position)
            {
                var card = State.GetSide(Side).GetCardAt(position);
                var attack = new NormalAttackCombatEvent(Metadata.CreateChild(Root.Metadata), new InstanceId(2), SourcePosition,
                    card.InstanceId, position, 5);
                Log.Append(attack); return attack;
            }
            public int Resolve(NormalAttackCombatEvent attack, int amount) =>
                new CombatNormalAttackTargetDamageReductionResolver(Reductions, Usage).ResolveDamage(attack, amount);
            private static CombatPetState[] Pets(long prefix) => new[] {
                new CombatPetState(new DefinitionId("test.koala"), new InstanceId(prefix + 2)),
                new CombatPetState(new DefinitionId("test.koala"), new InstanceId(prefix + 1)) };
            private static CombatCardState Card(long id) => new CombatCardState(new DefinitionId("test.koala_card"),
                new InstanceId(id), new CardRank(4), CombatCardSeason.Spring, 10, 5, 3, 2);
        }
    }
}
