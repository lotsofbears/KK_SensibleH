using HarmonyLib;
using KK_BetterSquirt;
using KK_SensibleH.AutoMode;
using KK_SensibleH.Caress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static KK_SensibleH.SensibleH;

namespace KK_SensibleH.Patches.StaticPatches
{
    public static class PatchH
    {

        public static int PrettyNumber(int number)
        {
            if (Mathf.Abs(number % 5) > 2)
                return number > 0 ? ((number / 5) + 1) * 5 : ((number / 5) - 1) * 5;
            else
                return (number / 5) * 5;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
        public static void ChangeAnimatorPostfix(HSceneProc.AnimationListInfo _nextAinmInfo, HSceneProc __instance)
        {
            // Pre heat of masturbation/lesbian scene. They didn't wait for us to start.
            // This breaks better squirt of all things? How in hell?
            if (__instance.flags.mode == HFlag.EMode.masturbation || __instance.flags.mode == HFlag.EMode.lesbian) // !__instance.flags.isFreeH && 
            {
                __instance.flags.gaugeFemale = UnityEngine.Random.Range(0, 70);
            }
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
        //[HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeCategory))]
        //public static void ChangeCategoryPostfix()
        //{
        //    SensibleHController.Instance.RepositionDirLight();
        //}

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
                if (__instance.hand.actionUseItem != -1 && __instance.flags.gaugeFemale > PatchLoop.FemaleUpThere
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
            _addPoint = _addPoint * BiasF;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.MaleGaugeUp))]
        public static void PrefixMaleGaugeUp(ref float _addPoint)
        {
            _addPoint = _addPoint * BiasM;
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
                //SensibleH.Logger.LogDebug($"CrossFadeInFixedTime:Kiss");
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
                    //SensibleH.Logger.LogDebug($"CrossFadeInFixedTime:AfterAction");
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
        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.GotoPointMoveScene))]
        public static void GotoPointMoveScenePrefix(HSceneProc __instance)
        {
            for (int i = 0; i < __instance.lstFemale.Count; i++)
            {
                __instance.lstFemale[i].visibleAll = __instance.lstOldFemaleVisible[i];
            }
            __instance.male.visibleAll = __instance.lstOldMaleVisible[0];
            if (__instance.male1)
            {
                __instance.male1.visibleAll = __instance.lstOldMaleVisible[1];
            }
            __instance.item.SetVisible(true);
            __instance.hand.SceneChangeItemEnable(true);
        }
    }
}
