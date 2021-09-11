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
using RWP.Service;
using Verse;

namespace RWP.WorkGiver
{
    /// <summary>
    /// Optimized <see cref="RimWorld.WorkGiver_Refuel_Turret"/> implementation that uses a cache to find turrets to be rearmed.
    /// </summary>
    public class WorkGiver_Refuel_Turret : RimWorld.WorkGiver_Refuel_Turret, ICustomForcedWorkGiver
    {
        private readonly RefuelService service;

        public WorkGiver_Refuel_Turret(RefuelService service) => this.service = service;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => this.service.GetRefuelableTurrets(pawn.Map);

        public ThingRequest PotentialWorkThingRequestForced() => ThingRequest.ForGroup(ThingRequestGroup.Refuelable);
    }
}
