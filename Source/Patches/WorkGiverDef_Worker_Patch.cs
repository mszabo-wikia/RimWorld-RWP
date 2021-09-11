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

namespace RWP.Patches
{
    /// <summary>
    /// Transpile the WorkGiver instantiator property to allow for dependency injection into WorkGivers.
    /// </summary>
    [HarmonyPatch(typeof(WorkGiverDef), nameof(WorkGiverDef.Worker), MethodType.Getter)]
    public static class WorkGiverDef_Worker_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionsList = instructions.ToList();

            MethodInfo getServiceMethod = AccessTools.Method(typeof(RWPMod), nameof(RWPMod.GetService));
            FieldInfo giverClassField = AccessTools.Field(typeof(WorkGiverDef), nameof(WorkGiverDef.giverClass));

            Label castClassLabel = generator.DefineLabel();

            for (int i = 0; i < instructionsList.Count; i++)
            {
                // Insert a service container lookup before the default activation logic, but continue as normal
                // if this work giver is not registered in the service container.
                if (i < instructionsList.Count - 1 && instructionsList[i + 1].LoadsField(giverClassField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, giverClassField);
                    yield return new CodeInstruction(OpCodes.Call, getServiceMethod);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, castClassLabel);
                    yield return new CodeInstruction(OpCodes.Pop);
                }

                if (instructionsList[i].opcode == OpCodes.Castclass)
                {
                    instructionsList[i].labels = instructionsList[i].labels ?? new List<Label>();
                    instructionsList[i].labels.Add(castClassLabel);
                }

                yield return instructionsList[i];
            }
        }
    }
}
