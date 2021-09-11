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
using RWP.Service;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Transpile <see cref="Hediff_Injury.Tick"/> to lookup whether or not the current injury is bleeding from <see cref="HediffInjuryBleedingService"/>.
    /// </summary>
    [HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.Tick))]
    public static class Hediff_Injury_Tick_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo rootProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPMod), nameof(RWPMod.Root));
            MethodInfo serviceProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPRoot), nameof(RWPRoot.HediffInjuryBleedingService));

            MethodInfo hediffPartProperty = AccessTools.DeclaredPropertyGetter(typeof(Hediff), nameof(Hediff.Part));
            FieldInfo bodyPartDefField = AccessTools.DeclaredField(typeof(BodyPartRecord), nameof(BodyPartRecord.def));
            FieldInfo bodyPartBleedRateField = AccessTools.DeclaredField(typeof(BodyPartDef), nameof(BodyPartDef.bleedRate));

            MethodInfo baseHediffTick = AccessTools.Method(typeof(Hediff), nameof(Hediff.Tick));
            MethodInfo isBleedingMethod = AccessTools.Method(typeof(HediffInjuryBleedingService), nameof(HediffInjuryBleedingService.IsBleeding));

            FieldInfo pawnField = AccessTools.Field(typeof(Hediff), nameof(Hediff.pawn));
            FieldInfo healthTrackerField = AccessTools.Field(typeof(Pawn), nameof(Pawn.health));
            MethodInfo hediffChangedMethod = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_HediffChanged));

            LocalBuilder wasBleeding = generator.DeclareLocal(typeof(bool));

            Label startCheckIfBleeding = generator.DefineLabel();
            Label exitMethod = generator.DefineLabel();

            // If the body part this injury is affecting affecting can never bleed (mechs),
            // there's no point in checking further.
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, hediffPartProperty);
            yield return new CodeInstruction(OpCodes.Brfalse_S, startCheckIfBleeding);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, hediffPartProperty);
            yield return new CodeInstruction(OpCodes.Ldfld, bodyPartDefField);
            yield return new CodeInstruction(OpCodes.Ldfld, bodyPartBleedRateField);
            yield return new CodeInstruction(OpCodes.Ldc_R4, 0.0f);
            yield return new CodeInstruction(OpCodes.Ceq);
            yield return new CodeInstruction(OpCodes.Brfalse_S, startCheckIfBleeding);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, baseHediffTick);
            yield return new CodeInstruction(OpCodes.Ret);

            // Check if the injury is currently bleeding prior to running base hediff tick
            // (which may change injury severity and may therefore stop the bleeding)
            CodeInstruction instruction = new CodeInstruction(OpCodes.Call, rootProperty);
            yield return instruction.WithLabels(startCheckIfBleeding);
            yield return new CodeInstruction(OpCodes.Call, serviceProperty);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Callvirt, isBleedingMethod);
            yield return new CodeInstruction(OpCodes.Stloc, wasBleeding);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, baseHediffTick);

            yield return new CodeInstruction(OpCodes.Call, rootProperty);
            yield return new CodeInstruction(OpCodes.Call, serviceProperty);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Callvirt, isBleedingMethod);
            yield return new CodeInstruction(OpCodes.Ldloc, wasBleeding);
            yield return new CodeInstruction(OpCodes.Beq_S, exitMethod);

            // Bleeding status has changed this tick, so notify the health tracker
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
            yield return new CodeInstruction(OpCodes.Ldfld, healthTrackerField);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Callvirt, hediffChangedMethod);

            CodeInstruction ret = new CodeInstruction(OpCodes.Ret);
            yield return ret.WithLabels(exitMethod);
        }
    }
}
