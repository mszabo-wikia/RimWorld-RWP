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

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RWP.Cache;
using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Manage caching for fully-grown, harvestable plants.
    /// </summary>
    public class HarvestService
    {
        private readonly ISet<string> cultivarPlantDefNames;
        private readonly RegionScopedThingCache cache;

        public HarvestService(ISet<string> cultivarPlantDefNames, RegionScopedThingCache cache)
        {
            this.cultivarPlantDefNames = cultivarPlantDefNames;
            this.cache = cache;
        }

        public ISet<int> GetRegions() => this.cache.GetRegions();

        public IEnumerable<IntVec3> GetHarvestablePlantCellsInRegion(Region region) => this.cache.GetThingsInRegion(region).Select(plant => plant.Position);

        public void MarkReadyForHarvest(Plant plant)
        {
            Map map = plant.Map;

            // Don't bother tracking plants that are not sowable or are outside grow zones
            if (this.cultivarPlantDefNames.Contains(plant.def.defName) || map.zoneManager.ZoneAt(plant.Position) is Zone_Growing)
            {
                this.cache.Add(plant, map);
            }
        }

        public void CompleteHarvest(Plant plant) => this.cache.Remove(plant);
    }
}
