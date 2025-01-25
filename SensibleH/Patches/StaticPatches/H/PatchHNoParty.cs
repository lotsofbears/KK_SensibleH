using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchHNoParty
    {
        [HarmonyPostfix, HarmonyPatch(typeof(H3PDarkSonyu), nameof(H3PDarkSonyu.Proc))]
        public static void H3PDarkSonyuProcPostfix(H3PDarkSonyu __instance)
        {
            if (SensibleH.OLoop)
            {
                __instance.LoopProc(true);
            }
        }

    }
}
