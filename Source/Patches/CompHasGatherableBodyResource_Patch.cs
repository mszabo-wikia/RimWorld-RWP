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
using Verse;

namespace RWP.Patches
{
    public static class CompHasGatherableBodyResource_Patch
    {
        /// <summary>
        /// Designate an animal as ready for harvesting if its resource has maxed out.
        /// </summary>
        [HarmonyPatch(typeof(CompHasGatherableBodyResource), nameof(CompHasGatherableBodyResource.CompTick))]
        public static class CompHasGatherableBodyResource_CompTick_Patch
        {
            public static void Postfix(CompHasGatherableBodyResource __instance)
            {
                if (__instance.Fullness == 1f && __instance.parent is Pawn animal)
                {
                    RWPMod.Root.ColonyAnimalsService.MarkAnimalAsReadyForHarvest(animal);
                }
            }
        }

        /// <summary>
        /// Unmark an animal from harvesting if its resources were gathered by a pawn (e.g. milked).
        /// </summary>
        [HarmonyPatch(typeof(CompHasGatherableBodyResource), nameof(CompHasGatherableBodyResource.Gathered))]
        public static class CompHasGatherableBodyResource_Gathered_Patch
        {
            public static void Postfix(CompHasGatherableBodyResource __instance)
            {
                if (__instance.parent is Pawn animal)
                {
                    RWPMod.Root.ColonyAnimalsService.RemoveAnimal(animal);
                }
            }
        }
    }
}
