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

namespace RWP.Service
{
    /// <summary>
    /// Provides per-pawn caching for effective area restrictions for the duration of a single tick.
    /// </summary>
    public class EffectiveAreaRestrictionService : IPawnLostEventListener, ITickStartedEventListener
    {
        private readonly IDictionary<int, Area> areaRestrictionsByPawn;

        public EffectiveAreaRestrictionService(IEqualityComparer<int> intEqualityComparer)
        {
            this.areaRestrictionsByPawn = new Dictionary<int, Area>(intEqualityComparer);
        }

        public Area GetEffectiveAreaRestriction(Pawn pawn)
        {
            if (this.areaRestrictionsByPawn.TryGetValue(pawn.thingIDNumber, out Area cachedArea))
            {
                return cachedArea;
            }

            Area curRestriction = pawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap;
            this.areaRestrictionsByPawn[pawn.thingIDNumber] = curRestriction;
            return curRestriction;
        }

        public void OnPawnLost(Pawn pawn) => this.areaRestrictionsByPawn.Remove(pawn.thingIDNumber);

        public void OnTickStarted() => this.areaRestrictionsByPawn.Clear();
    }
}
