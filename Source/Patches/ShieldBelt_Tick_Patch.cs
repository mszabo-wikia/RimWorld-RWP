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
    /// Patch <see cref="ShieldBelt.Tick"/> to avoid calculating energy gain per tick on the fly if the shield is fully charged.
    /// </summary>
    [HarmonyPatch(typeof(ShieldBelt), nameof(ShieldBelt.Tick))]
    public static class ShieldBelt_Tick_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo energyField = AccessTools.Field(typeof(ShieldBelt), "energy");
            MethodInfo maxEnergyProp = AccessTools.DeclaredPropertyGetter(typeof(ShieldBelt), "EnergyMax");
            MethodInfo energyGainProp = AccessTools.DeclaredPropertyGetter(typeof(ShieldBelt), "EnergyGainPerTick");

            LocalBuilder curMaxEnergy = generator.DeclareLocal(typeof(float));

            var instructionsList = instructions.ToList();
            CodeInstruction lastInstruction = instructionsList.Last();
            Label lastLabel = lastInstruction.labels.First();

            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i + 2].LoadsField(energyField))
                {
                    // Return early if the shield belt is already at maximum energy
                    // to avoid having to recalculate per-tick energy gain.
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, energyField);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, maxEnergyProp);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc, curMaxEnergy);
                    yield return new CodeInstruction(OpCodes.Ceq);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, lastLabel);

                    // Shield is not at maximum charge so apply the per-tick energy gain.
                    // Use the already-calculated maximum energy value in comparisons instead of recalculating it.
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, energyField);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, energyGainProp);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Stfld, energyField);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, energyField);
                    yield return new CodeInstruction(OpCodes.Ldloc, curMaxEnergy);
                    yield return new CodeInstruction(OpCodes.Ble_Un_S, lastLabel);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc, curMaxEnergy);
                    yield return new CodeInstruction(OpCodes.Stfld, energyField);

                    yield return lastInstruction;
                    yield break;
                }

                yield return instructionsList[i];
            }
        }
    }
}
