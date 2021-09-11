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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RWP.Service;
using Verse;

namespace RWP.Patches
{
    [HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.InAllowedArea))]
    public static class ForbidUtility_InAllowedArea_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();

            FieldInfo playerSettingsField = AccessTools.Field(typeof(Pawn), nameof(Pawn.playerSettings));
            MethodInfo rootProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPMod), nameof(RWPMod.Root));
            MethodInfo serviceProperty = AccessTools.DeclaredPropertyGetter(
                typeof(RWPRoot),
                nameof(RWPRoot.EffectiveAreaRestrictionService));
            MethodInfo getEffectiveAreaRestrictionMethod = AccessTools.Method(
                typeof(EffectiveAreaRestrictionService),
                nameof(EffectiveAreaRestrictionService.GetEffectiveAreaRestriction));

            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i].IsLdarg(1) && instructionsList[i + 1].LoadsField(playerSettingsField))
                {
                    yield return new CodeInstruction(OpCodes.Call, rootProperty);
                    yield return new CodeInstruction(OpCodes.Call, serviceProperty);
                    yield return instructionsList[i];
                    yield return new CodeInstruction(OpCodes.Callvirt, getEffectiveAreaRestrictionMethod);

                    i += 5;
                }
                else
                {
                    yield return instructionsList[i];
                }
            }
        }
    }
}
