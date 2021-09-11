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

using System;
using HarmonyLib;
using RimWorld;
using RWP.Cache;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Invalidate <see cref="PawnRelationsCache"/> when direct relations of two pawns change.
    /// </summary>
    public static class Pawn_RelationsTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.AddDirectRelation))]
        public static class AddDirectRelation
        {
            public static void Postfix(Pawn ___pawn, Pawn otherPawn) => RWPMod.Root.PawnRelationsCache.ClearRelationsBetween(___pawn, otherPawn);
        }

        [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.RemoveDirectRelation), new Type[] { typeof(DirectPawnRelation) })]
        public static class RemoveDirectRelation
        {
            public static void Postfix(Pawn ___pawn, DirectPawnRelation relation) => RWPMod.Root.PawnRelationsCache.ClearRelationsBetween(___pawn, relation.otherPawn);
        }
    }
}
