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
using KK_SensibleH.Patches.DynamicPatches;

namespace KK_SensibleH.Patches.StaticPatches
{
    class PatchHandCtrl
    {
        public struct CodeInfo
        {
            public OpCode firstOpcode;
            public string firstOperand;
            public OpCode secondOpcode;
            public string secondOperand;
        }

        public struct JudgeState
        {
            public float ActionMove;
            public int ActionLoop;
        }
        public static bool IsUseItemPrefix(HandCtrl hand, int index)
        {
            if (MoMiActive)
            {
                return MoMiController.FakePrefix[index] != null;
            }
            else
                return hand.useItems[index] != null;
        }

        public static bool IsUseItemPostfix(HandCtrl hand, int index)
        {
            if (MoMiActive)
            {
                return MoMiController.FakePostfix[index] != null;
            }
            else
                return hand.useItems[index] != null;
        }
        
        /// <summary>
        /// We feed our fake array for active items check that involves restart of animation of an aibu item.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
        public static IEnumerable<CodeInstruction> JudgeProcDynamicTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            SensibleH.Logger.LogDebug($"Trans:JudgeProc:Start");
            for (var i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld &&
                    code[i].operand.ToString().Contains("useItems"))
                {
                    //SensibleH.Logger.LogDebug($"Trans:JudgeProc:{code[i].opcode}:{code[i].operand}");
                    code[i].opcode = OpCodes.Nop;
                    code[i + 2].opcode = OpCodes.Call;
                    code[i + 2].operand = AccessTools.FirstMethod(typeof(PatchHandCtrl), m => m.Name.Equals(nameof(PatchHandCtrl.IsUseItemPrefix)));
                    break;
                }
            }
            return code.AsEnumerable();
        }

        /// <summary>
        /// We prevent counters for voiceProc from resetting on consecutive click actions.
        /// Disable click counter for voice when we have drag running simultaneously ?
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
        public static void HandCtrlJudgeProcPrefix(HandCtrl __instance, ref JudgeState __state)
        {
            if (MoMiActive)
            {
                __state = new JudgeState
                {
                    ActionMove = __instance.voicePlayActionMove,
                    ActionLoop = __instance.voicePlayActionLoop
                };
                //if (__instance.GetUseItemNumber().Count == 1)
                //{
                //    __state.ActionMove += UnityEngine.Random.Range(25f, 50f);
                //}
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
        public static void HandCtrlJudgeProcPostfix(HandCtrl __instance, JudgeState __state)
        {
            if (MoMiActive)
            {
                __instance.voicePlayActionMove = __state.ActionMove;
                __instance.voicePlayActionMoveOld = __state.ActionMove;
                __instance.voicePlayActionLoop = __state.ActionLoop;
            }
        }
        /// <summary>
        /// We feed our fake array for active items check that involves restart of animation of an aibu item.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.AnimatrotRestrart))]
        public static IEnumerable<CodeInstruction> AnimatrotRestrartDynamicTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var retFound = false;
            SensibleH.Logger.LogDebug($"Trans:AnimatrotRestrart:Start");
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
                        //SensibleH.Logger.LogDebug($"Trans:AnimatrotRestrart:{code[i].opcode}:{code[i].operand}");
                        code[i + 1].opcode = OpCodes.Nop;
                        code[i + 3].opcode = OpCodes.Call;
                        code[i + 3].operand = AccessTools.FirstMethod(typeof(PatchHandCtrl), m => m.Name.Equals(nameof(PatchHandCtrl.IsUseItemPrefix))); 
                        break;
                    }
                }
            }
            return code.AsEnumerable();
        }
        /// <summary>
        /// We feed our fake array for active items check that involves restart of animation of an aibu item.
        /// And removing vector adjustment of an item.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetDragStartLayer))]
        public static IEnumerable<CodeInstruction> SetDragStartLayerDynamicTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var firstPart = false;
            var secondPart = 0;

            SensibleH.Logger.LogDebug($"Trans:SetDragStartLayer:Start");
            for (var i = 0; code.Count > 0; i++)
            {
                if (!firstPart && code[i].opcode == OpCodes.Ldarg_0)
                {
                    //SensibleH.Logger.LogDebug($"Trans:SetDragStartLayer:{code[i].opcode}:{code[i].operand}");
                    code[i + 1].opcode = OpCodes.Nop;
                    code[i + 3].opcode = OpCodes.Call;
                    code[i + 3].operand = AccessTools.FirstMethod(typeof(PatchHandCtrl), m => m.Name.Equals(nameof(PatchHandCtrl.IsUseItemPostfix)));
                    firstPart = true;
                }
                else if (secondPart > 0)
                {
                    SensibleH.Logger.LogDebug($"Trans:SetDragStartLayer:{code[i].opcode}:{code[i].operand}");
#if KK
                    if (code[i].opcode == OpCodes.Stobj)
#else
					if (code[i].opcode == OpCodes.Stelem)
#endif
                    {
                        secondPart++;
                    }
                    code[i].opcode = OpCodes.Nop;
                    if (secondPart == 3)
                        break;
                }
                else if (code[i].opcode == OpCodes.Bne_Un)
                {
                    secondPart++;
                    //SensibleH.Logger.LogDebug($"Trans:SetDragStartLayer:{code[i].opcode}:{code[i].operand}");
                }

            }
            return code.AsEnumerable();
        }

        /// <summary>
        /// We disable (half the time) override of a voice with the short by HitReactionPlay().
        /// Otherwise happens way too often in non-Aibu modes during caress.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.HitReactionPlay))]
        public static void HitReactionPlayPrefix(ref bool _playShort, HandCtrl __instance)
        {
            if (__instance.actionUseItem != -1 && __instance.voice.nowVoices[__instance.numFemale].state == HVoiceCtrl.VoiceKind.voice && UnityEngine.Random.value < 0.67f)
            {
                _playShort = false;
            }
        }
        /// <summary>
        /// We run voiceProc for first item attached.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.FinishAction))]
        public static void HandCtrlFinishActionPostfix(HandCtrl __instance)
        {
            if (FirstTouch)
            {
                //SensibleH.Logger.LogDebug($"FinishAction:FirstTouch:{__instance.actionUseItem != -1}");
                if (__instance.flags.mode == HFlag.EMode.aibu)
                {
                    __instance.flags.voice.timeAibu.timeIdle = 0.75f;
                }
                else
                {
                    SensibleHController.Instance.DoFirstTouchProc();
                }
                FirstTouch = false;
            }
            // Obsolete due to CyuVR.
            //if (__instance.IsKissAction() && __instance.flags.mode != HFlag.EMode.aibu
            //    && __instance.voice.nowVoices[0].state != HVoiceCtrl.VoiceKind.voice)
            //{
            //    __instance.flags.voice.playVoices[0] = 102;
            //}
        }
    }
}
