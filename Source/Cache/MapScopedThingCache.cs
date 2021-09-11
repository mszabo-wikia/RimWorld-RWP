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
using Verse;

namespace RWP.Cache
{
    /// <summary>
    /// Generic per-map cache for <see cref="Thing"/> instances.
    /// </summary>
    public class MapScopedThingCache
    {
        private readonly IDictionary<int, ISet<Thing>> thingsByMap = new Dictionary<int, ISet<Thing>>();
        private readonly IEqualityComparer<Thing> thingEqualityComparer;

        public MapScopedThingCache(IEqualityComparer<Thing> thingEqualityComparer) => this.thingEqualityComparer = thingEqualityComparer;

        public void Add(Thing thing)
        {
            int mapId = thing.Map.uniqueID;
            if (this.thingsByMap.TryGetValue(mapId, out var things))
            {
                things.Add(thing);
            }
            else
            {
                this.thingsByMap[mapId] = new HashSet<Thing>(this.thingEqualityComparer) { thing };
            }
        }

        public IEnumerable<Thing> GetThingsForMap(Map map)
        {
            if (this.thingsByMap.TryGetValue(map.uniqueID, out var things))
            {
                return things;
            }

            return Enumerable.Empty<Thing>();
        }

        public void RemoveThing(Thing thing)
        {
            foreach (var things in this.thingsByMap.Values)
            {
                things.Remove(thing);
            }
        }

        public void RemoveMap(Map map)
        {
            this.thingsByMap.Remove(map.uniqueID);
        }
    }
}
