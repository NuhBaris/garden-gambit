using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class SnailPetTriggerSourceFactoryTests
    {
        [Test]
        public void Factory_PreservesRegistrationAndDependencies()
        {
            var e = new Environment();
            var factory = Factory(e);
            Assert.That(factory.PetDefinitionId, Is.EqualTo(e.Pet.DefinitionId));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(factory.ArmorGainResolver, Is.SameAs(e.Armor));
            var registry = new CombatPetTriggerSourceFactoryRegistry(new[] { factory });
            Assert.That(registry.GetFactory(e.Pet.DefinitionId), Is.SameAs(factory));
        }

        [Test]
        public void FactoryConstructor_RejectsInvalidRegistrationAndNullDependencies()
        {
            var e = new Environment();
            Assert.Throws<ArgumentException>(() => new SnailPetTriggerSourceFactory(default(DefinitionId), e.Usage, e.Armor));
            Assert.Throws<ArgumentNullException>(() => new SnailPetTriggerSourceFactory(e.Pet.DefinitionId, null, e.Armor));
            Assert.Throws<ArgumentNullException>(() => new SnailPetTriggerSourceFactory(e.Pet.DefinitionId, e.Usage, null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Factory_CreatesOneSourceForCorrectOwnerAndResolvesDamage(CombatSide side, BoardRow row)
        {
            var e = new Environment(side, row);
            var source = Create(Factory(e), side, e.Pet);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            Assert.That(source.Handler.Side, Is.EqualTo(side));
            Assert.That(source.Handler.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.ArmorGainResolver, Is.SameAs(e.Armor));
            Assert.That(source.OrderKeyProvider.Side, Is.EqualTo(side));
            Assert.That(source.OrderKeyProvider.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            var damage = e.Damage(3);
            var candidates = Discover(source, e.State, damage);
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            candidates[0].Trigger.Resolve(e.State, damage);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(Discover(source, e.State, damage), Is.Empty);
        }

        [Test]
        public void CreateSources_RejectsInvalidSideNullPetAndWrongDefinition()
        {
            var e = new Environment();
            var factory = Factory(e);
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(factory, (CombatSide)99, e.Pet));
            Assert.Throws<ArgumentNullException>(() => Create(factory, e.Side, null));
            var other = new CombatPetState(new DefinitionId("test.other_pet"), e.Pet.InstanceId);
            Assert.Throws<ArgumentException>(() => Create(factory, e.Side, other));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void SourceConstructor_RejectsInvalidOwnerAndNullDependencies()
        {
            var e = new Environment();
            Assert.Throws<ArgumentOutOfRangeException>(() => new SnailPetTriggerSource((CombatSide)99, e.Pet.InstanceId, e.Usage, e.Armor));
            Assert.Throws<ArgumentException>(() => new SnailPetTriggerSource(e.Side, default(InstanceId), e.Usage, e.Armor));
            Assert.Throws<ArgumentNullException>(() => new SnailPetTriggerSource(e.Side, e.Pet.InstanceId, null, e.Armor));
            Assert.Throws<ArgumentNullException>(() => new SnailPetTriggerSource(e.Side, e.Pet.InstanceId, e.Usage, null));
        }

        [Test]
        public void Discovery_IgnoresWrongEventAndUnbrokenArmorWithoutConsumingUsage()
        {
            var e = new Environment();
            var source = Create(Factory(e), e.Side, e.Pet);
            Assert.That(Discover(source, e.State, e.Root), Is.Empty);
            Assert.That(Discover(source, e.State, e.Damage(1)), Is.Empty);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(Discover(source, e.State, e.Damage(2)).Count, Is.EqualTo(1));
        }

        [Test]
        public void RebuiltSourceAndFactory_PreserveSharedOncePerBattleUsage()
        {
            var e = new Environment();
            var first = Create(Factory(e), e.Side, e.Pet);
            var damage = e.Damage(3);
            Discover(first, e.State, damage)[0].Trigger.Resolve(e.State, damage);
            var rebuilt = Create(Factory(e), e.Side, e.Pet);
            Assert.That(rebuilt, Is.Not.SameAs(first));
            Assert.That(rebuilt.Handler, Is.Not.SameAs(first.Handler));
            Assert.That(rebuilt.UsageCommitter, Is.SameAs(first.UsageCommitter));
            var secondDamage = e.Damage(2);
            Assert.That(Discover(rebuilt, e.State, secondDamage), Is.Empty);
            rebuilt.Handler.Resolve(e.State, secondDamage);
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [Test]
        public void DuplicateDefinitionPets_HaveIndependentUsageAndOwnRows()
        {
            var e = new Environment();
            var factory = Factory(e);
            var upper = Create(factory, e.Side, e.Pet);
            var lowerPet = e.State.GetPets(e.Side).Pets.GetPet(e.PetId(e.Side, BoardRow.Back));
            var lower = Create(factory, e.Side, lowerPet);
            var lowerPosition = e.Position(BoardRow.Back, 1);
            e.State.GetSide(e.Side).MoveCard(e.SecondPosition, lowerPosition);
            var upperDamage = e.Damage(3);
            Assert.That(Discover(lower, e.State, upperDamage), Is.Empty);
            Discover(upper, e.State, upperDamage)[0].Trigger.Resolve(e.State, upperDamage);
            Assert.That(e.Usage.HasTriggered(lowerPet.InstanceId), Is.False);
            var lowerDamage = e.DamageAt(lowerPosition, 3);
            Discover(lower, e.State, lowerDamage)[0].Trigger.Resolve(e.State, lowerDamage);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Second.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(5));
        }

        [Test]
        public void FactorySource_EngineResumesOverflowWithoutRepeatingDamageOrArmorGain()
        {
            var e = new Environment();
            var source = Create(Factory(e), e.Side, e.Pet);
            e.Damage(4);
            e.Target.ApplyArmorGain(int.MaxValue);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue,
                new CombatTriggerSourceRegistry(new ICombatTriggerSource[] { source }));
            Assert.Throws<OverflowException>(() => engine.Drain(20, 20));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Target.Armor, Is.EqualTo(int.MaxValue));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            e.Target.RemoveArmor(int.MaxValue);
            engine.Drain(20, 20);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.HasActiveBatch, Is.False);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        private static SnailPetTriggerSourceFactory Factory(Environment e) =>
            new SnailPetTriggerSourceFactory(e.Pet.DefinitionId, e.Usage, e.Armor);

        private static SnailPetTriggerSource Create(
            SnailPetTriggerSourceFactory factory, CombatSide side, CombatPetState pet)
        {
            var sources = new List<ICombatTriggerSource>(factory.CreateSources(side, pet));
            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(sources[0], Is.TypeOf<SnailPetTriggerSource>());
            return (SnailPetTriggerSource)sources[0];
        }

        private static List<CombatTriggerCandidate<ICombatTriggerHandler>> Discover(
            SnailPetTriggerSource source, CombatState state, CombatEvent sourceEvent) =>
            new List<CombatTriggerCandidate<ICombatTriggerHandler>>(source.DiscoverTriggers(state, sourceEvent));

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatCardState Second;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatPetTriggerUsageCommitter Usage = new CombatPetTriggerUsageCommitter(
                new CombatPetTriggerUsageRegistry());
            public readonly CombatArmorGainResolver Armor;
            public BoardPosition TargetPosition => Position(Row, 1);
            public BoardPosition SecondPosition => Position(Row, 2);
            private readonly BoardPosition _sourcePosition;

            public Environment(CombatSide side = CombatSide.Player, BoardRow row = BoardRow.Front)
            {
                Side = side; Row = row;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Target = Card(1); Second = Card(3);
                var attacker = Card(2);
                _sourcePosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                var own = new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(1), TargetPosition, Target.InstanceId),
                    new CombatSlotState(new SlotId(3), SecondPosition, Second.InstanceId),
                    new CombatSlotState(new SlotId(4), Position(row, 3)),
                    new CombatSlotState(new SlotId(5), Position(row == BoardRow.Front ? BoardRow.Back : BoardRow.Front, 1)) }),
                    new CombatCardRegistry(new[] { Target, Second }), new BattleHealth(20), new AttackMultiplier(1));
                var opposing = new CombatSideState(new CombatBoardState(other, new[] {
                    new CombatSlotState(new SlotId(2), _sourcePosition, attacker.InstanceId) }),
                    new CombatCardRegistry(new[] { attacker }), new BattleHealth(20), new AttackMultiplier(1));
                var playerPets = Pets(CombatSide.Player);
                var enemyPets = Pets(CombatSide.Enemy);
                Pet = (side == CombatSide.Player ? playerPets : enemyPets)[row == BoardRow.Front ? 0 : 1];
                State = new CombatState(side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Player ? opposing : own,
                    new CombatSidePetState(CombatSide.Player, new CombatPetRegistry(playerPets)),
                    new CombatSidePetState(CombatSide.Enemy, new CombatPetRegistry(enemyPets)));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot()); Log.Append(Root);
                Armor = new CombatArmorGainResolver(Metadata, Log);
            }
            public BoardPosition Position(BoardRow row, int column) => new BoardPosition(Side, row, new BoardColumn(column));
            public InstanceId PetId(CombatSide side, BoardRow row) =>
                new InstanceId((side == CombatSide.Player ? 1000 : 2000) + (row == BoardRow.Front ? 2 : 1));
            private CombatPetState[] Pets(CombatSide side) => new[] {
                new CombatPetState(new DefinitionId("test.snail"), PetId(side, BoardRow.Front)),
                new CombatPetState(new DefinitionId("test.snail"), PetId(side, BoardRow.Back)) };
            private static CombatCardState Card(long id) => new CombatCardState(
                new DefinitionId("test.snail_card"), new InstanceId(id), new CardRank(4), CombatCardSeason.Spring, 10, 5, 3, 2);
            public DamageAppliedCombatEvent Damage(int amount) => DamageAt(TargetPosition, amount);
            public DamageAppliedCombatEvent DamageAt(BoardPosition position, int amount) => new CombatDamageResolver(Metadata, Log)
                .ApplyResolvedCardDamage(State, Root, _sourcePosition, position, amount);
        }
    }
}
