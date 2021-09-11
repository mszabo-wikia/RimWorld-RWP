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
using Verse;

namespace RWP.Cache
{
    /// <summary>
    ///  Factory and manager for <see cref="RegionScopedThingCache"/> instances that centralizes initialization and cleanup logic.
    /// </summary>
    public class RegionScopedThingCacheManager
    {
        private readonly IDictionary<string, RegionScopedThingCache> cachesByName = new Dictionary<string, RegionScopedThingCache>();
        private readonly IEqualityComparer<Thing> thingEqualityComparer;

        public RegionScopedThingCacheManager(IEqualityComparer<Thing> thingEqualityComparer)
        {
            this.thingEqualityComparer = thingEqualityComparer;
        }

        public RegionScopedThingCache GetNamedCache(string name)
        {
            if (this.cachesByName.TryGetValue(name, out RegionScopedThingCache cache))
            {
                return cache;
            }

            RegionScopedThingCache newCache = new RegionScopedThingCache(this.thingEqualityComparer);
            this.cachesByName[name] = newCache;
            return newCache;
        }

        public void UpdateRegionFor(Thing thing, Map map)
        {
            foreach (RegionScopedThingCache cache in this.cachesByName.Values)
            {
                cache.UpdateRegionFor(thing, map);
            }
        }

        public void RemoveRegion(Region region)
        {
            foreach (RegionScopedThingCache cache in this.cachesByName.Values)
            {
                cache.RemoveRegion(region);
            }
        }

        public void Remove(Thing thing)
        {
            foreach (RegionScopedThingCache cache in this.cachesByName.Values)
            {
                cache.Remove(thing);
            }
        }
    }
}
