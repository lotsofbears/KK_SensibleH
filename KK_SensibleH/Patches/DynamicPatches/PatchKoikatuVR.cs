using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using KoikatuVR;
using KoikatuVR.Caress;

namespace KK_SensibleH.Patches.DynamicPatches
{
    class PatchKoikatuVR
    {
        /// <summary>
        /// We remove synthetic mouse up click that is being fed directly to "HandCtrl".
        /// </summary>

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HandCtrlHooks), nameof(HandCtrlHooks.InjectMouseButtonUp))]
        public static bool InjectMouseButtonUpPrefix()
        {
            return false;
        }
        //[HarmonyTranspiler, HarmonyPatch(typeof(HandCtrlHooks), nameof(HandCtrlHooks.InjectMouseButtonUp))]
        //public static IEnumerable<CodeInstruction> RemoveInjectMouseButtonUp(IEnumerable<CodeInstruction> instructions)
        //{
        //    foreach (var code in instructions)
        //    {
        //        if (code.opcode != OpCodes.Ret)
        //            yield return new CodeInstruction(OpCodes.Nop);
        //        else
        //            yield return code;
        //    }
        //}
        //[HarmonyTranspiler, HarmonyPatch(typeof(HandCtrlHooks), nameof(HandCtrlHooks.InjectMouseButtonDown))]
        //public static IEnumerable<CodeInstruction> RemoveInjectMouseButtonDown(IEnumerable<CodeInstruction> instructions)
        //{
        //    foreach (var code in instructions)
        //    {
        //        if (code.opcode != OpCodes.Ret)
        //            yield return new CodeInstruction(OpCodes.Nop);
        //        else
        //            yield return code;
        //    }
        //    //for (var i = 0; i < codes.Count; i++)
        //    //{
        //    //    if (codes[i].opcode != OpCodes.Ret)
        //    //        codes[i].opcode = OpCodes.Nop;

        //    //}
        //    //return codes.AsEnumerable();
        //}
    }
}
