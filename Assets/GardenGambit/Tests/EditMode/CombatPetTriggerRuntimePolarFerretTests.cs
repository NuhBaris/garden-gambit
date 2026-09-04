using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimePolarFerretTests
    {
        [Test]
        public void
            BuildSourceRegistry_WithPolarFerret_CreatesRuntimeSource()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var polarFerret =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .PolarFerret,
                    new InstanceId(1001));

            var state =
                CreateState(
                    new[]
                    {
                        polarFerret
                    });

            var registry =
                runtime.BuildSourceRegistry(
                    state);

            Assert.That(
                runtime.FactoryRegistry.Count,
                Is.EqualTo(2));

            Assert.That(
                runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds
                        .PolarFerret),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            var source =
                registry.Sources[0]
                    as PolarFerretPetTriggerSource;

            Assert.That(
                source,
                Is.Not.Null);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    polarFerret.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    runtime.UsageCommitter));

            Assert.That(
                source.TargetDamageReductionRegistry,
                Is.SameAs(
                    runtime
                        .TargetDamageReductionRegistry));
        }

        [Test]
        public void
            BuildSourceRegistry_WithSunBirdAndPolarFerret_PreservesPetOrder()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var sunBird =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .SunBird,
                    new InstanceId(1001));

            var polarFerret =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .PolarFerret,
                    new InstanceId(1002));

            var state =
                CreateState(
                    new[]
                    {
                        sunBird,
                        polarFerret
                    });

            var registry =
                runtime.BuildSourceRegistry(
                    state);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Sources[0],
                Is.TypeOf<
                    SunBirdPetTriggerSource>());

            Assert.That(
                registry.Sources[1],
                Is.TypeOf<
                    PolarFerretPetTriggerSource>());

            Assert.That(
                ((SunBirdPetTriggerSource)
                    registry.Sources[0])
                    .PetInstanceId,
                Is.EqualTo(
                    sunBird.InstanceId));

            Assert.That(
                ((PolarFerretPetTriggerSource)
                    registry.Sources[1])
                    .PetInstanceId,
                Is.EqualTo(
                    polarFerret.InstanceId));
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets)
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        Array.Empty<
                            CombatPetState>())));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<
                        CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<
                        CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}