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
using RWP.Service;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Use <see cref="ThingCompService"/> to determine if a given thing is broken down.
    /// </summary>
    [HarmonyPatch(typeof(BreakdownableUtility), nameof(BreakdownableUtility.IsBrokenDown))]
    public static class BreakdownableUtility_IsBrokenDown_Patch
    {
        public static bool Prefix(Thing t, ref bool __result)
        {
            __result = RWPMod.Root.ThingCompService.IsBrokenDown(t);
            return false;
        }
    }
}
