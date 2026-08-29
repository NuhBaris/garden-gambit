using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardStateTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsProperties()
        {
            var definitionId =
                new DefinitionId("card.test");

            var instanceId = new InstanceId(100);
            var rank = new CardRank(14);

            var card = new CombatCardState(
                definitionId,
                instanceId,
                rank,
                7,
                7,
                2,
                3);

            Assert.That(
                card.DefinitionId,
                Is.EqualTo(definitionId));

            Assert.That(
                card.InstanceId,
                Is.EqualTo(instanceId));

            Assert.That(card.Rank, Is.EqualTo(rank));
            Assert.That(card.HpCapacity, Is.EqualTo(7));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.Attack, Is.EqualTo(3));
            Assert.That(card.IsAtDeathThreshold, Is.False);
        }

        [Test]
        public void Constructor_WithInvalidDefinitionId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatCardState(
                    default(DefinitionId),
                    new InstanceId(100),
                    new CardRank(2),
                    7,
                    7,
                    2,
                    3);
            });
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatCardState(
                    new DefinitionId("card.test"),
                    default(InstanceId),
                    new CardRank(2),
                    7,
                    7,
                    2,
                    3);
            });
        }

        [Test]
        public void Constructor_WithInvalidCardRank_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatCardState(
                    new DefinitionId("card.test"),
                    new InstanceId(100),
                    default(CardRank),
                    7,
                    7,
                    2,
                    3);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithNonPositiveHpCapacity_Throws(
            int hpCapacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateCard(
                    hpCapacity: hpCapacity,
                    currentHp: 0));
        }

        [Test]
        public void Constructor_WithCurrentHpAboveCapacity_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateCard(
                    hpCapacity: 7,
                    currentHp: 8));
        }

        [Test]
        public void Constructor_WithNegativeArmor_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateCard(armor: -1));
        }

        [Test]
        public void Constructor_WithNegativeAttack_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateCard(attack: -1));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Constructor_WithNonPositiveCurrentHp_MarksDeathThreshold(
            int currentHp)
        {
            var card = CreateCard(
                currentHp: currentHp);

            Assert.That(card.CurrentHp, Is.EqualTo(currentHp));
            Assert.That(card.IsAtDeathThreshold, Is.True);
        }

        [Test]
        public void Constructor_WithPositiveCurrentHp_IsAboveDeathThreshold()
        {
            var card = CreateCard(currentHp: 1);

            Assert.That(card.IsAtDeathThreshold, Is.False);
        }

        [Test]
        public void ApplyIncomingDamage_WhenArmorCoversDamage_OnlyReducesArmor()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 5);

            var result =
                card.ApplyIncomingDamage(3);

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));

            Assert.That(result.IncomingDamage, Is.EqualTo(3));
            Assert.That(result.ArmorAbsorbed, Is.EqualTo(3));
            Assert.That(result.HpDamage, Is.Zero);
            Assert.That(result.PreviousArmor, Is.EqualTo(5));
            Assert.That(result.CurrentArmor, Is.EqualTo(2));
            Assert.That(result.PreviousHp, Is.EqualTo(7));
            Assert.That(result.CurrentHp, Is.EqualTo(7));
            Assert.That(result.HasPositiveDamage, Is.True);
            Assert.That(
                result.WasFullyAbsorbedByArmor,
                Is.True);
            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
        }

        [Test]
        public void ApplyIncomingDamage_WhenDamageExceedsArmor_ReducesHp()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var result =
                card.ApplyIncomingDamage(5);

            Assert.That(card.Armor, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(4));

            Assert.That(result.IncomingDamage, Is.EqualTo(5));
            Assert.That(result.ArmorAbsorbed, Is.EqualTo(2));
            Assert.That(result.HpDamage, Is.EqualTo(3));
            Assert.That(
                result.WasFullyAbsorbedByArmor,
                Is.False);
            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
        }

        [Test]
        public void ApplyIncomingDamage_WithZeroDamage_DoesNotChangeCard()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var result =
                card.ApplyIncomingDamage(0);

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
            Assert.That(result.ArmorAbsorbed, Is.Zero);
            Assert.That(result.HpDamage, Is.Zero);
            Assert.That(result.HasPositiveDamage, Is.False);
            Assert.That(
                result.WasFullyAbsorbedByArmor,
                Is.False);
            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
        }

        [TestCase(9, 0)]
        [TestCase(10, -1)]
        public void ApplyIncomingDamage_CrossingThreshold_ReportsDeathEntry(
            int incomingDamage,
            int expectedCurrentHp)
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var result =
                card.ApplyIncomingDamage(incomingDamage);

            Assert.That(card.Armor, Is.Zero);
            Assert.That(
                card.CurrentHp,
                Is.EqualTo(expectedCurrentHp));

            Assert.That(result.ArmorAbsorbed, Is.EqualTo(2));
            Assert.That(
                result.HpDamage,
                Is.EqualTo(incomingDamage - 2));

            Assert.That(
                result.EnteredDeathThreshold,
                Is.True);

            Assert.That(card.IsAtDeathThreshold, Is.True);
        }

        [Test]
        public void ApplyIncomingDamage_WhenAlreadyAtThreshold_DoesNotReportNewEntry()
        {
            var card = CreateCard(
                currentHp: 0,
                armor: 0);

            var result =
                card.ApplyIncomingDamage(2);

            Assert.That(card.CurrentHp, Is.EqualTo(-2));
            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
            Assert.That(card.IsAtDeathThreshold, Is.True);
        }

        [Test]
        public void ApplyIncomingDamage_WithNegativeDamage_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.ApplyIncomingDamage(-1));

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void ApplyIncomingDamage_WhenHpWouldOverflow_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: int.MinValue,
                armor: 0);

            Assert.Throws<OverflowException>(
                () => card.ApplyIncomingDamage(1));

            Assert.That(card.Armor, Is.Zero);
            Assert.That(
                card.CurrentHp,
                Is.EqualTo(int.MinValue));
        }

        [Test]
        public void Heal_WithAvailableCapacity_RestoresRequestedAmount()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 3);

            var actualRestoredAmount =
                card.Heal(2);

            Assert.That(actualRestoredAmount, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(5));
            Assert.That(card.HpCapacity, Is.EqualTo(7));
        }

        [Test]
        public void Heal_AboveMissingHp_CapsAtCapacity()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 5);

            var actualRestoredAmount =
                card.Heal(10);

            Assert.That(actualRestoredAmount, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
            Assert.That(card.HpCapacity, Is.EqualTo(7));
        }

        [Test]
        public void Heal_AtFullHp_ReturnsZero()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 7);

            var actualRestoredAmount =
                card.Heal(5);

            Assert.That(actualRestoredAmount, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(7));
            Assert.That(card.HpCapacity, Is.EqualTo(7));
        }

        [Test]
        public void Heal_FromNegativeHp_CanCrossAboveDeathThreshold()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: -2);

            var actualRestoredAmount =
                card.Heal(3);

            Assert.That(actualRestoredAmount, Is.EqualTo(3));
            Assert.That(card.CurrentHp, Is.EqualTo(1));
            Assert.That(card.IsAtDeathThreshold, Is.False);
        }

        [Test]
        public void Heal_WithZeroAmount_DoesNotChangeCard()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 3);

            var actualRestoredAmount =
                card.Heal(0);

            Assert.That(actualRestoredAmount, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(3));
            Assert.That(card.HpCapacity, Is.EqualTo(7));
        }

        [Test]
        public void Heal_WithNegativeAmount_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 3);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.Heal(-1));

            Assert.That(card.CurrentHp, Is.EqualTo(3));
            Assert.That(card.HpCapacity, Is.EqualTo(7));
        }

        [Test]
        public void ApplyHpStatGain_IncreasesCapacityAndCurrentHp()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 5);

            var actualHpIncrease =
                card.ApplyHpStatGain(2);

            Assert.That(actualHpIncrease, Is.EqualTo(2));
            Assert.That(card.HpCapacity, Is.EqualTo(9));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void ApplyHpStatGain_AtDeathThreshold_CanRestorePositiveHp()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: -2);

            var actualHpIncrease =
                card.ApplyHpStatGain(3);

            Assert.That(actualHpIncrease, Is.EqualTo(3));
            Assert.That(card.HpCapacity, Is.EqualTo(10));
            Assert.That(card.CurrentHp, Is.EqualTo(1));
            Assert.That(card.IsAtDeathThreshold, Is.False);
        }

        [Test]
        public void ApplyHpStatGain_WithZeroAmount_DoesNotChangeCard()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 5);

            var actualHpIncrease =
                card.ApplyHpStatGain(0);

            Assert.That(actualHpIncrease, Is.Zero);
            Assert.That(card.HpCapacity, Is.EqualTo(7));
            Assert.That(card.CurrentHp, Is.EqualTo(5));
        }

        [Test]
        public void ApplyHpStatGain_WithNegativeAmount_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                hpCapacity: 7,
                currentHp: 5);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.ApplyHpStatGain(-1));

            Assert.That(card.HpCapacity, Is.EqualTo(7));
            Assert.That(card.CurrentHp, Is.EqualTo(5));
        }

        [Test]
        public void ApplyHpStatGain_WhenCapacityWouldOverflow_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                hpCapacity: int.MaxValue,
                currentHp: 5);

            Assert.Throws<OverflowException>(
                () => card.ApplyHpStatGain(1));

            Assert.That(
                card.HpCapacity,
                Is.EqualTo(int.MaxValue));

            Assert.That(card.CurrentHp, Is.EqualTo(5));
        }

        [Test]
        public void ApplyArmorGain_WithPositiveAmount_IncreasesArmor()
        {
            var card = CreateCard(armor: 2);

            var actualGain =
                card.ApplyArmorGain(3);

            Assert.That(actualGain, Is.EqualTo(3));
            Assert.That(card.Armor, Is.EqualTo(5));
        }

        [Test]
        public void ApplyArmorGain_WithZeroAmount_DoesNotChangeArmor()
        {
            var card = CreateCard(armor: 2);

            var actualGain =
                card.ApplyArmorGain(0);

            Assert.That(actualGain, Is.Zero);
            Assert.That(card.Armor, Is.EqualTo(2));
        }

        [Test]
        public void ApplyArmorGain_WithNegativeAmount_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(armor: 2);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.ApplyArmorGain(-1));

            Assert.That(card.Armor, Is.EqualTo(2));
        }

        [Test]
        public void ApplyArmorGain_WhenArmorWouldOverflow_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                armor: int.MaxValue);

            Assert.Throws<OverflowException>(
                () => card.ApplyArmorGain(1));

            Assert.That(
                card.Armor,
                Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void RemoveArmor_BelowCurrentArmor_RemovesRequestedAmount()
        {
            var card = CreateCard(armor: 5);

            var actualRemovedAmount =
                card.RemoveArmor(3);

            Assert.That(
                actualRemovedAmount,
                Is.EqualTo(3));

            Assert.That(card.Armor, Is.EqualTo(2));
        }

        [Test]
        public void RemoveArmor_AboveCurrentArmor_RemovesOnlyAvailableArmor()
        {
            var card = CreateCard(armor: 2);

            var actualRemovedAmount =
                card.RemoveArmor(10);

            Assert.That(
                actualRemovedAmount,
                Is.EqualTo(2));

            Assert.That(card.Armor, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void RemoveArmor_WithZeroAmount_DoesNotChangeArmor()
        {
            var card = CreateCard(armor: 2);

            var actualRemovedAmount =
                card.RemoveArmor(0);

            Assert.That(actualRemovedAmount, Is.Zero);
            Assert.That(card.Armor, Is.EqualTo(2));
        }

        [Test]
        public void RemoveArmor_WhenArmorIsZero_ReturnsZero()
        {
            var card = CreateCard(armor: 0);

            var actualRemovedAmount =
                card.RemoveArmor(5);

            Assert.That(actualRemovedAmount, Is.Zero);
            Assert.That(card.Armor, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void RemoveArmor_WithNegativeAmount_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(armor: 2);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.RemoveArmor(-1));

            Assert.That(card.Armor, Is.EqualTo(2));
        }

        [Test]
        public void ApplyAttackGain_WithPositiveAmount_IncreasesAttack()
        {
            var card = CreateCard(attack: 3);

            var actualGain =
                card.ApplyAttackGain(2);

            Assert.That(actualGain, Is.EqualTo(2));
            Assert.That(card.Attack, Is.EqualTo(5));
        }

        [Test]
        public void ApplyAttackGain_WithZeroAmount_DoesNotChangeAttack()
        {
            var card = CreateCard(attack: 3);

            var actualGain =
                card.ApplyAttackGain(0);

            Assert.That(actualGain, Is.Zero);
            Assert.That(card.Attack, Is.EqualTo(3));
        }

        [Test]
        public void ApplyAttackGain_WithNegativeAmount_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(attack: 3);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.ApplyAttackGain(-1));

            Assert.That(card.Attack, Is.EqualTo(3));
        }

        [Test]
        public void ApplyAttackGain_WhenAttackWouldOverflow_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                attack: int.MaxValue);

            Assert.Throws<OverflowException>(
                () => card.ApplyAttackGain(1));

            Assert.That(
                card.Attack,
                Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void ReduceAttack_BelowCurrentAttack_ReducesRequestedAmount()
        {
            var card = CreateCard(attack: 5);

            var actualReducedAmount =
                card.ReduceAttack(3);

            Assert.That(
                actualReducedAmount,
                Is.EqualTo(3));

            Assert.That(card.Attack, Is.EqualTo(2));
        }

        [Test]
        public void ReduceAttack_AboveCurrentAttack_ReducesOnlyAvailableAttack()
        {
            var card = CreateCard(attack: 3);

            var actualReducedAmount =
                card.ReduceAttack(10);

            Assert.That(
                actualReducedAmount,
                Is.EqualTo(3));

            Assert.That(card.Attack, Is.Zero);
        }

        [Test]
        public void ReduceAttack_WithZeroAmount_DoesNotChangeAttack()
        {
            var card = CreateCard(attack: 3);

            var actualReducedAmount =
                card.ReduceAttack(0);

            Assert.That(actualReducedAmount, Is.Zero);
            Assert.That(card.Attack, Is.EqualTo(3));
        }

        [Test]
        public void ReduceAttack_WhenAttackIsZero_ReturnsZero()
        {
            var card = CreateCard(attack: 0);

            var actualReducedAmount =
                card.ReduceAttack(5);

            Assert.That(actualReducedAmount, Is.Zero);
            Assert.That(card.Attack, Is.Zero);
        }

        [Test]
        public void ReduceAttack_WithNegativeAmount_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(attack: 3);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.ReduceAttack(-1));

            Assert.That(card.Attack, Is.EqualTo(3));
        }

        [Test]
        public void SetRank_WithValidRank_ChangesCurrentRankAndReturnsPreviousRank()
        {
            var card = CreateCard();

            var originalDefinitionId =
                card.DefinitionId;

            var originalInstanceId =
                card.InstanceId;

            var previousRank =
                card.SetRank(new CardRank(14));

            Assert.That(
                previousRank,
                Is.EqualTo(new CardRank(2)));

            Assert.That(
                card.Rank,
                Is.EqualTo(new CardRank(14)));

            Assert.That(
                card.DefinitionId,
                Is.EqualTo(originalDefinitionId));

            Assert.That(
                card.InstanceId,
                Is.EqualTo(originalInstanceId));
        }

        [Test]
        public void SetRank_WithSameRank_ReturnsSameRank()
        {
            var card = CreateCard();

            var previousRank =
                card.SetRank(new CardRank(2));

            Assert.That(
                previousRank,
                Is.EqualTo(new CardRank(2)));

            Assert.That(
                card.Rank,
                Is.EqualTo(new CardRank(2)));
        }

        [Test]
        public void SetRank_WithInvalidRank_ThrowsWithoutChangingCard()
        {
            var card = CreateCard();

            Assert.Throws<ArgumentException>(
                () => card.SetRank(
                    default(CardRank)));

            Assert.That(
                card.Rank,
                Is.EqualTo(new CardRank(2)));

            Assert.That(
                card.DefinitionId,
                Is.EqualTo(
                    new DefinitionId("card.test")));

            Assert.That(
                card.InstanceId,
                Is.EqualTo(new InstanceId(100)));
        }

        [Test]
        public void DefaultDamageApplicationResult_IsInvalid()
        {
            var result =
                default(DamageApplicationResult);

            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ApplyIncomingDamage_ReturnsValidResult()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var result =
                card.ApplyIncomingDamage(5);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ApplyIncomingDamage_WithZero_ReturnsValidInitializedResult()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 0);

            var result =
                card.ApplyIncomingDamage(0);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.IncomingDamage, Is.Zero);
            Assert.That(result.ArmorAbsorbed, Is.Zero);
            Assert.That(result.HpDamage, Is.Zero);
        }

        [Test]
        public void PreviewIncomingDamage_CalculatesResultWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var result =
                card.PreviewIncomingDamage(5);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.IncomingDamage, Is.EqualTo(5));
            Assert.That(result.ArmorAbsorbed, Is.EqualTo(2));
            Assert.That(result.HpDamage, Is.EqualTo(3));
            Assert.That(result.CurrentArmor, Is.Zero);
            Assert.That(result.CurrentHp, Is.EqualTo(4));

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void PreviewIncomingDamage_WithZero_ReturnsValidResultWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var result =
                card.PreviewIncomingDamage(0);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.IncomingDamage, Is.Zero);
            Assert.That(result.CurrentArmor, Is.EqualTo(2));
            Assert.That(result.CurrentHp, Is.EqualTo(7));

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void PreviewIncomingDamage_WithNegativeDamage_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => card.PreviewIncomingDamage(-1));

            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
        }

        [Test]
        public void PreviewIncomingDamage_WhenHpWouldOverflow_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: int.MinValue,
                armor: 0);

            Assert.Throws<OverflowException>(
                () => card.PreviewIncomingDamage(1));

            Assert.That(card.Armor, Is.Zero);

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(int.MinValue));
        }

        [Test]
        public void ApplyIncomingDamage_ProducesSameResultAsPreviewAndThenChangesCard()
        {
            var card = CreateCard(
                currentHp: 7,
                armor: 2);

            var preview =
                card.PreviewIncomingDamage(5);

            var applied =
                card.ApplyIncomingDamage(5);

            Assert.That(
                applied.IncomingDamage,
                Is.EqualTo(preview.IncomingDamage));

            Assert.That(
                applied.ArmorAbsorbed,
                Is.EqualTo(preview.ArmorAbsorbed));

            Assert.That(
                applied.HpDamage,
                Is.EqualTo(preview.HpDamage));

            Assert.That(
                applied.CurrentArmor,
                Is.EqualTo(preview.CurrentArmor));

            Assert.That(
                applied.CurrentHp,
                Is.EqualTo(preview.CurrentHp));

            Assert.That(card.Armor, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(4));
        }

        [Test]
        public void PreviewIncomingDamage_WhenHpReachesZero_EntersDeathThreshold()
        {
            var card = CreateCard(
                currentHp: 3,
                armor: 0);

            var result =
                card.PreviewIncomingDamage(3);

            Assert.That(
                result.PreviousHp,
                Is.EqualTo(3));

            Assert.That(
                result.CurrentHp,
                Is.Zero);

            Assert.That(
                result.EnteredDeathThreshold,
                Is.True);
        }

        [Test]
        public void PreviewIncomingDamage_WhenHpFallsBelowZero_EntersDeathThreshold()
        {
            var card = CreateCard(
                currentHp: 3,
                armor: 0);

            var result =
                card.PreviewIncomingDamage(5);

            Assert.That(
                result.PreviousHp,
                Is.EqualTo(3));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(-2));

            Assert.That(
                result.EnteredDeathThreshold,
                Is.True);
        }

        [Test]
        public void PreviewIncomingDamage_WhenAlreadyAtZero_DoesNotEnterDeathThresholdAgain()
        {
            var card = CreateCard(
                currentHp: 0,
                armor: 0);

            var result =
                card.PreviewIncomingDamage(2);

            Assert.That(
                result.PreviousHp,
                Is.Zero);

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(-2));

            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
        }

        [Test]
        public void PreviewIncomingDamage_WhenAlreadyBelowZero_DoesNotEnterDeathThresholdAgain()
        {
            var card = CreateCard(
                currentHp: -1,
                armor: 0);

            var result =
                card.PreviewIncomingDamage(2);

            Assert.That(
                result.PreviousHp,
                Is.EqualTo(-1));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(-3));

            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
        }

        [Test]
        public void PreviewIncomingDamage_WhenHpDoesNotChange_DoesNotEnterDeathThreshold()
        {
            var card = CreateCard(
                currentHp: 3,
                armor: 2);

            var result =
                card.PreviewIncomingDamage(2);

            Assert.That(
                result.PreviousHp,
                Is.EqualTo(3));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(3));

            Assert.That(
                result.WasFullyAbsorbedByArmor,
                Is.True);

            Assert.That(
                result.EnteredDeathThreshold,
                Is.False);
        }

        [Test]
        public void RescueToOneHp_WhenHpIsZero_SetsHpToOneAndReturnsPreviousHp()
        {
            var card = CreateCard(
                currentHp: 0,
                armor: 2);

            var previousHp =
                card.RescueToOneHp();

            Assert.That(
                previousHp,
                Is.Zero);

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                card.Armor,
                Is.EqualTo(2));

            Assert.That(
                card.IsAtDeathThreshold,
                Is.False);
        }

        [Test]
        public void RescueToOneHp_WhenHpIsBelowZero_SetsHpDirectlyToOne()
        {
            var card = CreateCard(
                currentHp: -3,
                armor: 0);

            var previousHp =
                card.RescueToOneHp();

            Assert.That(
                previousHp,
                Is.EqualTo(-3));

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                card.IsAtDeathThreshold,
                Is.False);
        }

        [Test]
        public void RescueToOneHp_WhenCardIsAlive_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: 3,
                armor: 2);

            Assert.Throws<InvalidOperationException>(
                () => card.RescueToOneHp());

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(3));

            Assert.That(
                card.Armor,
                Is.EqualTo(2));
        }

        [Test]
        public void RescueToOneHp_WhenCalledAgainAfterRescue_ThrowsWithoutChangingCard()
        {
            var card = CreateCard(
                currentHp: 0,
                armor: 0);

            card.RescueToOneHp();

            Assert.Throws<InvalidOperationException>(
                () => card.RescueToOneHp());

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                card.IsAtDeathThreshold,
                Is.False);
        }

        private static CombatCardState CreateCard(
            int hpCapacity = 7,
            int currentHp = 7,
            int armor = 2,
            int attack = 3)
        {
            return new CombatCardState(
                new DefinitionId("card.test"),
                new InstanceId(100),
                new CardRank(2),
                hpCapacity,
                currentHp,
                armor,
                attack);
        }
    }
}