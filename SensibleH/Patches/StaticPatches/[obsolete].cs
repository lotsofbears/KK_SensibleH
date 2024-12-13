//using HarmonyLib;
//using KK_SensibleH.AutoMode;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using static Illusion.Component.ShortcutKey;
//using static KK_SensibleH.SensibleH;

//namespace KK_SensibleH.Patches.StaticPatches
//{
//    class ToSort
//    {
       

        
        
        
//        /// <summary>
//        /// We override basic neck with our pick, then in case of custom eyeCam we alter "_tag" of "SetNeckTarget()" too.
//        /// </summary>
//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookNeckPtn))]
//        //public static bool ChangeLookNeckPtnPrefix(int ptn, ref float rate, ChaControl __instance)
//        //{
//        //    if (MoveNeckGlobal && __instance.sex == 1)
//        //    {
//        //        //SensibleH.Logger.LogDebug($"ChangeLookNeckPtn");
//        //        if (__instance == _chaControl[0])
//        //        {
//        //            if (!IsNeckSet[0])
//        //            {
//        //                rate = NeckChangeRate[0]; 
//        //                return true;
//        //            }
//        //            else
//        //                return false;
//        //        }
//        //        else
//        //        {
//        //            if (!IsNeckSet[1])
//        //            {
//        //                rate = NeckChangeRate[1];
//        //                return true;
//        //            }
//        //            else
//        //                return false;
//        //        }
//        //    }
//        //    return true;
//        //    //if (__instance.chara.sex == 0 && _tag != 0 && MalePoI != null)
//        //    //{
//        //    //    _tag = 0;
//        //    //}

//        //}
//        //[HarmonyPostfix]
//        //[HarmonyPatch(typeof(FaceListCtrl), nameof(FaceListCtrl.SetFace))]
//        //public static void AlterSetFace(int _idFace, ChaControl _chara, int _voiceKind, int _action)
//        //{


//        //}

//        /// <summary>
//        /// TODO replace this with transpiler.
//        /// We run voiceProcs for caress actions in non-Aibu modes.
//        /// </summary>
//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
//        //public static void ClickActionPrefix(HandCtrl __instance)
//        //{
//        //    //SensibleH.Logger.LogDebug("ClickAction");
//        //    if ((__instance.flags.mode == HFlag.EMode.houshi || __instance.flags.mode == HFlag.EMode.sonyu) &&
//        //        (__instance.voicePlayClickCount + 1) % __instance.voicePlayClickLoop == 0 &&
//        //        __instance.voice.nowVoices[__instance.numFemale].state != HVoiceCtrl.VoiceKind.voice)
//        //    {
//        //        __instance.voicePlayClickCount = 0;
//        //        __instance.voicePlayClickLoop = UnityEngine.Random.Range(10, 20);
//        //        int num = __instance.useItems[__instance.actionUseItem].kindTouch - HandCtrl.AibuColliderKind.mouth;
//        //        int[] array = new int[]
//        //        {
//        //                0,
//        //                1,
//        //                1,
//        //                2,
//        //                3,
//        //                4,
//        //                4
//        //        };
//        //        int[,] array2 = new int[,]
//        //            {
//        //                    {
//        //                        -1,
//        //                        111,
//        //                        113,
//        //                        115,
//        //                        117,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        123,
//        //                        119,
//        //                        121,
//        //                        -1,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        131,
//        //                        125,
//        //                        127,
//        //                        129,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        137,
//        //                        133,
//        //                        -1,
//        //                        135,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        -1,
//        //                        139,
//        //                        -1,
//        //                        -1,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1
//        //                    },
//        //                    {
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1,
//        //                        -1
//        //                    }
//        //            };
//        //        __instance.flags.voice.playVoices[__instance.numFemale] = array2[__instance.useItems[__instance.actionUseItem].idObj, array[num]];
//        //        __instance.isClickDragVoice = true;
//        //        __instance.voice.nowVoices[__instance.numFemale].notOverWrite = false;
//        //    }
//        //}

        
        
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetLayerWeight))]
//        //public static void SetLayerWeight()
//        //{
//        //    //SensibleH.Logger.LogDebug($"SetLayerWeight");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.EnableDynamicBone))]
//        //public static void EnableDynamicBone()
//        //{
//        //    //SensibleH.Logger.LogDebug($"EnableDynamicBone");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetItem))]
//        //public static void SetItemPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("SetItem");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.AnimatrotRestrart))]
//        //public static void AnimatrotRestrartPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("AnimatrotRestrart");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.KissAction))]
//        //public static void KissActionPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("KissAction");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
//        //public static void DragActionPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("DragAction");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.FinishAction))]
//        //public static void FinishActionPrefix(HandCtrl __instance)
//        //{
//        //    //SensibleH.Logger.LogDebug($"FinishAction[timeDragCalc: {__instance.flags.timeDragCalc}]");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
//        //public static void JudgeProcPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("JudgeProc");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetDragStartLayer))]
//        //public static void SetDragStartLayerPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("SetDragStartLayer");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HFlag), nameof(HFlag.WaitSpeedProcItem))]
//        //public static void WaitSpeedProcItemPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("WaitSpeedProcItem");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HFlag), nameof(HFlag.WaitSpeedProcAibu))]
//        //public static void WaitSpeedProcAibuPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("WaitSpeedProcAibu");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(HAibu), nameof(HAibu.Proc))]
//        //public static void HaibuProcPrefix(HAibu __instance)
//        //{
//        //    //SensibleH.Logger.LogDebug($"HaibuProc[{__instance.hand.actionUseItem}");
//        //}

