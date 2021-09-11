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
using Verse;

namespace RWP.Patches
{
    public static class Hediff_Patch
    {
        /// <summary>
        /// Reorder visibility checks in <see cref="Hediff.Tick"/> to optimize for the common case where the hediff is already visible.
        /// </summary>
        [HarmonyPatch(typeof(Hediff), nameof(Hediff.Tick))]
        public static class Hediff_Tick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionsList = instructions.ToList();

                MethodInfo hediffVisibleProperty = AccessTools.DeclaredPropertyGetter(typeof(Hediff), nameof(Hediff.Visible));
                FieldInfo hediffVisibleField = AccessTools.Field(typeof(Hediff), "visible");
                Label? hediffNotVisibleLabel = null;

                for (int i = 0; i < instructionsList.Count; i++)
                {
                    CodeInstruction instruction = instructionsList[i];

                    // First check if the hediff isn't already visible...
                    if (instruction.Calls(hediffVisibleProperty) && instructionsList[i + 1].Branches(out hediffNotVisibleLabel) && hediffNotVisibleLabel != null)
                    {
                        yield return new CodeInstruction(OpCodes.Ldfld, hediffVisibleField);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, hediffNotVisibleLabel);
                        i++;
                        continue;
                    }

                    // ...and only then check if it should become visible now.
                    if (instruction.LoadsField(hediffVisibleField) && hediffNotVisibleLabel != null)
                    {
                        yield return new CodeInstruction(OpCodes.Callvirt, hediffVisibleProperty);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, hediffNotVisibleLabel);
                        i++;
                        continue;
                    }

                    yield return instruction;
                }
            }
        }

        /// <summary>
        /// Invalidate the projected age at which an injury will stop bleeding if its severity changes.
        /// </summary>
        [HarmonyPatch(typeof(Hediff), nameof(Hediff.Severity), MethodType.Setter)]
        public static class Hediff_Severity_Patch
        {
            public static void Postfix(Hediff __instance)
            {
                if (__instance is Hediff_Injury injury)
                {
                    RWPMod.Root.HediffInjuryBleedingService.RemoveCachedAge(injury);
                }
            }
        }
    }
}
