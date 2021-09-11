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
    /// Cache that stores items per region.
    /// </summary>
    public class RegionScopedThingCache
    {
        private readonly IDictionary<int, ISet<Thing>> thingsByRegion = new Dictionary<int, ISet<Thing>>();
        private readonly IDictionary<int, int> regionsByThing = new Dictionary<int, int>();
        private readonly IEqualityComparer<Thing> thingEqualityComparer;

        public RegionScopedThingCache(IEqualityComparer<Thing> thingEqualityComparer) => this.thingEqualityComparer = thingEqualityComparer;

        /// <summary>
        /// Add a new item to the cache.
        /// </summary>
        /// <param name="thing">Item to be added.</param>
        /// <param name="map">The map the item belongs to.</param>
        public void Add(Thing thing, Map map)
        {
            int thingId = thing.thingIDNumber;

            // Already added
            if (this.regionsByThing.ContainsKey(thingId))
            {
                return;
            }

            Region region = this.GetRegionFor(thing, map);

            if (region == null)
            {
                return;
            }

            if (this.thingsByRegion.TryGetValue(region.id, out var contents))
            {
                contents.Add(thing);
            }
            else
            {
                this.thingsByRegion[region.id] = new HashSet<Thing>(this.thingEqualityComparer) { thing };
            }

            this.regionsByThing[thingId] = region.id;
        }

        /// <summary>
        /// Get the list of regions that this cache is storing items for.
        /// </summary>
        /// <returns>Set of region IDs this cache contains items for.</returns>
        public ISet<int> GetRegions() => new HashSet<int>(this.thingsByRegion.Keys);

        /// <summary>
        /// Get list of items for the given region.
        /// </summary>
        /// <param name="region">Region to fetch items for.</param>
        /// <returns>List of items for the given region, or an empty enumerable if none exist.</returns>
        public IEnumerable<Thing> GetThingsInRegion(Region region)
        {
            if (this.thingsByRegion.TryGetValue(region.id, out var contents))
            {
                return contents;
            }

            return Enumerable.Empty<Thing>();
        }

        /// <summary>
        /// Update the region associated with a given item.
        /// </summary>
        /// <param name="thing">Item to update region for.</param>
        /// <param name="map">Map associated with the item.</param>
        public void UpdateRegionFor(Thing thing, Map map)
        {
            if (!this.regionsByThing.TryGetValue(thing.thingIDNumber, out int oldRegionId))
            {
                return;
            }

            var thingsInOldRegion = this.thingsByRegion[oldRegionId];
            thingsInOldRegion.Remove(thing);

            // This likely means that the region is no longer valid; clear out the stale entry.
            if (thingsInOldRegion.Count == 0)
            {
                this.thingsByRegion.Remove(oldRegionId);
            }

            this.regionsByThing.Remove(thing.thingIDNumber);

            this.Add(thing, map);
        }

        /// <summary>
        /// Remove a region and associated things when its parent map is deinitialized.
        /// </summary>
        /// <param name="region">Region to remove.</param>
        public void RemoveRegion(Region region)
        {
            if (!this.thingsByRegion.TryGetValue(region.id, out var contents))
            {
                return;
            }

            foreach (var thing in contents)
            {
                this.regionsByThing.Remove(thing.thingIDNumber);
            }

            this.thingsByRegion.Remove(region.id);
        }

        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="thing">Item to remove.</param>
        public void Remove(Thing thing)
        {
            int thingId = thing.thingIDNumber;
            if (this.regionsByThing.TryGetValue(thingId, out int regionId))
            {
                this.thingsByRegion[regionId].Remove(thing);
                this.regionsByThing.Remove(thingId);
            }
        }

        private Region GetRegionFor(Thing thing, Map map)
        {
            // Avoid premature access to the region grid during game initialization
            if (!map.regionAndRoomUpdater.Enabled)
            {
                return null;
            }

            Region region = map.regionGrid.GetValidRegionAt(thing.Position);
            if (region == null || (region.type & RegionType.Set_Passable) == RegionType.None)
            {
                return null;
            }

            return region;
        }
    }
}
