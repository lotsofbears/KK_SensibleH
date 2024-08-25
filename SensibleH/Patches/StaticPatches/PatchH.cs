using HarmonyLib;
using KK_BetterSquirt;
using KK_SensibleH.AutoMode;
using KK_SensibleH.Caress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static KK_SensibleH.SensibleH;

namespace KK_SensibleH.Patches.StaticPatches
{
    public static class PatchH
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

        public static int PrettyNumber(int number)
        {
            if (Mathf.Abs(number % 5) > 2)
                return number > 0 ? ((number / 5) + 1) * 5 : ((number / 5) - 1) * 5;
            else
                return (number / 5) * 5;
        }
        //[HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
        //public static void ChangeAnimatorPrefix()
        //{
        //    PatchObi.SetObiPersistence(false);
        //}


        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
        public static void ChangeAnimatorPostfix(HSceneProc.AnimationListInfo _nextAinmInfo, HSceneProc __instance)
        {
            // Pre heat of masturbation/lesbian scene. They didn't wait for us to start.
            //if ((__instance.flags.mode == HFlag.EMode.masturbation || __instance.flags.mode == HFlag.EMode.lesbian)) // !__instance.flags.isFreeH && 
            //{
            //    __instance.flags.gaugeFemale = UnityEngine.Random.Range(0, 70);
            //}
            if (__instance.flags.mode == HFlag.EMode.masturbation)
            {
                __instance.flags.timeMasturbation.timeMin = 25f; //UnityEngine.Random.Range(20, 30);
                __instance.flags.timeMasturbation.timeMax = 35f;// __instance.flags.timeMasturbation.timeMin + 20f;
            }
            if (__instance.lstFemale.Count > 1)
            {
                for (int i = 0; i < __instance.eyeneckFemale1.dicEyeNeck.Count; i++)
                {
                    var eyeNeck = __instance.eyeneckFemale1.dicEyeNeck.ElementAt(i).Value;
                    eyeNeck.rangeNeck.up = PrettyNumber((int)(eyeNeck.rangeNeck.up * NeckLimit.Value));
                    eyeNeck.rangeNeck.down = PrettyNumber((int)(eyeNeck.rangeNeck.down * NeckLimit.Value));
                    eyeNeck.rangeNeck.left = PrettyNumber((int)(eyeNeck.rangeNeck.left * NeckLimit.Value));
                    eyeNeck.rangeNeck.right = PrettyNumber((int)(eyeNeck.rangeNeck.right * NeckLimit.Value));
                    eyeNeck.rangeFace.up = PrettyNumber((int)(eyeNeck.rangeFace.up * NeckLimit.Value));
                    eyeNeck.rangeFace.down = PrettyNumber((int)(eyeNeck.rangeFace.down * NeckLimit.Value));
                    eyeNeck.rangeFace.left = PrettyNumber((int)(eyeNeck.rangeFace.left * NeckLimit.Value));
                    eyeNeck.rangeFace.right = PrettyNumber((int)(eyeNeck.rangeFace.right * NeckLimit.Value));
                }
            }
            for (int i = 0; i < __instance.eyeneckFemale.dicEyeNeck.Count; i++)
            {
                var eyeNeck = __instance.eyeneckFemale.dicEyeNeck.ElementAt(i).Value;
                eyeNeck.rangeNeck.up = PrettyNumber((int)(eyeNeck.rangeNeck.up * NeckLimit.Value));
                eyeNeck.rangeNeck.down = PrettyNumber((int)(eyeNeck.rangeNeck.down * NeckLimit.Value));
                eyeNeck.rangeNeck.left = PrettyNumber((int)(eyeNeck.rangeNeck.left * NeckLimit.Value));
                eyeNeck.rangeNeck.right = PrettyNumber((int)(eyeNeck.rangeNeck.right * NeckLimit.Value));
                eyeNeck.rangeFace.up = PrettyNumber((int)(eyeNeck.rangeFace.up * NeckLimit.Value));
                eyeNeck.rangeFace.down = PrettyNumber((int)(eyeNeck.rangeFace.down * NeckLimit.Value));
                eyeNeck.rangeFace.left = PrettyNumber((int)(eyeNeck.rangeFace.left * NeckLimit.Value));
                eyeNeck.rangeFace.right = PrettyNumber((int)(eyeNeck.rangeFace.right * NeckLimit.Value));
            }

            if (__instance.flags.mode == HFlag.EMode.lesbian)
            {
                __instance.flags.timeLesbian.timeMin = 25f;// UnityEngine.Random.Range(20, 30);
                __instance.flags.timeLesbian.timeMax = 35f;//__instance.flags.timeMasturbation.timeMin + 20f;
            }
            SensibleHController.Instance.OnPositionChange(_nextAinmInfo);
        }

