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
    /// Use <see cref="ThingCompService"/> to determine if a given thing is turned on.
    /// </summary>
    [HarmonyPatch(typeof(FlickUtility), nameof(FlickUtility.WantsToBeOn))]
    public static class FlickUtility_WantsToBeOn_Patch
    {
        public static bool Prefix(Thing t, ref bool __result)
        {
            __result = RWPMod.Root.ThingCompService.WantsToBeOn(t);
            return false;
        }
    }
}
