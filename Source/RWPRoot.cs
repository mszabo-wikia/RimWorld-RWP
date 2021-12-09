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

using RWP.Cache;
using RWP.Events;
using RWP.Service;

namespace RWP
{
    /// <summary>
    /// Exposes service instances to Harmony patches.
    /// </summary>
    public class RWPRoot
    {
        private readonly EventBus eventBus;
        private readonly RefuelService refuelService;
        private readonly ColonyAnimalsService colonyAnimalsService;
        private readonly HaulingService haulingService;
        private readonly HarvestService harvestService;
        private readonly HediffInjuryBleedingService hediffInjuryBleedingService;
        private readonly EffectiveAreaRestrictionService effectiveAreaRestrictionService;
        private readonly PawnRelationsCache pawnRelationsCache;
        private readonly MapScopedThingCacheManager mapScopedThingCacheManager;
        private readonly PrisonerStateCache prisonerStateCache;
        private readonly QuestPartWorkDisabledCache questPartWorkDisabledCache;
        private readonly RegionScopedThingCacheManager regionScopedThingCacheManager;
        private readonly LordsPawnsCache lordsPawnsCache;
        private readonly ExtraFactionCache extraFactionCache;
        private readonly HediffImmunizableCache hediffImmunizableCache;

        public RWPRoot(
            EventBus eventBus,
            RefuelService refuelService,
            ColonyAnimalsService colonyAnimalsService,
            HaulingService haulingService,
            HarvestService harvestService,
            HediffInjuryBleedingService hediffInjuryBleedingService,
            PawnRelationsCache pawnRelationsCache,
            MapScopedThingCacheManager perMapThingCacheManager,
            PrisonerStateCache prisonerStateCache,
            QuestPartWorkDisabledCache questPartWorkDisabledCache,
            RegionScopedThingCacheManager regionScopedThingCacheManager,
            LordsPawnsCache lordsPawnsCache,
            ExtraFactionCache extraFactionCache,
            HediffImmunizableCache hediffImmunizableCache,
            EffectiveAreaRestrictionService effectiveAreaRestrictionService)
        {
            this.eventBus = eventBus;
            this.refuelService = refuelService;
            this.colonyAnimalsService = colonyAnimalsService;
            this.haulingService = haulingService;
            this.harvestService = harvestService;
            this.hediffInjuryBleedingService = hediffInjuryBleedingService;
            this.pawnRelationsCache = pawnRelationsCache;
            this.mapScopedThingCacheManager = perMapThingCacheManager;
            this.prisonerStateCache = prisonerStateCache;
            this.questPartWorkDisabledCache = questPartWorkDisabledCache;
            this.regionScopedThingCacheManager = regionScopedThingCacheManager;
            this.lordsPawnsCache = lordsPawnsCache;
            this.extraFactionCache = extraFactionCache;
            this.hediffImmunizableCache = hediffImmunizableCache;
            this.effectiveAreaRestrictionService = effectiveAreaRestrictionService;
        }

        public ColonyAnimalsService ColonyAnimalsService => this.colonyAnimalsService;

        public RefuelService RefuelService => this.refuelService;

        public HaulingService HaulingService => this.haulingService;

        public PawnRelationsCache PawnRelationsCache => this.pawnRelationsCache;

        public MapScopedThingCacheManager MapScopedThingCacheManager => this.mapScopedThingCacheManager;

        public PrisonerStateCache PrisonerStateCache => this.prisonerStateCache;

        public QuestPartWorkDisabledCache QuestPartWorkDisabledCache => this.questPartWorkDisabledCache;

        public RegionScopedThingCacheManager RegionScopedThingCacheManager => this.regionScopedThingCacheManager;

        public LordsPawnsCache LordsPawnsCache => this.lordsPawnsCache;

        public ExtraFactionCache ExtraFactionCache => this.extraFactionCache;

        public HarvestService HarvestService => this.harvestService;

        public HediffImmunizableCache HediffImmunizableCache => this.hediffImmunizableCache;

        public HediffInjuryBleedingService HediffInjuryBleedingService => this.hediffInjuryBleedingService;

        public EventBus EventBus => this.eventBus;

        public EffectiveAreaRestrictionService EffectiveAreaRestrictionService => this.effectiveAreaRestrictionService;
    }
}
