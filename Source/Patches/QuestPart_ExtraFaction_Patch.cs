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

using HarmonyLib;
using RimWorld;
using RWP.Cache;
using Verse;

namespace RWP.Patches
{
    public static class QuestPart_ExtraFaction_Patch
    {
        /// <summary>
        /// Update the active <see cref="ExtraFactionCache"/> instance if a pawn is removed from a quest.
        /// </summary>
        [HarmonyPatch(typeof(QuestPart_ExtraFaction), nameof(QuestPart_ExtraFaction.Notify_QuestSignalReceived))]
        public static class QuestPart_ExtraFaction_Notify_QuestSignalReceived_Patch
        {
            public static void Postfix(Signal signal, QuestPart_ExtraFaction __instance)
            {
                if (signal.tag == __instance.inSignalRemovePawn)
                {
                    RWPMod.Root.ExtraFactionCache.RemoveEntriesFor(__instance);
                }
            }
        }

        /// <summary>
        /// Invalidate caches if a pawn substitutes another pawn in a quest.
        /// </summary>
        [HarmonyPatch(typeof(QuestPart_ExtraFaction), nameof(QuestPart_ExtraFaction.ReplacePawnReferences))]
        public static class QuestPart_ExtraFaction_ReplacePawnReferences_Patch
        {
            public static void Postfix(Pawn replace, Pawn with)
            {
                RWPMod.Root.ExtraFactionCache.RemovePawn(replace);
                RWPMod.Root.ExtraFactionCache.RemovePawn(with);
            }
        }
    }
}
