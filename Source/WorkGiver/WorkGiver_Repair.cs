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
    /// Optimized <see cref="RimWorld.WorkGiver_Repair"/> implementation that avoids scanning repairable buildings outside the home area.
    /// </summary>
    public class WorkGiver_Repair : RimWorld.WorkGiver_Repair
    {
        private readonly Faction playerFaction;

        public WorkGiver_Repair(Faction playerFaction) => this.playerFaction = playerFaction;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !this.GetRepairableBuildingsInHomeArea(pawn).Any();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return this.GetRepairableBuildingsInHomeArea(pawn);
        }

        private IEnumerable<Thing> GetRepairableBuildingsInHomeArea(Pawn pawn)
        {
            var buildings = pawn.Map.listerBuildingsRepairable.RepairableBuildings(pawn.Faction);
            if (pawn.Faction == this.playerFaction)
            {
                Area_Home homeArea = pawn.Map.areaManager.Home;
                return from building in buildings where homeArea[building.Position] select building;
            }

            return buildings;
        }
    }
}
