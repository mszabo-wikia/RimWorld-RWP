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
using RWP.Service;
using Verse;
using Verse.AI;

namespace RWP.WorkGiver
{
    /// <summary>
    /// Optimized <see cref="RimWorld.WorkGiver_GrowerHarvest"/> implementation that uses region-based scanning
    /// when looking for harvestable plants and only scans regions known to contain such plants.
    /// </summary>
    public class WorkGiver_GrowerHarvest : RimWorld.WorkGiver_GrowerHarvest
    {
        private const int MaxRegionsToScan = 512;
        private readonly HarvestService service;

        public WorkGiver_GrowerHarvest(HarvestService service) => this.service = service;

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) => Enumerable.Empty<IntVec3>();

        public override Job NonScanJob(Pawn pawn)
        {
            TraverseParms traverseParms = TraverseParms.For(pawn);
            IntVec3? result = null;
            var regionsToScan = this.service.GetRegions();

            RegionTraverser.BreadthFirstTraverse(
                pawn.Position,
                pawn.Map,
                (_, to) => to.Allows(traverseParms, false),
                region => this.FindInRegion(region, pawn, pawn.Map, regionsToScan, out result),
                MaxRegionsToScan);

            if (result != null)
            {
                return this.JobOnCell(pawn, result.Value);
            }

            return null;
        }

        private bool FindInRegion(
            Region region,
            Pawn worker,
            Map map,
            ISet<int> regionsToScan,
            out IntVec3? result)
        {
            regionsToScan.Remove(region.id);

            foreach (IntVec3 plantCell in this.service.GetHarvestablePlantCellsInRegion(region))
            {
                bool isValidHarvestCell = map.zoneManager.ZoneAt(plantCell) is Zone_Growing || plantCell.GetEdifice(map) is Building_PlantGrower;
                if (isValidHarvestCell && this.HasJobOnCell(worker, plantCell))
                {
                    result = plantCell;
                    return true;
                }
            }

            result = null;

            // Abort if all regions with candidate plants have been processed
            return !regionsToScan.Any();
        }
    }
}
