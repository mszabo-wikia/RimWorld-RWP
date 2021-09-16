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
using Verse.AI.Group;

namespace RWP.Patches
{
    /// <summary>
    /// Remove cached mappings referencing a given lord when it is removed from the game.
    /// </summary>
    [HarmonyPatch(typeof(LordManager), nameof(LordManager.RemoveLord))]
    public static class LordManager_RemoveLord_Patch
    {
        public static void Prefix(Lord oldLord) => RWPMod.Root.LordsPawnsCache.RemoveLord(oldLord);
    }
}
