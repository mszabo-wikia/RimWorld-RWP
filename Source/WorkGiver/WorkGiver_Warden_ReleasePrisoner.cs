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
    /// Optimized <see cref="RimWorld.WorkGiver_Warden_ReleasePrisoner"/> implementation that avoids scanning if no prisoners need to be released.
    /// </summary>
    public class WorkGiver_Warden_ReleasePrisoner : RimWorld.WorkGiver_Warden_ReleasePrisoner
    {
        private readonly PrisonerStateCache cache;

        public WorkGiver_Warden_ReleasePrisoner(PrisonerStateCache cache) => this.cache = cache;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !this.cache.GetPrisonerStateFor(pawn).HasPrisonerForRelease;
        }
    }
}
