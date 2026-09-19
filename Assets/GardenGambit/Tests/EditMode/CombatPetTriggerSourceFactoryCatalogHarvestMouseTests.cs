using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerSourceFactoryCatalogHarvestMouseTests
    {
        [Test]
        public void HarvestMouseIdentity_IsStableValidAndDistinctFromExistingPets()
        {
            Assert.That(CombatPetDefinitionIds.HarvestMouseValue, Is.EqualTo("pet.harvest_mouse"));
            Assert.That(CombatPetDefinitionIds.HarvestMouse,
                Is.EqualTo(new DefinitionId(CombatPetDefinitionIds.HarvestMouseValue)));
            Assert.That(CombatPetDefinitionIds.HarvestMouse.IsValid, Is.True);
            var ids = new HashSet<DefinitionId>
            {
                CombatPetDefinitionIds.SunBird, CombatPetDefinitionIds.PolarFerret,
                CombatPetDefinitionIds.MuskCat, CombatPetDefinitionIds.RainSparrow,
                CombatPetDefinitionIds.HarvestMouse
            };
            Assert.That(ids.Count, Is.EqualTo(5));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void FullRegistry_RejectsMissingDependencies(int missing)
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => e.Runtime.FactoryCatalog.CreateRegistry(
                missing == 0 ? null : e.Armor, missing == 1 ? null : e.Attack,
                missing == 2 ? null : e.Lookup, missing == 3 ? null : e.Runtime.PetUsageCommitter));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void FullRegistry_IncludesExistingFactoriesAndSharesHarvestMouseDependencies()
        {
            var e = new Environment();
            var registry = e.CreateRegistry();
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            Assert.That(registry.GetFactory(CombatPetDefinitionIds.SunBird), Is.TypeOf<SunBirdPetTriggerSourceFactory>());
            Assert.That(registry.GetFactory(CombatPetDefinitionIds.PolarFerret), Is.TypeOf<PolarFerretPetTriggerSourceFactory>());
            Assert.That(registry.GetFactory(CombatPetDefinitionIds.MuskCat), Is.TypeOf<MuskCatPetTriggerSourceFactory>());
            Assert.That(registry.GetFactory(CombatPetDefinitionIds.RainSparrow), Is.TypeOf<RainSparrowPetTriggerSourceFactory>());
            var mouse = registry.GetFactory(CombatPetDefinitionIds.HarvestMouse) as HarvestMousePetTriggerSourceFactory;
            Assert.That(mouse, Is.Not.Null);
            Assert.That(mouse.PetDefinitionId, Is.EqualTo(CombatPetDefinitionIds.HarvestMouse));
            Assert.That(mouse.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(mouse.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(mouse.CardLookup, Is.SameAs(e.Lookup));
            var pet = new CombatPetState(CombatPetDefinitionIds.HarvestMouse, new InstanceId(1001));
            var sources = new List<ICombatTriggerSource>(mouse.CreateSources(CombatSide.Player, pet));
            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(sources[0], Is.TypeOf<HarvestMousePetTriggerSource>());
        }

        [Test]
        public void ExistingOverloads_KeepTheirPreviousFactorySets()
        {
            var e = new Environment();
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry().Count, Is.EqualTo(3));
            var rainRegistry = e.Runtime.FactoryCatalog.CreateRegistry(e.Armor);
            Assert.That(rainRegistry.Count, Is.EqualTo(4));
            Assert.That(rainRegistry.Contains(CombatPetDefinitionIds.RainSparrow), Is.True);
            Assert.That(rainRegistry.Contains(CombatPetDefinitionIds.HarvestMouse), Is.False);
            Assert.That(e.Runtime.FactoryRegistry.Count, Is.EqualTo(3));
            Assert.That(e.CreateRegistry().Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
        }

        [Test]
        public void RecreatingFullRegistry_KeepsSamePetUsageState()
        {
            var e = new Environment();
            var first = (HarvestMousePetTriggerSourceFactory)e.CreateRegistry().GetFactory(CombatPetDefinitionIds.HarvestMouse);
            var petId = new InstanceId(1001);
            first.UsageCommitter.TryCommit(petId, () => { });
            var second = (HarvestMousePetTriggerSourceFactory)e.CreateRegistry().GetFactory(CombatPetDefinitionIds.HarvestMouse);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.UsageCommitter, Is.SameAs(first.UsageCommitter));
            Assert.That(second.UsageCommitter.HasTriggered(petId), Is.True);
        }

        private sealed class Environment
        {
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatCardLookup Lookup;

            public Environment()
            {
                var metadata = new CombatEventMetadataFactory(new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
                var log = new CombatEventLog();
                Armor = new CombatArmorGainResolver(metadata, log);
                Attack = new CombatAttackGainResolver(metadata, log);
                Lookup = new CombatCardLookup(log);
            }

            public CombatPetTriggerSourceFactoryRegistry CreateRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(Armor, Attack, Lookup, Runtime.PetUsageCommitter);
            }
        }
    }
}
