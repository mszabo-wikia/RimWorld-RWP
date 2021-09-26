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
using RWP.Service;
using Verse;
using Verse.AI;

namespace RWP.WorkGiver
{
    /// <summary>
    /// <see cref="RimWorld.WorkGiver_HaulGeneral"/> implementation that uses BFS across regions to find haulable items instead of a global search.
    /// </summary>
    public class WorkGiver_HaulGeneral : RimWorld.WorkGiver_HaulGeneral, ICustomForcedWorkGiver
    {
        private const int MaxRegionsToScan = 32068;
        private readonly EffectiveAreaRestrictionEvaluatorFactory effectiveAreaRestrictionEvaluatorFactory;
        private readonly HaulingService haulingService;

        public WorkGiver_HaulGeneral(
            EffectiveAreaRestrictionEvaluatorFactory effectiveAreaRestrictionEvaluatorFactory,
            HaulingService haulingService)
        {
            this.effectiveAreaRestrictionEvaluatorFactory = effectiveAreaRestrictionEvaluatorFactory;
            this.haulingService = haulingService;
        }

        public ThingRequest PotentialWorkThingRequestForced() => ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => Enumerable.Empty<Thing>();

        public override Job NonScanJob(Pawn pawn)
        {
            IntVec3 startFrom = pawn.Position;
            Map curMap = pawn.Map;
            EffectiveAreaRestrictionEvaluator effectiveAreaRestrictionEvaluator = this.effectiveAreaRestrictionEvaluatorFactory.NewEffectiveAreaRestrictionEvaluator(pawn);

            TraverseParms traverseParms = TraverseParms.For(pawn);
            var regionsToScan = this.haulingService.GetRegions();
            Thing result = null;

            RegionTraverser.BreadthFirstTraverse(
                startFrom,
                curMap,
                (from, to) => to.Allows(traverseParms, false),
                region => this.FindInRegion(region, pawn, effectiveAreaRestrictionEvaluator, regionsToScan, out result),
                MaxRegionsToScan);

            if (result != null)
            {
                return this.JobOnThing(pawn, result);
            }

            return null;
        }

        private bool FindInRegion(
            Region region,
            Pawn worker,
            EffectiveAreaRestrictionEvaluator effectiveAreaRestrictionEvaluator,
            ISet<int> regionsToScan,
            out Thing result)
        {
            regionsToScan.Remove(region.id);

            AreaOverlap overlapWithRestricted = effectiveAreaRestrictionEvaluator.OverlapWith(region);

            if (overlapWithRestricted == AreaOverlap.None)
            {
                result = null;
                return !regionsToScan.Any();
            }

            foreach (Thing thing in this.haulingService.GetHaulablesInRegion(region))
            {
                if (effectiveAreaRestrictionEvaluator.CanReach(overlapWithRestricted, thing.Position) && this.HasJobOnThing(worker, thing))
                {
                    result = thing;
                    return true;
                }
            }

            result = null;
            return !regionsToScan.Any();
        }
    }
}
