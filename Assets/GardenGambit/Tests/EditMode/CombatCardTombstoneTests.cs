using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;
using System;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatCardTombstoneTests
    {
        [Test]
        public void Constructor_WithDeathRemoval_SnapshotsCard()
        {
            var card =
                CreateCard(
                    currentHp: 0,
                    armor: 2,
                    attack: 3);

            var position =
                CreatePosition();

            var metadata =
                CreateRemovalMetadata();

            var tombstone =
                new CombatCardTombstone(
                    card,
                    position,
                    CombatCardRemovalReason.DeathRemoval,
                    metadata);

            Assert.That(
                tombstone.DefinitionId,
                Is.EqualTo(card.DefinitionId));

            Assert.That(
                tombstone.InstanceId,
                Is.EqualTo(card.InstanceId));

            Assert.That(
                tombstone.Rank,
                Is.EqualTo(card.Rank));

            Assert.That(
                tombstone.HpCapacity,
                Is.EqualTo(7));

            Assert.That(
                tombstone.CurrentHp,
                Is.Zero);

            Assert.That(
                tombstone.Armor,
                Is.EqualTo(2));

            Assert.That(
                tombstone.Attack,
                Is.EqualTo(3));

            Assert.That(
                tombstone.LastPosition,
                Is.EqualTo(position));

            Assert.That(
                tombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DeathRemoval));

            Assert.That(
                tombstone.RemovalMetadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                tombstone.RemovalMetadata.SequenceNo,
                Is.EqualTo(metadata.SequenceNo));

            Assert.That(
                tombstone.RemovalMetadata.ParentEventId,
                Is.EqualTo(metadata.ParentEventId));

            Assert.That(
                tombstone.RemovalMetadata.TriggerRootId,
                Is.EqualTo(metadata.TriggerRootId));

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void Constructor_WithDirectDeleteAboveDeathThreshold_AllowsSnapshot()
        {
            var card =
                CreateCard(
                    currentHp: 5,
                    armor: 2,
                    attack: 3);

            var tombstone =
                new CombatCardTombstone(
                    card,
                    CreatePosition(),
                    CombatCardRemovalReason.DirectDelete,
                    CreateRemovalMetadata());

            Assert.That(
                tombstone.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                tombstone.RemovalReason,
                Is.EqualTo(
                    CombatCardRemovalReason.DirectDelete));

            Assert.That(
                tombstone.WasAtDeathThreshold,
                Is.False);
        }

        [Test]
        public void Constructor_WithNullCard_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardTombstone(
                        null,
                        CreatePosition(),
                        CombatCardRemovalReason.DirectDelete,
                        CreateRemovalMetadata()));
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatCardTombstone(
                        CreateCard(
                            currentHp: 5,
                            armor: 0,
                            attack: 3),
                        default(BoardPosition),
                        CombatCardRemovalReason.DirectDelete,
                        CreateRemovalMetadata()));
        }

        [Test]
        public void Constructor_WithUnspecifiedRemovalReason_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatCardTombstone(
                        CreateCard(
                            currentHp: 5,
                            armor: 0,
                            attack: 3),
                        CreatePosition(),
                        CombatCardRemovalReason.Unspecified,
                        CreateRemovalMetadata()));
        }

        [Test]
        public void Constructor_WithUnknownRemovalReason_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatCardTombstone(
                        CreateCard(
                            currentHp: 5,
                            armor: 0,
                            attack: 3),
                        CreatePosition(),
                        (CombatCardRemovalReason)99,
                        CreateRemovalMetadata()));
        }

        [Test]
        public void Constructor_WithInvalidRemovalMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatCardTombstone(
                        CreateCard(
                            currentHp: 5,
                            armor: 0,
                            attack: 3),
                        CreatePosition(),
                        CombatCardRemovalReason.DirectDelete,
                        default(CombatEventMetadata)));
        }

        [Test]
        public void Constructor_WithRootRemovalMetadata_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatCardTombstone(
                        CreateCard(
                            currentHp: 5,
                            armor: 0,
                            attack: 3),
                        CreatePosition(),
                        CombatCardRemovalReason.DirectDelete,
                        rootMetadata));
        }

        [Test]
        public void Constructor_WithDeathRemovalAboveThreshold_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatCardTombstone(
                        CreateCard(
                            currentHp: 1,
                            armor: 0,
                            attack: 3),
                        CreatePosition(),
                        CombatCardRemovalReason.DeathRemoval,
                        CreateRemovalMetadata()));
        }

        [Test]
        public void Snapshot_RemainsUnchangedAfterCardMutation()
        {
            var card =
                CreateCard(
                    currentHp: 5,
                    armor: 2,
                    attack: 3);

            var tombstone =
                new CombatCardTombstone(
                    card,
                    CreatePosition(),
                    CombatCardRemovalReason.DirectDelete,
                    CreateRemovalMetadata());

            card.ApplyIncomingDamage(3);
            card.ApplyHpStatGain(2);
            card.ApplyArmorGain(4);
            card.ApplyAttackGain(2);
            card.SetRank(
                new CardRank(3));

            Assert.That(
                card.Rank,
                Is.EqualTo(new CardRank(3)));

            Assert.That(
                tombstone.Rank,
                Is.EqualTo(new CardRank(2)));

            Assert.That(
                tombstone.HpCapacity,
                Is.EqualTo(7));

            Assert.That(
                tombstone.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                tombstone.Armor,
                Is.EqualTo(2));

            Assert.That(
                tombstone.Attack,
                Is.EqualTo(3));
        }

        private static CombatCardState CreateCard(
            int currentHp,
            int armor,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId("test-card"),
                new InstanceId(100),
                new CardRank(2),
                7,
                currentHp,
                armor,
                attack);
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
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            return metadataFactory.CreateChild(
                rootMetadata);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }
    }
}