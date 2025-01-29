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
                //SensibleH.Logger.LogDebug($"FakeMouse:Up:Reroute:MoMi");
                return false;
            }
            else if (SensibleHController.IsVR)
            {
                //SensibleH.Logger.LogDebug($"FakeMouse:Up:Reroute:Vr");
                return KK_VR.Caress.HandCtrlHooks.GetMouseButtonUp(button);
            }
            else
            {
                //SensibleH.Logger.LogDebug($"FakeMouse:Up:Reroute:Original");
                return Input.GetMouseButtonUp(button);
            }
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
        /// And remove one termination condition.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
        public static IEnumerable<CodeInstruction> ClickActionDynamicTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var first = false;
            var field = AccessTools.Field(typeof(HFlag), "rateWeakPoint");
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
            var found = false;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (!found)
                    {
                        if (code.opcode == OpCodes.Stfld && code.operand.ToString().Contains("voicePlayClickLoop"))
                        {
                            found = true;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Brtrue || code.opcode == OpCodes.Brtrue_S)
                        {
#if DEBUG
                            SensibleH.Logger.LogDebug($"HandCtrl.ClickAction:{code.opcode},{code.operand}");
#endif
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SensibleHController), nameof(SensibleHController.IsAppropriateMode)));
                            done = true;
                        }
                        else
                        {
#if DEBUG
                            SensibleH.Logger.LogDebug($"HandCtrl.ClickAction:{code.opcode},{code.operand}");
#endif
                            yield return new CodeInstruction(OpCodes.Nop);
                            continue;
                        }
                    }
                    
                }
                yield return code;
            }
        }
    }
}