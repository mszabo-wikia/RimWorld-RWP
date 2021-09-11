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
using RWP.Cache;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Use <see cref="HediffImmunizableCache"/> to determine whether a given <see cref="Hediff"/>
    /// can result in immunity gain instead of iterating over its comps every time.
    /// </summary>
    [HarmonyPatch(typeof(HediffDef), nameof(HediffDef.PossibleToDevelopImmunityNaturally))]
    public static class HediffDef_PossibleToDevelopImmunityNaturally_Patch
    {
        public static bool Prefix(HediffDef __instance, ref bool __result)
        {
            __result = RWPMod.Root.HediffImmunizableCache.IsHediffImmunizable(__instance);
            return false;
        }
    }
}
