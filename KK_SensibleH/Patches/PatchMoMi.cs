using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;
using static KK_SensibleH.SensibleH;
using KKAPI.MainGame;
using ADV.Commands.Base;
using NodeCanvas.Tasks.Actions;

namespace KK_SensibleH.Patches
{
    class PatchMoMi
    {
        public static bool GetMouseButtonUp(int button) => false;
        public static bool GetMouseButton(int button) => true;

        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
        public static IEnumerable<CodeInstruction> ClickActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var first = false;
            var method = nameof(Input.GetMouseButtonUp);
            //var breaks = 0;
            //var second = 0;
            foreach (var code in instructions)
            {
                if (!first && code.opcode == OpCodes.Call &&
                    code.operand.ToString().Equals(method))
                {
                    first = true;
                    //SensibleH.Logger.LogDebug($"ClickActionTranspiler[Found][First][{code.opcode}][{code.operand}]");
                    var newMethod = AccessTools.Method(typeof(PatchMoMi), nameof(GetMouseButtonUp)); // "GetMouseButtonUp");
                    yield return new CodeInstruction(OpCodes.Call, newMethod);
                }
                //else if (breaks == 12 && second < 3)
                //{
                //    //SensibleH.Logger.LogDebug($"ClickActionTranspiler[Found][Second][{code.opcode}][{code.operand}]");
                //    code.opcode = OpCodes.Nop;
                //    second++;
                //    yield return code;
                //}
                //else if (code.opcode == OpCodes.Br)
                //{
                //    breaks++;
                //    yield return code;
                //}
                else
                {
                    yield return code;
                }
            }
        }
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
                    //SensibleH.Logger.LogDebug($"DragActionTranspiler[Found][First]");
                    first = true;
                    code[i].opcode = OpCodes.Ldsfld;
                    code[i].operand = fakeDragLength;
                    code[i + 1].opcode = OpCodes.Ldc_R4;
                    code[i + 1].operand = 3f;
                    code[i + 2].opcode = OpCodes.Call;
                    code[i + 2].operand = AccessTools.FirstMethod(typeof(Vector2),  method => method.Name.Equals("op_Multiply"));
                    code[i + 3].opcode = OpCodes.Stfld;
                    code[i + 3].operand = AccessTools.Field(typeof(HandCtrl), nameof(HandCtrl.calcDragLength));// name: "calcDragLength");
                    code[i + 4].opcode = OpCodes.Call;
                    code[i + 4].operand = AccessTools.FirstMethod(typeof(Vector2), method => method.Name.Equals("get_zero"));
                    code[i + 5].opcode = OpCodes.Stsfld;
                    code[i + 5].operand = fakeDragLength;
                }
                else if (code[i].opcode == OpCodes.Call &&
                    code[i].operand is MethodInfo methodInfo &&
                    methodInfo.Name.Equals("GetMouseButton"))
                {
                    second++;
                    //SensibleH.Logger.LogDebug($"DragActionTranspiler[Found][Second][{second}]");
                    code[i].operand = AccessTools.Method(typeof(PatchMoMi), nameof(GetMouseButton));// "GetMouseButton");
                    if (second == 2)
                        break;
                }
            }
            return code.AsEnumerable();
        }
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
                }
                else if (secondPart == 2)
                {
                    if (code[i].opcode == OpCodes.Stobj)
                    {
                        //SensibleH.Logger.LogDebug($"SetDragStartLayerTranspiler[Found][Second]");
                        secondPart++;
                    }
                    code[i].opcode = OpCodes.Nop;
                    if (secondPart == 3)
                        break;
                }
                else if (code[i].opcode == OpCodes.Bne_Un)
                {
                    secondPart++;
                }

            }
            return code.AsEnumerable();
        }
    }
}
