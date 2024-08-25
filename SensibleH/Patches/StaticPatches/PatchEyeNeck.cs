using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static KK_SensibleH.SensibleH;

namespace KK_SensibleH.Patches.StaticPatches
{
    class PatchEyeNeck
    {
        //public static int TestNumber;
        public static int[] NeckTargetTag = { 0, 0 };
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeck), nameof(HMotionEyeNeck.SetEyeNeckPtn))]
        public static void PrefixSetEyeNeckPtn(ref int _id, ref GameObject _objCamera, bool _isConfigEyeDisregard, bool _isConfigNeckDisregard, HMotionEyeNeckMale __instance)
        {
            if (MoveNeckGlobal && __instance.chara.sex == 1)
            {
                //SensibleH.Logger.LogDebug($"SetEyeNeckPtn");
                if (__instance.chara == _chaControl[0])
                {
                    _id = EyeNeckPtn[0];
                }
                else
                {
                    _id = EyeNeckPtn[1];
                }
            }
            //if (__instance.chara.sex == 0 && _chaControlM != null && _chaControlM.visibleAll)
            //{
            //    _id = EyeNeckPtn[2];
            //    if (MalePoI != null)
            //        _objCamera = MalePoI;
            //}

        }
        /// <summary>
        /// 
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HMotionEyeNeck), nameof(HMotionEyeNeck.SetNeckTarget))]
        public static bool SetNeckTargetPrefix(ref int _tag, float _rate, ref GameObject _objCamera, bool _isConfigDisregard, HMotionEyeNeckMale __instance)
        {
            if (MoveNeckGlobal && __instance.chara.sex == 1)
            {
                //if (_tag != TestNumber)
                //{
                //    TestNumber = _tag;
                //    SensibleH.Logger.LogDebug($"SetNeckTarget[NewNeck - {_tag}]");
                //}
                if (__instance.chara == _chaControl[0])
                {
                    if (!IsNeckSet[0])
                    {
                        if (FemalePoI[0] != null)
                        {
                            _objCamera = FemalePoI[0];
                            _tag = NeckTargetTag[0];
                        }
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (!IsNeckSet[1])
                    {
                        if (FemalePoI[1] != null)
                        {
                            _objCamera = FemalePoI[1];
                            _tag = NeckTargetTag[1];
                        }
                        return true;
                    }
                    else
                        return false;
                }
            }
            return true;
            //if (__instance.chara.sex == 0 && _tag != 0 && MalePoI != null)
            //{
            //    _tag = 0;
            //}

        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookNeckTarget))]
        public static bool ChangeLookNeckTargetPrefix(ChaControl __instance)
        {
            if (MoveNeckGlobal && __instance.sex == 1)
            {
                //SensibleH.Logger.LogDebug($"ChangeLookNeckTarget");
                if (__instance == _chaControl[0])
                {
                    if (!IsNeckSet[0])
                    {
                        IsNeckSet[0] = true;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (!IsNeckSet[1])
                    {
                        IsNeckSet[1] = true;
                        return true;
                    }
                    else
                        return false;
                }
            }
            return true;
            //if (__instance.chara.sex == 0 && _tag != 0 && MalePoI != null)
            //{
            //    _tag = 0;
            //}

        }
    }
}
