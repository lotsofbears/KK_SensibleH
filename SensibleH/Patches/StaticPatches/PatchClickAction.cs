using HarmonyLib;
using KK_SensibleH.Caress;
using KK_SensibleH.Patches.DynamicPatches;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    class PatchClickAction
    {
        //public static bool GetMouseButtonUp(int button) => false;
        public static bool GetMouseButtonUp(int button)
        {
            if (MoMiController.FakeMouseButton)
            {
                return false;
            }
            else if (SensibleHController._vr)
            {
#if KK
                return KK_VR.Caress.HandCtrlHooks.GetMouseButtonUp(button);
#else
                return KKS_VR.Caress.HandCtrlHooks.GetMouseButtonUp(button);
#endif
            }
            else
                return Input.GetMouseButtonUp(button);
        }
        public static bool IsFinishAction(HandCtrl hand)
        {
            if (SensibleH.MoMiActive)
            {
                return false;
            }
            else
                return hand.FinishAction();
        }
        /// <summary>
        /// We substitute original mouse button up with the fake that returns "false".
        /// And remove one termination condition, which will most likely break the game immediately if we leave it around.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
        public static IEnumerable<CodeInstruction> ClickActionDynamicTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var first = false;
            var field = AccessTools.Field(typeof(HFlag), "rateWeakPoint");
            SensibleH.Logger.LogDebug($"Trans:Dynamic:ClickAction:Start");
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Call && code.operand is MethodInfo method)
                {
                    if (!first)
                    {
                        if (method.Name.Equals("GetMouseButtonUp"))
                        {
                            first = true;
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchClickAction), nameof(PatchClickAction.GetMouseButtonUp)));
                            continue;
                        }
                    }
                    else
                    {
                        if (method.Name.Equals("FinishAction"))
                        {
                            first = true;
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchClickAction), nameof(PatchClickAction.IsFinishAction)));
                            continue;
                        }
                    }
                }
                yield return code;
            }
        }

        /// <summary>
        /// We delete the restriction to play caress voice only in Aibu.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
        public static IEnumerable<CodeInstruction> ClickActionConstantTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // TODO Consolidate static patches that crossover with dynamic ones.
            SensibleH.Logger.LogDebug($"Trans:Static:ClickAction:Start");
            var targets = new Dictionary<int, PatchHandCtrl.CodeInfo>()
            {
                {
                    0, new PatchHandCtrl.CodeInfo {
                        firstOpcode = OpCodes.Call,
                        firstOperand = "Range",
                        secondOpcode = OpCodes.Stfld,
                        secondOperand = "voicePlayClickLoop"
                    }
                }
            };
            var counter = 0;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (counter == 0 && code.opcode == targets[0].firstOpcode
                        && code.operand.ToString().Contains(targets[0].firstOperand))
                    {
                        counter++;
                        //SensibleH.Logger.LogDebug($"Trans:ClickAction:{code.opcode}:{code.operand}");
                    }
                    else if (counter == 1)
                    {
                        //SensibleH.Logger.LogDebug($"Trans:ClickAction:{code.opcode}:{code.operand}");
                        if (code.opcode == targets[0].secondOpcode
                        && code.operand.ToString().Contains(targets[0].secondOperand))
                        {
                            counter++;
                        }
                        else
                            counter = 0;
                    }
                    else if (counter == 2)
                    {
                        //SensibleH.Logger.LogDebug($"Trans:ClickAction:{code.opcode}:{code.operand}");
                        if (code.opcode == OpCodes.Brtrue)
                        {
                            done = true;
                        }
                        yield return new CodeInstruction(OpCodes.Nop);
                        continue;
                    }
                }
                yield return code;
            }
        }
    }
}