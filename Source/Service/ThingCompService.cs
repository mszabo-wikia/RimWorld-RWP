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

using RimWorld;
using RWP.Cache;
using RWP.Events;
using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Serve various <see cref="ThingComp"/>-derived data from caches.
    /// </summary>
    public class ThingCompService : IThingLostEventListener
    {
        private readonly ThingCompCache<CompQuality> compQualityCache;
        private readonly ThingCompCache<CompFlickable> compFlickableCache;
        private readonly ThingCompCache<CompSchedule> compScheduleCache;
        private readonly ThingCompCache<CompBreakdownable> compBreakdownCache;

        public ThingCompService(
            ThingCompCache<CompQuality> compQualityCache,
            ThingCompCache<CompFlickable> compFlickableCache,
            ThingCompCache<CompSchedule> compScheduleCache,
            ThingCompCache<CompBreakdownable> compBreakdownCache)
        {
            this.compQualityCache = compQualityCache;
            this.compFlickableCache = compFlickableCache;
            this.compScheduleCache = compScheduleCache;
            this.compBreakdownCache = compBreakdownCache;
        }

        public bool GetQualityCategory(Thing thing, out QualityCategory qualityCategory)
        {
            CompQuality compQuality = this.compQualityCache.TryGetComp(thing);

            qualityCategory = compQuality?.Quality ?? QualityCategory.Normal;
            return compQuality != null;
        }

        public bool WantsToBeOn(Thing thing)
        {
            if (this.compFlickableCache.TryGetComp(thing)?.SwitchIsOn ?? true)
            {
                return this.compScheduleCache.TryGetComp(thing)?.Allowed ?? true;
            }

            return true;
        }

        public bool IsBrokenDown(Thing thing) => this.compBreakdownCache.TryGetComp(thing)?.BrokenDown ?? false;

        public void OnThingLost(Thing thing)
        {
            this.compQualityCache.Remove(thing);
            this.compFlickableCache.Remove(thing);
            this.compQualityCache.Remove(thing);
            this.compScheduleCache.Remove(thing);
        }
    }
}
