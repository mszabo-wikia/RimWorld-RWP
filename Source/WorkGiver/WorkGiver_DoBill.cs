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

namespace RWP.WorkGiver
{
    /// <summary>
    /// Optimized <see cref="RimWorld.WorkGiver_DoBill"/> implementation that only scans bill givers appropriate for this bill type.
    /// </summary>
    public class WorkGiver_DoBill : RimWorld.WorkGiver_DoBill
    {
        public override ThingRequest PotentialWorkThingRequest => this.def.fixedBillGiverDefs.NullOrEmpty() ? base.PotentialWorkThingRequest : ThingRequest.ForUndefined();

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (this.def.fixedBillGiverDefs.NullOrEmpty())
            {
                return base.ShouldSkip(pawn, forced);
            }

            return !this.PotentialWorkThingsGlobal(pawn).Any();
        }

        /// <summary>
        /// If the bills for this work type are provided by a fixed set of bill givers, return only available instances of those.
        /// </summary>
        /// <param name="pawn">Pawn to fetch bill givers for.</param>
        /// <returns>List of bill givers for this work type.</returns>
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (this.def.fixedBillGiverDefs.NullOrEmpty())
            {
                yield break;
            }

            ListerThings listerThings = pawn.Map.listerThings;

            foreach (ThingDef thingDef in this.def.fixedBillGiverDefs)
            {
                foreach (Thing thingOfDef in listerThings.ThingsOfDef(thingDef))
                {
                    yield return thingOfDef;
                }
            }
        }
    }
}
