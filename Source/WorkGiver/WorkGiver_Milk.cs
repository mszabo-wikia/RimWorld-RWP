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

namespace RWP.WorkGiver
{
    /// <summary>
    /// Optimized <see cref="RimWorld.WorkGiver_Milk"/> implementation that uses a cache to determine if any animals are ready for milking.
    /// </summary>
    public class WorkGiver_Milk : RimWorld.WorkGiver_Milk
    {
        private readonly ColonyAnimalsService service;

        public WorkGiver_Milk(ColonyAnimalsService service) => this.service = service;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return this.service.GetAnimalsReadyForHarvest<CompMilkable>(pawn.Map).Any();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return this.service.GetAnimalsReadyForHarvest<CompMilkable>(pawn.Map);
        }
    }
}
