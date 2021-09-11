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
using Verse.AI;

namespace RWP.Patches
{
    /// <summary>
    /// Replacement for <see cref="Toils_Bed.FailOnBedNoLongerUsable"/> that avoids expensive health/medical checks on non-medical beds.
    /// </summary>
    [HarmonyPatch(typeof(Toils_Bed), nameof(Toils_Bed.FailOnBedNoLongerUsable))]
    public static class Toils_Bed_FailOnBedNoLongerUsable_Patch
    {
        public static bool Prefix(Toil toil, TargetIndex bedIndex)
        {
            toil.FailOnDespawnedOrNull(bedIndex);
            toil.FailOnNonMedicalBedNotOwned(bedIndex);
            toil.FailOn(() =>
            {
                var actor = toil.actor;
                var bed = actor.CurJob.GetTarget(bedIndex).Thing as Building_Bed;

                if (actor.IsColonist && !actor.CurJob.ignoreForbidden && !actor.Downed && bed.IsForbidden(actor))
                {
                    return true;
                }

                if (actor.IsPrisoner != bed.ForPrisoners)
                {
                    return true;
                }

                if (bed.Medical && !HealthAIUtility.ShouldSeekMedicalRest(actor) &&
                    !HealthAIUtility.ShouldSeekMedicalRestUrgent(actor))
                {
                    return true;
                }

                return bed.IsBurning();
            });

            return false;
        }
    }
}
