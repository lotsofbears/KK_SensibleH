using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using KK_SensibleH;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchHParty
    {

        /// <summary>
        /// Breath in OLoop enabler.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.BreathProc))]
        public static void BreathProcPrefix(ref AnimatorStateInfo _ai, HVoiceCtrl __instance)
        {
            if (SensibleH.OLoop
                && (__instance.flags.mode == HFlag.EMode.sonyu
                || __instance.flags.mode == HFlag.EMode.sonyu3P))
            {
                _ai = SensibleH.sLoopInfo;
            }
        }
    }
}
