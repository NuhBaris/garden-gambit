using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class ChinchillaPetNormalAttackTriggerHandlerTests
    {
        [Test]
        public void Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ChinchillaPetNormalAttackTriggerHandler(
                    CombatSide.Player, new InstanceId(1001), null,
                    new CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [Test]
        public void Constructor_WithNullModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ChinchillaPetNormalAttackTriggerHandler(
                    CombatSide.Player, new InstanceId(1001), CreateUsage(), null));
        }

        [TestCase(-1)]
        [TestCase(99)]
        public void Constructor_WithInvalidSide_Throws(int side)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ChinchillaPetNormalAttackTriggerHandler(
                    (CombatSide)side, new InstanceId(1001), CreateUsage(),
                    new CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [Test]
        public void Constructor_WithInvalidPetId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new ChinchillaPetNormalAttackTriggerHandler(
                    CombatSide.Player, default(InstanceId), CreateUsage(),
                    new CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FirstOwnedSeasonlessAttack_AddsOneDamage(
            CombatSide side, BoardRow row)
        {
            var e = new TestEnvironment(side, row);
            var attack = CreateAttack(side: side, row: row);

            Assert.That(e.Handler.CanTrigger(e.State, attack), Is.True);
            Assert.That(e.Handler.CanTrigger(e.State, attack), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Modifiers.Count, Is.Zero);

            e.Handler.Resolve(e.State, attack);

            Assert.That(e.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Modifiers.ResolveDamage(attack), Is.EqualTo(6));
            Assert.That(attack.BaseDamage, Is.EqualTo(5));
            Assert.That(e.Usage.HasTriggered(e.PetId, attack.AttackerInstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Handler.CanTrigger(e.State, attack), Is.False);
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void AttackInOtherRow_DoesNotConsumeUsage(
            CombatSide side, BoardRow petRow)
        {
            var e = new TestEnvironment(side, petRow);
            var otherRow = petRow == BoardRow.Front ? BoardRow.Back : BoardRow.Front;
            AssertIgnored(e, CreateAttack(side: side, row: otherRow));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void OpposingAttack_DoesNotConsumeUsage(
            CombatSide side, BoardRow row)
        {
            var e = new TestEnvironment(side, row);
            AssertIgnored(e, CreateAttack(side: Opposite(side), row: row));
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Autumn)]
        [TestCase(CombatCardSeason.Winter)]
        public void NonSeasonlessAttack_DoesNotConsumeUsage(CombatCardSeason season)
        {
            var e = new TestEnvironment();
            AssertIgnored(e, CreateAttack(season: season));

            var eligible = CreateAttack(eventId: 3);
            e.Handler.Resolve(e.State, eligible);
            Assert.That(e.Modifiers.GetTotalModifier(eligible.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void LegacyAttackWithoutSeason_IsNotSeasonless()
        {
            var e = new TestEnvironment();
            var sample = CreateAttack();
            var legacy = new NormalAttackCombatEvent(
                sample.Metadata, sample.AttackerInstanceId, sample.AttackerPosition,
                sample.TargetInstanceId, sample.TargetPosition, sample.BaseDamage);

            Assert.That(legacy.AttackerSeason, Is.EqualTo(CombatCardSeason.Unspecified));
            AssertIgnored(e, legacy);
        }

        [TestCase(CombatCardSeason.Seasonless, CombatCardSeason.Winter, 1)]
        [TestCase(CombatCardSeason.Winter, CombatCardSeason.Seasonless, 0)]
        public void EligibilityUsesAttackerSeason(
            CombatCardSeason attackerSeason, CombatCardSeason targetSeason, int expected)
        {
            var e = new TestEnvironment();
            var attack = CreateAttack(season: attackerSeason, targetSeason: targetSeason);

            Assert.That(e.Handler.CanTrigger(e.State, attack), Is.EqualTo(expected == 1));
            e.Handler.Resolve(e.State, attack);

            Assert.That(e.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(expected));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(expected));
        }

        [Test]
        public void ResolvingSameEventTwice_DoesNotDuplicateBonus()
        {
            var e = new TestEnvironment();
            var attack = CreateAttack();

            e.Handler.Resolve(e.State, attack);
            e.Handler.Resolve(e.State, attack);

            Assert.That(e.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void SameCardsSecondAttack_DoesNotGainBonus()
        {
            var e = new TestEnvironment();
            var first = CreateAttack();
            var second = CreateAttack(eventId: 3);

            e.Handler.Resolve(e.State, first);
            Assert.That(e.Handler.CanTrigger(e.State, second), Is.False);
            e.Handler.Resolve(e.State, second);

            Assert.That(e.Modifiers.GetTotalModifier(first.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Modifiers.HasModifier(second.Metadata.EventId), Is.False);
            Assert.That(e.Modifiers.ResolveDamage(second), Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void DifferentCards_HaveIndependentFirstAttackUsage()
        {
            var e = new TestEnvironment();
            var first = CreateAttack();
            var second = CreateAttack(eventId: 3, cardId: 2);

            e.Handler.Resolve(e.State, first);
            Assert.That(e.Handler.CanTrigger(e.State, second), Is.True);
            e.Handler.Resolve(e.State, second);

            Assert.That(e.Modifiers.GetTotalModifier(first.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Modifiers.GetTotalModifier(second.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Usage.HasTriggered(e.PetId, first.AttackerInstanceId), Is.True);
            Assert.That(e.Usage.HasTriggered(e.PetId, second.AttackerInstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void TwoSameDefinitionPets_KeepSeparateUsageForSameCard()
        {
            var e = new TestEnvironment();
            var lower = new ChinchillaPetNormalAttackTriggerHandler(
                CombatSide.Player, new InstanceId(1002), e.Usage, e.Modifiers);
            var frontAttack = CreateAttack();
            var backAttack = CreateAttack(eventId: 3, row: BoardRow.Back);

            e.Handler.Resolve(e.State, frontAttack);
            lower.Resolve(e.State, frontAttack);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(lower.CanTrigger(e.State, backAttack), Is.True);

            e.Handler.Resolve(e.State, backAttack);
            lower.Resolve(e.State, backAttack);

            Assert.That(e.Modifiers.GetTotalModifier(frontAttack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Modifiers.GetTotalModifier(backAttack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Usage.HasTriggered(new InstanceId(1001), frontAttack.AttackerInstanceId), Is.True);
            Assert.That(e.Usage.HasTriggered(new InstanceId(1002), backAttack.AttackerInstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void ExistingSourceModifier_IsAddedToAndOtherEventIsUntouched()
        {
            var e = new TestEnvironment();
            var attack = CreateAttack();
            var other = CreateAttack(eventId: 3, cardId: 2);
            e.Modifiers.AddModifier(attack.Metadata.EventId, 4);
            e.Modifiers.AddModifier(other.Metadata.EventId, 7);

            e.Handler.Resolve(e.State, attack);

            Assert.That(e.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(5));
            Assert.That(e.Modifiers.ResolveDamage(attack), Is.EqualTo(10));
            Assert.That(e.Modifiers.GetTotalModifier(other.Metadata.EventId), Is.EqualTo(7));
            Assert.That(attack.BaseDamage, Is.EqualTo(5));
        }

        [Test]
        public void ModifierOverflow_DoesNotConsumeUsage_AndAllowsRetry()
        {
            var e = new TestEnvironment();
            var attack = CreateAttack();
            e.Modifiers.AddModifier(attack.Metadata.EventId, int.MaxValue);

            Assert.Throws<OverflowException>(() => e.Handler.Resolve(e.State, attack));

            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(int.MaxValue));
            Assert.That(e.Handler.CanTrigger(e.State, attack), Is.True);

            e.Modifiers.AddModifier(attack.Metadata.EventId, -int.MaxValue);
            e.Handler.Resolve(e.State, attack);
            e.Handler.Resolve(e.State, attack);

            Assert.That(e.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void RebuiltHandler_WithSharedUsage_DoesNotRepeatBonus()
        {
            var e = new TestEnvironment();
            var first = CreateAttack();
            e.Handler.Resolve(e.State, first);

            var rebuilt = new ChinchillaPetNormalAttackTriggerHandler(
                CombatSide.Player, e.PetId, e.Usage, e.Modifiers);
            var second = CreateAttack(eventId: 3);
            Assert.That(rebuilt.CanTrigger(e.State, second), Is.False);
            rebuilt.Resolve(e.State, second);

            Assert.That(e.Modifiers.HasModifier(second.Metadata.EventId), Is.False);
            Assert.That(e.Modifiers.GetTotalModifier(first.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void FreshBattleUsage_AllowsSameIdsAgain()
        {
            var firstBattle = new TestEnvironment();
            var nextBattle = new TestEnvironment();
            var attack = CreateAttack();
            firstBattle.Handler.Resolve(firstBattle.State, attack);

            Assert.That(nextBattle.Handler.CanTrigger(nextBattle.State, attack), Is.True);
            nextBattle.Handler.Resolve(nextBattle.State, attack);

            Assert.That(firstBattle.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(nextBattle.Modifiers.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(firstBattle.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(nextBattle.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        private static void AssertIgnored(TestEnvironment e, NormalAttackCombatEvent attack)
        {
            Assert.That(e.Handler.CanTrigger(e.State, attack), Is.False);
            e.Handler.Resolve(e.State, attack);
            Assert.That(e.Modifiers.Count, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        private static CombatPetCardTriggerUsageCommitter CreateUsage()
        {
            return new CombatPetCardTriggerUsageCommitter(
                new CombatPetCardTriggerUsageRegistry());
        }

        private static CombatSide Opposite(CombatSide side)
        {
            return side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
        }

        private static NormalAttackCombatEvent CreateAttack(
            long eventId = 2,
            long cardId = 1,
            CombatSide side = CombatSide.Player,
            BoardRow row = BoardRow.Front,
            CombatCardSeason season = CombatCardSeason.Seasonless,
            CombatCardSeason targetSeason = CombatCardSeason.Unspecified)
        {
            var rootId = new CombatEventId(1);
            return new NormalAttackCombatEvent(
                new CombatEventMetadata(
                    new CombatEventId(eventId), new CombatSequenceNumber(eventId), rootId, rootId),
                new InstanceId(cardId), new BoardPosition(side, row, new BoardColumn(1)), season,
                new InstanceId(cardId + 100),
                new BoardPosition(Opposite(side), BoardRow.Front, new BoardColumn(1)),
                targetSeason, baseDamage: 5);
        }

        private static CombatSideState CreateEmptySide(CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(side, Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(Array.Empty<CombatCardState>()),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));
        }

        private sealed class TestEnvironment
        {
            public TestEnvironment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                // Like SunBird handler tests, these exercise attack event snapshots.
                // Full board execution belongs to the runtime integration tests.
                var pets = new[]
                {
                    new CombatPetState(new DefinitionId("pet.chinchilla"), new InstanceId(1001)),
                    new CombatPetState(new DefinitionId("pet.chinchilla"), new InstanceId(1002))
                };
                var noPets = Array.Empty<CombatPetState>();
                State = new CombatState(
                    CreateEmptySide(CombatSide.Player),
                    CreateEmptySide(CombatSide.Enemy),
                    new CombatSidePetState(CombatSide.Player,
                        new CombatPetRegistry(side == CombatSide.Player ? pets : noPets)),
                    new CombatSidePetState(CombatSide.Enemy,
                        new CombatPetRegistry(side == CombatSide.Enemy ? pets : noPets)));
                PetId = new InstanceId(row == BoardRow.Front ? 1001 : 1002);
                Usage = CreateUsage();
                Modifiers = new CombatNormalAttackSourceDamageModifierRegistry();
                Handler = new ChinchillaPetNormalAttackTriggerHandler(side, PetId, Usage, Modifiers);
            }

            public CombatState State { get; }
            public InstanceId PetId { get; }
            public CombatPetCardTriggerUsageCommitter Usage { get; }
            public CombatNormalAttackSourceDamageModifierRegistry Modifiers { get; }
            public ChinchillaPetNormalAttackTriggerHandler Handler { get; }
        }
    }
}
