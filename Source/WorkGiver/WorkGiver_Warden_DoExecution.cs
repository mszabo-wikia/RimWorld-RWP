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

using RWP.Cache;
using Verse;

namespace RWP.WorkGiver
{
    /// <summary>
    /// Optimized <see cref="RimWorld.WorkGiver_Warden_DoExecution"/> implementation that avoids scanning if no prisoners are scheduled for execution.
    /// </summary>
    public class WorkGiver_Warden_DoExecution : RimWorld.WorkGiver_Warden_DoExecution
    {
        private readonly PrisonerStateCache cache;

        public WorkGiver_Warden_DoExecution(PrisonerStateCache cache) => this.cache = cache;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.WorkTagIsDisabled(WorkTags.Violent) || !this.cache.GetPrisonerStateFor(pawn).HasPrisonerForExecution;
        }
    }
}
