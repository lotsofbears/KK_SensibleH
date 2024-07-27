using ADV.Commands.Base;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace KK_SensibleH.Patches.DynamicPatches
{
    class PatchSteamVR
    {
        //[HarmonyTranspiler, HarmonyPatch(typeof(SteamVR_Controller.Device), nameof(SteamVR_Controller.Device.GetPress), new Type[] { typeof(ulong) })]
        //public static IEnumerable<CodeInstruction> GetPressUpTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var found = false;
        //    foreach (var code in instructions)
        //    {
        //        if (!found)
        //        {
        //            if (code.opcode == OpCodes.Cgt_Un)
        //            {
        //                found = true;
        //                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
        //            }
        //            else
        //                yield return new CodeInstruction(OpCodes.Nop);
        //        }
        //        else
        //            yield return code;
        //    }
        //}
        /// <summary>
        /// We intercept "Grip" GetPress and return "false".
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamVR_Controller.Device), nameof(SteamVR_Controller.Device.GetPress), new Type[] { typeof(ulong) })]
        public static bool GetPressPrefix(ulong buttonMask, ref bool __result)
        {
            if (buttonMask == 4)
            {
                __result = false;
                return false;
            }
            else
                return true;
        }
    }
}
