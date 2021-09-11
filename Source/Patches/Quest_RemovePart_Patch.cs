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

namespace RWP.Patches
{
    /// <summary>
    /// Invalidate caches when a quest part is removed from its parent quest.
    /// </summary>
    [HarmonyPatch(typeof(Quest), nameof(Quest.RemovePart))]
    public static class Quest_RemovePart_Patch
    {
        public static void Postfix(QuestPart part)
        {
            if (part is QuestPart_ExtraFaction extraFactionPart)
            {
                RWPMod.Root.ExtraFactionCache.RemoveEntriesFor(extraFactionPart);
            }
            else if (part is QuestPart_WorkDisabled workDisabledPart)
            {
                RWPMod.Root.QuestPartWorkDisabledCache.RemoveQuestPart(workDisabledPart);
            }
        }
    }
}
