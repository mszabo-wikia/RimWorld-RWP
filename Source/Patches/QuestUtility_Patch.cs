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
using RimWorld;
using RWP.Cache;
using Verse;

namespace RWP.Patches
{
    public static class QuestUtility_Patch
    {
        /// <summary>
        /// Use the active <see cref="ExtraFactionCache"/> instance to fetch extra (quest-related) factions for a given pawn.
        /// </summary>
        /// <remarks>
        /// This reduces the overhead of the colonist bar, which needs to check if a given colonist is part of a secondary faction.
        /// </remarks>
        [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.GetExtraFaction))]
        public static class QuestUtility_GetExtraFaction_Patch
        {
            public static bool Prefix(
                Pawn p,
                ExtraFactionType extraFactionType,
                Quest forQuest,
                ref Faction __result)
            {
                // Don't use the cache if it is necessary to fetch the faction for a particular quest
                if (forQuest == null)
                {
                    __result = RWPMod.Root.ExtraFactionCache.GetExtraFactionFor(p, extraFactionType);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Use the active <see cref="QuestPartWorkDisabledCache"/> instance to fetch quest parts that disable work types for a given pawn.
        /// </summary>
        [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.GetWorkDisabledQuestPart))]
        public static class QuestUtility_GetWorkDisabledQuestPart_Patch
        {
            public static bool Prefix(Pawn p, ref IEnumerable<QuestPart_WorkDisabled> __result)
            {
                __result = RWPMod.Root.QuestPartWorkDisabledCache.GetQuestPartsFor(p);
                return false;
            }
        }
    }
}
