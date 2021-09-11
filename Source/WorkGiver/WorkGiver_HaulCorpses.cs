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
using Verse;

namespace RWP.WorkGiver
{
    /// <summary>
    /// Optimized <see cref="RimWorld.WorkGiver_HaulCorpses"/> implementation that only scans non-forbidden corpses on the map.
    /// </summary>
    public class WorkGiver_HaulCorpses : RimWorld.WorkGiver_HaulCorpses
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Corpse);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return from aCorpse in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse) where !aCorpse.IsForbidden(pawn.Faction) select aCorpse;
        }
    }
}
