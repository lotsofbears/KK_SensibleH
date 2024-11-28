using HarmonyLib;
using KK_SensibleH.AutoMode;
using Manager;
using NodeCanvas.Tasks.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VRGIN.Core;
using static Illusion.Utils;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class TestH
    {
        public static float size = 1f;
        /// <summary>
        /// Adjustments for non-standard(small) dick diameters in houshi.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(FaceListCtrl), nameof(FaceListCtrl.SetFace))]
        public static void SetFacePrefix(int _idFace, int _voiceKind, int _action, FaceListCtrl __instance)
        {
            var dic = __instance.facelib[_voiceKind][_action][_idFace];
            if (SensibleH.hFlag != null && SensibleH.hFlag.mode == HFlag.EMode.houshi && dic.openMinMouth == 1f && (dic.mouth == 22 || dic.mouth == 21))
            {
                //SensibleH.Logger.LogDebug($"TestH:SetFace:AlteringHoushiMouth[{_idFace}][{_voiceKind}][{_action}]");
                dic.openMinMouth = size;
            }
        }
        public static int GetRandomBinary() => UnityEngine.Random.value > 0.5f ? 1 : 0;
        /// <summary>
        /// We substitute rigid set of targets to play voices with random one.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HLesbian), nameof(HLesbian.Proc))]
        public static IEnumerable<CodeInstruction> HLesbianProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var counter = 0;
            var operand = AccessTools.FirstMethod(typeof(TestH), m => m.Name.Equals(nameof(GetRandomBinary)));
            foreach (var code in instructions)
            {
                if (counter == 0)
                {
                    if (code.opcode == OpCodes.Ldfld
                    && code.operand is FieldInfo info && info.Name.Equals("playVoices"))
                    {
                        counter++;
                    }
                }
                else
                {
                    counter = 0;
                    if (code.opcode == OpCodes.Ldc_I4_1 || code.opcode == OpCodes.Ldc_I4_0)
                    {
                        //SensibleH.Logger.LogDebug($"HLesbianProc:Patch:{code.opcode}:{code.operand}");
                        yield return new CodeInstruction(OpCodes.Call, operand);
                        continue;
                    }
                }
                yield return code;
            }
        }
        [HarmonyTranspiler, HarmonyPatch(typeof(HLesbian), nameof(HLesbian.MotionChange))]
        public static IEnumerable<CodeInstruction> HLesbianMotionChangeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (!found)
                    {
                        if (code.opcode == OpCodes.Callvirt
                        && code.operand is MethodInfo info && info.Name.Equals("set_enabled"))
                        {
                            found = true;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Ldc_I4_0 || code.opcode == OpCodes.Ldc_I4_3)
                        {
                            //SensibleH.Logger.LogDebug($"HLesbianMotionChange:Patch:{code.opcode}:{code.operand}");
                            yield return new CodeInstruction(OpCodes.Nop);
                            continue;
                        }
                        if (code.opcode == OpCodes.Call)
                        {
                            //SensibleH.Logger.LogDebug($"HLesbianMotionChange:Patch:{code.opcode}:{code.operand}");
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            done = true;
                            continue;
                        }
                    }
                }
                yield return code;
            }
        }
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HFlag), nameof(HFlag.SetInsertKokanVoiceCondition))]
        //[HarmonyPatch(typeof(HFlag), nameof(HFlag.SetInsertAnalVoiceCondition))]
        //public static void HSonyuMotionChange()
        //{
        //    SensibleH.Logger.LogDebug($"HSonyuMotionChangePrefix");
        //}

        //}
        /// <summary>
        /// Used as trigger to look at particular item.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetItem))]
        public static void HandCtrlSetItemPostfix(int _arrayArea)
        {
            if (_arrayArea < 3 && UnityEngine.Random.value < 0.67f)
            {
                SensibleHController.Instance.OnTouch(_arrayArea);
            }
        }
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(MotionIK), nameof(MotionIK.LinkIK))]
        //public static void LinkIKPostfix(int index, MotionIKData.State state, MotionIK.IKTargetPair pair, float __state)
        //{
        //    if (__state == 1f && pair.effector.positionWeight != 1f)
        //    {
        //        SensibleH.Logger.LogWarning($"LinkIK:ChangedPositionWeight:{pair.effector.bone.name}");
        //    }

        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiEmitterCtrl), nameof(ObiEmitterCtrl.OnEnable))]
        //public static void ObiEmitterCtrlOnEnablePrefix(ObiEmitterCtrl __instance)
        //{
        //    SensibleH.Logger.LogDebug($"ObiEmitterCtrlOnEnablePrefix [{__instance.gameObject.name}]");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiEmitterCtrl), nameof(ObiEmitterCtrl.SetCamera))]
        //public static void ObiEmitterCtrlSetCameraPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiEmitterCtrlSetCameraPrefix");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiEmitterCtrl), nameof(ObiEmitterCtrl.RemoveCamera))]
        //public static void ObiEmitterCtrlRemoveCameraPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiEmitterCtrlRemoveCameraPrefix");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiFluidManager), MethodType.Constructor)]
        //public static void ObiFluidManagerPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiFluidManagerConstructorPrefix");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiFluidManager), nameof(ObiFluidManager.Setup))]
        //public static void ObiFluidManagerSetupPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiFluidManagerSetupPrefix");
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiCtrl), MethodType.Constructor, new Type[] {typeof(HFlag)})]
        //public static void ObiCtrlContructorPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiCtrlContructorPrefix");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HActionBase), nameof(HActionBase.SetPlay))]
        //public static void SetPlayPrefix(HActionBase __instance)
        //{
        //    SensibleH.Logger.LogDebug($"SetPlay:{__instance.flags.nowAnimStateName}:{__instance.hand.action}\n{new StackTrace(0)}");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.FinishAction))]
        //public static void FinishActionPrefix(HandCtrl __instance)
        //{
        //    SensibleH.Logger.LogDebug($"FinishAction");
        ////}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.BreathProc))]
        //public static void ForceFinishPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"BreathProc");
        ////}
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.GetPLayNumBreathList))]
        //public static void OnCollisionPrefix(List<HVoiceCtrl.VoiceSelect> __result)
        //{
        //    SensibleH.Logger.LogDebug($"GetPLayNumBreathList:{__result.Count}");
        //}
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.IsPlayBreathVoicePtn))]
        //public static void SetAnimationPrefix(bool __result)
        //{
        //    SensibleH.Logger.LogDebug($"IsPlayBreathVoicePtn:{__result}");
        //}
