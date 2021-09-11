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
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace RWP.Patches
{
    public static class CompRefuelable_Patch
    {
        /// <summary>
        /// Unmark this refuelable thing if the player explicitly forbade it from being refueled.
        /// </summary>
        [HarmonyPatch]
        public static class CompRefuelable_OnAllowAutoRefuelChanged_Patch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return typeof(CompRefuelable).GetMethod("<CompGetGizmosExtra>b__42_1", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            public static void Postfix(CompRefuelable __instance)
            {
                if (!__instance.allowAutoRefuel)
                {
                    RWPMod.Root.RefuelService.UnmarkForRefueling(__instance.parent);
                }
            }
        }

        /// <summary>
        /// Mark this refuelable thing as ready to be fueled if it's allowed to be refueled and its fuel level is below target.
        /// </summary>
        [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.CompTick))]
        public static class CompRefuelable_CompTick_Patch
        {
            public static void Postfix(CompRefuelable __instance)
            {
                if (__instance.allowAutoRefuel && __instance.FuelPercentOfTarget < __instance.Props.autoRefuelPercent)
                {
                    RWPMod.Root.RefuelService.MarkAsReadyForRefueling(__instance.parent);
                }
            }
        }
    }
}
