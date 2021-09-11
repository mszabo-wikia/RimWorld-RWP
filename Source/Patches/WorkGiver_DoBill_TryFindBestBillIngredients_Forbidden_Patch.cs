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
using Verse.AI;

namespace RWP.Patches
{
    /// <summary>
    /// Transpile a lambda expression used in TryFindBestBillIngredients to make it check that a potential ingredient
    /// is suitable for the current bill before running expensive forbidden checks against it.
    /// </summary>
    [HarmonyPatch]
    public static class WorkGiver_DoBill_TryFindBestBillIngredients_Forbidden_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            Type lambdaClass = AccessTools.FirstInner(typeof(WorkGiver_DoBill), innerType => innerType.GetField("billGiver") != null);
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(lambdaClass))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Thing))
                {
                    yield return method;
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();

            MethodInfo canReserveMethod = AccessTools.Method(typeof(ReservationUtility), nameof(ReservationUtility.CanReserve));
            MethodInfo isForbiddenMethod = AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.IsForbidden), new Type[] { typeof(Thing), typeof(Pawn) });

            // Make sure this transpiler does not do anything if the detection logic fails
            int startOfForbiddenCheck = instructionsList.Count;
            int endOfForbiddenCheck = instructionsList.Count;
            int insertPosition = instructionsList.Count;

            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i].Calls(isForbiddenMethod))
                {
                    startOfForbiddenCheck = i - 4;
                    endOfForbiddenCheck = i + 2;
                }

                // We want to move the forbidden check right before the reservation check
                if (instructionsList[i].Calls(canReserveMethod))
                {
                    insertPosition = i - 9;
                }
            }

            // (1) return original instructions up to the forbidden check
            for (int i = 0; i < startOfForbiddenCheck; i++)
            {
                yield return instructionsList[i];
            }

            // (2) return instructions after the forbidden check up to the reservation check
            for (int i = endOfForbiddenCheck; i < insertPosition; i++)
            {
                yield return instructionsList[i];
            }

            // (3) insert the forbidden check before the reservation check
            for (int i = startOfForbiddenCheck; i < endOfForbiddenCheck; i++)
            {
                yield return instructionsList[i];
            }

            // (4) return the rest of the instructions
            for (int i = insertPosition; i < instructionsList.Count; i++)
            {
                yield return instructionsList[i];
            }
        }
    }
}
