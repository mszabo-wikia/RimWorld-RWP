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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Transpile a lambda expression used in TryFindBestBillIngredients to make it check that a potential ingredient
    /// is suitable for the current bill before running expensive reachability checks against it.
    /// </summary>
    [HarmonyPatch]
    public static class WorkGiver_DoBill_TryFindBestBillIngredients_ReachabilityCheck_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            Type lambdaClass = AccessTools.FirstInner(typeof(WorkGiver_DoBill), innerType => innerType.GetField("entryCondition") != null);
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(lambdaClass))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Region))
                {
                    yield return method;
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();

            FieldInfo newRelevantThingsField = AccessTools.Field(typeof(WorkGiver_DoBill), "newRelevantThings");
            MethodInfo thingFromRegionListerReachableMethod = AccessTools.Method(typeof(ReachabilityWithinRegion), nameof(ReachabilityWithinRegion.ThingFromRegionListerReachable));

            // Not the method we're targeting
            if (!instructions.Any(instruction => instruction.Calls(thingFromRegionListerReachableMethod)))
            {
                foreach (CodeInstruction instruction in instructions)
                {
                    yield return instruction;
                }

                yield break;
            }

            // Make sure this transpiler does not do anything if the detection logic fails
            int startOfThingReachabilityCondition = instructionsList.Count;
            int endOfThingReachabilityCondition = instructionsList.Count;
            int endOfConditionList = instructionsList.Count;

            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i].Calls(thingFromRegionListerReachableMethod))
                {
                    startOfThingReachabilityCondition = i - 5;
                    endOfThingReachabilityCondition = i + 2;
                }

                // First instruction inside the condition block
                if (instructionsList[i].LoadsField(newRelevantThingsField))
                {
                    endOfConditionList = i;
                    break;
                }
            }

            // (1) return original instructions up to the reachability check
            for (int i = 0; i < startOfThingReachabilityCondition; i++)
            {
                yield return instructionsList[i];
            }

            // (2) return instructions for the rest of the conditions
            for (int i = endOfThingReachabilityCondition; i < endOfConditionList; i++)
            {
                yield return instructionsList[i];
            }

            // (3) insert reachability check condition instructions at the end of the condition list
            for (int i = startOfThingReachabilityCondition; i < endOfThingReachabilityCondition; i++)
            {
                yield return instructionsList[i];
            }

            // (4) return the rest of the original instructions
            for (int i = endOfConditionList; i < instructionsList.Count; i++)
            {
                yield return instructionsList[i];
            }
        }
    }
}
