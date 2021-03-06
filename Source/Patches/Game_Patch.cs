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
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace RWP.Patches
{
    public static class Game_Patch
    {
        /// <summary>
        /// Patch <see cref="Game.LoadGame"/> to reset the dependency injection scope each time a saved game is loaded.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        public static class Game_LoadGame_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo exposeSmallComponentsMethod = AccessTools.Method(typeof(Game), "ExposeSmallComponents");
                MethodInfo resetGameScopeMethod = AccessTools.Method(typeof(RWPMod), nameof(RWPMod.ResetGameScope));

                foreach (CodeInstruction instruction in instructions)
                {
                    yield return instruction;

                    // Reset the dependency injection scope after core components such as QuestManager are unserialized
                    // so that they can be used as service dependencies
                    if (instruction.Calls(exposeSmallComponentsMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Call, resetGameScopeMethod);
                    }
                }
            }
        }

        /// <summary>
        /// Clear references to map pawns from caches when a map is deinitialized.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap))]
        public static class Game_DeinitAndRemoveMap_Patch
        {
            public static void Postfix(Map map)
            {
                RWPMod.Root.MapScopedThingCacheManager.RemoveMap(map);
            }
        }
    }
}
