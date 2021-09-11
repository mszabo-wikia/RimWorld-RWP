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
using RimWorld;
using RWP.Cache;
using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Provides managed access to cached refuelable items (turrets and otherwise).
    /// </summary>
    public class RefuelService
    {
        private readonly MapScopedThingCache turretCache;
        private readonly MapScopedThingCache nonTurretCache;

        public RefuelService(MapScopedThingCache turretCache, MapScopedThingCache nonTurretCache)
        {
            this.turretCache = turretCache;
            this.nonTurretCache = nonTurretCache;
        }

        public void MarkAsReadyForRefueling(Thing refuelable)
        {
            // Rearming turrets is a separate job in vanilla
            if (refuelable is Building_Turret)
            {
                this.turretCache.Add(refuelable);
            }
            else
            {
                this.nonTurretCache.Add(refuelable);
            }
        }

        public void UnmarkForRefueling(Thing refuelable)
        {
            if (refuelable is Building_Turret)
            {
                this.turretCache.RemoveThing(refuelable);
            }
            else
            {
                this.nonTurretCache.RemoveThing(refuelable);
            }
        }

        public IEnumerable<Thing> GetRefuelableTurrets(Map map) => this.turretCache.GetThingsForMap(map);

        public IEnumerable<Thing> GetRefuelableNonTurrets(Map map) => this.nonTurretCache.GetThingsForMap(map);
    }
}
