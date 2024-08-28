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
        /// Adjustments for non-standard dick diameters in houshi.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(FaceListCtrl), nameof(FaceListCtrl.SetFace))]
        public static void SetFacePrefix(int _idFace, int _voiceKind, int _action, FaceListCtrl __instance)
        {
            var dic = __instance.facelib[_voiceKind][_action][_idFace];
            if (SensibleH._hFlag != null && SensibleH._hFlag.mode == HFlag.EMode.houshi && dic.openMinMouth == 1f && (dic.mouth == 22 || dic.mouth == 21))
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
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ObiEmitterCtrl), MethodType.Constructor)]
        //public static void ObiEmitterCtrlPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiEmitterCtrlConstructorPrefix");
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
        //[HarmonyPatch(typeof(ObiSolver), "OnEnable")]
        //public static void ObiSolverOnEnablePrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ObiSolverOnEnablePrefix");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HSceneProc), MethodType.Constructor)]
        //public static void HSceneProcConstructorPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"HSceneProcConstructorPrefix");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.FinishAction))]
        //public static void FinishActionPrefix(HandCtrl __instance)
        //{
        //    SensibleH.Logger.LogDebug($"FinishAction");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
        //public static void ClickActionPrefix(HandCtrl __instance)
        //{
        //    SensibleH.Logger.LogDebug($"ClickAction {__instance.useItems[__instance.actionUseItem] == null}");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ForceFinish))]
        //public static void ForceFinishPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"ForceFinish");
        //}
        ////[HarmonyPrefix]
        ////[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.OnCollision))]
        ////public static void OnCollisionPrefix()
        ////{
        ////    SensibleH.Logger.LogDebug($"OnCollision");
        ////}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetAnimation))]
        //public static void SetAnimationPrefix(HandCtrl __instance)
        //{
        //    SensibleH.Logger.LogDebug($"SetAnimation");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.AnimatrotRestrart))]
        //public static void AnimatrotRestrartPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"AnimatrotRestrart");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetDragStartLayer))]
        //public static void SetDragStartLayernPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"SetDragStartLayer");
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.EnableDynamicBone))]
        //public static void EnableDynamicBonePrefix()
        //{
        //    SensibleH.Logger.LogDebug($"EnableDynamicBone");
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
        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetIdleLayerWeight))]
        //public static void SetIdleLayerWeightPrefix()
        //{
        //    SensibleH.Logger.LogDebug($"SetIdleLayerWeight");
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
