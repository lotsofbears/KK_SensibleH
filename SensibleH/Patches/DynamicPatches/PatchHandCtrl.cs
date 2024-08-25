using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;
using KK_SensibleH.Caress;
using static KK_SensibleH.SensibleH;
using KKAPI.MainGame;
using ADV.Commands.Base;
using NodeCanvas.Tasks.Actions;

namespace KK_SensibleH.Patches.DynamicPatches
{
    class PatchHandCtrl
    {
        public static bool GetMouseButtonUp(int button) => false;
        public static bool GetMouseButton(int button) => true;

        /// <summary>
        /// We substitute original mouse button up with fake that returns "false".
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
        public static IEnumerable<CodeInstruction> ClickActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var first = false;
            var done = false;
            var methodName = nameof(Input.GetMouseButtonUp);
            var breaks = 0;
            var second = 0;
            foreach (var code in instructions)
            {
                if (!first && code.opcode == OpCodes.Call &&
                    code.operand is MethodInfo method &&
                    method.Name.Equals(methodName))
                {
                    //SensibleH.Logger.LogDebug($"Pile:HandCtrl.ClickAction:Breaks[{breaks}][{code.opcode}][{code.operand}]");
                    first = true;
                    var newMethod = AccessTools.Method(typeof(PatchHandCtrl), nameof(GetMouseButtonUp)); // "GetMouseButtonUp");
                    yield return new CodeInstruction(OpCodes.Call, newMethod);
                }
#if KK
                else if (!done && breaks == 12)
#else
                else if (!done && breaks == 10)
#endif
                {
                    //SensibleH.Logger.LogDebug($"Pile:HandCtrl.ClickAction:Breaks[{breaks}][{code.opcode}][{code.operand}]");

                    code.opcode = OpCodes.Nop;
                    second++;
                    if (second == 3)
                    {
                        done = true;
                    }
                    yield return code;
                }
                else
                {
                    if (!done && code.opcode == OpCodes.Br)
                    {
                        //SensibleH.Logger.LogDebug($"Pile:HandCtrl.ClickAction:Breaks[{breaks}][{code.opcode}][{code.operand}]");
                        breaks++;
                    }
                    yield return code;
                }
            }
        }

