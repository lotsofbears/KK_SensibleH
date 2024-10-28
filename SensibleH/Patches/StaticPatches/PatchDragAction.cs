using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using KK_SensibleH.Caress;
using System.Linq;
using static Studio.AnimeGroupList;

namespace KK_SensibleH.Patches.StaticPatches
{
    class PatchDragAction
    {
        public static bool GetMouseButton(int button)
        {
            if (MoMiController.FakeMouseButton)
            {
                //SensibleH.Logger.LogDebug($"FakeMouse:Press:Reroute:Vr");
                return true;
            }
            else if (SensibleHController._vr)
            {
                //SensibleH.Logger.LogDebug($"FakeMouse:Press:Reroute:Vr");
#if KK
                return KK_VR.Caress.HandCtrlHooks.GetMouseButton(button);
#else
                return KKS_VR.Caress.HandCtrlHooks.GetMouseButton(button);
#endif
            }
            else
            {
                //SensibleH.Logger.LogDebug($"FakeMouse:Press:Reroute:Original");
                return Input.GetMouseButton(button);
            }
        }
        public static void GetDragLength(HandCtrl hand)
        {
            if (MoMiController.FakeDrag)
            {
                hand.calcDragLength = MoMiController.FakeDragLength;
                if (MoMiController.ResetDrag)
                {
                    MoMiController.FakeDragLength = Vector2.zero;
                }
            }
            else
                hand.calcDragLength.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
        /// <summary>
        /// We feed the game our vector of movement to add excitement from it. (and ask to reset it also).
        /// We substitute mouse button press with the fake that returns "true".
        /// </summary>
        //[HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        //public static IEnumerable<CodeInstruction> DragActionDynamicTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var first = false;
        //    var second = 0;
        //    var code = new List<CodeInstruction>(instructions);
        //    SensibleH.Logger.LogDebug($"Trans:DragAction:Start");
        //    for (var i = 0; i < code.Count; i++)
        //    {
        //        if (!first && code[i].opcode == OpCodes.Ldflda &&
        //            code[i].operand.ToString().Contains("calcDragLength"))
        //        {
        //            //SensibleH.Logger.LogDebug($"Trans:DragAction:{code[i].opcode}:{code[i].operand}");
        //            first = true;
        //            code[i].opcode = OpCodes.Call;
        //            code[i].operand = AccessTools.FirstMethod(typeof(PatchDragAction), m => m.Name.Equals(nameof(PatchDragAction.GetDragLength)));
        //            code[i + 1].opcode = OpCodes.Nop;
        //            code[i + 2].opcode = OpCodes.Nop;
        //            //code[i + 2].operand = AccessTools.Field(typeof(HandCtrl), nameof(HandCtrl.calcDragLength));// name: "calcDragLength");
        //            code[i + 3].opcode = OpCodes.Nop;
        //            code[i + 4].opcode = OpCodes.Nop;
        //            code[i + 5].opcode = OpCodes.Nop;
        //        }
        //        else if (second != 2 && code[i].opcode == OpCodes.Call &&
        //            code[i].operand is MethodInfo methodInfo &&
        //            methodInfo.Name.Equals("GetMouseButton"))
        //        {
        //            second++;
        //            code[i].operand = AccessTools.Method(typeof(PatchDragAction), nameof(PatchDragAction.GetMouseButton));// "GetMouseButton");
        //            if (second == 2)
        //                break;
        //        }
        //    }
        //    return code.AsEnumerable();
        //}

        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var first = false;
            var counter = 0;
            foreach (var code in instructions)
            {
                if (!first)
                {
                    if (counter == 0)
                    {
                        if (code.opcode == OpCodes.Ldflda && code.operand is FieldInfo field
                            && field.Name.Equals("calcDragLength"))
                        {
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.FirstMethod(typeof(PatchDragAction), m => m.Name.Equals(nameof(PatchDragAction.GetDragLength))));
                            counter++;
                            continue;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Call && code.operand is MethodInfo method
                            && method.Name.Equals("Set"))
                        {
                            first = true;
                            counter = 0;
                        }
                        yield return new CodeInstruction(OpCodes.Nop);
                        continue;
                    }
                }
                else if (code.opcode == OpCodes.Call && code.operand is MethodInfo method &&
                    method.Name.Equals("GetMouseButton"))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchDragAction), nameof(PatchDragAction.GetMouseButton)));
                    continue;
                }
                else if (code.opcode == OpCodes.Ldc_I4 && code.operand is int number)
                {
                    if (number == 300)
                    {
                        code.operand = 1000;
                    }
                    else if (number == 400)
                    {
                        code.operand = 1500;
                    }
                }
                yield return code;
            }
        }
        /// <summary>
        /// We delete the restriction to play caress voice only in Aibu.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionConstantTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var targets = new Dictionary<int, PatchHandCtrl.CodeInfo>()
            {
                {
                    0, new PatchHandCtrl.CodeInfo {
                        firstOpcode = OpCodes.Call,
                        firstOperand = "Range",
                        secondOpcode = OpCodes.Stfld,
                        secondOperand = "voicePlayActionLoop"
                    }
                },
                {
                    1, new PatchHandCtrl.CodeInfo {
                        firstOpcode = OpCodes.Ldfld,
                        firstOperand = "voicePlayActionMove",
                        secondOpcode = OpCodes.Ble_Un,
                        secondOperand = ""
                    }
                }
            };
            var counter = 0;
            var tarCount = 0;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (counter == 0 && code.opcode == targets[tarCount].firstOpcode
                        && code.operand.ToString().Contains(targets[tarCount].firstOperand))
                    {
                        counter++;
                    }
                    else if (counter == 1 && code.opcode == targets[tarCount].secondOpcode
                        && code.operand.ToString().Contains(targets[tarCount].secondOperand))
                    {
                        counter++;
                    }
                    else if (counter == 2)
                    {
                        if (code.opcode == OpCodes.Brtrue)
                        {
                            counter = 0;
                            tarCount++;
                            if (tarCount == 2)
                            {
                                done = true;
                            }
                        }
                        yield return new CodeInstruction(OpCodes.Nop);
                        continue;
                    }
                    else
                        counter = 0;
                }
                yield return code;
            }
        }
        //public static Vector2[] testArray = new Vector2[2];
        /// <summary>
        /// We DragAction during crossfader.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static bool DragActionPrefix()
        {
            if (SensibleH.MoMiActive && MoMiController.Instance.IsTouchCrossFade)
            {
                return false;
            } 
            else
            {
                return true;
            }
        }
        /// <summary>
        /// This is a fix for KK(S)_VR, to be able to move items separately.
        /// MoMi uses EndOfFrame timings, which completely disrespect DragAction, and thus don't require fix.
        /// We simply substitute count of additional items with 0, thus it doesn't perform adjustment.
        /// It resides here for organization purposes.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionFixKKVR(IEnumerable<CodeInstruction> instructions)
        {
            var counter = 0;
            var found = false;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (!found)
                    {
                        if (counter == 0)
                        {
                            if (code.opcode == OpCodes.Newobj)
                            {
                                counter++;
                            }
                        }
                        else if (counter == 1)
                        {
                            if (code.opcode == OpCodes.Stobj)
                            {
                                counter++;
                            }
                            else
                            {
                                counter = 0;
                            }
                        }
                        else if (counter == 2)
                        {
                            // Label
                            code.opcode = OpCodes.Nop;
                            found = true;

                        }
                    }
                    else
                    {
                        //SensibleH.Logger.LogDebug($"DragActionFixKKVR:{code.opcode}:{code.operand}");
                        if (code.opcode == OpCodes.Sub)
                        {
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            done = true;
                        }
                        continue;
                    }
                }
                yield return code;
            }
        }

    }
}
