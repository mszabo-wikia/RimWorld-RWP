// Copyright 2021 Máté Szabó
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using RWP.Cache;
using RWP.Events;
using RWP.Service;
using RWP.WorkGiver;
using Verse;

namespace RWP
{
    [StaticConstructorOnStartup]
    public static class RWPMod
    {
        public static readonly Logger Logger;

        private static readonly IServiceProvider Injector;

        private static IServiceScope gameScope;
        private static RWPRoot root;

        static RWPMod()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IEqualityComparer<int>, IntEqualityComparer>();
            services.AddSingleton<IEqualityComparer<Pawn>, ThingByIdEqualityComparer<Pawn>>();
            services.AddSingleton<IEqualityComparer<Thing>, ThingByIdEqualityComparer<Thing>>();
            services.AddSingleton<EffectiveAreaRestrictionEvaluatorFactory>();

            services.AddScoped<RWPRoot>();
            services.AddScoped<EventBus>();

            services.AddScoped(provider => new ExtraFactionCache(Find.QuestManager));
            services.AddScoped<QuestPartWorkDisabledCache>();
            services.AddScoped<PawnRelationsCache>();
            services.AddScoped<MapScopedThingCacheManager>();
            services.AddScoped<LordsPawnsCache>();
            services.AddScoped<PrisonerStateCache>();
            services.AddScoped<RegionScopedThingCacheManager>();
            services.AddScoped(_ =>
            {
                var immunizableHediffDefs =
                    from def in DefDatabase<HediffDef>.AllDefs
                    let imm = def.CompProps<HediffCompProperties_Immunizable>()
                    where imm != null && (imm.immunityPerDaySick > 0 || imm.immunityPerDayNotSick > 0)
                    select def.defName;
                return new HediffImmunizableCache(immunizableHediffDefs.ToHashSet());
            });

            services.AddScoped<EffectiveAreaRestrictionService>();
            services.AddScoped<HediffInjuryBleedingService>();
            services.AddScoped(provider => new RefuelService(
                provider.GetRequiredService<MapScopedThingCacheManager>().GetNamedCache("refuelable-turrets"),
                provider.GetRequiredService<MapScopedThingCacheManager>().GetNamedCache("refuelable-non-turrets")));
            services.AddScoped(provider => new ColonyAnimalsService(
                provider.GetRequiredService<MapScopedThingCacheManager>().GetNamedCache("colony-animals")));
            services.AddScoped(provider =>
            {
                var cultivarPlantDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.plant?.sowTags.Any() ?? false).Select(def => def.defName).ToHashSet();
                return new HarvestService(
                    cultivarPlantDefs,
                    provider.GetRequiredService<RegionScopedThingCacheManager>().GetNamedCache("harvestable-plants"));
             });
            services.AddScoped(provider => new HaulingService(provider.GetRequiredService<RegionScopedThingCacheManager>().GetNamedCache("haulable-items")));

            RWPMod.RegisterImplementationAliases(services, typeof(IPawnLostEventListener));
            RWPMod.RegisterImplementationAliases(services, typeof(IThingLostEventListener));
            RWPMod.RegisterImplementationAliases(services, typeof(ITickStartedEventListener));

            services.AddTransient<WorkGiver_GrowerHarvest>();
            services.AddTransient<WorkGiver_Milk>();
            services.AddTransient<WorkGiver_Shear>();
            services.AddTransient<WorkGiver_Warden_Chat>();
            services.AddTransient<WorkGiver_Warden_DeliverFood>();
            services.AddTransient<WorkGiver_Warden_DoExecution>();
            services.AddTransient<WorkGiver_Warden_Feed>();
            services.AddTransient<WorkGiver_Warden_ReleasePrisoner>();
            services.AddTransient<WorkGiver_Warden_TakeToBed>();
            services.AddTransient<WorkGiver_HaulGeneral>();
            services.AddTransient<WorkGiver_Refuel>();
            services.AddTransient<WorkGiver_Refuel_Turret>();
            services.AddTransient(_ => new WorkGiver_Repair(Find.FactionManager.OfPlayer));

            Injector = services.BuildServiceProvider(validateScopes: true);
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Harmony harmony = new Harmony("rimworld.mods.custom.rwp");
            harmony.PatchAll();
            Logger = new Logger();

            Logger.Message("Initialization complete");
        }

        public static RWPRoot Root => root;

        /// <summary>
        /// Reset the active DI scope when a new game is started or loaded.
        /// </summary>
        public static void ResetGameScope()
        {
            gameScope?.Dispose();
            gameScope = Injector.CreateScope();
            root = gameScope.ServiceProvider.GetRequiredService<RWPRoot>();
        }

        /// <summary>
        /// Obtain an instance of the given service from the service container.
        /// </summary>
        /// <param name="serviceType">Service type to obtain.</param>
        /// <remarks>
        /// This is useful for extension points such as workgiver construction.
        /// </remarks>
        public static object GetService(Type serviceType) => RWPMod.gameScope.ServiceProvider.GetService(serviceType);

        /// <summary>
        /// Register all known implementations of the given interface in the service container
        /// such that they are aliased to the existing service registration of the implementation.
        /// </summary>
        private static void RegisterImplementationAliases(IServiceCollection services, Type serviceType)
        {
            foreach (Type implementationType in serviceType.Assembly.GetTypes().Where(type => !type.IsAbstract && serviceType.IsAssignableFrom(type)))
            {
                services.AddScoped(serviceType, provider => provider.GetRequiredService(implementationType));
            }
        }
    }
}