        /// <summary>
        /// We check for non Orgasm/OrgasmAfter loops and run the timer that by default is being used only for the action restart after the finish.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HMasturbation), nameof(HMasturbation.Proc))]
        public static void HMasturbationProc(HMasturbation __instance)
        {
            if (!__instance.flags.nowAnimStateName.StartsWith("O", StringComparison.Ordinal) 
                && __instance.voice.nowVoices[0].state != HVoiceCtrl.VoiceKind.voice && __instance.flags.timeMasturbation.IsIdleTime())
            {
                if (__instance.flags.gaugeFemale < 40f)
                {
                    __instance.flags.voice.playVoices[0] = 402;
                }
                else if (__instance.flags.gaugeFemale < 70f)
                {
                    __instance.flags.voice.playVoices[0] = 403;
                }
                else// if (__instance.flags.gaugeFemale < 100f)
                {
                    __instance.flags.voice.playVoices[0] = 404;
                }
            }

        }

        /// <summary>
        /// We check for non Orgasm, OrgasmAfter loops and run the timer that by default is being used only for the action restart after the climax.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HLesbian), nameof(HLesbian.Proc))]
        public static void HLesbianProc(HLesbian __instance)
        {
            if (!__instance.flags.nowAnimStateName.StartsWith("O", StringComparison.Ordinal) 
                && __instance.voice.nowVoices[0].state != HVoiceCtrl.VoiceKind.voice  && __instance.voice.nowVoices[1].state != HVoiceCtrl.VoiceKind.voice
                && __instance.flags.timeLesbian.IsIdleTime())
            {
                __instance.speek = false;
            }
        }

        /// <summary>
        /// We catch the VoiceProc to, perhaps, interrupt it and run it at the latter time.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.VoiceProc))]
        public static void PrefixVoiceProc(HVoiceCtrl __instance, int _main)
        {
            if (__instance.flags.voice.playVoices[_main] != -1)
            {
                if (__instance.flags.gaugeFemale > PatchLoop.FemaleUpThere && __instance.hand.actionUseItem != -1
                    && UnityEngine.Random.value < 0.5f)
                {
                    __instance.flags.voice.playVoices[_main] = 141;
                }
                SensibleHController.Instance.OnVoiceProc(_main);
            }
        }

