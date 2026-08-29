using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class DamageAppliedCombatEventTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsProperties()
        {
            var metadata = CreateMetadata();
            var sourceInstanceId = new InstanceId(100);
            var sourcePosition = CreateSourcePosition();
            var targetInstanceId = new InstanceId(200);
            var targetPosition = CreateTargetPosition();
            var result = CreateDamageResult(5);

            var damageEvent =
                new DamageAppliedCombatEvent(
                    metadata,
                    sourceInstanceId,
                    sourcePosition,
                    targetInstanceId,
                    targetPosition,
                    result);

            Assert.That(
                damageEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                damageEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DamageApplied));

            Assert.That(
                damageEvent.SourceInstanceId,
                Is.EqualTo(sourceInstanceId));

            Assert.That(
                damageEvent.SourcePosition,
                Is.EqualTo(sourcePosition));

            Assert.That(
                damageEvent.TargetInstanceId,
                Is.EqualTo(targetInstanceId));

            Assert.That(
                damageEvent.TargetPosition,
                Is.EqualTo(targetPosition));

            Assert.That(
                damageEvent.Result.IncomingDamage,
                Is.EqualTo(5));

            Assert.That(
                damageEvent.Result.ArmorAbsorbed,
                Is.EqualTo(2));

            Assert.That(
                damageEvent.Result.HpDamage,
                Is.EqualTo(3));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DamageAppliedCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreateSourcePosition(),
                        new InstanceId(200),
                        CreateTargetPosition(),
                        CreateDamageResult(5)));
        }

        [Test]
        public void Constructor_WithInvalidSourceInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DamageAppliedCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreateSourcePosition(),
                        new InstanceId(200),
                        CreateTargetPosition(),
                        CreateDamageResult(5)));
        }

        [Test]
        public void Constructor_WithInvalidSourcePosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DamageAppliedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        new InstanceId(200),
                        CreateTargetPosition(),
                        CreateDamageResult(5)));
        }

        [Test]
        public void Constructor_WithInvalidTargetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DamageAppliedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateSourcePosition(),
                        default(InstanceId),
                        CreateTargetPosition(),
                        CreateDamageResult(5)));
        }

        [Test]
        public void Constructor_WithInvalidTargetPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DamageAppliedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateSourcePosition(),
                        new InstanceId(200),
                        default(BoardPosition),
                        CreateDamageResult(5)));
        }

        [Test]
        public void Constructor_WithInvalidDamageResult_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DamageAppliedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateSourcePosition(),
                        new InstanceId(200),
                        CreateTargetPosition(),
                        default(DamageApplicationResult)));
        }

        [Test]
        public void Constructor_WithValidZeroDamageResult_AllowsEvent()
        {
            var damageEvent =
                new DamageAppliedCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreateSourcePosition(),
                    new InstanceId(200),
                    CreateTargetPosition(),
                    CreateDamageResult(0));

            Assert.That(
                damageEvent.Result.IsValid,
                Is.True);

            Assert.That(
                damageEvent.Result.IncomingDamage,
                Is.Zero);
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private static BoardPosition
            CreateSourcePosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static BoardPosition
            CreateTargetPosition()
        {
            return new BoardPosition(
                CombatSide.Enemy,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static DamageApplicationResult
            CreateDamageResult(int incomingDamage)
        {
            var target = new CombatCardState(
                new DefinitionId("card.target"),
                new InstanceId(200),
                new CardRank(2),
                7,
                7,
                2,
                3);

            return target.ApplyIncomingDamage(
                incomingDamage);
        }
    }
}