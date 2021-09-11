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
using RimWorld.Planet;

namespace RWP.Patches
{
    /// <summary>
    /// Patch <see cref="WorldGenerator.GenerateWorld"/> to reset the dependency injection scope each time a new game world is generated.
    /// </summary>
    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateWorld))]
    public static class WorldGenerator_GenerateWorld_Patch
    {
        public static void Prefix() => RWPMod.ResetGameScope();
    }
}