        /// <summary>
        /// We slowdown the excitement buildup, because 50 seconds session in vr is laughable.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.FemaleGaugeUp))]
        public static void PrefixFemaleGaugeUp(ref float _addPoint)
        {
            //if (_addPoint < 0)// && biasF < 1f)
            //    _addPoint = _addPoint * 0.25f;
            //else
            _addPoint = _addPoint * gaugeMultiplier * BiasF;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.MaleGaugeUp))]
        public static void PrefixMaleGaugeUp(ref float _addPoint)
        {
            //if (_addPoint < 0)// && biasM < 1f)
            //    _addPoint = _addPoint * 0.25f;
            // else
            _addPoint = _addPoint * gaugeMultiplier * BiasM;
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
        /// <summary>
        /// We delete the restriction to play caress voice only in Aibu.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> ClickActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            //SensibleH.Logger.LogDebug($"DragActionTranspiler[start]");
            var targets = new Dictionary<int, CodeInfo>()
            {
                {
                    0, new CodeInfo {
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
                        //SensibleH.Logger.LogDebug($"ClickActionTranspiler[first] {code.opcode} / {code.operand}");
                    }
                    else if (counter == 1 && code.opcode == targets[0].secondOpcode
                        && code.operand.ToString().Contains(targets[0].secondOperand))
                    {
                        counter++;
                        //SensibleH.Logger.LogDebug($"ClickActionTranspiler[second] {code.opcode} / {code.operand}");
                    }
                    else if (counter == 2)
                    {
                        //SensibleH.Logger.LogDebug($"ClickActionTranspiler[found] {code.opcode}");
                        if (code.opcode == OpCodes.Brtrue)
                        {
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
#if KK
        public static void CrossFadeInFixedTimePrefix(string stateName, ref float transitionDuration, int layer)
        {
            if (stateName.Equals("K_Touch"))
            {
                SensibleH.Logger.LogDebug($"CrossFadeInFixedTime:Kiss");
                transitionDuration = 1f;
            }
            else if (stateName.Equals("Idle"))
            {
                if (LoopProperties.IsHoushi)
                {
                    transitionDuration = UnityEngine.Random.Range(0.5f, 1f);
                }
                else
                {
                    transitionDuration = UnityEngine.Random.Range(1.5f, 2.5f);
                    SensibleH.Logger.LogDebug($"CrossFadeInFixedTime:AfterAction");
                }
            }
            if (MoMiController.Instance != null)
            {
                MoMiController.Instance.SetCrossFadeWait(transitionDuration);
            }
        }
#else
        public static void CrossFadeInFixedTimePrefix(string stateName, ref float fixedTransitionDuration, int layer)
        {
            if (stateName.Equals("K_Touch"))
            {
                SensibleH.Logger.LogDebug($"CrossFadeInFixedTime:Kiss");
                fixedTransitionDuration = 1f;
            }
            else if (stateName.Equals("Idle"))
            {
                if (LoopProperties.IsHoushi)
                {
                    fixedTransitionDuration = UnityEngine.Random.Range(0.5f, 1f);
                }
                else
                {
                    fixedTransitionDuration = UnityEngine.Random.Range(1.5f, 2.5f);
                    SensibleH.Logger.LogDebug($"CrossFadeInFixedTime:AfterAction");
                }
            }
            if (MoMiController.Instance != null)
            {
                MoMiController.Instance.SetCrossFadeWait(fixedTransitionDuration);
            }
        }
#endif
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
            if (OverrideSquirt)
            {
                __result = true;
                return false;
            }
            return true;
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
        /// OLoop outside of orgasm enabler.
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(HSonyu), nameof(HSonyu.Proc))]
        public static void HSonyuProcPostfix(HSonyu __instance)
        {
            if (OLoop)
            {
                __instance.LoopProc(true);
            }
        }
        [HarmonyPostfix, HarmonyPatch(typeof(H3PSonyu), nameof(H3PSonyu.Proc))]
        public static void H3PSonyuProcPostfix(H3PSonyu __instance)
        {
            if (OLoop)
            {
                __instance.LoopProc(true);
            }
        }
        /// <summary>
        /// Breath in OLoop enabler.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.BreathProc))]
        public static void BreathProcPrefix(ref AnimatorStateInfo _ai, HVoiceCtrl __instance)
        {
            if (OLoop && __instance.flags.mode == HFlag.EMode.sonyu)
                _ai = sLoopInfo;
        }
        /// <summary>
        /// By default when there is an item and no action is conducted, there is no voice procs.
        /// We look for lack of action and non "Idle" or "Orgasm" pose and run IsIdleTime() timer on proc, and then we handle the voice ourselves.
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(HAibu), nameof(HAibu.Proc))]
        public static void HAibuProcPostfix(HAibu __instance)
        {
            if (__instance.hand.actionUseItem == -1 && __instance.voice.nowVoices[0].state != HVoiceCtrl.VoiceKind.voice
                && (__instance.flags.nowAnimStateName.EndsWith("_Idle", StringComparison.Ordinal)
                || __instance.flags.nowAnimStateName.EndsWith("A", StringComparison.Ordinal)))
            {
                if (__instance.flags.voice.timeAibu.IsIdleTime())
                {
                    if (__instance.flags.nowAnimStateName.EndsWith("A", StringComparison.Ordinal) && UnityEngine.Random.value < 0.75f)
                    {
                        // We run after orgasm voice.
                        __instance.flags.voice.isAfterVoicePlay = false;
                        __instance.flags.voice.playVoices[0] = 143;
                    }
                    else
                    {
                        // We run caress voice for items attached.
                        SensibleHController.Instance.DoFirstTouchProc();
                    }
                }
                    
            }
        }
    }
}
