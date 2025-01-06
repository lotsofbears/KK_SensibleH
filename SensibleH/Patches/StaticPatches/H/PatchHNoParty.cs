using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchHNoParty
    {
        [HarmonyPostfix, HarmonyPatch(typeof(H3PSonyu), nameof(H3PDarkSonyu.Proc))]
        public static void H3PDarkSonyuProcPostfix(H3PDarkSonyu __instance)
        {
            if (SensibleH.OLoop)
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
            if (SensibleH.OLoop 
                && (__instance.flags.mode == HFlag.EMode.sonyu 
                || __instance.flags.mode == HFlag.EMode.sonyu3P
                || __instance.flags.mode == HFlag.EMode.sonyu3PMMF))
            {
                _ai = SensibleH.sLoopInfo;
            }
        }
    }
}
