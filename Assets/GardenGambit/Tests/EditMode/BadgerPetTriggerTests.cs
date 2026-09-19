using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class BadgerPetTriggerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front, 7)]
        [TestCase(CombatSide.Player, BoardRow.Back, 8)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 9)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 10)]
        public void EligibleRank_FactorySourceReducesFirstNormalDamage(
            CombatSide side, BoardRow row, int rank)
        {
            var e = new Environment(side, row, rank);
            var source = e.Source(row);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pet(side, row).InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.TargetDamageReductionRegistry, Is.SameAs(e.Reductions));
            var attack = e.Attack(e.Position(row, 1));
            var candidates = Discover(source, e.State, attack);
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            candidates[0].Trigger.Resolve(e.State, attack);
            Assert.That(e.Used(side, row, e.Target), Is.False);
            Assert.That(e.Resolve(attack, 5), Is.EqualTo(4));
            Assert.That(e.Used(side, row, e.Target), Is.True);
            Assert.That(e.Reductions.Count, Is.Zero);
            var next = e.Attack(e.Position(row, 1));
            Assert.That(Discover(e.Source(row), e.State, next), Is.Empty);
            Assert.That(e.Resolve(next, 5), Is.EqualTo(5));
        }

        [TestCase(2)]
        [TestCase(6)]
        [TestCase(11)]
        [TestCase(14)]
        public void RankOutsideSevenThroughTen_DoesNotRegister(int rank)
        {
            var e = new Environment(rank: rank);
            var attack = e.Attack(e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, attack), Is.False);
            handler.Resolve(e.State, attack);
            Assert.That(e.Resolve(attack, 3), Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WrongRowOrSide_DoesNotProtectTarget(bool opposingSide)
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            BadgerPetNormalAttackTriggerHandler handler;
            if (opposingSide)
            {
                var pet = e.Pet(CombatSide.Enemy, BoardRow.Front);
                handler = new BadgerPetNormalAttackTriggerHandler(
                    CombatSide.Enemy, pet.InstanceId, e.Usage, e.Reductions);
            }
            else
            {
                handler = e.Source(BoardRow.Back).Handler;
            }
            Assert.That(handler.CanTrigger(e.State, attack), Is.False);
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [Test]
        public void ZeroDamage_DoesNotConsumeOpportunityAndLaterDamageDoes()
        {
            var e = new Environment();
            var first = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, first);
            Assert.That(e.Resolve(first, 0), Is.Zero);
            Assert.That(e.Used(e.Side, e.Row, e.Target), Is.False);
            var next = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, next);
            Assert.That(e.Resolve(next, 2), Is.EqualTo(1));
            Assert.That(e.Used(e.Side, e.Row, e.Target), Is.True);
        }

        [Test]
        public void SamePet_TracksDifferentCardsIndependently()
        {
            var e = new Environment();
            foreach (var column in new[] { 1, 2 })
            {
                var attack = e.Attack(e.Position(e.Row, column));
                e.Source(e.Row).Handler.Resolve(e.State, attack);
                Assert.That(e.Resolve(attack, 3), Is.EqualTo(2));
            }
            Assert.That(e.Used(e.Side, e.Row, e.Target), Is.True);
            Assert.That(e.Used(e.Side, e.Row, e.Second), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_RechecksCurrentRankAfterDiscovery()
        {
            var e = new Environment(rank: 7);
            var attack = e.Attack(e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, attack), Is.True);
            e.Target.SetRank(new CardRank(6));
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.Zero);
            Assert.That(e.Resolve(attack, 3), Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void CurrentEligibleRankCanTriggerEvenIfCardWasPreviouslyIneligible()
        {
            var e = new Environment(rank: 6);
            e.Target.SetRank(new CardRank(10));
            var attack = e.Attack(e.Position(e.Row, 1));
            e.Source(e.Row).Handler.Resolve(e.State, attack);
            Assert.That(e.Resolve(attack, 3), Is.EqualTo(2));
            Assert.That(e.Used(e.Side, e.Row, e.Target), Is.True);
        }

        [Test]
        public void RemovedTargetAndReplacement_DoNotInheritEligibility()
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            e.State.GetSide(e.Side).RemoveCardFromCombat(e.Position(e.Row, 1));
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Row, 2), e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, attack), Is.False);
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void RepeatedResolve_RegistersSingleRequest()
        {
            var e = new Environment();
            var attack = e.Attack(e.Position(e.Row, 1));
            var handler = e.Source(e.Row).Handler;
            handler.Resolve(e.State, attack);
            handler.Resolve(e.State, attack);
            Assert.That(e.Reductions.Count, Is.EqualTo(1));
            Assert.That(e.Resolve(attack, 5), Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void NonNormalEvent_DoesNotRegisterRequest()
        {
            var e = new Environment();
            Assert.That(e.Source(e.Row).DiscoverTriggers(e.State, e.Root), Is.Empty);
            Assert.That(e.Reductions.Count, Is.Zero);
        }

        [Test]
        public void ConstructorsAndFactory_ValidateDependenciesAndRegistration()
        {
            var e = new Environment();
            var pet = e.Pet(e.Side, e.Row);
            Assert.Throws<ArgumentNullException>(() =>
                new BadgerPetNormalAttackTriggerHandler(e.Side, pet.InstanceId, null, e.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new BadgerPetNormalAttackTriggerHandler(e.Side, pet.InstanceId, e.Usage, null));
            Assert.Throws<ArgumentException>(() =>
                new BadgerPetTriggerSourceFactory(default(DefinitionId), e.Usage, e.Reductions));
            var factory = e.Factory();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new List<ICombatTriggerSource>(factory.CreateSources((CombatSide)99, pet)));
            Assert.Throws<ArgumentNullException>(() =>
                new List<ICombatTriggerSource>(factory.CreateSources(e.Side, null)));
            var other = new CombatPetState(new DefinitionId("test.other"), pet.InstanceId);
            Assert.Throws<ArgumentException>(() =>
                new List<ICombatTriggerSource>(factory.CreateSources(e.Side, other)));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(factory.TargetDamageReductionRegistry, Is.SameAs(e.Reductions));
        }

        [Test]
        public void Reduction_IsAppliedBeforeArmorAndHpDamage()
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
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
        }

        private static List<CombatTriggerCandidate<ICombatTriggerHandler>> Discover(
            BadgerPetTriggerSource source, CombatState state, CombatEvent sourceEvent) =>
            new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                source.DiscoverTriggers(state, sourceEvent));

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatCardState Second;
            public readonly BoardPosition SourcePosition;
            public readonly CombatPetCardTriggerUsageCommitter Usage =
                new CombatPetCardTriggerUsageCommitter(new CombatPetCardTriggerUsageRegistry());
            public readonly CombatNormalAttackTargetDamageReductionRegistry Reductions =
                new CombatNormalAttackTargetDamageReductionRegistry();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            private readonly CombatPetState[] _playerPets;
            private readonly CombatPetState[] _enemyPets;

            public Environment(CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front, int rank = 7)
            {
                Side = side;
                Row = row;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                SourcePosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                Target = Card(1, rank);
                Second = Card(3, 10);
                var attacker = Card(2, 4);
                var otherRow = row == BoardRow.Front ? BoardRow.Back : BoardRow.Front;
                var own = new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(1), Position(row, 1), Target.InstanceId),
                    new CombatSlotState(new SlotId(3), Position(row, 2), Second.InstanceId),
                    new CombatSlotState(new SlotId(4), Position(otherRow, 1)) }),
                    new CombatCardRegistry(new[] { Target, Second }),
                    new BattleHealth(20), new AttackMultiplier(1));
                var opposing = new CombatSideState(new CombatBoardState(other, new[] {
                    new CombatSlotState(new SlotId(2), SourcePosition, attacker.InstanceId) }),
                    new CombatCardRegistry(new[] { attacker }),
                    new BattleHealth(20), new AttackMultiplier(1));
                _playerPets = Pets(1000);
                _enemyPets = Pets(2000);
                State = new CombatState(side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Player ? opposing : own,
                    new CombatSidePetState(CombatSide.Player, new CombatPetRegistry(_playerPets)),
                    new CombatSidePetState(CombatSide.Enemy, new CombatPetRegistry(_enemyPets)));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
            }

            public BoardPosition Position(BoardRow row, int column) =>
                new BoardPosition(Side, row, new BoardColumn(column));

            public CombatPetState Pet(CombatSide side, BoardRow row) =>
                (side == CombatSide.Player ? _playerPets : _enemyPets)
                    [row == BoardRow.Front ? 0 : 1];

            public bool Used(CombatSide side, BoardRow row, CombatCardState card) =>
                Usage.HasTriggered(Pet(side, row).InstanceId, card.InstanceId);

            public BadgerPetTriggerSourceFactory Factory() =>
                new BadgerPetTriggerSourceFactory(
                    Pet(Side, Row).DefinitionId, Usage, Reductions);

            public BadgerPetTriggerSource Source(BoardRow row)
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory().CreateSources(Side, Pet(Side, row)));
                Assert.That(sources.Count, Is.EqualTo(1));
                return (BadgerPetTriggerSource)sources[0];
            }

            public NormalAttackCombatEvent Attack(BoardPosition position)
            {
                var card = State.GetSide(Side).GetCardAt(position);
                var attack = new NormalAttackCombatEvent(
                    Metadata.CreateChild(Root.Metadata), new InstanceId(2),
                    SourcePosition, card.InstanceId, position, 5);
                Log.Append(attack);
                return attack;
            }

            public int Resolve(NormalAttackCombatEvent attack, int amount) =>
                new CombatNormalAttackTargetDamageReductionResolver(
                    Reductions, Usage).ResolveDamage(attack, amount);

            private static CombatPetState[] Pets(long prefix) => new[] {
                new CombatPetState(new DefinitionId("test.badger"), new InstanceId(prefix + 2)),
                new CombatPetState(new DefinitionId("test.badger"), new InstanceId(prefix + 1)) };

            private static CombatCardState Card(long id, int rank) =>
                new CombatCardState(new DefinitionId("test.badger_card"),
                    new InstanceId(id), new CardRank(rank), CombatCardSeason.Spring,
                    10, 5, 3, 2);
        }
    }
}
