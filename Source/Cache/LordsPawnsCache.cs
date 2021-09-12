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
using Verse;
using Verse.AI.Group;

namespace RWP.Cache
{
    /// <summary>
    /// Per-pawn cache for lord (group AI) associations.
    /// </summary>
    public class LordsPawnsCache
    {
        private readonly IDictionary<int, Lord> lordsByPawn;

        public LordsPawnsCache(IEqualityComparer<int> intEqualityComparer) => this.lordsByPawn = new Dictionary<int, Lord>(intEqualityComparer);

        /// <summary>
        /// Add all pawns currently owned by the given Lord to the cache.
        /// If a mapping already exists for a given pawn, the existing mapping will be preserved.
        /// NOTE: This is invoked on loading save game data, but also when exporting data during save.
        /// </summary>
        public void MapOwnedPawnsToLord(Lord lord) => this.MapPawnsToLord(lord.ownedPawns, lord);

        /// <summary>
        /// Map the given set of pawns to the specified Lord.
        /// </summary>
        public void MapPawnsToLord(IEnumerable<Pawn> pawns, Lord lord)
        {
            foreach (Pawn pawn in pawns)
            {
                this.lordsByPawn[pawn.thingIDNumber] = lord;
            }
        }

        /// <summary>
        /// Register the given Lord as the lord of the given Pawn.
        /// If the pawn is already associated with a different lord, or a mapping already exists for this pawn,
        /// no new mapping will be created.
        /// </summary>
        public void MapLordToPawn(Pawn pawn, Lord lord)
        {
            if (pawn.GetLord() != null)
            {
                return;
            }

            this.lordsByPawn[pawn.thingIDNumber] = lord;
        }

        /// <summary>
        /// Get the lord of the given pawn, or null if no lord is associated with this pawn.
        /// </summary>
        public Lord GetLordOfPawn(Pawn pawn)
        {
            if (this.lordsByPawn.TryGetValue(pawn.thingIDNumber, out Lord cachedLord))
            {
                return cachedLord;
            }

            return null;
        }

        /// <summary>
        /// Remove any cached mapping between the given pawn and lord.
        /// If the pawn is associated with a different lord, the existing mapping is preserved.
        /// </summary>
        public void RemoveLordOfPawn(Pawn pawn) => this.lordsByPawn.Remove(pawn.thingIDNumber);
    }
}
