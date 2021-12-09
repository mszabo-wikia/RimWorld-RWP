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
using RWP.WorkGiver;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Let <see cref="ICustomForcedWorkGiver"/> instances use a different <see cref="ThingRequestGroup"/>
    /// when the player is forcing a job via the UI.
    /// </summary>
    ///
#if RW13
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders")]
#else
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders_NewTmp")]
#endif
    public static class FloatMenuMakerMap_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionsList = instructions.ToList();

            MethodInfo potentialWorkThingRequestMethod = AccessTools.DeclaredPropertyGetter(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.PotentialWorkThingRequest));
            MethodInfo thingRequestAcceptsMethod = AccessTools.Method(typeof(ThingRequest), nameof(ThingRequest.Accepts));
            MethodInfo potentialWorkThingRequestForcedMethod = AccessTools.Method(typeof(ICustomForcedWorkGiver), nameof(ICustomForcedWorkGiver.PotentialWorkThingRequestForced));

            Label continueRegularChecksLabel = generator.DefineLabel();
            Label? shouldSkipCheckLabel = FloatMenuMakerMap_Patch.FindShouldSkipCheckLabel(instructionsList, thingRequestAcceptsMethod);

            LocalBuilder tmpThingRequest = generator.DeclareLocal(typeof(ThingRequest));

            if (shouldSkipCheckLabel == null)
            {
                RWPMod.Logger.Error("Failed to patch FloatMenuMakerMap.AddJobGiverWorkOrders_NewTmp. Some forced jobs may not work as expected.");
                foreach (CodeInstruction instruction in instructionsList)
                {
                    yield return instruction;
                }

                yield break;
            }

            foreach (CodeInstruction instruction in instructionsList)
            {
                if (instruction.Calls(potentialWorkThingRequestMethod))
                {
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(ICustomForcedWorkGiver));
                    yield return new CodeInstruction(OpCodes.Dup);

                    // For regular workgivers, do the same checks as before i.e. consult PotentialWorkThingRequest and PotentialWorkThingsGlobal first.
                    yield return new CodeInstruction(OpCodes.Brfalse_S, continueRegularChecksLabel);
                    yield return new CodeInstruction(OpCodes.Callvirt, potentialWorkThingRequestForcedMethod);
                    yield return new CodeInstruction(OpCodes.Stloc_S, tmpThingRequest.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, tmpThingRequest.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, thingRequestAcceptsMethod);

                    // If the workgiver has a custom thing request group for forced jobs and it accepts this candidate,
                    // ensure we still check ShouldSkip() as well.
                    yield return new CodeInstruction(OpCodes.Brtrue_S, shouldSkipCheckLabel);

                    CodeInstruction pop = new CodeInstruction(OpCodes.Pop);
                    pop.labels.Add(continueRegularChecksLabel);
                    yield return pop;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 9);
                }

                yield return instruction;
            }
        }

        private static Label? FindShouldSkipCheckLabel(IList<CodeInstruction> instructionsList, MethodInfo thingRequestAcceptsMethod)
        {
            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i].Calls(thingRequestAcceptsMethod) && instructionsList[i + 1].Branches(out Label? shouldSkipCheckLabel))
                {
                    return shouldSkipCheckLabel;
                }
            }

            return null;
        }
    }
}