#if KK
        /// <summary>
        /// A clutch to synchronize cloth states between action/talk scenes after H. Ugly but can't seem to find a culprit otherwise.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TalkScene), nameof(TalkScene.Start), MethodType.Enumerator)]
        public static void TalkSceneStartPrefix(TalkScene __instance)
        {
            SensibleH.Logger.LogDebug($"TalkScene:Start:{__instance}:{__instance.GetType()}");
            // Coroutine yields few times before cloning a chara, we grab the original and wait for a clone.
            // For some reason the clone comes with cloth states from a previous H Scene. We overwrite them with original states.
            // No clue where it hides those states, all the SaveData and ChaFiles are actual. 

            TalkSceneClothesState();
        }
        public static TalkScene talkScene;
        public static ChaControl originalChara;
        public static void TalkSceneClothesState()
        {
            if (talkScene == null)
            {
                talkScene = UnityEngine.Object.FindObjectOfType<TalkScene>();
            }
            if (talkScene.targetHeroine != null)
            {
                if (originalChara == null)
                {
                    originalChara = talkScene.targetHeroine.chaCtrl;
                }
                else if (originalChara != talkScene.targetHeroine.chaCtrl)
                {
                    var target = talkScene.targetHeroine.chaCtrl.fileStatus.clothesState;
                    var precursor = originalChara.fileStatus.clothesState;
                    for (var i = 0; i < precursor.Length; i++)
                    {
                        target[i] = precursor[i];
                    }
                    originalChara = null;
                }
            }
        }