        /// <summary>
        /// We feed the game our vector of movement to add excitement from it. (and ask to reset it also).
        /// We substitute mouse button press with the fake that returns "true".
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var fakeDragLength = AccessTools.Field(typeof(MoMiController), name: "FakeDragLength");
            var first = false;
            var second = 0;
            var code = new List<CodeInstruction>(instructions);
            for (var i = 0; i < code.Count; i++)
            {
                if (!first && code[i].opcode == OpCodes.Ldflda &&
                    code[i].operand.ToString().Contains("calcDragLength"))
                {
                    first = true;
                    code[i].opcode = OpCodes.Ldsfld;
                    code[i].operand = fakeDragLength;
                    code[i + 1].opcode = OpCodes.Nop;// OpCodes.Ldc_R4;
                    //code[i + 1].operand = 3f;
                    code[i + 2].opcode = OpCodes.Nop;// OpCodes.Call;
                    //code[i + 2].operand = AccessTools.Method(typeof(Vector2), "op_Multiply", new Type[]{
                    //    typeof(Vector2),
                    //    typeof(float)
                    //});
                    code[i + 3].opcode = OpCodes.Stfld;
                    code[i + 3].operand = AccessTools.Field(typeof(HandCtrl), nameof(HandCtrl.calcDragLength));// name: "calcDragLength");
                    code[i + 4].opcode = OpCodes.Call;
                    code[i + 4].operand = AccessTools.FirstMethod(typeof(Vector2), method => method.Name.Equals("get_zero"));
                    code[i + 5].opcode = OpCodes.Stsfld;
                    code[i + 5].operand = fakeDragLength;
                    //code[i].opcode = OpCodes.Ldsfld;
                    //code[i].operand = fakeDragLength;
                    //code[i + 1].opcode = OpCodes.Nop;
                    //code[i + 2].opcode = OpCodes.Nop;
                    //code[i + 3].opcode = OpCodes.Stfld;
                    //code[i + 3].operand = AccessTools.Field(typeof(HandCtrl), nameof(HandCtrl.calcDragLength));// name: "calcDragLength");
                    //code[i + 4].opcode = OpCodes.Nop;
                    //code[i + 5].opcode = OpCodes.Nop;
                }
                else if (second != 2 && code[i].opcode == OpCodes.Call &&
                    code[i].operand is MethodInfo methodInfo &&
                    methodInfo.Name.Equals("GetMouseButton"))
                {
                    second++;
                    //SensibleH.Logger.LogDebug($"DragActionTranspiler[Found][Second][{second}]");
                    code[i].operand = AccessTools.Method(typeof(PatchHandCtrl), nameof(GetMouseButton));// "GetMouseButton");
                    if (second == 2)
                        break;
                }
            }
            return code.AsEnumerable();
        }
        /// <summary>
        /// We feed our fake array for active items check that involves restart of animation of an aibu item.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
        public static IEnumerable<CodeInstruction> JudgeProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            for (var i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld &&
                    code[i].operand.ToString().Contains("useItems"))
                {
                    //SensibleH.Logger.LogDebug($"JudgeProcTranspiler[Found]");
                    code[i].opcode = OpCodes.Ldsfld;
                    code[i].operand = AccessTools.Field(typeof(MoMiController), nameof(MoMiController.FakePrefix)); // name: "FakePrefix");
                    code[i - 1].opcode = OpCodes.Nop;
                    break;
                }
                
            }
            return code.AsEnumerable();
        }

        /// <summary>
        /// We feed our fake array for active items check that involves restart of animation of an aibu item.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.AnimatrotRestrart))]
        public static IEnumerable<CodeInstruction> AnimatrotRestrartTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var retFound = false;
            for (var i = 0; code.Count > 0; i++)
            {
                if (!retFound)
                {
                    if (code[i].opcode == OpCodes.Ret)
                    {
                        retFound = true;
                    }
                }
                else
                {
                    if (code[i].opcode == OpCodes.Ldarg_0)
                    {
                        //SensibleH.Logger.LogDebug($"AnimatrotRestrartTranspiler[Found]");
                        code[i].opcode = OpCodes.Nop;
                        code[i + 1].opcode = OpCodes.Ldsfld;
                        code[i + 1].operand = AccessTools.Field(typeof(MoMiController), nameof(MoMiController.FakePrefix)); // name: "FakePrefix");
                        break;
                    }
                }
            }
            return code.AsEnumerable();
        }
        /// <summary>
        /// We feed our fake array for active items check that involves restart of animation of an aibu item.
        /// And something else..
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetDragStartLayer))]
        public static IEnumerable<CodeInstruction> SetDragStartLayerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var firstPart = false;
            var secondPart = 0;
            
            for (var i = 0; code.Count > 0; i++)
            { 
                if (!firstPart && code[i].opcode == OpCodes.Ldarg_0)
                {
                    //SensibleH.Logger.LogDebug($"SetDragStartLayerTranspiler[Found][First]");
                    code[i].opcode = OpCodes.Nop;
                    code[i + 1].opcode = OpCodes.Ldsfld;
                    code[i + 1].operand = AccessTools.Field(typeof(MoMiController), nameof(MoMiController.FakePostfix)); //name: "FakePostfix");
                    firstPart = true;
                    //SensibleH.Logger.LogDebug($"SetDragStartLayerTranspiler[Found][First][Done]");
                }
                else if (secondPart > 0)
                {
                    //SensibleH.Logger.LogDebug($"SetDragStartLayerTranspiler[Second][{code[i].opcode}][{code[i].operand}]");
#if KK
                    if (code[i].opcode == OpCodes.Stobj)
#else
                    if (code[i].opcode == OpCodes.Stelem)
#endif
                    {
                        //SensibleH.Logger.LogDebug($"SetDragStartLayerTranspiler[Second][Done]");
                        secondPart++;
                    }
                    code[i].opcode = OpCodes.Nop;
                    if (secondPart == 3)
                        break;
                }
                else if (code[i].opcode == OpCodes.Bne_Un)
                {
                    secondPart++;
                    //SensibleH.Logger.LogDebug($"SetDragStartLayerTranspiler[Found][Bne_un]");
                }

            }
            return code.AsEnumerable();
        }
        // Removing it from the method is too much pain.
        //public static Vector2[] TempStorage = new Vector2[6];
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static bool DragActionPrefix(HandCtrl __instance)
        {
            //SensibleH.Logger.LogDebug($"DragAction:{MoMiController.FakeDragLength}");
            //TempStorage = __instance.flags.xy;

            if (MoMiActive && MoMiController.Instance.IsTouchCrossFade)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        //public static void DragActionPostfix(HandCtrl __instance)
        //{
        //    __instance.flags.xy = TempStorage;
        //}
    }
}
