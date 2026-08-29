using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;
using System;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardLookupResultTests
    {
        [Test]
        public void FromActiveCard_WithValidValues_ExposesActiveCard()
        {
            var card =
                CreateCard();

            var position =
                CreatePosition();

            var result =
                CombatCardLookupResult
                    .FromActiveCard(
                        card,
                        position);

            Assert.That(
                result.IsActive,
                Is.True);

            Assert.That(
                result.IsRemoved,
                Is.False);

            Assert.That(
                result.ActiveCard,
                Is.SameAs(card));

            Assert.That(
                result.Tombstone,
                Is.Null);

            Assert.That(
                result.Position,
                Is.EqualTo(position));

            Assert.That(
                result.DefinitionId,
                Is.EqualTo(card.DefinitionId));

            Assert.That(
                result.InstanceId,
                Is.EqualTo(card.InstanceId));

            Assert.That(
                result.Rank,
                Is.EqualTo(card.Rank));

            Assert.That(
                result.HpCapacity,
                Is.EqualTo(7));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                result.Armor,
                Is.EqualTo(2));

            Assert.That(
                result.Attack,
                Is.EqualTo(3));

            Assert.That(
                result.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.Unspecified));
        }

        [Test]
        public void FromActiveCard_ReflectsLaterCardMutation()
        {
            var card =
                CreateCard();

            var result =
                CombatCardLookupResult
                    .FromActiveCard(
                        card,
                        CreatePosition());

            card.ApplyIncomingDamage(3);
            card.ApplyAttackGain(2);
            card.SetRank(
                new CardRank(3));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                result.Armor,
                Is.Zero);

            Assert.That(
                result.Attack,
                Is.EqualTo(5));

            Assert.That(
                result.Rank,
                Is.EqualTo(new CardRank(3)));

            Assert.That(
                result.IsActive,
                Is.True);
        }

        [Test]
        public void FromActiveCard_WithNullCard_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => CombatCardLookupResult
                    .FromActiveCard(
                        null,
                        CreatePosition()));
        }

        [Test]
        public void FromActiveCard_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => CombatCardLookupResult
                    .FromActiveCard(
                        CreateCard(),
                        default(BoardPosition)));
        }

        [Test]
        public void FromTombstone_WithValidValue_ExposesImmutableSnapshot()
        {
            var card =
                CreateCard();

            var position =
                CreatePosition();

            var tombstone =
                new CombatCardTombstone(
                    card,
                    position,
                    CombatCardRemovalReason.DirectDelete,
                    CreateRemovalMetadata());

            var result =
                CombatCardLookupResult
                    .FromTombstone(
                        tombstone);

            card.ApplyIncomingDamage(3);
            card.ApplyAttackGain(2);
            card.SetRank(
                new CardRank(3));

            Assert.That(
                result.IsActive,
                Is.False);

            Assert.That(
                result.IsRemoved,
                Is.True);

            Assert.That(
                result.ActiveCard,
                Is.Null);

            Assert.That(
                result.Tombstone,
                Is.SameAs(tombstone));

            Assert.That(
                result.Position,
                Is.EqualTo(position));

            Assert.That(
                result.DefinitionId,
                Is.EqualTo(tombstone.DefinitionId));

            Assert.That(
                result.InstanceId,
                Is.EqualTo(tombstone.InstanceId));

            Assert.That(
                result.Rank,
                Is.EqualTo(new CardRank(2)));

            Assert.That(
                result.HpCapacity,
                Is.EqualTo(7));

            Assert.That(
                result.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                result.Armor,
                Is.EqualTo(2));

            Assert.That(
                result.Attack,
                Is.EqualTo(3));

            Assert.That(
                result.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));
        }

        [Test]
        public void FromTombstone_WithNullTombstone_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => CombatCardLookupResult
                    .FromTombstone(
                        null));
        }

        private static CombatCardState CreateCard()
        {
            return new CombatCardState(
                new DefinitionId("test-card"),
                new InstanceId(100),
                new CardRank(2),
                7,
                5,
                2,
                3);
        }

        private static BoardPosition CreatePosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static CombatEventMetadata
            CreateRemovalMetadata()
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var rootMetadata =
                metadataFactory.CreateRoot();

            return metadataFactory.CreateChild(
                rootMetadata);
        }
    }
}