#endif
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(UnityEngine.Animator), nameof(UnityEngine.Animator.PlayInFixedTime), typeof(int))]
        //public static void AnimatorPlay1(int stateNameHash)
        //{
        //    SensibleH.Logger.LogDebug($"PlayInFixedTime:6:{stateNameHash}");
        //}
#if KKS
        //        [HarmonyPrefix]
         //      [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.playDynamicBoneBust), new Type[]            
        //        public static bool DisableShapeBodyIDPrefix(int LR, int id, bool disable)
        //        {
        //            SensibleH.Logger.LogInfo($"DisableShapeBodyID:{id}:{LR}:{disable}");

        //            return !disable;
        //        }
        //        [HarmonyPrefix]
        //        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.DisableShapeBust))]              
        //        public static bool DisableShapeBodyIDPrefix(int LR, bool disable)
        //        {
        //            SensibleH.Logger.LogInfo($"DisableShapeBodyID:{LR}:{disable}");
        //            return !disable;
        //        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.playDynamicBoneBust), [ typeof(int), typeof(bool) ])]
        //public static void playDynamicBoneBustPrefix(int _nArea, ref bool _bPlay)
        //{
        //    SensibleH.Logger.LogInfo($"playDynamicBoneBustPrefix:{_nArea}:{_bPlay}");
        //    _bPlay = true;

        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.playDynamicBoneBust), [ typeof(ChaInfo.DynamicBoneKind), typeof(bool) ])]
        //public static void playDynamicBoneBustPrefix(ChaInfo.DynamicBoneKind _eArea, ref bool _bPlay)
        //{
        //    SensibleH.Logger.LogInfo($"playDynamicBoneBustPrefix:{_eArea}:{_bPlay}");
        //    _bPlay = true;
        //}
#endif

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(UnityEngine.Animator), nameof(UnityEngine.Animator.PlayInFixedTime), new System.Type[]{
        //    typeof(string),
        //    typeof(int),
        //    typeof(float)
        //})]
        //public static void AnimatorPlay4(string stateName, int layer, float fixedTime)
        //{
        //    SensibleH.Logger.LogDebug($"AnimatorPlayInFixedTime:3:{stateName}:{layer}:{fixedTime}");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(UnityEngine.Animator), nameof(UnityEngine.Animator.PlayInFixedTime), new System.Type[]{
        //    typeof(string)
        //})]
        //public static void AnimatorPlay5(string stateName)
        //{
        //    SensibleH.Logger.LogDebug($"AnimatorPlayInFixedTime:2:{stateName}");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(UnityEngine.Animator), nameof(UnityEngine.Animator.PlayInFixedTime), new System.Type[]{
        //    typeof(string),
        //    typeof (int)
        //})]
        //public static void AnimatorPlay6(string stateName, int layer)
        //{
        //    SensibleH.Logger.LogDebug($"AnimatorPlayInFixedTime:1:{stateName}:{layer}");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetLayerWeightDefault))]
        //public static void SetLayerWeightDefaultPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"SetLayerWeightDefault");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetLayerWeight))]
        //public static void SetLayerWeightPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"SetLayerWeight");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl.AibuItem), nameof(HandCtrl.AibuItem.SetHandColor), typeof(Color))]
        //public static void SetHandColorPrefix(Color _color)
        //{
        //    SensibleH.Logger.LogInfo($"SetHandColor:{_color}");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.EnableShape))]
        //public static void EnableShapePrefix()
        //{
        //    SensibleH.Logger.LogDebug($"EnableShape");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetShapeON))]
        //public static void SetShapeONPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"SetShapeON");
        //}
    }
}
