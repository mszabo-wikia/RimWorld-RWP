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
    /// <summary>
    /// Transpile ListerHaulables to also update <see cref="HaulingService"/> when its internal list changes.
    /// </summary>
    [HarmonyPatch]
    public static class ListerHaulables_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ListerHaulables), "CheckAdd");
            yield return AccessTools.Method(typeof(ListerHaulables), "Check");
            yield return AccessTools.Method(typeof(ListerHaulables), "TryRemove");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo addMethod = AccessTools.Method(typeof(List<Thing>), nameof(List<Thing>.Add));
            MethodInfo removeMethod = AccessTools.Method(typeof(List<Thing>), nameof(List<Thing>.Remove));

            MethodInfo addHaulableMethod = AccessTools.Method(typeof(HaulingService), nameof(HaulingService.AddHaulable));
            MethodInfo removeHaulableMethod = AccessTools.Method(typeof(HaulingService), nameof(HaulingService.RemoveHaulable));

            MethodInfo rootProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPMod), nameof(RWPMod.Root));
            MethodInfo serviceProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPRoot), nameof(RWPRoot.HaulingService));

            FieldInfo mapField = AccessTools.Field(typeof(ListerHaulables), "map");

            var instructionsList = instructions.ToList();

            for (int i = 0; i < instructionsList.Count; i++)
            {
                // Remove any stray Pop instructions after ListerHaulables.Remove() calls
                // since we insert one after our own method call.
                if (instructionsList[i].opcode == OpCodes.Pop && instructionsList[i - 1].Calls(removeMethod))
                {
                    continue;
                }

                yield return instructionsList[i];

                bool callsAddMethod = instructionsList[i].Calls(addMethod);
                bool callsRemoveMethod = instructionsList[i].Calls(removeMethod);

                if (!callsAddMethod && !callsRemoveMethod)
                {
                    continue;
                }

                // Clean up the ListerHaulables.Remove() return value
                if (callsRemoveMethod)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                }

                MethodInfo methodToCall = callsRemoveMethod ? removeHaulableMethod : addHaulableMethod;

                yield return new CodeInstruction(OpCodes.Call, rootProperty);
                yield return new CodeInstruction(OpCodes.Call, serviceProperty);
                yield return new CodeInstruction(OpCodes.Ldarg_1);

                if (callsAddMethod)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, mapField);
                }

                yield return new CodeInstruction(OpCodes.Callvirt, methodToCall);
            }
        }
    }
}
