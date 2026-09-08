using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleEndResultPrerequisiteValidatorTests
    {
        [Test]
        public void
            Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatBattleEndResultPrerequisiteValidator(
                            null));
        }

        [Test]
        public void Validate_WithNullCombatStartedEvent_Throws()
        {
            var validator =
                new
                    CombatBattleEndResultPrerequisiteValidator(
                        new CombatEventLog());

            Assert.Throws<ArgumentNullException>(
                () => validator.Validate(
                    null));
        }

        [Test]
        public void
            Validate_WithUnloggedCombatStartedEvent_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            var validator =
                new
                    CombatBattleEndResultPrerequisiteValidator(
                        new CombatEventLog());

            Assert.Throws<ArgumentException>(
                () => validator.Validate(
                    combatStartedEvent));
        }

        [Test]
        public void
            Validate_WithDifferentCombatStartedInstance_Throws()
        {
            var environment =
                CreateEnvironment();

            var differentInstance =
                new CombatStartedCombatEvent(
                    environment
                        .CombatStartedEvent
                        .Metadata);

            AppendBattleEnd(
                environment);

            Assert.Throws<ArgumentException>(
                () => environment.Validator.Validate(
                    differentInstance));
        }

        [Test]
        public void
            Validate_WithoutBattleEndEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment
                        .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            Validate_WithOneBattleEndEvent_ReturnsExactEvent()
        {
            var environment =
                CreateEnvironment();

            var battleEndEvent =
                AppendBattleEnd(
                    environment);

            var result =
                environment.Validator.Validate(
                    environment
                        .CombatStartedEvent);

            Assert.That(
                result,
                Is.SameAs(
                    battleEndEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void
            Validate_WithTwoBattleEndEvents_Throws()
        {
            var environment =
                CreateEnvironment();

            AppendBattleEnd(
                environment);

            AppendBattleEnd(
                environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment
                        .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void
            Validate_WithColumnStartedAfterBattleEnd_Throws()
        {
            var environment =
                CreateEnvironment();

            AppendBattleEnd(
                environment);

            var columnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment
                                .CombatStartedEvent
                                .Metadata),
                    new BoardColumn(1));

            environment.EventLog.Append(
                columnEvent);

            Assert.Throws<InvalidOperationException>(
                () => environment.Validator.Validate(
                    environment
                        .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        private static BattleEndStartedCombatEvent
            AppendBattleEnd(
                TestEnvironment environment)
        {
            var battleEndEvent =
                new BattleEndStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment
                                .CombatStartedEvent
                                .Metadata));

            environment.EventLog.Append(
                battleEndEvent);

            return battleEndEvent;
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            return new TestEnvironment
            {
                MetadataFactory =
                    metadataFactory,

                EventLog =
                    eventLog,

                CombatStartedEvent =
                    combatStartedEvent,

                Validator =
                    new
                        CombatBattleEndResultPrerequisiteValidator(
                            eventLog)
            };
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new
                    CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public
                CombatBattleEndResultPrerequisiteValidator
                Validator
            {
                get;
                set;
            }
        }
    }
}