//        //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
//        //public static void ClickActionPrefix()
//        //{
//        //    //SensibleH.Logger.LogDebug("ClickAction");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookNeckTarget))]
//        //public static void ChangeChangeLookNeckTargetPrefix()//(int targetType, Transform trfTarg = null, float rate = 0.5f, float rotDeg = 0f, float range = 1f, float dis = 0.8f)
//        //{
//        //    //SensibleH.Logger.LogDebug("ChangeLookNeckTarget ");
//        //}
//        //[HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookNeckPtn))]
//        //public static void ChangeLookNeckPtnPrefix(int ptn, float rate = 1f)
//        //{
//        //    //SensibleH.Logger.LogDebug($"ChangeLookNeckPtn {ptn}");
//        //}
//        /*
//         * During kiss neckPtn is 3;
//         * eyes - 0, 2, 4, 25
//         * brows - 0, 2, 11
//         */

//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(FBSBase), nameof(ChaInfo.eyesCtrl.ChangePtn))]
//        //public static void ChangePtnEyesPrefix(ref int ptn, ref bool blend)
//        //{
//        //    ptn = 25;
//        //    blend = true;
//        //    //SensibleH.Logger.LogDebug($"ChangePtnEyes {ptn}");
//        //}

//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(FBSBase), nameof(ChaInfo.eyebrowCtrl.ChangePtn))]
//        //public static void ChangePtnEyebrowPrefix(ref int ptn, ref bool blend)
//        //{
//        //    ptn = 14;
//        //    blend = true;
//        //    //SensibleH.Logger.LogDebug($"ChangePtnEyebrow {ptn}");
//        //}
//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
//        //public static void ChangeEyebrowOpenMaxPrefix(ref float maxValue)
//        //{
//        //    maxValue = 0f;
//        //}
//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
//        //public static void ChangeEyesOpenMaxPrefix(ref float maxValue)
//        //{
//        //    maxValue = 0f;
//        //}ChangeCoordinateTypeAndReload
//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), new System.Type[] { typeof(bool) })]
//        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), new System.Type[] { typeof(ChaFileDefine.CoordinateType), typeof(bool) })]
//        //public static void ChangeCoordinateType(ref bool changeBackCoordinateType)
//        //{
//        //    changeBackCoordinateType = false;
//        //}
//    }
//}
