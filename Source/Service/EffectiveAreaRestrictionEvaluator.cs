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

using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Provides utilities related to determining the effective allowed area of a given pawn.
    /// </summary>
    public class EffectiveAreaRestrictionEvaluator
    {
        private readonly Area effectiveAreaRestriction;

        public EffectiveAreaRestrictionEvaluator(Area effectiveAreaRestriction) => this.effectiveAreaRestriction = effectiveAreaRestriction;

        /// <summary>
        /// Determine the overlap between a given region and the effective allowed area.
        /// If there is no active area restriction, the entire region is presumed to be overlapping.
        /// </summary>
        public AreaOverlap OverlapWith(Region region) => this.effectiveAreaRestriction != null ? region.OverlapWith(this.effectiveAreaRestriction) : AreaOverlap.Entire;

        /// <summary>
        /// Determine if a given position is reachable under the currently effective area restriction.
        /// </summary>
        /// <param name="overlapWithRestricted">Overlap type of the position's region with the effective area restriction.</param>
        /// <param name="position">Position to determine reachability for.</param>
        public bool CanReach(AreaOverlap overlapWithRestricted, IntVec3 position) => overlapWithRestricted == AreaOverlap.Entire || this.effectiveAreaRestriction?[position] != false;
    }
}
