using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeMuskCatTests
    {
        [Test]
        public void
            BuildSourceRegistry_WithMuskCat_CreatesRuntimeSource()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var muskCat =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .MuskCat,
                    new InstanceId(1001));

            var state =
                CreateState(
                    new[]
                    {
                        muskCat
                    });

            var registry =
                runtime.BuildSourceRegistry(
                    state);

            Assert.That(
                runtime.FactoryRegistry.Count,
                Is.EqualTo(3));

            Assert.That(
                runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds
                        .MuskCat),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            var source =
                registry.Sources[0]
                    as MuskCatPetTriggerSource;

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
                    muskCat.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    runtime.UsageCommitter));

            Assert.That(
                source.FinalRankModifierRegistry,
                Is.SameAs(
                    runtime
                        .FinalRankModifierRegistry));
        }

        [Test]
        public void
            BuildSourceRegistry_WithSunBirdAndMuskCat_PreservesPetOrder()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var sunBird =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .SunBird,
                    new InstanceId(1001));

            var muskCat =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .MuskCat,
                    new InstanceId(1002));

            var state =
                CreateState(
                    new[]
                    {
                        sunBird,
                        muskCat
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
                    MuskCatPetTriggerSource>());

            var sunBirdSource =
                (SunBirdPetTriggerSource)
                    registry.Sources[0];

            var muskCatSource =
                (MuskCatPetTriggerSource)
                    registry.Sources[1];

            Assert.That(
                sunBirdSource.PetInstanceId,
                Is.EqualTo(
                    sunBird.InstanceId));

            Assert.That(
                muskCatSource.PetInstanceId,
                Is.EqualTo(
                    muskCat.InstanceId));

            Assert.That(
                muskCatSource.OrderKeyProvider.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                muskCatSource.FinalRankModifierRegistry,
                Is.SameAs(
                    runtime
                        .FinalRankModifierRegistry));
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