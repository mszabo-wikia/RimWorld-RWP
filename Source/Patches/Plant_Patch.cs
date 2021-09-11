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

namespace RWP.Patches
{
    /// <summary>
    /// Manage caching for harvestable plants via <see cref="HarvestService"/>.
    /// </summary>
    public static class Plant_Patch
    {
        [HarmonyPatch(typeof(Plant), nameof(Plant.TickLong))]
        public static class Plant_TickLong_Patch
        {
            public static void Postfix(Plant __instance)
            {
                if (__instance.LifeStage == PlantLifeStage.Mature)
                {
                    RWPMod.Root.HarvestService.MarkReadyForHarvest(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
        public static class Plant_PlantCollected_Patch
        {
            public static void Postfix(Plant __instance) => RWPMod.Root.HarvestService.CompleteHarvest(__instance);
        }
    }
}
