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
using HarmonyLib;
using RWP.Cache;
using Verse;
using Verse.AI.Group;

namespace RWP.Patches
{
    public static class Lord_Patch
    {
        /// <summary>
        /// Update the active <see cref="LordsPawnsCache"/> instance with a mapping between the given lord and pawn.
        /// </summary>
        [HarmonyPatch(typeof(Lord), nameof(Lord.AddPawn))]
        public static class Lord_AddPawn_Patch
        {
            public static void Postfix(Pawn p, Lord __instance) => RWPMod.Root.LordsPawnsCache.MapLordToPawn(p, __instance);
        }

#if RW13
        /// <summary>
        /// Update the active <see cref="LordsPawnsCache"/> instance with mappings between the given lord and pawns.
        /// </summary>
        [HarmonyPatch(typeof(Lord), nameof(Lord.AddPawns))]
        public static class Lord_AddPawns_Patch
        {
            public static void Postfix(IEnumerable<Pawn> pawns, Lord __instance) => RWPMod.Root.LordsPawnsCache.MapPawnsToLord(pawns, __instance);
        }

        /// <summary>
        /// Remove the mapping between a given lord and pawn from the active <see cref="LordsPawnsCache"/> instance.
        /// </summary>
        [HarmonyPatch(typeof(Lord), nameof(Lord.RemovePawn))]
        public static class Lord_RemovePawn_Patch
        {
            public static void Postfix(Pawn p) => RWPMod.Root.LordsPawnsCache.RemoveLordOfPawn(p);
        }
#else

        /// <summary>
        /// Remove the mapping between a given lord and pawn from the active <see cref="LordsPawnsCache"/> instance.
        /// </summary>
        [HarmonyPatch(typeof(Lord), nameof(Lord.Notify_PawnLost))]
        public static class Lord_Notify_PawnLost_Patch
        {
            public static void Prefix(Pawn pawn) => RWPMod.Root.LordsPawnsCache.RemoveLordOfPawn(pawn);
        }
#endif

        /// <summary>
        /// Initialize the active <see cref="LordsPawnsCache"/> instance with stored mappings between lords and pawns on game initialization.
        /// </summary>
        [HarmonyPatch(typeof(Lord), nameof(Lord.ExposeData))]
        public static class Lord_ExposeData_Patch
        {
            public static void Postfix(Lord __instance) => RWPMod.Root.LordsPawnsCache.MapOwnedPawnsToLord(__instance);
        }
    }
}
