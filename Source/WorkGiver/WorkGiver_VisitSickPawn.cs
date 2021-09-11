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
    /// Optimized VisitSickPawn implementation that limits scanning to pawns that are awake and in bed.
    /// </summary>
    internal class WorkGiver_VisitSickPawn : RimWorld.WorkGiver_VisitSickPawn
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !InteractionUtility.CanInitiateInteraction(pawn) || !this.GetPotentiallyVisitablePawns(pawn).Any();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return this.GetPotentiallyVisitablePawns(pawn);
        }

        private IEnumerable<Thing> GetPotentiallyVisitablePawns(Pawn doctor)
        {
            return from patient in doctor.Map.mapPawns.SpawnedPawnsInFaction(doctor.Faction)
                where patient.InBed() && patient.Awake()
                select patient;
        }
    }
}
