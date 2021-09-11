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
using RWP.Events;
using Verse;

namespace RWP.Cache
{
    /// <summary>
    /// Factory and manager for <see cref="MapScopedThingCache"/> instances that centralizes initialization and cleanup logic.
    /// </summary>
    public class MapScopedThingCacheManager : IThingLostEventListener
    {
        private readonly IDictionary<string, MapScopedThingCache> cachesByName = new Dictionary<string, MapScopedThingCache>();
        private readonly IEqualityComparer<Thing> thingEqualityComparer;

        public MapScopedThingCacheManager(IEqualityComparer<Thing> thingEqualityComparer) => this.thingEqualityComparer = thingEqualityComparer;

        public MapScopedThingCache GetNamedCache(string name)
        {
            if (this.cachesByName.TryGetValue(name, out MapScopedThingCache cache))
            {
                return cache;
            }

            MapScopedThingCache newCache = new MapScopedThingCache(this.thingEqualityComparer);
            this.cachesByName[name] = newCache;
            return newCache;
        }

        public void OnThingLost(Thing thing)
        {
            foreach (MapScopedThingCache cache in this.cachesByName.Values)
            {
                cache.RemoveThing(thing);
            }
        }

        public void RemoveMap(Map map)
        {
            foreach (MapScopedThingCache cache in this.cachesByName.Values)
            {
                cache.RemoveMap(map);
            }
        }
    }
}
