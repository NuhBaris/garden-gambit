using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class SnailPetDamageTriggerHandlerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void ArmorBreak_GrantsTwoArmorAndCommitsPetOnce(CombatSide side, BoardRow row)
        {
            var e = new Environment(side, row);
            var damage = e.Damage(3);
            Assert.That(e.Handler.CanTrigger(e.State, damage), Is.True);
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.False);
            e.Handler.Resolve(e.State, damage);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Handler.CanTrigger(e.State, damage), Is.False);
            var gain = (ArmorGainCombatEvent)e.Log.Events[2];
            Assert.That(gain.TargetInstanceId, Is.EqualTo(e.Target.InstanceId));
            Assert.That(gain.TargetPosition, Is.EqualTo(e.TargetPosition));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(2));
            Assert.That(gain.Metadata.ParentEventId, Is.EqualTo(damage.Metadata.EventId));
            Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(damage.Metadata.TriggerRootId));
            e.Handler.Resolve(e.State, damage);
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(e.Target.Armor, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void DamageWithoutArmorBreak_DoesNotConsumeUsage(int amount)
        {
            var e = new Environment();
            var damage = e.Damage(amount);
            e.AssertIgnored(damage);
            Assert.That(e.Target.Armor, Is.EqualTo(3 - amount));
        }

        [Test]
        public void InitiallyUnarmoredCard_DoesNotCountAsArmorReachingZero()
        {
            var e = new Environment();
            e.Target.RemoveArmor(3);
            e.AssertIgnored(e.Damage(1));
            Assert.That(e.Target.Armor, Is.Zero);
        }

        [TestCase(8, 0)]
        [TestCase(9, -1)]
        public void ThresholdTarget_ReceivesArmorWithoutBeingHealed(int damageAmount, int expectedHp)
        {
            var e = new Environment();
            e.Handler.Resolve(e.State, e.Damage(damageAmount));
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(expectedHp));
            Assert.That(e.Target.IsAtDeathThreshold, Is.True);
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        public void OtherRowOrSide_DoesNotConsumePetUsage(CombatSide side, BoardRow row)
        {
            var e = new Environment();
            var damage = e.Damage(3);
            var pet = e.State.GetPets(side).Pets.GetPet(e.PetId(side, row));
            var handler = new SnailPetDamageTriggerHandler(side, pet.InstanceId, e.Usage, e.Armor);
            Assert.That(handler.CanTrigger(e.State, damage), Is.False);
            handler.Resolve(e.State, damage);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void OncePerPet_AppliesAcrossDifferentCardsAndRebuiltHandler()
        {
            var e = new Environment();
            e.Handler.Resolve(e.State, e.Damage(3));
            var secondDamage = e.DamageAt(e.SecondPosition, 3);
            var rebuilt = new SnailPetDamageTriggerHandler(e.Side, e.Pet.InstanceId, e.Usage, e.Armor);
            Assert.That(rebuilt.CanTrigger(e.State, secondDamage), Is.False);
            rebuilt.Resolve(e.State, secondDamage);
            Assert.That(e.Second.Armor, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [Test]
        public void MovedWithinRow_FollowsIdentityAndDoesNotBuffReplacement()
        {
            var e = new Environment();
            var damage = e.Damage(3);
            var destination = e.Position(e.Row, 3);
            e.State.GetSide(e.Side).MoveCard(e.TargetPosition, destination);
            e.State.GetSide(e.Side).MoveCard(e.SecondPosition, e.TargetPosition);
            e.Handler.Resolve(e.State, damage);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Second.Armor, Is.EqualTo(3));
            Assert.That(((ArmorGainCombatEvent)e.Log.Events[2]).TargetPosition, Is.EqualTo(destination));
        }

        [Test]
        public void MovedOutOfRow_ResolveRechecksPreviouslyEligibleTarget()
        {
            var e = new Environment();
            var damage = e.Damage(3);
            Assert.That(e.Handler.CanTrigger(e.State, damage), Is.True);
            e.State.GetSide(e.Side).MoveCard(e.TargetPosition, e.Position(BoardRow.Back, 1));
            e.AssertIgnored(damage);
        }

        [Test]
        public void RemovedTarget_DoesNotTransferBonusToReplacement()
        {
            var e = new Environment();
            var damage = e.Damage(3);
            e.State.GetSide(e.Side).RemoveCardFromCombat(e.TargetPosition);
            e.State.GetSide(e.Side).MoveCard(e.SecondPosition, e.TargetPosition);
            e.AssertIgnored(damage);
            Assert.That(e.Second.Armor, Is.EqualTo(3));
        }

        [Test]
        public void RestoredArmor_DoesNotEraseRecordedBreak()
        {
            var e = new Environment();
            var damage = e.Damage(3);
            e.Target.ApplyArmorGain(5);
            e.Handler.Resolve(e.State, damage);
            Assert.That(e.Target.Armor, Is.EqualTo(7));
            Assert.That(damage.Result.CurrentArmor, Is.Zero);
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [Test]
        public void Overflow_DoesNotConsumeUsageAndCanRetryAfterRepair()
        {
            var e = new Environment();
            var damage = e.Damage(3);
            e.Target.ApplyArmorGain(int.MaxValue);
            Assert.Throws<OverflowException>(() => e.Handler.Resolve(e.State, damage));
            Assert.That(e.Target.Armor, Is.EqualTo(int.MaxValue));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            e.Target.RemoveArmor(int.MaxValue);
            e.Handler.Resolve(e.State, damage);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void UnloggedEventCopy_IsRejectedWithoutConsumingUsage()
        {
            var e = new Environment();
            var damage = e.Damage(3);
            var copy = new DamageAppliedCombatEvent(damage.Metadata, damage.SourceInstanceId,
                damage.SourcePosition, damage.TargetInstanceId, damage.TargetPosition, damage.Result);
            Assert.Throws<ArgumentException>(() => e.Handler.Resolve(e.State, copy));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            e.Handler.Resolve(e.State, damage);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_ValidatesDependenciesAndExposesThem()
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new SnailPetDamageTriggerHandler(e.Side, e.Pet.InstanceId, null, e.Armor));
            Assert.Throws<ArgumentNullException>(() => new SnailPetDamageTriggerHandler(e.Side, e.Pet.InstanceId, e.Usage, null));
            Assert.That(e.Handler.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Handler.ArmorGainResolver, Is.SameAs(e.Armor));
        }

        [Test]
        public void Engine_ProcessesArmorGainWithoutRetriggeringOrRepeatingDamage()
        {
            var e = new Environment();
            e.Damage(4);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue,
                new CombatTriggerSourceRegistry(new ICombatTriggerSource[] {
                    new CombatPetDamageTriggerSource(e.Handler) }));
            engine.Drain(20, 20);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

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
            public readonly SnailPetDamageTriggerHandler Handler;
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
                Handler = new SnailPetDamageTriggerHandler(side, Pet.InstanceId, Usage, Armor);
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
            public void AssertIgnored(DamageAppliedCombatEvent damage)
            {
                var count = Log.Count;
                Assert.That(Handler.CanTrigger(State, damage), Is.False);
                Handler.Resolve(State, damage);
                Assert.That(Usage.UsageRegistry.Count, Is.Zero);
                Assert.That(Log.Count, Is.EqualTo(count));
            }
        }
    }
}
