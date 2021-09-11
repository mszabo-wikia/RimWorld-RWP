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
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Avoid duplicate <see cref="Pawn.DrawPos"/> calculations in <see cref="Pawn_RotationTracker.Face(Vector3)"/>.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_RotationTracker), nameof(Pawn_RotationTracker.Face), new Type[] { typeof(Vector3) })]
    public static class Pawn_RotationTracker_Face_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionsList = instructions.ToList();

            FieldInfo pawnField = AccessTools.Field(typeof(Pawn_RotationTracker), "pawn");
            MethodInfo getDrawPos = AccessTools.DeclaredPropertyGetter(typeof(Thing), nameof(Thing.DrawPos));
            LocalBuilder curPos = generator.DeclareLocal(typeof(Vector3));

            bool drawPosStored = false;

            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (!instruction.IsLdarg(0) || !instructionsList[i + 2].Calls(getDrawPos))
                {
                    yield return instruction;
                    continue;
                }

                if (drawPosStored)
                {
                    // Already stored - reuse the existing local
                    yield return new CodeInstruction(OpCodes.Ldloc, curPos);
                }
                else
                {
                    // Stash the current draw position in a local for reuse
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Callvirt, getDrawPos);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc, curPos);
                    drawPosStored = true;
                }

                i += 2;
            }
        }
    }
}
