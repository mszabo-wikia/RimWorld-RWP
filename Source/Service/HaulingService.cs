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
using RWP.Cache;
using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Manage caching for items that are ready to be hauled to storage.
    /// </summary>
    public class HaulingService
    {
        private readonly RegionScopedThingCache cache;

        public HaulingService(RegionScopedThingCache cache) => this.cache = cache;

        /// <summary>
        /// Add all haulable items on the given map to the cache.
        /// </summary>
        /// <remarks>
        /// This is used during game initialization to warm the cache after the region grid has been setup.
        /// </remarks>
        /// <param name="map">Map to initialize the cache for.</param>
        public void AddAllHaulables(Map map)
        {
            foreach (Thing candidate in map.listerHaulables.ThingsPotentiallyNeedingHauling())
            {
                this.cache.Add(candidate, map);
            }
        }

        public void AddHaulable(Thing haulable, Map map) => this.cache.Add(haulable, map);

        public void RemoveHaulable(Thing haulable) => this.cache.Remove(haulable);

        public ISet<int> GetRegions() => this.cache.GetRegions();

        public IEnumerable<Thing> GetHaulablesInRegion(Region region) => this.cache.GetThingsInRegion(region);
    }
}
