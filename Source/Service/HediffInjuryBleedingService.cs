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
using UnityEngine;
using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Cache the projected age in ticks at which a given <see cref="Hediff_Injury"/> will stop bleeding.
    /// </summary>
    public class HediffInjuryBleedingService
    {
        private readonly RWP.Cache.LRUCache<int, int> ageTicksToStopBleedingById;

        public HediffInjuryBleedingService(IEqualityComparer<int> intEqualityComparer)
        {
            this.ageTicksToStopBleedingById = new RWP.Cache.LRUCache<int, int>(2048, intEqualityComparer);
        }

        public bool IsBleeding(Hediff_Injury injury)
        {
            int cachedAgeTicks = this.ageTicksToStopBleedingById.Get(injury.loadID);
            if (cachedAgeTicks != 0)
            {
                return injury.ageTicks < cachedAgeTicks;
            }

            // Vanilla logic to calculate the projected tick at which the injury will stop bleeding
            // given its severity
            float severity = injury.Severity;
            float t = Mathf.Clamp(Mathf.InverseLerp(1f, 30f, severity), 0f, 1f);
            int ageTicksToStopBleeding = 90000 + Mathf.RoundToInt(Mathf.Lerp(0f, 90000f, t));

            this.ageTicksToStopBleedingById.Add(injury.loadID, ageTicksToStopBleeding);
            return injury.ageTicks < ageTicksToStopBleeding;
        }

        public void RemoveCachedAge(Hediff_Injury injury) => this.ageTicksToStopBleedingById.Remove(injury.loadID);
    }
}
