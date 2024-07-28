using HarmonyLib;
using KK_BetterSquirt;
using KK_SensibleH.AutoMode;
using KK_SensibleH.Caress;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchMoMiAuxiliary
    {
        class CodeInfo
        {
            public OpCode firstOpcode;
            public string firstOperand;
            public OpCode secondOpcode;
            public string secondOperand;
        }


        /// <summary>
        /// We delete the restriction to play caress voice only in Aibu.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            //SensibleH.Logger.LogDebug($"DragActionTranspiler[start]");
            var targets = new Dictionary<int, CodeInfo>()
            {
                {
                    0, new CodeInfo {
                        firstOpcode = OpCodes.Call,
                        firstOperand = "Range",
                        secondOpcode = OpCodes.Stfld,
                        secondOperand = "voicePlayActionLoop"
                    }
                },
                {
                    1, new CodeInfo {
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
                        //SensibleH.Logger.LogDebug($"DragActionTranspiler[first] {code.opcode}");
                    }
                    else if (counter == 1 && code.opcode == targets[tarCount].secondOpcode
                        && code.operand.ToString().Contains(targets[tarCount].secondOperand))
                    {
                        counter++;
                        //SensibleH.Logger.LogDebug($"DragActionTranspiler[second] {code.opcode}");
                    }
                    else if (counter == 2)
                    {
                        //SensibleH.Logger.LogDebug($"DragActionTranspiler[found] {code.opcode}");
                        if (code.opcode == OpCodes.Brtrue)
                        {
                            counter = 0;
                            tarCount++;
                            if (tarCount == 2)
                                done = true;
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
        /// <summary>
        /// We adjust CrossFader's FadeTime for specific animations.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Animator), nameof(Animator.CrossFadeInFixedTime), new Type[]
        {
            typeof(string),
            typeof(float),
            typeof(int)
        })]
        public static void CrossFadeInFixedTimePrefix(string stateName, ref float transitionDuration)
        {
            if (stateName.Equals("K_Touch"))
            {
                SensibleH.Logger.LogDebug($"CrossFadeInFixedTime Kiss");
                transitionDuration = 1f;
            }
            else if (stateName.Equals("Idle"))
            {
                if (LoopProperties.IsHoushi)
                    transitionDuration = UnityEngine.Random.Range(0.5f, 1f);
                else
                    transitionDuration = UnityEngine.Random.Range(1.5f, 2.5f);
                SensibleH.Logger.LogDebug($"CrossFadeInFixedTime AfterAction");
            }
        }
        /// <summary>
        /// A hook for CyuVR's tongue manipulations.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FaceBlendShape), nameof(FaceBlendShape.LateUpdate))]
        public static void FaceBlendShapeLateUpdateHook()
        {
            if (Kiss.Instance != null)
            {
                Kiss.Instance.LateUpdateHook();
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BetterSquirtController), "TouchChanceCalc")]
        public static bool TouchChanceCalcPrefix(ref bool __result)
        {
            if (SensibleH.OverrideSquirt)
            {
                __result = true;
                return false;
            }
            return true;
        }

    }
}
