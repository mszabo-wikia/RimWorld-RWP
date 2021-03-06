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
    /// Optimized <see cref="RimWorld.WorkGiver_TendOther"/> implementation that only scans patients who are in bed.
    /// </summary>
    public class WorkGiver_TendOther : RimWorld.WorkGiver_TendOther
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !this.GetPawnsReadyToBeTended(pawn).Any();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return this.GetPawnsReadyToBeTended(pawn);
        }

        private IEnumerable<Thing> GetPawnsReadyToBeTended(Pawn pawn)
        {
            return from patient in pawn.Map.mapPawns.AllPawnsSpawned
                where patient.health.hediffSet.hediffs.Any() && GoodLayingStatusForTend(patient, pawn)
                select patient;
        }
    }
}
