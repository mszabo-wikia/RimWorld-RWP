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
using RWP.Events;
using Verse;

namespace RWP.Patches
{
    /// <summary>
    /// Patch <see cref="TickManager.DoSingleTick"/> to call event handlers each time a new tick is started.
    /// </summary>
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    public static class TickManager_DoSingleTick_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();

            MethodInfo rootProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPMod), nameof(RWPMod.Root));
            MethodInfo eventBusProperty = AccessTools.DeclaredPropertyGetter(typeof(RWPRoot), nameof(RWPRoot.EventBus));
            MethodInfo onTickStartedEvent = AccessTools.Method(typeof(EventBus), nameof(EventBus.OnTickStarted));

            FieldInfo tickListNormalField = AccessTools.Field(typeof(TickManager), "tickListNormal");
            MethodInfo tickMethod = AccessTools.Method(typeof(TickList), nameof(TickList.Tick));

            for (int i = 0; i < instructionsList.Count; i++)
            {
                // Insert our event handler call right before processing the regular tick list
                if (instructionsList[i].IsLdarg(0) &&
                    instructionsList[i + 1].LoadsField(tickListNormalField) &&
                    instructionsList[i + 2].Calls(tickMethod))
                {
                    yield return new CodeInstruction(OpCodes.Call, rootProperty);
                    yield return new CodeInstruction(OpCodes.Call, eventBusProperty);
                    yield return new CodeInstruction(OpCodes.Call, onTickStartedEvent);
                }

                yield return instructionsList[i];
            }
        }
    }
}
