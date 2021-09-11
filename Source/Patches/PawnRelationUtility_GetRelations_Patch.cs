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
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RWP.Cache;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Use the active <see cref="PawnRelationsCache"/> instance to serve and store relations between given pawns.
    /// </summary>
    /// <remarks>
    /// This aims to reduce microfreezes from <see cref="WorldPawns.WorldPawnsTick"/> when it cleans the list of known world pawns.
    /// </remarks>
    [HarmonyPatch(typeof(PawnRelationUtility), nameof(PawnRelationUtility.GetRelations))]
    public static class PawnRelationUtility_GetRelations_Patch
    {
        public static IEnumerable<PawnRelationDef> Postfix(IEnumerable<PawnRelationDef> relations, Pawn me, Pawn other)
        {
            var cachedRelations = RWPMod.Root.PawnRelationsCache.GetRelationsBetween(me, other);
            if (cachedRelations != null)
            {
                foreach (PawnRelationDef cachedPawnRelation in cachedRelations)
                {
                    yield return cachedPawnRelation;
                }

                yield break; // cache hit
            }

            var relationsList = relations.ToList();
            RWPMod.Root.PawnRelationsCache.AddRelationsBetween(me, other, relationsList);

            foreach (PawnRelationDef pawnRelation in relationsList)
            {
                yield return pawnRelation;
            }
        }
    }
}
